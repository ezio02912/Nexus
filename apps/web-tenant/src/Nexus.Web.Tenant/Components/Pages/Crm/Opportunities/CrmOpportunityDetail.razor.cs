using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm.Opportunities;

public partial class CrmOpportunityDetail
{
    [Inject] private FileApiClient FileApi { get; set; } = default!;

    [Parameter] public Guid Id { get; set; }

    private bool _loading;
    private OpportunityDto? _item;
    private CustomerDto? _customer;
    private LeadDto? _lead;
    private IReadOnlyList<QuotationDto> _quotations = [];
    private IReadOnlyList<ContractDto> _contracts = [];
    private IReadOnlyList<ActivityDto> _activities = [];
    private Modal? _editModal;
    private OpportunityEditModel _editModel = new();
    private string _editCustomerIdText = "";
    private string _editStageText = OpportunityStage.Prospecting.ToString();
    private List<SelectedItem> _stageOptions = CrmLabels.OpportunityStageOptions();
    private Dictionary<string, (string Text, string BadgeClass)> _stageMeta = CrmLabels.OpportunityStageMeta();
    private Modal? _quotationModal;
    private Modal? _contractModal;
    private QuickQuotationModel _quotationModel = new();
    private QuickContractModel _contractModel = new();

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _item = await CrmApi.GetOpportunityAsync(Id);
            _customer = null;
            _lead = null;
            _quotations = [];
            _contracts = [];
            _activities = [];
            if (_item is not null)
            {
                if (_item.CustomerId.HasValue)
                {
                    _customer = await CrmApi.GetCustomerAsync(_item.CustomerId.Value);
                }

                if (_item.LeadId.HasValue)
                {
                    _lead = await CrmApi.GetLeadAsync(_item.LeadId.Value);
                }

                _quotations = (await CrmApi.GetQuotationsAsync(new QuotationListQuery
                {
                    CustomerId = _item.CustomerId,
                    MaxResultCount = 50
                }))?.Items.Where(x => x.OpportunityId == Id).ToArray() ?? [];

                _contracts = (await CrmApi.GetContractsAsync(new ContractListQuery
                {
                    CustomerId = _item.CustomerId,
                    MaxResultCount = 50
                }))?.Items.Where(x => x.OpportunityId == Id).ToArray() ?? [];

                _activities = (await CrmApi.GetActivitiesAsync(new ActivityListQuery
                {
                    RelatedEntityType = CrmRelatedEntityType.Opportunity,
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

    private void GoBack() => Navigation.NavigateTo("crm/opportunities");
    private void OpenCustomer(Guid id) => Navigation.NavigateTo($"crm/customers/{id}");
    private void OpenLead(Guid id) => Navigation.NavigateTo($"crm/leads/{id}");
    private void OpenQuotation(Guid id) => Navigation.NavigateTo($"crm/quotations/{id}");
    private void OpenContract(Guid id) => Navigation.NavigateTo($"crm/contracts/{id}");

    private Task ShowEditModalAsync()
    {
        if (_item is null)
        {
            return Task.CompletedTask;
        }

        _editCustomerIdText = _item.CustomerId?.ToString() ?? "";
        _editStageText = _item.Stage.ToString();
        _editModel = new OpportunityEditModel
        {
            Name = _item.Name,
            Amount = _item.Amount,
            Probability = _item.Probability,
            Currency = _item.Currency,
            Description = _item.Description,
            NextStep = _item.NextStep,
            Source = _item.Source,
            Competitor = _item.Competitor
        };
        return _editModal!.Show();
    }

    private Task CloseEditModalAsync() => _editModal!.Close();

    private async Task SaveEditAsync()
    {
        if (_item is null || string.IsNullOrWhiteSpace(_editModel.Name))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập tên cơ hội."));
            return;
        }

        Guid? customerId = Guid.TryParse(_editCustomerIdText, out var parsedCustomerId) ? parsedCustomerId : null;

        try
        {
            _item = await CrmApi.UpdateOpportunityAsync(_item.Id, new UpdateOpportunityRequest(
                customerId,
                _item.ContactId,
                _editModel.Name.Trim(),
                _editModel.Amount,
                _editModel.Probability,
                string.IsNullOrWhiteSpace(_editModel.Currency) ? _item.Currency : _editModel.Currency.Trim(),
                _item.ExpectedCloseDate,
                _editModel.Description,
                _editModel.NextStep,
                _item.NextStepDate,
                _editModel.Source,
                _editModel.Competitor,
                _item.OwnerId));

            if (Enum.TryParse<OpportunityStage>(_editStageText, out var stage) && stage != _item.Stage)
            {
                _item = await CrmApi.ChangeOpportunityStageAsync(_item.Id, new ChangeOpportunityStageRequest(stage, _editModel.Probability, null, null));
            }

            await _editModal!.Close();
            await ToastService.Success("Thành công", "Đã cập nhật cơ hội.");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private Task ShowCreateQuotationAsync()
    {
        if (_item is null)
        {
            return Task.CompletedTask;
        }

        _quotationModel = new QuickQuotationModel
        {
            Subject = $"Báo giá - {_item.Name}",
            Lines = []
        };
        return _quotationModal!.Show();
    }

    private Task CloseQuotationModalAsync() => _quotationModal!.Close();

    private async Task CreateQuotationAsync()
    {
        var lines = _quotationModel.Lines
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
            .ToArray();

        if (_item?.CustomerId is not Guid customerId || lines.Length == 0)
        {
            await ShowErrorAsync(new InvalidOperationException("Cơ hội cần có khách hàng và dòng hàng hợp lệ để tạo báo giá."));
            return;
        }

        try
        {
            var quotation = await CrmApi.CreateQuotationAsync(new CreateQuotationRequest(
                customerId,
                "AUTO",
                _item.Id,
                _item.ContactId,
                _quotationModel.Subject,
                null,
                lines));

            await _quotationModal!.Close();
            await ToastService.Success("Thành công", "Đã tạo báo giá từ cơ hội.");
            if (quotation is not null)
            {
                Navigation.NavigateTo($"crm/quotations/{quotation.Id}");
            }
            else
            {
                await LoadAsync();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private Task ShowCreateContractAsync()
    {
        if (_item is null)
        {
            return Task.CompletedTask;
        }

        _contractModel = new QuickContractModel
        {
            Title = $"Hợp đồng - {_item.Name}",
            ContractValue = _item.Amount,
            Currency = string.IsNullOrWhiteSpace(_item.Currency) ? "VND" : _item.Currency,
            StartDate = DateTime.Today,
            PendingFiles = []
        };
        return _contractModal!.Show();
    }

    private Task CloseContractModalAsync() => _contractModal!.Close();

    private async Task CreateContractAsync()
    {
        if (_item?.CustomerId is not Guid customerId || string.IsNullOrWhiteSpace(_contractModel.Title))
        {
            await ShowErrorAsync(new InvalidOperationException("Cơ hội cần có khách hàng và tiêu đề hợp đồng."));
            return;
        }

        if (_contractModel.PendingFiles.Count == 0)
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng đính kèm tệp hợp đồng trước khi lưu."));
            return;
        }

        try
        {
            var contract = await CrmApi.CreateContractAsync(new CreateContractRequest(
                customerId,
                "AUTO",
                _contractModel.Title.Trim(),
                null,
                _item.Id,
                _item.ContactId,
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
                await MarkOpportunityWonAsync(_item.Id);
                await FileApi.UploadAndLinkAsync(_contractModel.PendingFiles, "CRM", "Contract", contract.Id.ToString(), DocumentFileCatalog.Contract);
            }

            await _contractModal!.Close();
            await ToastService.Success("Thành công", "Đã tạo hợp đồng và đánh dấu cơ hội thắng 100%.");
            if (contract is not null)
            {
                Navigation.NavigateTo($"crm/contracts/{contract.Id}");
            }
            else
            {
                await LoadAsync();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private sealed class QuickQuotationModel
    {
        public string? Subject { get; set; }
        public List<ProductLineInput> Lines { get; set; } = [];
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

    private static DateOnly? ToDateOnly(DateTime? value) =>
        value.HasValue ? DateOnly.FromDateTime(value.Value) : null;

    private sealed class OpportunityEditModel
    {
        public string Name { get; set; } = "";
        public decimal Amount { get; set; }
        public int Probability { get; set; } = 10;
        public string Currency { get; set; } = "VND";
        public string? Description { get; set; }
        public string? NextStep { get; set; }
        public string? Source { get; set; }
        public string? Competitor { get; set; }
    }
}
