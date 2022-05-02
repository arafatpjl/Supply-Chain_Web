using SupplyChain.Application.MasterData;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>Countries. Replaces <c>frmNewCountry.frm</c>.</summary>
public sealed class CountriesController : NamedMasterDataController
{
    /// <summary>Initialises a new instance.</summary>
    public CountriesController(ICountryService service) : base(service)
    {
    }

    /// <inheritdoc />
    protected override MasterDataDescriptor Descriptor { get; } = new(
        Controller: "Countries",
        Title: "Countries",
        SingularTitle: "Country",
        NameLabel: "Country Name",
        LegacyForm: "frmNewCountry.frm");
}

/// <summary>File numbers. Replaces <c>frmNewPFNo.frm</c>.</summary>
public sealed class PfNumbersController : NamedMasterDataController
{
    /// <summary>Initialises a new instance.</summary>
    public PfNumbersController(IPfNumberService service) : base(service)
    {
    }

    /// <inheritdoc />
    protected override MasterDataDescriptor Descriptor { get; } = new(
        Controller: "PfNumbers",
        Title: "File Numbers",
        SingularTitle: "File Number",
        NameLabel: "PF No.",
        LegacyForm: "frmNewPFNo.frm");
}

/// <summary>Bill parties. Replaces <c>frmParty.frm</c>.</summary>
public sealed class PartiesController : NamedMasterDataController
{
    /// <summary>Initialises a new instance.</summary>
    public PartiesController(IPartyService service) : base(service)
    {
    }

    /// <inheritdoc />
    protected override MasterDataDescriptor Descriptor { get; } = new(
        Controller: "Parties",
        Title: "Parties",
        SingularTitle: "Party",
        NameLabel: "Party Name",
        LegacyForm: "frmParty.frm");
}

/// <summary>Accounts heads. Replaces <c>frmNewAccountsHead.FRM</c>.</summary>
public sealed class AccountsHeadsController : NamedMasterDataController
{
    /// <summary>Initialises a new instance.</summary>
    public AccountsHeadsController(IAccountsHeadService service) : base(service)
    {
    }

    /// <inheritdoc />
    protected override MasterDataDescriptor Descriptor { get; } = new(
        Controller: "AccountsHeads",
        Title: "Accounts Heads",
        SingularTitle: "Accounts Head",
        NameLabel: "Accounts Head Name",
        LegacyForm: "frmNewAccountsHead.FRM");
}

/// <summary>Banks. Migrated from <c>New_Bank</c>, which had no screen of its own.</summary>
public sealed class BanksController : NamedMasterDataController
{
    /// <summary>Initialises a new instance.</summary>
    public BanksController(IBankService service) : base(service)
    {
    }

    /// <inheritdoc />
    protected override MasterDataDescriptor Descriptor { get; } = new(
        Controller: "Banks",
        Title: "Banks",
        SingularTitle: "Bank",
        NameLabel: "Bank Name",
        LegacyForm: "New_Bank (no screen existed)");
}
