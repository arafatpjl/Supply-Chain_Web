using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplyChain.Application.Abstractions;
using SupplyChain.Application.Security;
using SupplyChain.Infrastructure.Identity;
using SupplyChain.Infrastructure.Persistence;
using SupplyChain.Web.Services;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// Sign-in and sign-out. Migrated from <c>frmUserLogIn.frm</c>.
/// </summary>
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SupplyChainDbContext _context;
    private readonly IDateTimeProvider _clock;
    private readonly IUserAdminService _userAdmin;
    private readonly ILogger<AccountController> _logger;

    /// <summary>Initialises a new instance.</summary>
    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        SupplyChainDbContext context,
        IDateTimeProvider clock,
        IUserAdminService userAdmin,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
        _clock = clock;
        _userAdmin = userAdmin;
        _logger = logger;
    }

    /// <summary>Shows the sign-in form.</summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    /// <summary>Attempts to sign the user in.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.UserName);

        // A single message for "no such user" and "wrong password" so the form cannot be used to
        // discover valid user names. The legacy application distinguished the two cases.
        if (user is null || !user.IsActive)
        {
            _logger.LogWarning("Failed sign-in for {UserName}: unknown or inactive account", model.UserName);
            ModelState.AddModelError(string.Empty, "Invalid user name or password.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Account {UserName} is locked out", model.UserName);
            ModelState.AddModelError(string.Empty,
                "This account is temporarily locked after too many failed attempts. Try again later.");
            return View(model);
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed sign-in for {UserName}: incorrect password", model.UserName);
            ModelState.AddModelError(string.Empty, "Invalid user name or password.");
            return View(model);
        }

        user.LastLoginAt = _clock.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserName} signed in", user.UserName);

        // Where the user has access to exactly one company, select it automatically rather than
        // showing a picker with a single option — matching how frmSplash behaved.
        var companies = await _context.UserCompanyAccess
            .Where(a => a.UserId == user.Id)
            .Include(a => a.Company)
            .Where(a => a.Company.IsActive)
            .ToListAsync(cancellationToken);

        if (companies.Count == 1)
        {
            return RedirectToAction(
                nameof(CompanyController.Select),
                "Company",
                new { id = companies[0].CompanyId, returnUrl = model.ReturnUrl });
        }

        return RedirectToAction(nameof(CompanyController.Index), "Company", new { returnUrl = model.ReturnUrl });
    }

    /// <summary>Signs the user out.</summary>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userName = User.Identity?.Name;
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User {UserName} signed out", userName);
        return RedirectToAction(nameof(Login));
    }

    /// <summary>Shown when a signed-in user lacks the permission an action requires.</summary>
    [HttpGet]
    [Authorize]
    public IActionResult AccessDenied() => View();

    /// <summary>Shows the change-password form.</summary>
    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    /// <summary>
    /// Changes the signed-in user's own password.
    /// </summary>
    /// <remarks>Replaces the password flow in <c>frmCreateModifyUser.frm</c>.</remarks>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _userAdmin.ChangeOwnPasswordAsync(
            user.Id,
            new ChangePasswordDto(model.CurrentPassword, model.NewPassword, model.ConfirmPassword),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), result.Error!);
            return View(model);
        }

        TempData["Success"] = "New Password Set Successfully..";
        return RedirectToAction("Index", "Home");
    }
}
