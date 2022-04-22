using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChain.Application.Abstractions;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// The application shell. Migrated from <c>frmMDIMain.frm</c>.
/// </summary>
[Authorize]
public class HomeController : Controller
{
    private readonly ITenantContext _tenant;

    /// <summary>Initialises a new instance.</summary>
    public HomeController(ITenantContext tenant) => _tenant = tenant;

    /// <summary>The dashboard shown after sign-in and company selection.</summary>
    public IActionResult Index()
    {
        // Every screen below the shell is company-scoped, so a session without a company has
        // nowhere useful to go.
        if (_tenant.CompanyId is null)
        {
            return RedirectToAction("Index", "Company");
        }

        return View();
    }

    /// <summary>Renders the error page.</summary>
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
