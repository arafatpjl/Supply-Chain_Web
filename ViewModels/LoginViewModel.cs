using System.ComponentModel.DataAnnotations;

namespace SupplyChain.Web.ViewModels;

/// <summary>Sign-in form. Migrated from <c>frmUserLogIn.frm</c>.</summary>
public class LoginViewModel
{
    /// <summary>The user name.</summary>
    [Required(ErrorMessage = "Type User Name")]
    [Display(Name = "User Name")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>The password.</summary>
    [Required(ErrorMessage = "Type Password")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Whether to issue a persistent authentication cookie.</summary>
    [Display(Name = "Keep me signed in")]
    public bool RememberMe { get; set; }

    /// <summary>Where to send the user after a successful sign-in.</summary>
    public string? ReturnUrl { get; set; }
}
