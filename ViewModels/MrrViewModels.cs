using System.ComponentModel.DataAnnotations;
using SupplyChain.Application.Common;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Receiving;
using SupplyChain.Domain.Enums;
using SupplyChain.Domain.Interfaces;

namespace SupplyChain.Web.ViewModels;

/// <summary>Goods receipt list.</summary>
public class MrrIndexViewModel
{
    /// <summary>The page of receipts to display.</summary>
    public PagedResult<MrrSummaryDto> Results { get; set; } = PagedResult<MrrSummaryDto>.Empty();

    /// <summary>The active search term, if any.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>The active supplier filter, if any.</summary>
    public int? SupplierId { get; set; }

    /// <summary>Suppliers available for filtering.</summary>
    public IReadOnlyList<NamedItemDto> Suppliers { get; set; } = Array.Empty<NamedItemDto>();
}

/// <summary>
/// Goods receipt create and edit form.
/// </summary>
/// <remarks>
/// Replaces <c>frmMrrReceiveChallan.frm</c> and <c>frmMrrReceiveChallanOtherCurr.frm</c>.
/// </remarks>
public class MrrFormViewModel
{
    /// <summary>The record identifier, or <c>null</c> when creating.</summary>
    public int? Id { get; set; }

    /// <summary>The issued document number, shown once the receipt exists.</summary>
    public string? Number { get; set; }

    /// <summary>The supplier that delivered.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Select Supplier Name")]
    [Display(Name = "Supplier")]
    public int SupplierId { get; set; }

    /// <summary>When the goods were received.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "MRR Date")]
    public DateOnly ReceiptDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Currency the receipt is valued in.</summary>
    [Display(Name = "MRR Currency")]
    public CurrencyCode Currency { get; set; } = CurrencyCode.BDT;

    /// <summary>Charges added. Legacy column: <c>MRRCharge</c>.</summary>
    [Range(0, double.MaxValue)]
    [Display(Name = "Charges")]
    public decimal Charges { get; set; }

    /// <summary>Discount deducted. Legacy column: <c>MRRDiscount</c>.</summary>
    [Range(0, double.MaxValue)]
    [Display(Name = "Discount")]
    public decimal Discount { get; set; }

    /// <summary>The lines received.</summary>
    public List<MrrLineInputModel> Lines { get; set; } = [];

    /// <summary>Employee code certifying quality.</summary>
    [Display(Name = "Quality Certified By")]
    public string? QualityCertifiedBy { get; set; }

    /// <summary>Employee code that prepared the receipt.</summary>
    [Display(Name = "Prepared By")]
    public string? PreparedBy { get; set; }

    /// <summary>Employee code that checked quantity.</summary>
    [Display(Name = "Quantity Checked By")]
    public string? QuantityCheckedBy { get; set; }

    /// <summary>Approving manager.</summary>
    [Display(Name = "Manager")]
    public string? ManagerBy { get; set; }

    /// <summary>Senior manager.</summary>
    [Display(Name = "Quality Senior Manager")]
    public string? SeniorManagerBy { get; set; }

    /// <summary>Suppliers available for selection.</summary>
    public IReadOnlyList<NamedItemDto> Suppliers { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Items available for selection on a line.</summary>
    public IReadOnlyList<NamedItemDto> Items { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Purchase orders available for selection on a line.</summary>
    public IReadOnlyList<NamedItemDto> PurchaseOrders { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Whether this form is editing an existing receipt.</summary>
    public bool IsEdit => Id.HasValue;

    /// <summary>Whether any approval field has been filled in.</summary>
    public bool HasApproval =>
        !string.IsNullOrWhiteSpace(QualityCertifiedBy) ||
        !string.IsNullOrWhiteSpace(PreparedBy) ||
        !string.IsNullOrWhiteSpace(QuantityCheckedBy) ||
        !string.IsNullOrWhiteSpace(ManagerBy) ||
        !string.IsNullOrWhiteSpace(SeniorManagerBy);
}

/// <summary>One received line as posted by the form.</summary>
public class MrrLineInputModel
{
    /// <summary>The item received.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Select Name of Item")]
    public int ItemId { get; set; }

    /// <summary>The purchase order this fulfils, where named.</summary>
    public int? PurchaseOrderId { get; set; }

    /// <summary>Free-text description.</summary>
    [StringLength(250)]
    public string? Description { get; set; }

    /// <summary>Material code.</summary>
    [StringLength(50)]
    public string? MaterialCode { get; set; }

    /// <summary>Purchase requisition number.</summary>
    [Required(ErrorMessage = "Type PR No")]
    [StringLength(50)]
    public string PrNo { get; set; } = string.Empty;

    /// <summary>Quantity the receipt was raised against.</summary>
    public decimal OrderedQty { get; set; }

    /// <summary>Quantity actually received.</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Enter quantity properly")]
    public decimal ReceivedQty { get; set; }

    /// <summary>Unit of measure.</summary>
    [Required(ErrorMessage = "Enter UNIT")]
    [StringLength(20)]
    public string QtyUnit { get; set; } = string.Empty;

    /// <summary>Price per unit.</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Type Unit Price")]
    public decimal UnitPrice { get; set; }

    /// <summary>Challan number.</summary>
    [Required(ErrorMessage = "Type Challan No")]
    [StringLength(50)]
    public string ChallanNo { get; set; } = string.Empty;

    /// <summary>Challan date.</summary>
    [DataType(DataType.Date)]
    public DateOnly ChallanDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>File number.</summary>
    [StringLength(20)]
    public string? MainPf { get; set; }

    /// <summary>Container the goods arrived in.</summary>
    [Required(ErrorMessage = "Enter Container No")]
    [StringLength(50)]
    public string ContainerNo { get; set; } = string.Empty;

    /// <summary>Free-text note.</summary>
    [StringLength(500)]
    public string? Remarks { get; set; }
}

/// <summary>Purchase order balance report. Replaces <c>rptPOBalanceQty.rpt</c>.</summary>
public class PurchaseOrderBalanceViewModel
{
    /// <summary>Restrict to one purchase order.</summary>
    [Display(Name = "Purchase Order")]
    public int? PurchaseOrderId { get; set; }

    /// <summary>Restrict to one supplier.</summary>
    [Display(Name = "Supplier")]
    public int? SupplierId { get; set; }

    /// <summary>Show only lines where more was received than ordered.</summary>
    [Display(Name = "Over-received only")]
    public bool OverReceivedOnly { get; set; }

    /// <summary>Suppliers available for filtering.</summary>
    public IReadOnlyList<NamedItemDto> Suppliers { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Purchase orders available for filtering.</summary>
    public IReadOnlyList<NamedItemDto> PurchaseOrders { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>The balances.</summary>
    public IReadOnlyList<PurchaseOrderBalance> Balances { get; set; } = Array.Empty<PurchaseOrderBalance>();
}
