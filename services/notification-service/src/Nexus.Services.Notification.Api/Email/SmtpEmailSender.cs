using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Nexus.Services.Notification.Api.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string FromAddress { get; set; } = "noreply@nexus.local";
    public string FromName { get; set; } = "Nexus Platform";
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public bool UseSsl { get; set; }
}

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        var secureSocketOptions = _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(_options.Host, _options.Port, secureSocketOptions, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
        _logger.LogInformation("Email sent to {Email} with subject {Subject}.", toEmail, subject);
    }
}

public static class TenantWelcomeEmailTemplate
{
    public static string Build(string tenantName, string tenantCode, string portalUrl)
    {
        return $"""
            <html>
            <body style="font-family:Segoe UI,Arial,sans-serif;color:#0F172A;">
              <h2>Chào mừng đến với Nexus</h2>
              <p>Tenant <strong>{tenantName}</strong> của bạn đã được tạo thành công.</p>
              <p>Mã tenant của bạn: <strong style="font-size:20px;">{tenantCode}</strong></p>
              <p>Bạn có thể đăng nhập bằng Google hoặc email/mật khẩu tại:</p>
              <p><a href="{portalUrl}">{portalUrl}</a></p>
              <p style="color:#64748B;font-size:13px;">Mỗi email chỉ liên kết với một tenant.</p>
            </body>
            </html>
            """;
    }
}
