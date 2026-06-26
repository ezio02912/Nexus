using BootstrapBlazor.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmContacts
{
    private Table<ContactDto>? _table;
    private Modal? _editModal;
    private bool _isCreate;
    private string _modalTitle = "";
    private ContactDto? _editing;
    private ContactFormModel _model = new();
    private string _customerIdText = "";
    private List<SelectedItem> _customerOptions = [];
    private Dictionary<Guid, string> _customerNames = [];

    protected override async Task OnInitializedAsync() => await LoadCustomersAsync();

    private async Task LoadCustomersAsync()
    {
        try
        {
            var result = await CrmApi.GetCustomersAsync(new CustomerListQuery { MaxResultCount = 500 });
            _customerNames = (result?.Items ?? []).ToDictionary(x => x.Id, x => $"{x.Code} - {x.Name}");
            _customerOptions = _customerNames.Select(x => new SelectedItem(x.Key.ToString(), x.Value)).ToList();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private string CustomerName(Guid customerId) =>
        _customerNames.TryGetValue(customerId, out var name) ? name : customerId.ToString();

    private async Task<QueryData<ContactDto>> OnQueryAsync(QueryPageOptions options)
    {
        try
        {
            var result = await CrmApi.GetContactsAsync(new ContactListQuery
            {
                Search = options.SearchText,
                SkipCount = (options.PageIndex - 1) * options.PageItems,
                MaxResultCount = options.PageItems,
                Sorting = options.SortName
            });
            return ToQueryData(result, options);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
            return EmptyQuery<ContactDto>();
        }
    }

    private Task ShowCreateAsync(IEnumerable<ContactDto> _)
    {
        _isCreate = true;
        _modalTitle = "Thêm liên hệ";
        _editing = null;
        _model = new ContactFormModel { IsPrimary = false, IsDecisionMaker = false };
        _customerIdText = _customerOptions.FirstOrDefault()?.Value ?? "";
        return _editModal!.Show();
    }

    private Task ShowEditAsync(ContactDto contact)
    {
        _isCreate = false;
        _modalTitle = "Sửa liên hệ";
        _editing = contact;
        _customerIdText = contact.CustomerId.ToString();
        _model = new ContactFormModel
        {
            FullName = contact.FullName,
            Email = contact.Email,
            Phone = contact.Phone,
            Mobile = contact.Mobile,
            Position = contact.Position,
            Department = contact.Department,
            IsPrimary = contact.IsPrimary,
            IsDecisionMaker = contact.IsDecisionMaker
        };
        return _editModal!.Show();
    }

    private Task CloseModalAsync() => _editModal!.Close();

    private async Task SaveAsync()
    {
        if (!Guid.TryParse(_customerIdText, out var customerId) || string.IsNullOrWhiteSpace(_model.FullName))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng chọn khách hàng và nhập họ tên."));
            return;
        }

        try
        {
            if (_isCreate)
            {
                await CrmApi.CreateContactAsync(new CreateContactRequest(
                    customerId, _model.FullName.Trim(), _model.Email, _model.Phone, _model.Mobile,
                    _model.Position, _model.Department, _model.IsPrimary, _model.IsDecisionMaker, null));
                await ToastService.Success("Thành công", "Đã tạo liên hệ.");
            }
            else if (_editing is not null)
            {
                await CrmApi.UpdateContactAsync(_editing.Id, new UpdateContactRequest(
                    customerId, _model.FullName.Trim(), _model.Email, _model.Phone, _model.Mobile,
                    _model.Position, _model.Department, _model.IsPrimary, _model.IsDecisionMaker,
                    _editing.LinkedInUrl, _editing.Notes, null));
                await ToastService.Success("Thành công", "Đã cập nhật liên hệ.");
            }

            await _editModal!.Close();
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private async Task DeleteAsync(Guid id)
    {
        if (!await ConfirmDeleteAsync())
        {
            return;
        }

        try
        {
            await CrmApi.DeleteContactAsync(id);
            await ToastService.Success("Đã xoá", "Liên hệ đã được xoá.");
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, "Xoá thất bại");
        }
    }

    private async Task ReloadTableAsync()
    {
        if (_table is not null)
        {
            await _table.QueryAsync();
        }
    }

    private sealed class ContactFormModel
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
}
