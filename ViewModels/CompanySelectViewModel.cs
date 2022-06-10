using SupplyChain.Application.Companies;

namespace SupplyChain.Web.ViewModels;

/// <summary>
/// Company picker shown after sign-in. Migrated from the company selection on <c>frmSplash.frm</c>.
/// </summary>
public class CompanySelectViewModel
{
    /// <summary>Companies the signed-in user has been granted access to.</summary>
    public IReadOnlyList<CompanyDto> Companies { get; set; } = Array.Empty<CompanyDto>();

    /// <summary>The currently active company, if one has been chosen.</summary>
    public int? CurrentCompanyId { get; set; }

    /// <summary>Where to send the user after selecting a company.</summary>
    public string? ReturnUrl { get; set; }
}
