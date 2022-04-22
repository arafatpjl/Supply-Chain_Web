using SupplyChain.Application.Reports;
using SupplyChain.Application.Reports.TransactionStatement;

namespace SupplyChain.Web.Reporting;

/// <summary>
/// The printed columns of the transaction statement.
/// </summary>
/// <remarks>
/// <para>
/// Order and content follow the SELECT list in <c>frmrptTransactionWiseReport.frm</c> line 447, so
/// a printed page can be compared against the legacy output column by column during the parallel
/// run.
/// </para>
/// <para>
/// Defined once and used by the PDF; the Razor view renders the same fields as an HTML table it
/// can sort. Sharing the definition would force the screen to give up sorting and the page to give
/// up its column widths, which is why only the query is shared.
/// </para>
/// </remarks>
public static class TransactionStatementColumns
{
    /// <summary>The column definitions, in print order.</summary>
    public static readonly IReadOnlyList<ReportColumn<TransactionStatementRow>> Definition =
    [
        new("Stmt No", r => r.StatementNo.ToString(), 0.7f),
        new("Tr No", r => r.TransactionNo.ToString(), 0.6f),
        new("Type", r => Describe(r.Type), 0.8f),
        new("Tran Date", r => Format(r.TransactionDate), 0.9f),
        new("Party", r => r.PartyName ?? r.SupplierName ?? string.Empty, 2f),
        new("Bill No", r => r.BillNo ?? string.Empty, 1f),
        new("Bill Date", r => Format(r.BillDate), 0.9f),
        new("MRR No", r => r.MrrNo ?? string.Empty, 1f),
        new("MRR Date", r => Format(r.MrrDate), 0.9f),
        new("Challan No", r => r.ChallanNo ?? string.Empty, 1f),
        new("Challan Date", r => Format(r.ChallanDate), 0.9f),
        new("Remark", r => r.Remark ?? string.Empty, 1.6f),
        new("Amount", r => r.Amount.ToString("N2"), 1f, IsNumeric: true, Total: r => r.Amount),
    ];

    /// <summary>
    /// Renders a transaction type using the legacy wording.
    /// </summary>
    /// <remarks>
    /// The VB6 filter combo listed "Cash", "Credit", "Cheque" and "Cash Cheque"; the enum name
    /// <c>CashCheque</c> would print without its space.
    /// </remarks>
    public static string Describe(Domain.Enums.TransactionType type) => type switch
    {
        Domain.Enums.TransactionType.CashCheque => "Cash Cheque",
        _ => type.ToString(),
    };

    private static string Format(DateOnly? date) => date?.ToString("dd-MMM-yyyy") ?? string.Empty;

    private static string Format(DateOnly date) => date.ToString("dd-MMM-yyyy");
}
