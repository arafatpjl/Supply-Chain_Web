using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplyChain.Application.Abstractions;
using SupplyChain.Application.Companies;
using SupplyChain.Infrastructure.Identity;
using SupplyChain.Infrastructure.Persistence;
using SupplyChain.Web.Services;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// Company selection and switching. Migrated from the company picker on <c>frmSplash.frm</c>.
/// </summary>
/// <remarks>
/// Selecting a company is the point at which tenant access is verified. Everything downstream
/// trusts the resulting claim, so the check here is the one that matters.
/// </remarks>
[Authorize]
public class CompanyController : Controller
{
    private readonly SupplyChainDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantContext _tenant;
    private readonly ILogger<CompanyController> _logger;

    /// <summary>Initialises a new instance.</summary>
    public CompanyController(
        SupplyChainDbContext context,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ITenantContext tenant,
        ILogger<CompanyController> logger)
    {
        _context = context;
        _signInManager = signInManager;
        _userManager = userManager;
        _tenant = tenant;
        _logger = logger;
    }

    /// <summary>Lists the companies the signed-in user may work in.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var companies = await LoadAccessibleCompaniesAsync(cancellationToken);

        return View(new CompanySelectViewModel
        {
            Companies = companies,
            CurrentCompanyId = _tenant.CompanyId,
            ReturnUrl = returnUrl,
        });
    }

    /// <summary>
    /// Makes the given company the active one for this session.
    /// </summary>
    /// <remarks>
    /// The access check below is the security boundary. It queries <c>UserCompanyAccess</c> for
    /// <em>this</em> user and <em>this</em> company; a company the user has not been granted
    /// cannot be selected regardless of what was posted. Only after it passes is the company
    /// written into the authentication cookie, where <see cref="TenantContext"/> reads it.
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> Select(
        int id,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var access = await _context.UserCompanyAccess
            .Include(a => a.Company)
            .FirstOrDefaultAsync(a => a.UserId == user.Id && a.CompanyId == id, cancellationToken);

        if (access is null || !access.Company.IsActive)
        {
            _logger.LogWarning(
                "User {UserName} attempted to select company {CompanyId} without access",
                user.UserName, id);
            return Forbid();
        }

        // Re-issuing the principal refreshes the cookie with the new company claims.
        await _signInManager.SignInWithClaimsAsync(user, isPersistent: false, new[]
        {
            new Claim(TenantContext.CompanyIdClaim, access.CompanyId.ToString()),
            new Claim(TenantContext.CompanyShortCodeClaim, access.Company.ShortCode),
            new Claim(TenantContext.CompanyNameClaim, access.Company.Name),
        });

        _logger.LogInformation(
            "User {UserName} switched to company {CompanyShortCode}",
            user.UserName, access.Company.ShortCode);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    private async Task<IReadOnlyList<CompanyDto>> LoadAccessibleCompaniesAsync(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Array.Empty<CompanyDto>();
        }

        return await _context.UserCompanyAccess
            .Where(a => a.UserId == user.Id && a.Company.IsActive)
            .OrderBy(a => a.Company.Name)
            .Select(a => new CompanyDto(
                a.Company.Id,
                a.Company.Name,
                a.Company.ShortCode,
                a.Company.Address,
                a.Company.LegacyDatabaseName,
                a.Company.IsActive))
            .ToListAsync(cancellationToken);
    }
}
