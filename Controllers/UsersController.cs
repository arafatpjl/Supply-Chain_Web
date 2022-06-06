using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// The permission-editing actions are named Permissions, which would shadow the catalogue class.
// An alias keeps both the readable action name and the readable policy references.
using Perm = SupplyChain.Application.Authorization.Permissions;
using SupplyChain.Application.Security;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// User administration: accounts, roles, permissions and company access.
/// </summary>
/// <remarks>
/// Replaces <c>frmCreateUser.frm</c>, <c>frmPermissionMenu.frm</c> and
/// <c>frmPermissionCom.frm</c>. Every action carries its own policy: in the legacy application
/// these screens were reachable through menu items that <c>prcMenuInvisibleAll</c> left visible
/// to non-administrators.
/// </remarks>
[Authorize]
public class UsersController : Controller
{
    private readonly IUserAdminService _users;

    /// <summary>Initialises a new instance.</summary>
    public UsersController(IUserAdminService users) => _users = users;

    /// <summary>Lists user accounts.</summary>
    [HttpGet]
    [Authorize(Policy = Perm.Users.View)]
    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken cancellationToken = default)
    {
        return View(new UserIndexViewModel
        {
            Results = await _users.SearchAsync(search, page, 25, cancellationToken),
            SearchTerm = search,
        });
    }

    /// <summary>Shows the create-user form.</summary>
    [HttpGet]
    [Authorize(Policy = Perm.Users.Manage)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return View(new CreateUserViewModel
        {
            AvailableRoles = await _users.ListRolesAsync(cancellationToken),
        });
    }

    /// <summary>Creates a user account.</summary>
    [HttpPost]
    [Authorize(Policy = Perm.Users.Manage)]
    public async Task<IActionResult> Create(CreateUserViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = await _users.ListRolesAsync(cancellationToken);
            return View(model);
        }

        var result = await _users.CreateAsync(
            new CreateUserDto(model.UserName, model.FullName, model.Password, model.ConfirmPassword, model.SelectedRoles),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(nameof(model.UserName), result.Error!);
            model.AvailableRoles = await _users.ListRolesAsync(cancellationToken);
            return View(model);
        }

        TempData["Success"] = "Successfully Saved";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Shows the edit-user form.</summary>
    [HttpGet]
    [Authorize(Policy = Perm.Users.Manage)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await _users.GetAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View(new EditUserViewModel
        {
            Id = result.Value.Id,
            UserName = result.Value.UserName,
            FullName = result.Value.FullName,
            IsActive = result.Value.IsActive,
            SelectedRoles = result.Value.Roles.ToList(),
            AvailableRoles = await _users.ListRolesAsync(cancellationToken),
        });
    }

    /// <summary>Updates a user account.</summary>
    [HttpPost]
    [Authorize(Policy = Perm.Users.Manage)]
    public async Task<IActionResult> Edit(int id, EditUserViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            model.AvailableRoles = await _users.ListRolesAsync(cancellationToken);
            return View(model);
        }

        var result = await _users.UpdateAsync(
            id, new UpdateUserDto(model.FullName, model.IsActive, model.SelectedRoles), cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.AvailableRoles = await _users.ListRolesAsync(cancellationToken);
            return View(model);
        }

        TempData["Success"] = "Successfully Updated";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Shows the password reset form.</summary>
    [HttpGet]
    [Authorize(Policy = Perm.Users.Manage)]
    public async Task<IActionResult> ResetPassword(int id, CancellationToken cancellationToken)
    {
        var result = await _users.GetAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View(new ResetPasswordViewModel { Id = id, UserName = result.Value.UserName });
    }

    /// <summary>Resets a user's password.</summary>
    [HttpPost]
    [Authorize(Policy = Perm.Users.Manage)]
    public async Task<IActionResult> ResetPassword(
        int id,
        ResetPasswordViewModel model,
        CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _users.ResetPasswordAsync(
            id, new ResetPasswordDto(model.Password, model.ConfirmPassword), cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(nameof(model.Password), result.Error!);
            return View(model);
        }

        TempData["Success"] = "New Password Set Successfully..";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Shows the permission assignment screen.</summary>
    [HttpGet]
    [Authorize(Policy = Perm.Users.ManagePermissions)]
    public async Task<IActionResult> Permissions(int id, CancellationToken cancellationToken)
    {
        var result = await _users.GetPermissionsAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View(Build(result.Value));
    }

    /// <summary>Replaces the permissions granted directly to a user.</summary>
    [HttpPost]
    [Authorize(Policy = Perm.Users.ManagePermissions)]
    public async Task<IActionResult> Permissions(
        int id,
        UserPermissionsViewModel model,
        CancellationToken cancellationToken)
    {
        var result = await _users.SetPermissionsAsync(id, model.SelectedPermissions, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Update successfully.";
        }

        return RedirectToAction(nameof(Permissions), new { id });
    }

    /// <summary>Shows the company access screen.</summary>
    [HttpGet]
    [Authorize(Policy = Perm.Users.ManageCompanyAccess)]
    public async Task<IActionResult> CompanyAccess(int id, CancellationToken cancellationToken)
    {
        var result = await _users.GetCompanyAccessAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View(new UserCompanyAccessViewModel
        {
            Id = result.Value.UserId,
            UserName = result.Value.UserName,
            Companies = result.Value.Companies,
            SelectedCompanyIds = result.Value.Companies.Where(c => c.IsGranted).Select(c => c.CompanyId).ToList(),
            DefaultCompanyId = result.Value.Companies.FirstOrDefault(c => c.IsDefault)?.CompanyId,
        });
    }

    /// <summary>Replaces the set of companies a user may access.</summary>
    [HttpPost]
    [Authorize(Policy = Perm.Users.ManageCompanyAccess)]
    public async Task<IActionResult> CompanyAccess(
        int id,
        UserCompanyAccessViewModel model,
        CancellationToken cancellationToken)
    {
        var result = await _users.SetCompanyAccessAsync(
            id, model.SelectedCompanyIds, model.DefaultCompanyId, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Update successfully.";
        }

        return RedirectToAction(nameof(CompanyAccess), new { id });
    }

    private static UserPermissionsViewModel Build(UserPermissionsDto dto) => new()
    {
        Id = dto.UserId,
        UserName = dto.UserName,
        Grants = dto.Grants.GroupBy(g => g.Module).OrderBy(g => g.Key).ToList(),
        SelectedPermissions = dto.Grants.Where(g => g.IsGranted).Select(g => g.Name).ToList(),
    };
}
