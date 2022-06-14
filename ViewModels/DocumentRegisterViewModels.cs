using System.ComponentModel.DataAnnotations;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Reports;
using SupplyChain.Application.Reports.Registers;

namespace SupplyChain.Web.ViewModels;

/// <summary>The purchase order register screen. Replaces thirteen Crystal files.</summary>
public class PurchaseOrderRegisterViewModel
{
    /// <summary>Which filter is applied.</summary>
    [Display(Name = "Filter By")]
    public PurchaseOrderRegisterMode Mode { get; set; } = PurchaseOrderRegisterMode.All;

    /// <summary>The order, for the single-order mode.</summary>
    [Display(Name = "Purchase Order")]
    public int? PurchaseOrderId { get; set; }

    /// <summary>The supplier, for the supplier mode.</summary>
    [Display(Name = "Supplier")]
    public int? SupplierId { get; set; }

    /// <summary>The item type, for the item-type mode.</summary>
    [Display(Name = "Item Type")]
    public int? ItemTypeId { get; set; }

    /// <summary>Start of the order-date range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateOnly? FromDate { get; set; }

    /// <summary>End of that range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateOnly? ToDate { get; set; }

    /// <summary>Whether the form has been submitted.</summary>
    public bool HasRun { get; set; }

    /// <summary>Purchase orders available for selection.</summary>
    public IReadOnlyList<NamedItemDto> PurchaseOrders { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Suppliers available for selection.</summary>
    public IReadOnlyList<NamedItemDto> Suppliers { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Item types available for selection.</summary>
    public IReadOnlyList<NamedItemDto> ItemTypes { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>The report output, once run.</summary>
    public ReportResult<PurchaseOrderRegisterRow>? Report { get; set; }
}

/// <summary>The goods receipt register screen. Replaces twelve Crystal files.</summary>
public class MrrRegisterViewModel
{
    /// <summary>Which filter is applied.</summary>
    [Display(Name = "Filter By")]
    public MrrRegisterMode Mode { get; set; } = MrrRegisterMode.MrrDate;

    /// <summary>The receipt or challan number, for the reference modes.</summary>
    [Display(Name = "Reference")]
    public string? Reference { get; set; }

    /// <summary>The supplier, for the supplier modes.</summary>
    [Display(Name = "Supplier")]
    public int? SupplierId { get; set; }

    /// <summary>The item, for the supplier-and-item mode.</summary>
    [Display(Name = "Item")]
    public int? ItemId { get; set; }

    /// <summary>Start of the date range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateOnly? FromDate { get; set; }

    /// <summary>End of that range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateOnly? ToDate { get; set; }

    /// <summary>Whether the form has been submitted.</summary>
    public bool HasRun { get; set; }

    /// <summary>Suppliers available for selection.</summary>
    public IReadOnlyList<NamedItemDto> Suppliers { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Items available for selection.</summary>
    public IReadOnlyList<NamedItemDto> Items { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>The report output, once run.</summary>
    public ReportResult<MrrRegisterRow>? Report { get; set; }

    /// <summary>Whether the chosen mode filters on a typed reference.</summary>
    public bool UsesReference => Mode is MrrRegisterMode.MrrNo or MrrRegisterMode.ChallanNo;

    /// <summary>Whether the chosen mode filters on a date range.</summary>
    public bool UsesDateRange => Mode is MrrRegisterMode.MrrDate
        or MrrRegisterMode.ChallanDate or MrrRegisterMode.SupplierAndDate;

    /// <summary>Whether the chosen mode filters on a supplier.</summary>
    public bool UsesSupplier => Mode is MrrRegisterMode.SupplierAndDate
        or MrrRegisterMode.SupplierAndItem;
}

/// <summary>The item movement screen. Replaces five Crystal files.</summary>
public class ItemMovementViewModel
{
    /// <summary>What the report covers.</summary>
    [Display(Name = "Subject")]
    public ItemMovementSubject Subject { get; set; } = ItemMovementSubject.OrderedAndReceived;

    /// <summary>Restrict to one item; blank covers every item.</summary>
    [Display(Name = "Item")]
    public int? ItemId { get; set; }

    /// <summary>Start of the date range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateOnly? FromDate { get; set; }

    /// <summary>End of that range.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateOnly? ToDate { get; set; }

    /// <summary>Report across every company.</summary>
    [Display(Name = "All Company")]
    public bool AllCompanies { get; set; }

    /// <summary>Whether the user may report across companies.</summary>
    public bool CanRunAllCompanies { get; set; }

    /// <summary>Whether the form has been submitted.</summary>
    public bool HasRun { get; set; }

    /// <summary>Items available for selection.</summary>
    public IReadOnlyList<NamedItemDto> Items { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>The report output, once run.</summary>
    public ReportResult<ItemMovementRow>? Report { get; set; }
}
