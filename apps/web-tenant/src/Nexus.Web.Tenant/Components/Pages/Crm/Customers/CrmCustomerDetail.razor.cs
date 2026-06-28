using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm.Customers;

public partial class CrmCustomerDetail
{
    [Inject] private FileApiClient FileApi { get; set; } = default!;

    [Parameter] public Guid Id { get; set; }

    private bool _loading;
    private CustomerDto? _customer;
    private IReadOnlyList<ContactDto> _contacts = [];
    private IReadOnlyList<OpportunityDto> _opportunities = [];
    private IReadOnlyList<QuotationDto> _quotations = [];
    private IReadOnlyList<ContractDto> _contracts = [];
    private IReadOnlyList<ActivityDto> _activities = [];
    private Modal? _editModal;
    private CustomerEditModel _editModel = new();
    private string _customerTypeText = CustomerType.Company.ToString();
    private List<SelectedItem> _customerTypeOptions = CrmLabels.CustomerTypeOptions();
    private string _customerStatusText = CustomerStatus.Active.ToString();
    private List<SelectedItem> _customerStatusOptions = CrmLabels.CustomerStatusOptions();
    private Dictionary<string, (string Text, string BadgeClass)> _customerStatusMeta = CrmLabels.CustomerStatusMeta();
    private Modal? _contactModal;
    private Modal? _opportunityModal;
    private Modal? _quotationModal;
    private Modal? _contractModal;
    private Modal? _activityModal;
    private QuickContactModel _contactModel = new();
    private QuickOpportunityModel _opportunityModel = new();
    private QuickQuotationModel _quotationModel = new();
    private QuickContractModel _contractModel = new();
    private QuickActivityModel _activityModel = new();
    private string _activityTypeText = CrmActivityType.Task.ToString();
    private string _contractOpportunityIdText = "";
    private List<SelectedItem> _activityTypeOptions = CrmLabels.ActivityTypeOptions();
    private List<SelectedItem> ContractOpportunityOptions =>
    [
        new("", "Không gắn cơ hội"),
        .. _opportunities.Select(x => new SelectedItem(x.Id.ToString(), $"{x.Name} - {VnMoney.Format(x.Amount)} {x.Currency} / {CrmLabels.FormatOpportunityStage(x.Stage)}"))
    ];

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _customer = await CrmApi.GetCustomerAsync(Id);
            if (_customer is not null)
            {
                _contacts = (await CrmApi.GetContactsAsync(new ContactListQuery { CustomerId = Id, MaxResultCount = 50 }))?.Items ?? [];
                _opportunities = (await CrmApi.GetOpportunitiesAsync(new OpportunityListQuery { CustomerId = Id, MaxResultCount = 50 }))?.Items ?? [];
                _quotations = (await CrmApi.GetQuotationsAsync(new QuotationListQuery { CustomerId = Id, MaxResultCount = 50 }))?.Items ?? [];
                _contracts = (await CrmApi.GetContractsAsync(new ContractListQuery { CustomerId = Id, MaxResultCount = 50 }))?.Items ?? [];
                _activities = (await CrmApi.GetActivitiesAsync(new ActivityListQuery
                {
                    RelatedEntityType = CrmRelatedEntityType.Customer,
                    RelatedEntityId = Id,
                    MaxResultCount = 20
                }))?.Items ?? [];
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            _loading = false;
        }
    }

    private void GoBack() => Navigation.NavigateTo("crm/customers");
    private void OpenOpportunity(Guid id) => Navigation.NavigateTo($"crm/opportunities/{id}");
    private void OpenQuotation(Guid id) => Navigation.NavigateTo($"crm/quotations/{id}");
    private void OpenContract(Guid id) => Navigation.NavigateTo($"crm/contracts/{id}");

    private Task ShowCreateContactAsync()
    {
        _contactModel = new QuickContactModel();
        return _contactModal!.Show();
    }

    private Task CloseContactModalAsync() => _contactModal!.Close();

    private async Task CreateContactAsync()
    {
        if (_customer is null || string.IsNullOrWhiteSpace(_contactModel.FullName))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập họ tên liên hệ."));
            return;
        }

        try
        {
            await CrmApi.CreateContactAsync(new CreateContactRequest(
                _customer.Id,
                _contactModel.FullName.Trim(),
                _contactModel.Email,
                _contactModel.Phone,
                _contactModel.Mobile,
                _contactModel.Position,
                _contactModel.Department,
                _contactModel.IsPrimary,
                _contactModel.IsDecisionMaker,
                null));
            await _contactModal!.Close();
            _contactModel = new QuickContactModel();
            await ToastService.Success("Thành công", "Đã tạo liên hệ.");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private Task ShowCreateOpportunityAsync()
    {
        _opportunityModel = new QuickOpportunityModel
        {
            Name = _customer is null ? "" : $"Cơ hội - {_customer.Name}",
            Amount = 0
        };
        return _opportunityModal!.Show();
    }

    private Task CloseOpportunityModalAsync() => _opportunityModal!.Close();

    private async Task CreateOpportunityAsync()
    {
        if (_customer is null || string.IsNullOrWhiteSpace(_opportunityModel.Name))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập tên cơ hội."));
            return;
        }

        try
        {
            await CrmApi.CreateOpportunityAsync(new CreateOpportunityRequest(
                _customer.Id,
                null,
                _opportunityModel.Name.Trim(),
                _opportunityModel.Amount,
                null,
                null));
            await _opportunityModal!.Close();
            _opportunityModel = new QuickOpportunityModel();
            await ToastService.Success("Thành công", "Đã tạo cơ hội.");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private Task ShowCreateQuotationAsync()
    {
        _quotationModel = new QuickQuotationModel
        {
            Subject = _customer is null ? "" : $"Báo giá - {_customer.Name}",
            Lines = [],
            PendingFiles = []
        };
        return _quotationModal!.Show();
    }

    private Task CloseQuotationModalAsync() => _quotationModal!.Close();

    private async Task CreateQuotationAsync()
    {
        if (_customer is null)
        {
            return;
        }

        var lines = BuildQuotationLines(_quotationModel.Lines);
        if (lines.Count == 0)
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng thêm ít nhất một dòng sản phẩm."));
            return;
        }

        if (_quotationModel.PendingFiles.Count == 0)
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng đính kèm tệp báo giá trước khi lưu."));
            return;
        }

        try
        {
            var quotation = await CrmApi.CreateQuotationAsync(new CreateQuotationRequest(
                _customer.Id,
                "AUTO",
                null,
                null,
                _quotationModel.Subject,
                null,
                lines));

            if (quotation is not null)
            {
                await FileApi.UploadAndLinkAsync(_quotationModel.PendingFiles, "CRM", "Quotation", quotation.Id.ToString(), DocumentFileCatalog.Quotation);
            }

            await _quotationModal!.Close();
            _quotationModel = new QuickQuotationModel();
            await ToastService.Success("Thành công", "Đã tạo báo giá.");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private Task ShowCreateContractAsync()
    {
        var openOpportunity = _opportunities.FirstOrDefault(x => x.Stage is not OpportunityStage.ClosedWon and not OpportunityStage.ClosedLost);
        _contractOpportunityIdText = openOpportunity?.Id.ToString() ?? "";
        _contractModel = new QuickContractModel
        {
            Title = openOpportunity is not null
                ? $"Hợp đồng - {openOpportunity.Name}"
                : _customer is null ? "" : $"Hợp đồng - {_customer.Name}",
            ContractValue = openOpportunity?.Amount ?? 0,
            Currency = "VND",
            StartDate = DateTime.Today,
            PendingFiles = []
        };
        return _contractModal!.Show();
    }

    private Task CloseContractModalAsync() => _contractModal!.Close();

    private async Task CreateContractAsync()
    {
        if (_customer is null || string.IsNullOrWhiteSpace(_contractModel.Title))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập tiêu đề hợp đồng."));
            return;
        }

        if (_contractModel.PendingFiles.Count == 0)
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng đính kèm tệp hợp đồng trước khi lưu."));
            return;
        }

        try
        {
            var opportunityId = Guid.TryParse(_contractOpportunityIdText, out var parsedOpportunityId)
                ? parsedOpportunityId
                : (Guid?)null;
            var contract = await CrmApi.CreateContractAsync(new CreateContractRequest(
                _customer.Id,
                "AUTO",
                _contractModel.Title.Trim(),
                null,
                opportunityId,
                null,
                _contractModel.ContractValue,
                string.IsNullOrWhiteSpace(_contractModel.Currency) ? "VND" : _contractModel.Currency.Trim(),
                ToDateOnly(_contractModel.StartDate),
                ToDateOnly(_contractModel.EndDate),
                null,
                _contractModel.PaymentTerms,
                null,
                _contractModel.Terms,
                null,
                null,
                []));

            if (contract is not null)
            {
                await MarkOpportunityWonAsync(opportunityId);
                await FileApi.UploadAndLinkAsync(_contractModel.PendingFiles, "CRM", "Contract", contract.Id.ToString(), DocumentFileCatalog.Contract);
            }

            await _contractModal!.Close();
            _contractModel = new QuickContractModel();
            _contractOpportunityIdText = "";
            await ToastService.Success("Thành công", "Đã tạo hợp đồng.");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private Task ShowCreateActivityAsync()
    {
        _activityTypeText = CrmActivityType.Task.ToString();
        _activityModel = new QuickActivityModel
        {
            Subject = _customer is null ? "" : $"Chăm sóc - {_customer.Name}",
            ActivityDate = DateTimeOffset.Now
        };
        return _activityModal!.Show();
    }

    private Task CloseActivityModalAsync() => _activityModal!.Close();

    private async Task CreateActivityAsync()
    {
        if (_customer is null || string.IsNullOrWhiteSpace(_activityModel.Subject))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập tiêu đề hoạt động."));
            return;
        }

        if (!Enum.TryParse<CrmActivityType>(_activityTypeText, out var activityType))
        {
            activityType = CrmActivityType.Task;
        }

        try
        {
            await CrmApi.CreateActivityAsync(new CreateActivityRequest(
                CrmRelatedEntityType.Customer,
                _customer.Id,
                activityType,
                _activityModel.Subject.Trim(),
                _activityModel.ActivityDate,
                null,
                null,
                null));
            await _activityModal!.Close();
            _activityModel = new QuickActivityModel();
            await ToastService.Success("Thành công", "Đã tạo hoạt động.");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private Task ShowEditModalAsync()
    {
        if (_customer is null)
        {
            return Task.CompletedTask;
        }

        _customerTypeText = _customer.CustomerType.ToString();
        _customerStatusText = _customer.Status.ToString();
        _editModel = new CustomerEditModel
        {
            Name = _customer.Name,
            Email = _customer.Email,
            Phone = _customer.Phone,
            TaxCode = _customer.TaxCode,
            Website = _customer.Website,
            Industry = _customer.Industry,
            City = _customer.City,
            Source = _customer.Source,
            AddressLine1 = _customer.AddressLine1,
            Description = _customer.Description
        };
        return _editModal!.Show();
    }

    private Task CloseEditModalAsync() => _editModal!.Close();

    private async Task SaveEditAsync()
    {
        if (_customer is null || string.IsNullOrWhiteSpace(_editModel.Name))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập tên khách hàng."));
            return;
        }

        if (!Enum.TryParse<CustomerStatus>(_customerStatusText, out var status))
        {
            status = _customer.Status;
        }

        if (!Enum.TryParse<CustomerType>(_customerTypeText, out var customerType))
        {
            customerType = _customer.CustomerType;
        }

        try
        {
            _customer = await CrmApi.UpdateCustomerAsync(_customer.Id, new UpdateCustomerRequest(
                _editModel.Name.Trim(),
                customerType,
                _editModel.Email,
                _editModel.Phone,
                _editModel.TaxCode,
                _editModel.Website,
                _editModel.Industry,
                _editModel.AddressLine1,
                _customer.AddressLine2,
                _editModel.City,
                _customer.State,
                _customer.PostalCode,
                _customer.Country,
                _customer.OwnerId,
                _editModel.Description,
                _customer.Rating,
                _editModel.Source,
                status));

            await _editModal!.Close();
            await ToastService.Success("Thành công", "Đã cập nhật khách hàng.");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private static IReadOnlyList<CreateQuotationLineRequest> BuildQuotationLines(IEnumerable<ProductLineInput> lines) =>
        lines
            .Where(x => !string.IsNullOrWhiteSpace(x.ProductCode) && !string.IsNullOrWhiteSpace(x.ProductName))
            .Select((x, i) => new CreateQuotationLineRequest(
                i + 1,
                x.ProductCode.Trim(),
                x.ProductName.Trim(),
                null,
                x.Quantity,
                string.IsNullOrWhiteSpace(x.Unit) ? "Cái" : x.Unit.Trim(),
                x.UnitPrice,
                0,
                0,
                i + 1))
            .ToList();

    private static DateOnly? ToDateOnly(DateTime? value) =>
        value.HasValue ? DateOnly.FromDateTime(value.Value) : null;

    private sealed class QuickContactModel
    {
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsDecisionMaker { get; set; }
    }

    private sealed class QuickOpportunityModel
    {
        public string Name { get; set; } = "";
        public decimal Amount { get; set; }
    }

    private sealed class QuickQuotationModel
    {
        public string? Subject { get; set; }
        public List<ProductLineInput> Lines { get; set; } = [];
        public List<PendingFileAttachment> PendingFiles { get; set; } = [];
    }

    private sealed class QuickContractModel
    {
        public string Title { get; set; } = "";
        public decimal ContractValue { get; set; }
        public string Currency { get; set; } = "VND";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PaymentTerms { get; set; }
        public string? Terms { get; set; }
        public List<PendingFileAttachment> PendingFiles { get; set; } = [];
    }

    private sealed class QuickActivityModel
    {
        public string Subject { get; set; } = "";
        public DateTimeOffset ActivityDate { get; set; } = DateTimeOffset.Now;
    }

    private sealed class CustomerEditModel
    {
        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? TaxCode { get; set; }
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public string? City { get; set; }
        public string? Source { get; set; }
        public string? AddressLine1 { get; set; }
        public string? Description { get; set; }
    }
}
