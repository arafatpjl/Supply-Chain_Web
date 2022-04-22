using Microsoft.AspNetCore.Mvc;
using SupplyChain.Application.Abstractions;
using SupplyChain.Application.Authorization;

namespace SupplyChain.Web.ViewComponents;

/// <summary>
/// Renders the main navigation, showing only what the current user may reach.
/// </summary>
/// <remarks>
/// <para>
/// This is the successor to <c>modPermission.bas</c>, which walked the MDI menu tree setting
/// <c>.Visible = False</c> on items the user lacked rights to.
/// </para>
/// <para>
/// The essential difference is that this is now <em>only</em> a convenience. Each target action
/// carries its own authorisation policy, so hiding an item shapes the interface rather than
/// enforcing the rule. In the legacy application, hiding it <em>was</em> the rule.
/// </para>
/// </remarks>
public class NavigationViewComponent : ViewComponent
{
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenant;

    /// <summary>Initialises a new instance.</summary>
    public NavigationViewComponent(ICurrentUser currentUser, ITenantContext tenant)
    {
        _currentUser = currentUser;
        _tenant = tenant;
    }

    /// <summary>Builds the navigation model for the current user.</summary>
    public IViewComponentResult Invoke()
    {
        var model = new NavigationViewModel
        {
            CompanyName = _tenant.CompanyName,
            CompanyShortCode = _tenant.CompanyShortCode,
            UserName = _currentUser.UserName,
            CanViewCompanies = _currentUser.HasPermission(Permissions.Companies.View),
            CanViewMasterData = _currentUser.HasPermission(Permissions.MasterData.View),
            CanViewUsers = _currentUser.HasPermission(Permissions.Users.View),
            CanManageUsers = _currentUser.HasPermission(Permissions.Users.Manage),
            CanViewReports = _currentUser.HasPermission(Permissions.Reports.View),
        };

        return View(model);
    }
}

/// <summary>Data required to render the navigation bar.</summary>
public class NavigationViewModel
{
    /// <summary>Active company name.</summary>
    public string? CompanyName { get; set; }

    /// <summary>Active company short code.</summary>
    public string? CompanyShortCode { get; set; }

    /// <summary>Signed-in user name.</summary>
    public string? UserName { get; set; }

    /// <summary>Whether to show the company administration link.</summary>
    public bool CanViewCompanies { get; set; }

    /// <summary>Whether to show the House Keeping (master data) menu.</summary>
    public bool CanViewMasterData { get; set; }

    /// <summary>Whether to show user administration.</summary>
    public bool CanViewUsers { get; set; }

    /// <summary>Whether to show the user administration link.</summary>
    public bool CanManageUsers { get; set; }

    /// <summary>Whether to show the reports menu.</summary>
    public bool CanViewReports { get; set; }
}
