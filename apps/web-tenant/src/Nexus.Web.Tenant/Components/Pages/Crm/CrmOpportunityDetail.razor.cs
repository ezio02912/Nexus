using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmOpportunityDetail
{
    [Parameter] public Guid Id { get; set; }

    private bool _loading;
    private OpportunityDto? _item;
    private CustomerDto? _customer;
    private LeadDto? _lead;
    private IReadOnlyList<QuotationDto> _quotations = [];
    private IReadOnlyList<ContractDto> _contracts = [];
    private IReadOnlyList<ActivityDto> _activities = [];
    private Modal? _quotationModal;
    private QuickQuotationModel _quotationModel = new();

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

    private Task ShowCreateQuotationAsync()
    {
        if (_item is null)
        {
            return Task.CompletedTask;
        }

        _quotationModel = new QuickQuotationModel
        {
            Subject = $"Báo giá - {_item.Name}",
            ProductCode = "SP001",
            ProductName = _item.Name,
            Quantity = 1,
            UnitPrice = _item.Amount
        };
        return _quotationModal!.Show();
    }

    private Task CloseQuotationModalAsync() => _quotationModal!.Close();

    private async Task CreateQuotationAsync()
    {
        if (_item?.CustomerId is not Guid customerId || string.IsNullOrWhiteSpace(_quotationModel.ProductCode) || string.IsNullOrWhiteSpace(_quotationModel.ProductName))
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
                [
                    new CreateQuotationLineRequest(
                        1,
                        _quotationModel.ProductCode.Trim(),
                        _quotationModel.ProductName.Trim(),
                        null,
                        _quotationModel.Quantity,
                        "EA",
                        _quotationModel.UnitPrice,
                        0,
                        0,
                        1)
                ]));

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

    private sealed class QuickQuotationModel
    {
        public string? Subject { get; set; }
        public string ProductCode { get; set; } = "SP001";
        public string ProductName { get; set; } = "";
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
    }
}
