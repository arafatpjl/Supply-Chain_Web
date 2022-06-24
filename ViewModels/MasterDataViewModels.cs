using SupplyChain.Application.Common;
using SupplyChain.Application.MasterData;

namespace SupplyChain.Web.ViewModels;

/// <summary>
/// Labels and route information that let one set of Razor views serve several master-data screens.
/// </summary>
/// <param name="Controller">The controller name, used to build action links.</param>
/// <param name="Title">Plural heading, e.g. "Countries".</param>
/// <param name="SingularTitle">Singular label, e.g. "Country".</param>
/// <param name="NameLabel">Label for the name field, e.g. "Country Name" or "PF No.".</param>
/// <param name="LegacyForm">The VB6 form this screen replaces, shown as provenance in the UI.</param>
public record MasterDataDescriptor(
    string Controller,
    string Title,
    string SingularTitle,
    string NameLabel,
    string LegacyForm);

/// <summary>List screen for a master-data entity identified by a unique name.</summary>
public class NamedMasterDataIndexViewModel
{
    /// <summary>Labels and routes for the screen being rendered.</summary>
    public MasterDataDescriptor Descriptor { get; set; } = null!;

    /// <summary>The page of records to display.</summary>
    public PagedResult<NamedItemDto> Results { get; set; } = PagedResult<NamedItemDto>.Empty();

    /// <summary>The active search term, if any.</summary>
    public string? SearchTerm { get; set; }
}

/// <summary>Create and edit form for a master-data entity identified by a unique name.</summary>
public class NamedMasterDataFormViewModel
{
    /// <summary>Labels and routes for the screen being rendered.</summary>
    public MasterDataDescriptor Descriptor { get; set; } = null!;

    /// <summary>The record identifier, or <c>null</c> when creating.</summary>
    public int? Id { get; set; }

    /// <summary>The unique name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Whether this form is editing an existing record.</summary>
    public bool IsEdit => Id.HasValue;
}
