using System.ComponentModel.DataAnnotations;

namespace ItBudgetingV4.Web.ViewModels;

public class LoginViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
