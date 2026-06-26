using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Nexus.BuildingBlocks.Application;
using Nexus.EventContracts.Identity;
using Nexus.Services.Identity.Application.Users;
using Nexus.Services.Identity.Contracts.Onboarding;
using Nexus.Services.Identity.Contracts.Users;
using Nexus.Services.Identity.Domain.Onboarding;
using Nexus.Services.Identity.Domain.Users;
using Nexus.Services.Tenant.Contracts.Tenants;
using Nexus.SharedKernel.Auditing;
using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Events;
using Nexus.SharedKernel.Exceptions;

namespace Nexus.Services.Identity.Application.Onboarding;

public sealed class OnboardingAppService : NexusAppServiceBase, IOnboardingAppService
{
    private const string ServiceName = "identity-service";
    private const string GoogleProvider = "Google";

    private readonly IOnboardingRepository _onboardingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly ITenantServiceClient _tenantServiceClient;
    private readonly IEventBus _eventBus;
    private readonly IAuditWriter _auditWriter;
    private readonly IConfiguration _configuration;

    public OnboardingAppService(
        IOnboardingRepository onboardingRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IGoogleTokenValidator googleTokenValidator,
        ITenantServiceClient tenantServiceClient,
        IEventBus eventBus,
        IAuditWriter auditWriter,
        IConfiguration configuration,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext) : base(currentTenant, currentUser, correlationContext)
    {
        _onboardingRepository = onboardingRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _googleTokenValidator = googleTokenValidator;
        _tenantServiceClient = tenantServiceClient;
        _eventBus = eventBus;
        _auditWriter = auditWriter;
        _configuration = configuration;
    }

