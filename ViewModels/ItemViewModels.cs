using System.ComponentModel.DataAnnotations;
using SupplyChain.Application.Common;
using SupplyChain.Application.MasterData;

namespace SupplyChain.Web.ViewModels;

/// <summary>Item type list screen. Replaces <c>frmNewItemType.frm</c>.</summary>
public class ItemTypeIndexViewModel
{
    /// <summary>The page of item types to display.</summary>
    public PagedResult<ItemTypeDto> Results { get; set; } = PagedResult<ItemTypeDto>.Empty();

    /// <summary>The active search term, if any.</summary>
    public string? SearchTerm { get; set; }
}

/// <summary>Item type create and edit form.</summary>
public class ItemTypeFormViewModel
{
    /// <summary>The record identifier, or <c>null</c> when creating.</summary>
    public int? Id { get; set; }

    /// <summary>The type name. Legacy column: <c>itemType</c>.</summary>
    [Required(ErrorMessage = "Type Name of Item Type")]
    [StringLength(30, ErrorMessage = "Maximum size is '30'")]
    [Display(Name = "Name of Item Type")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional higher-level grouping. Legacy column: <c>itemGroup</c>.</summary>
    [StringLength(30)]
    [Display(Name = "Item Group")]
    public string? Group { get; set; }

    /// <summary>Optional abbreviation used on reports. Legacy column: <c>itemShortName</c>.</summary>
    [StringLength(20)]
    [Display(Name = "Short Name")]
    public string? ShortName { get; set; }

    /// <summary>Whether this form is editing an existing record.</summary>
    public bool IsEdit => Id.HasValue;
}

/// <summary>Item list screen. Replaces <c>frmNewItemName.frm</c>.</summary>
public class ItemIndexViewModel
{
    /// <summary>The page of items to display.</summary>
    public PagedResult<ItemDto> Results { get; set; } = PagedResult<ItemDto>.Empty();

    /// <summary>The active search term, if any.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>The active item type filter, if any.</summary>
    public int? ItemTypeId { get; set; }

    /// <summary>Item types available for filtering.</summary>
    public IReadOnlyList<NamedItemDto> ItemTypes { get; set; } = Array.Empty<NamedItemDto>();
}

/// <summary>Item create and edit form.</summary>
public class ItemFormViewModel
{
    /// <summary>The record identifier, or <c>null</c> when creating.</summary>
    public int? Id { get; set; }

    /// <summary>The item name. Legacy column: <c>itemName</c>.</summary>
    [Required(ErrorMessage = "Type Name of Item")]
    [StringLength(30, ErrorMessage = "Maximum size is '30'")]
    [Display(Name = "Name of Item")]
    public string Name { get; set; } = string.Empty;

    /// <summary>The classification this item falls under.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Select Under of Item")]
    [Display(Name = "Under of Item")]
    public int ItemTypeId { get; set; }

    /// <summary>Item types available for selection.</summary>
    public IReadOnlyList<NamedItemDto> ItemTypes { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Whether this form is editing an existing record.</summary>
    public bool IsEdit => Id.HasValue;
}

/// <summary>Supplier list screen. Replaces <c>frmNewSupplier.frm</c>.</summary>
public class SupplierIndexViewModel
{
    /// <summary>The page of suppliers to display.</summary>
    public PagedResult<SupplierDto> Results { get; set; } = PagedResult<SupplierDto>.Empty();

    /// <summary>The active search term, if any.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>The active country filter, if any.</summary>
    public int? CountryId { get; set; }

    /// <summary>Countries available for filtering.</summary>
    public IReadOnlyList<NamedItemDto> Countries { get; set; } = Array.Empty<NamedItemDto>();
}

/// <summary>Supplier create and edit form.</summary>
public class SupplierFormViewModel
{
    /// <summary>The record identifier, or <c>null</c> when creating.</summary>
    public int? Id { get; set; }

    /// <summary>The supplier name. Legacy column: <c>suppName</c>.</summary>
    [Required(ErrorMessage = "Type name of supplier")]
    [StringLength(50, ErrorMessage = "Maximum size is '50'")]
    [Display(Name = "Name of Supplier")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Postal address. Legacy column: <c>suppAddress</c>.</summary>
    [StringLength(250)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    /// <summary>Country of origin. Legacy column: <c>suppCountry</c>.</summary>
    [Display(Name = "Country")]
    public int? CountryId { get; set; }

    /// <summary>Countries available for selection.</summary>
    public IReadOnlyList<NamedItemDto> Countries { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Whether this form is editing an existing record.</summary>
    public bool IsEdit => Id.HasValue;
}
