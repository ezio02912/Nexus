using System.ComponentModel.DataAnnotations;

namespace Nexus.Web.Tenant.Components.Pages;

public sealed class OnboardingFormState
{
    [Required(ErrorMessage = "Vui lòng nhập tên công ty.")]
    public string CompanyName { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
    public string Address { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập tên người đại diện.")]
    public string RepresentativeName { get; set; } = "";

    public string TenantCode { get; set; } = "";

    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public int Step { get; set; } = 1;
}