    public async Task<GoogleAuthResultDto> GoogleAuthAsync(GoogleAuthDto input, CancellationToken cancellationToken = default)
    {
        var payload = await _googleTokenValidator.ValidateAsync(input.IdToken, input.AccessToken, cancellationToken);
        var email = User.NormalizeEmail(payload.Email);

        var registration = await _onboardingRepository.FindRegistrationByEmailAsync(email, cancellationToken);
        if (registration is not null)
        {
            var user = await ResolveRegisteredUserAsync(registration, cancellationToken);
            await EnsureExternalLoginAsync(user, payload.Subject, cancellationToken);
            var login = await IssueLoginAsync(user, cancellationToken);
            var tenant = await _tenantServiceClient.GetTenantByIdAsync(registration.TenantId, cancellationToken);
            return new GoogleAuthResultDto
            {
                Status = "Authenticated",
                Email = email,
                DisplayName = payload.Name,
                Login = EnrichLogin(login, tenant?.Code, user.UserName, user.Email)
            };
        }

        var externalLogin = await _onboardingRepository.FindExternalLoginAsync(GoogleProvider, payload.Subject, cancellationToken);
        if (externalLogin is not null)
        {
            var user = await _userRepository.FindByIdWithRolesAsync(externalLogin.UserId, cancellationToken)
                ?? throw new NexusBusinessException(UserErrorCodes.NotFound, "Linked user was not found.");
            var login = await IssueLoginAsync(user, cancellationToken);
            return new GoogleAuthResultDto
            {
                Status = "Authenticated",
                Email = email,
                DisplayName = payload.Name,
                Login = EnrichLogin(login, null, user.UserName, user.Email)
            };
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        var session = new OnboardingSession(
            Guid.NewGuid(),
            token,
            email,
            payload.Subject,
            payload.Name ?? email,
            DateTimeOffset.UtcNow.AddHours(2));
        await _onboardingRepository.InsertSessionAsync(session, cancellationToken);

        return new GoogleAuthResultDto
        {
            Status = "NeedOnboarding",
            OnboardingToken = token,
            Email = email,
            DisplayName = payload.Name
        };
    }

    public async Task<PreviewTenantCodeResultDto> PreviewCodeAsync(PreviewTenantCodeDto input, CancellationToken cancellationToken = default)
    {
        string suggestedCode;
        var available = false;

        for (var attempt = 0; attempt < 5; attempt++)
        {
            suggestedCode = TenantCodeGenerator.BuildSuggestedCode(input.CompanyName);
            available = await _tenantServiceClient.IsCodeAvailableAsync(suggestedCode, cancellationToken);
            if (available)
            {
                return new PreviewTenantCodeResultDto
                {
                    SuggestedCode = suggestedCode,
                    Available = true
                };
            }
        }

        suggestedCode = TenantCodeGenerator.BuildSuggestedCode($"{input.CompanyName}{Random.Shared.Next(100, 999)}");
        available = await _tenantServiceClient.IsCodeAvailableAsync(suggestedCode, cancellationToken);
        return new PreviewTenantCodeResultDto
        {
            SuggestedCode = suggestedCode,
            Available = available
        };
    }

    public async Task<CompleteOnboardingResultDto> CompleteAsync(CompleteOnboardingDto input, CancellationToken cancellationToken = default)
    {
        var session = await _onboardingRepository.FindSessionByTokenAsync(input.OnboardingToken, cancellationToken)
            ?? throw new NexusBusinessException(OnboardingErrorCodes.InvalidSession, "Onboarding session is invalid.");

        if (session.IsExpired(DateTimeOffset.UtcNow))
        {
            throw new NexusBusinessException(OnboardingErrorCodes.InvalidSession, "Onboarding session has expired.");
        }

        var email = User.NormalizeEmail(session.Email);
        if (await _onboardingRepository.FindRegistrationByEmailAsync(email, cancellationToken) is not null)
        {
            throw new NexusBusinessException(OnboardingErrorCodes.EmailAlreadyRegistered, "Email is already registered to a tenant.");
        }

        if (!await _tenantServiceClient.IsCodeAvailableAsync(input.Code, cancellationToken))
        {
            throw new NexusBusinessException(OnboardingErrorCodes.CodeUnavailable, "Tenant code is not available.");
        }

        var defaultPlanCode = _configuration["Onboarding:DefaultPlanCode"] ?? "FREE";
        var tenant = await _tenantServiceClient.CreateTenantAsync(new CreateInternalTenantDto
        {
            Code = input.Code,
            Name = input.CompanyName,
            Address = input.Address,
            Phone = input.Phone,
            RepresentativeName = input.RepresentativeName,
            ContactEmail = email,
            PlanCode = defaultPlanCode
        }, cancellationToken);

        var userName = ResolveUserName(input.UserName, email);
        var passwordHash = string.IsNullOrWhiteSpace(input.Password)
            ? null
            : _passwordHasher.HashPassword(input.Password);

        if (!string.IsNullOrWhiteSpace(input.Password) && input.Password.Length < 6)
        {
            throw new NexusBusinessException(UserErrorCodes.InvalidCredentials, "Password must be at least 6 characters.");
        }

        var user = new User(Guid.NewGuid(), tenant.Id, userName, email, passwordHash, null, DateTimeOffset.UtcNow);
        user.SetRoles(["TENANTADMIN"], null, DateTimeOffset.UtcNow);
        await _userRepository.InsertAsync(user, cancellationToken);
        await _onboardingRepository.InsertExternalLoginAsync(new ExternalLogin(Guid.NewGuid(), user.Id, GoogleProvider, session.GoogleSub), cancellationToken);
        await _onboardingRepository.InsertRegistrationAsync(new TenantRegistration(Guid.NewGuid(), email, tenant.Id, user.Id, DateTimeOffset.UtcNow), cancellationToken);
        await _onboardingRepository.DeleteSessionAsync(session, cancellationToken);

        await _auditWriter.WriteAsync(new AuditLogEntry(Guid.NewGuid(), tenant.Id, user.Id, ServiceName, nameof(User), user.Id.ToString(), AuditAction.Create, "Tenant admin created via onboarding.", CorrelationContext.CorrelationId, DateTimeOffset.UtcNow), cancellationToken);
        await _eventBus.PublishAsync(new UserCreatedIntegrationEvent(Guid.NewGuid(), tenant.Id, DateTimeOffset.UtcNow, ServiceName, CorrelationContext.CorrelationId, user.Id, user.UserName, user.Email), cancellationToken);

        var login = await IssueLoginAsync(user, cancellationToken);
        return new CompleteOnboardingResultDto
        {
            TenantId = tenant.Id,
            TenantCode = tenant.Code,
            TenantName = tenant.Name,
            Login = EnrichLogin(login, tenant.Code, user.UserName, user.Email)
        };
    }

    public async Task<LoginResultDto> LoginEmailAsync(LoginEmailDto input, CancellationToken cancellationToken = default)
    {
        var email = User.NormalizeEmail(input.Email);
        var registration = await _onboardingRepository.FindRegistrationByEmailAsync(email, cancellationToken)
            ?? throw new NexusBusinessException(UserErrorCodes.InvalidCredentials, "Invalid email or password.");

        var user = await _userRepository.FindByEmailAsync(registration.TenantId, email, cancellationToken)
            ?? throw new NexusBusinessException(UserErrorCodes.InvalidCredentials, "Invalid email or password.");

        if (!user.IsActive || !user.HasPassword || !_passwordHasher.VerifyPassword(input.Password, user.PasswordHash))
        {
            throw new NexusBusinessException(UserErrorCodes.InvalidCredentials, "Invalid email or password.");
        }

        var login = await IssueLoginAsync(user, cancellationToken);
        var tenant = await _tenantServiceClient.GetTenantByIdAsync(registration.TenantId, cancellationToken);
        return EnrichLogin(login, tenant?.Code, user.UserName, user.Email);
    }

    public async Task<TenantByEmailResultDto?> GetTenantByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = User.NormalizeEmail(email);
        var registration = await _onboardingRepository.FindRegistrationByEmailAsync(normalized, cancellationToken);
        if (registration is null)
        {
            return null;
        }

        var tenant = await _tenantServiceClient.GetTenantByIdAsync(registration.TenantId, cancellationToken);
        if (tenant is null)
        {
            return null;
        }

        return new TenantByEmailResultDto
        {
            TenantId = tenant.Id,
            TenantCode = tenant.Code,
            TenantName = tenant.Name
        };
    }

