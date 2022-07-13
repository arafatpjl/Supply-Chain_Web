using System.ComponentModel.DataAnnotations;
using SupplyChain.Application.Common;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Statements;
using SupplyChain.Domain.Enums;

namespace SupplyChain.Web.ViewModels;

/// <summary>Statement list.</summary>
public class StatementIndexViewModel
{
    /// <summary>The page of statements to display.</summary>
    public PagedResult<StatementSummaryDto> Results { get; set; } =
        PagedResult<StatementSummaryDto>.Empty();

    /// <summary>The active search term, if any.</summary>
    public string? SearchTerm { get; set; }

    /// <summary>The active type filter, if any.</summary>
    public TransactionType? Type { get; set; }
}

/// <summary>
/// Statement create and edit form.
/// </summary>
/// <remarks>
/// Replaces <c>frmStatementCheque.frm</c>, the last screen in the system that wrote to a table
/// this application only read.
/// </remarks>
public class StatementFormViewModel
{
    /// <summary>The statement number, once issued.</summary>
    public int? StatementNo { get; set; }

    /// <summary>How the entries were settled.</summary>
    [Display(Name = "Transaction Type")]
    public TransactionType Type { get; set; } = TransactionType.Cash;

    /// <summary>When they were recorded.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "Transaction Date")]
    public DateOnly TransactionDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Currency the amounts are recorded in.</summary>
    [Display(Name = "Currency")]
    public CurrencyCode Currency { get; set; } = CurrencyCode.BDT;

    /// <summary>The entries.</summary>
    public List<StatementLineInputModel> Lines { get; set; } = [];

    /// <summary>Suppliers available for selection.</summary>
    public IReadOnlyList<NamedItemDto> Suppliers { get; set; } = Array.Empty<NamedItemDto>();

    /// <summary>Whether this form is editing an existing statement.</summary>
    public bool IsEdit => StatementNo.HasValue;
}

/// <summary>One statement entry as posted by the form.</summary>
public class StatementLineInputModel
{
    /// <summary>The supplier the entry concerns.</summary>
    public int? SupplierId { get; set; }

    /// <summary>The party as recorded on the entry.</summary>
    [Required(ErrorMessage = "You should provide Name Of Party")]
    [StringLength(150)]
    public string PartyName { get; set; } = string.Empty;

    /// <summary>Invoice number.</summary>
    [StringLength(50)]
    public string? BillNo { get; set; }

    /// <summary>Invoice date.</summary>
    [DataType(DataType.Date)]
    public DateOnly? BillDate { get; set; }

    /// <summary>Goods receipt referenced.</summary>
    [StringLength(50)]
    public string? MrrNo { get; set; }

    /// <summary>Goods receipt date referenced.</summary>
    [DataType(DataType.Date)]
    public DateOnly? MrrDate { get; set; }

    /// <summary>Challan referenced.</summary>
    [StringLength(50)]
    public string? ChallanNo { get; set; }

    /// <summary>Challan date referenced.</summary>
    [DataType(DataType.Date)]
    public DateOnly? ChallanDate { get; set; }

    /// <summary>Free-text note.</summary>
    [StringLength(500)]
    public string? Remark { get; set; }

    /// <summary>The value of the entry.</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "You should provide Amount")]
    public decimal Amount { get; set; }
}
