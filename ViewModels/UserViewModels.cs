using System.ComponentModel.DataAnnotations;
using SupplyChain.Application.Common;
using SupplyChain.Application.Security;

namespace SupplyChain.Web.ViewModels;

/// <summary>User administration list. Replaces <c>frmCreateUser.frm</c>'s browse mode.</summary>
public class UserIndexViewModel
{
    /// <summary>The page of users to display.</summary>
    public PagedResult<UserDto> Results { get; set; } = PagedResult<UserDto>.Empty();

    /// <summary>The active search term, if any.</summary>
    public string? SearchTerm { get; set; }
}

/// <summary>Create-user form.</summary>
public class CreateUserViewModel
{
    /// <summary>Sign-in name.</summary>
    [Required(ErrorMessage = "Enter User Name")]
    [StringLength(50)]
    [Display(Name = "User Name")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>Display name.</summary>
    [Required(ErrorMessage = "Enter Full Name")]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Initial password.</summary>
    [Required(ErrorMessage = "Enter User Password")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Repeat of the password. The legacy form called this "Verify Password".</summary>
    [Required(ErrorMessage = "Enter Verify Password")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "User password does not match with Verify Password")]
    [Display(Name = "Verify Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Roles to assign.</summary>
    public List<string> SelectedRoles { get; set; } = [];

    /// <summary>Roles available for assignment.</summary>
    public IReadOnlyList<string> AvailableRoles { get; set; } = Array.Empty<string>();
}

/// <summary>Edit-user form.</summary>
public class EditUserViewModel
{
    /// <summary>Identity user identifier.</summary>
    public int Id { get; set; }

    /// <summary>Sign-in name. Shown but not editable — it is the account's identity.</summary>
    [Display(Name = "User Name")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>Display name.</summary>
    [Required(ErrorMessage = "Enter Full Name")]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Whether the account may sign in. Legacy column: <c>UserStatus</c>.</summary>
    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    /// <summary>Roles to assign.</summary>
    public List<string> SelectedRoles { get; set; } = [];

    /// <summary>Roles available for assignment.</summary>
    public IReadOnlyList<string> AvailableRoles { get; set; } = Array.Empty<string>();
}

/// <summary>Administrator-initiated password reset.</summary>
public class ResetPasswordViewModel
{
    /// <summary>Identity user identifier.</summary>
    public int Id { get; set; }

    /// <summary>Sign-in name, for display.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>The new password.</summary>
    [Required(ErrorMessage = "Type new password..")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Repeat of the new password.</summary>
    [Required(ErrorMessage = "Type Verify password..")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "New Password does not match with Verify Password")]
    [Display(Name = "Verify Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>A user changing their own password. Replaces <c>frmCreateModifyUser.frm</c>.</summary>
public class ChangePasswordViewModel
{
    /// <summary>The existing password. The legacy form called this "Old Password".</summary>
    [Required(ErrorMessage = "Type Old Password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Old Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>The replacement password.</summary>
    [Required(ErrorMessage = "Type new password..")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>Repeat of the replacement password.</summary>
    [Required(ErrorMessage = "Type Verify password..")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "New Password does not match with Verify Password")]
    [Display(Name = "Verify Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>Permission assignment screen. Replaces <c>frmPermissionMenu.frm</c>.</summary>
public class UserPermissionsViewModel
{
    /// <summary>Identity user identifier.</summary>
    public int Id { get; set; }

    /// <summary>Sign-in name, for display.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Permissions granted directly to the user, as posted by the form.</summary>
    public List<string> SelectedPermissions { get; set; } = [];

    /// <summary>Every catalogue permission with the user's current state, grouped by module.</summary>
    public IReadOnlyList<IGrouping<string, PermissionGrantDto>> Grants { get; set; } =
        Array.Empty<IGrouping<string, PermissionGrantDto>>();
}

/// <summary>Company access screen. Replaces <c>frmPermissionCom.frm</c>.</summary>
public class UserCompanyAccessViewModel
{
    /// <summary>Identity user identifier.</summary>
    public int Id { get; set; }

    /// <summary>Sign-in name, for display.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Companies granted, as posted by the form.</summary>
    public List<int> SelectedCompanyIds { get; set; } = [];

    /// <summary>Which granted company is selected on sign-in.</summary>
    public int? DefaultCompanyId { get; set; }

    /// <summary>Every company with the user's current state.</summary>
    public IReadOnlyList<CompanyAccessDto> Companies { get; set; } = Array.Empty<CompanyAccessDto>();
}
