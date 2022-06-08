using System.ComponentModel.DataAnnotations;
using SupplyChain.Application.Accounts;
using SupplyChain.Application.Common;
using SupplyChain.Application.MasterData;
using SupplyChain.Domain.Enums;

namespace SupplyChain.Web.ViewModels;

/// <summary>Bill list.</summary>
public class BillIndexViewModel
{
    /// <summary>The page of bills to display.</summary>
    public PagedResult<BillSummaryDto> Results { get; set; } = PagedResult<BillSummaryDto>.Empty();

    /// <summary>The active search term, if any.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>The active party filter, if any.</summary>
    public int? PartyId { get; set; }

    /// <summary>Parties available for filtering.</summary>
    public IReadOnlyList<NamedItemDto> Parties { get; set; } = Array.Empty<NamedItemDto>();
}

/// <summary>
/// Bill create and edit form.
/// </summary>
/// <remarks>
/// Replaces <c>frmBillNO.frm</c> and <c>frmBillNOCash.frm</c>. The cash-or-credit choice that
/// justified two separate forms is now a field on this one.
/// </remarks>
public class BillFormViewModel
{
    /// <summary>The record identifier, or <c>null</c> when creating.</summary>
    public int? Id { get; set; }

    /// <summary>The identification number, shown once the bill exists.</summary>
    public string? Number { get; set; }

    /// <summary>When the batch was identified. Legacy column: <c>IDDate</c>.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "ID Date")]
    public DateOnly IdentificationDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Cash or credit. Legacy column: <c>Status</c>.</summary>
    [Display(Name = "Type")]
    public BillKind Kind { get; set; } = BillKind.Credit;

    /// <summary>Currency the amounts are recorded in.</summary>
    [Display(Name = "Currency")]
    public CurrencyCode Currency { get; set; } = CurrencyCode.BDT;

    /// <summary>The invoices.</summary>
    public List<BillLineInputModel> Lines { get; set; } = [];

    /// <summary>Parties available for selection.</summary>
    public IReadOnlyList<NamedItemDto> Parties { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Accounts heads available for selection.</summary>
    public IReadOnlyList<NamedItemDto> AccountsHeads { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Whether this form is editing an existing bill.</summary>
    public bool IsEdit => Id.HasValue;
}

/// <summary>One invoice as posted by the form.</summary>
public class BillLineInputModel
{
    /// <summary>The accounts head posted to.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Select Head of Accounts")]
    public int AccountsHeadId { get; set; }

    /// <summary>The party billed.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Select party name")]
    public int PartyId { get; set; }

    /// <summary>Invoice number.</summary>
    [Required(ErrorMessage = "You should provide Bill/Invoice NO")]
    [StringLength(50)]
    public string BillNo { get; set; } = string.Empty;

    /// <summary>Invoice date.</summary>
    [DataType(DataType.Date)]
    public DateOnly BillDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Invoice amount.</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "You should provide Bill/Invoice Amount")]
    public decimal Amount { get; set; }

    /// <summary>Goods receipt referenced.</summary>
    [StringLength(50)]
    public string? MrrNo { get; set; }

    /// <summary>Goods receipt date referenced.</summary>
    [DataType(DataType.Date)]
    public DateOnly? MrrDate { get; set; }

    /// <summary>Free-text note.</summary>
    [StringLength(500)]
    public string? Remarks { get; set; }
}

/// <summary>Cheque book list.</summary>
public class ChequeBookIndexViewModel
{
    /// <summary>The page of cheque books to display.</summary>
    public PagedResult<ChequeBookSummaryDto> Results { get; set; } =
        PagedResult<ChequeBookSummaryDto>.Empty();

    /// <summary>The active search term, if any.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>The active bank filter, if any.</summary>
    public int? BankId { get; set; }

    /// <summary>Banks available for filtering.</summary>
    public IReadOnlyList<NamedItemDto> Banks { get; set; } = Array.Empty<NamedItemDto>();
}

/// <summary>Cheque book create and edit form. Replaces <c>frmChequeInfo.frm</c>.</summary>
public class ChequeBookFormViewModel
{
    /// <summary>The record identifier, or <c>null</c> when creating.</summary>
    public int? Id { get; set; }

    /// <summary>The issuing bank.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "You should provide issue bank name")]
    [Display(Name = "Issue Bank")]
    public int BankId { get; set; }

    /// <summary>The book number.</summary>
    [Required(ErrorMessage = "You should provide issue book no")]
    [StringLength(50)]
    [Display(Name = "Cheque Book No")]
    public string BookNo { get; set; } = string.Empty;

    /// <summary>First cheque number in the book.</summary>
    [Range(1, int.MaxValue)]
    [Display(Name = "Start No")]
    public int StartNo { get; set; } = 1;

    /// <summary>Last cheque number in the book.</summary>
    [Range(1, int.MaxValue)]
    [Display(Name = "End No")]
    public int EndNo { get; set; } = 50;

    /// <summary>Optional prefix printed before the number.</summary>
    [StringLength(10)]
    [Display(Name = "Prefix")]
    public string? Prefix { get; set; }

    /// <summary>Currency the cheques are drawn in.</summary>
    [Display(Name = "Currency")]
    public CurrencyCode Currency { get; set; } = CurrencyCode.BDT;

    /// <summary>The cheques drawn.</summary>
    public List<ChequeInputModel> Cheques { get; set; } = [];

    /// <summary>Banks available for selection.</summary>
    public IReadOnlyList<NamedItemDto> Banks { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Whether this form is editing an existing book.</summary>
    public bool IsEdit => Id.HasValue;
}

/// <summary>One cheque as posted by the form.</summary>
public class ChequeInputModel
{
    /// <summary>Cheque number.</summary>
    [Required(ErrorMessage = "You should provide cheque no.")]
    [StringLength(30)]
    public string ChequeNo { get; set; } = string.Empty;

    /// <summary>When it was issued.</summary>
    [DataType(DataType.Date)]
    public DateOnly IssueDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>The amount.</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "You should provide cheque value.")]
    public decimal Value { get; set; }

    /// <summary>Who it is drawn in favour of.</summary>
    [Required(ErrorMessage = "You should provide beneficiary name.")]
    [StringLength(150)]
    public string Beneficiary { get; set; } = string.Empty;

    /// <summary>What the payment is for.</summary>
    [StringLength(250)]
    public string? PaymentParticulars { get; set; }

    /// <summary>When it was honoured, if it has been.</summary>
    [DataType(DataType.Date)]
    public DateOnly? EncashmentDate { get; set; }

    /// <summary>Free-text note.</summary>
    [StringLength(500)]
    public string? Remarks { get; set; }
}
