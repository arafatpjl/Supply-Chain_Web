using System.Security.Claims;
using SupplyChain.Application.Abstractions;
using SupplyChain.Application.Authorization;

namespace SupplyChain.Web.Services;

/// <inheritdoc cref="ITenantContext" />
/// <remarks>
/// <para>
/// The active company is carried in the authentication cookie as a claim. The cookie is signed
/// and encrypted by the data protection stack, so a user cannot edit it to reach a company they
/// have not been granted. Access is verified against <c>UserCompanyAccess</c> at the moment the
/// company is selected, in <c>CompanyController.Select</c>, and the resulting claim is the
/// evidence of that check.
/// </para>
/// <para>
/// This replaces the legacy global <c>WDBName</c>, which held the name of whichever of the
/// eighteen databases the user picked at login and was read directly by every form.
/// </para>
/// </remarks>
public sealed class TenantContext : ITenantContext
{
    /// <summary>Claim type carrying the active company identifier.</summary>
    public const string CompanyIdClaim = "company_id";

    /// <summary>Claim type carrying the active company short code.</summary>
    public const string CompanyShortCodeClaim = "company_code";

    /// <summary>Claim type carrying the active company display name.</summary>
    public const string CompanyNameClaim = "company_name";

    private readonly IHttpContextAccessor _accessor;

    /// <summary>Initialises a new instance.</summary>
    public TenantContext(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    /// <inheritdoc />
    public int? CompanyId =>
        int.TryParse(Principal?.FindFirstValue(CompanyIdClaim), out var id) ? id : null;

    /// <inheritdoc />
    public string? CompanyShortCode => Principal?.FindFirstValue(CompanyShortCodeClaim);

    /// <inheritdoc />
    public string? CompanyName => Principal?.FindFirstValue(CompanyNameClaim);

    /// <inheritdoc />
    public bool CanAccessAllCompanies =>
        Principal?.HasClaim(Permissions.ClaimType, Permissions.Reports.AllCompanies) ?? false;

    /// <inheritdoc />
    public int RequireCompanyId() =>
        CompanyId ?? throw new InvalidOperationException(
            "No company is selected. This operation requires an active company scope.");
}