    private async Task<User> ResolveRegisteredUserAsync(TenantRegistration registration, CancellationToken cancellationToken)
    {
        return await _userRepository.FindByIdWithRolesAsync(registration.UserId, cancellationToken)
            ?? await _userRepository.FindByEmailAsync(registration.TenantId, registration.Email, cancellationToken)
            ?? throw new NexusBusinessException(UserErrorCodes.NotFound, "Registered user was not found.");
    }

    private async Task EnsureExternalLoginAsync(User user, string googleSub, CancellationToken cancellationToken)
    {
        var existing = await _onboardingRepository.FindExternalLoginAsync(GoogleProvider, googleSub, cancellationToken);
        if (existing is null)
        {
            await _onboardingRepository.InsertExternalLoginAsync(new ExternalLogin(Guid.NewGuid(), user.Id, GoogleProvider, googleSub), cancellationToken);
        }
    }

    private async Task<LoginResultDto> IssueLoginAsync(User user, CancellationToken cancellationToken)
    {
        await _auditWriter.WriteAsync(new AuditLogEntry(Guid.NewGuid(), user.TenantId, user.Id, ServiceName, nameof(User), user.Id.ToString(), AuditAction.Access, "User logged in.", CorrelationContext.CorrelationId, DateTimeOffset.UtcNow), cancellationToken);
        var tokens = await _tokenService.IssueAsync(user, cancellationToken);
        return new LoginResultDto
        {
            UserId = user.Id,
            TenantId = user.TenantId!.Value,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresAt = tokens.ExpiresAt,
            UserName = user.UserName,
            Email = user.Email
        };
    }

    private static string ResolveUserName(string? requestedUserName, string email)
    {
        if (!string.IsNullOrWhiteSpace(requestedUserName))
        {
            return User.NormalizeUserName(requestedUserName);
        }

        var localPart = email.Split('@')[0];
        return User.NormalizeUserName(localPart);
    }

    private static LoginResultDto EnrichLogin(LoginResultDto login, string? tenantCode, string userName, string email)
    {
        return new LoginResultDto
        {
            UserId = login.UserId,
            TenantId = login.TenantId,
            AccessToken = login.AccessToken,
            RefreshToken = login.RefreshToken,
            ExpiresAt = login.ExpiresAt,
            TenantCode = tenantCode,
            UserName = userName,
            Email = email
        };
    }
}
