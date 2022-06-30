using System.ComponentModel.DataAnnotations;
using SupplyChain.Application.Common;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Purchasing;
using SupplyChain.Domain.Enums;

namespace SupplyChain.Web.ViewModels;

/// <summary>Purchase order list.</summary>
public class PurchaseOrderIndexViewModel
{
    /// <summary>The page of orders to display.</summary>
    public PagedResult<PurchaseOrderSummaryDto> Results { get; set; } =
        PagedResult<PurchaseOrderSummaryDto>.Empty();

    /// <summary>The active search term, if any.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>The active supplier filter, if any.</summary>
    public int? SupplierId { get; set; }

    /// <summary>Suppliers available for filtering.</summary>
    public IReadOnlyList<NamedItemDto> Suppliers { get; set; } = Array.Empty<NamedItemDto>();
}

/// <summary>
/// Purchase order create and edit form.
/// </summary>
/// <remarks>
/// Replaces <c>frmProformaInvoice.frm</c> and <c>frmProformaInvoiceOC.frm</c>. The legacy forms
/// built their line list in an <c>MSFlexGrid</c>; here the lines are posted as a collection.
/// </remarks>
public class PurchaseOrderFormViewModel
{
    /// <summary>The record identifier, or <c>null</c> when creating.</summary>
    public int? Id { get; set; }

    /// <summary>The issued document number, shown once the order exists.</summary>
    public string? Number { get; set; }

    /// <summary>The supplier the order is placed with.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Select Supplier Name")]
    [Display(Name = "Supplier")]
    public int SupplierId { get; set; }

    /// <summary>The order date.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "P.O. Date")]
    public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Start of the period the order covers. Legacy column: <c>FDate</c>.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateOnly? FromDate { get; set; }

    /// <summary>End of that period. Legacy column: <c>TDate</c>.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateOnly? ToDate { get; set; }

    /// <summary>The order lines, posted as a collection.</summary>
    public List<PurchaseOrderLineInputModel> Lines { get; set; } = [];

    /// <summary>Suppliers available for selection.</summary>
    public IReadOnlyList<NamedItemDto> Suppliers { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Items available for selection on a line.</summary>
    public IReadOnlyList<NamedItemDto> Items { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Whether this form is editing an existing order.</summary>
    public bool IsEdit => Id.HasValue;
}

/// <summary>One line as posted by the form.</summary>
public class PurchaseOrderLineInputModel
{
    /// <summary>The item ordered.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Select Name of Item")]
    public int ItemId { get; set; }

    /// <summary>Free-text description.</summary>
    [StringLength(250)]
    public string? Description { get; set; }

    /// <summary>Purchase requisition number.</summary>
    [Required(ErrorMessage = "Enter PR No.")]
    [StringLength(50)]
    public string PrNo { get; set; } = string.Empty;

    /// <summary>Quantity ordered.</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Enter P.O. Qty")]
    public decimal Quantity { get; set; }

    /// <summary>Unit of measure.</summary>
    [StringLength(20)]
    public string? QtyUnit { get; set; }

    /// <summary>Price per unit.</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Enter Item Unit Price")]
    public decimal UnitPrice { get; set; }

    /// <summary>Currency the price is expressed in.</summary>
    public CurrencyCode Currency { get; set; } = CurrencyCode.BDT;

    /// <summary>Delivery or payment condition.</summary>
    [StringLength(100)]
    public string? Condition { get; set; }
}
