using SupplyChain.Application.Reports;
using SupplyChain.Application.Reports.MonthlyStatement;
using SupplyChain.Application.Reports.PartyStatement;

namespace SupplyChain.Web.Reporting;

/// <summary>Printed columns for the party statement, in detail.</summary>
public static class PartyStatementColumns
{
    /// <summary>The column definitions, in print order.</summary>
    public static readonly IReadOnlyList<ReportColumn<PartyStatementRow>> Detail =
    [
        new("Party", r => r.PartyName ?? string.Empty, 2f),
        new("Stmt No", r => r.StatementNo.ToString(), 0.7f),
        new("Tr No", r => r.TransactionNo.ToString(), 0.6f),
        new("Type", r => TransactionStatementColumns.Describe(r.Type), 0.9f),
        new("Tran Date", r => r.TransactionDate.ToString("dd-MMM-yyyy"), 1f),
        new("Bill No", r => r.BillNo ?? string.Empty, 1f),
        new("Bill Date", r => r.BillDate?.ToString("dd-MMM-yyyy") ?? string.Empty, 1f),
        new("MRR No", r => r.MrrNo ?? string.Empty, 1f),
        new("Challan No", r => r.ChallanNo ?? string.Empty, 1f),
        new("Remark", r => r.Remark ?? string.Empty, 1.5f),
        new("Amount", r => r.Amount.ToString("N2"), 1f, IsNumeric: true, Total: r => r.Amount),
    ];

    /// <summary>The summary column definitions.</summary>
    public static readonly IReadOnlyList<ReportColumn<PartyStatementSummaryRow>> Summary =
    [
        new("Company", r => r.CompanyName, 2f),
        new("Party", r => r.PartyName ?? string.Empty, 3f),
        new("Entries", r => r.EntryCount.ToString(), 1f, IsNumeric: true),
        new("Amount", r => r.Amount.ToString("N2"), 1.5f, IsNumeric: true, Total: r => r.Amount),
    ];
}

/// <summary>Printed columns for the monthly statement.</summary>
public static class MonthlyStatementColumns
{
    /// <summary>The column definitions, in print order.</summary>
    public static readonly IReadOnlyList<ReportColumn<MonthlyStatementRow>> Definition =
    [
        new("Company", r => r.CompanyName, 2f),
        new("Month", r => $"{r.MonthName} {r.Year}", 1.5f),
        new("Type", r => TransactionStatementColumns.Describe(r.Type), 1.2f),
        new("Entries", r => r.EntryCount.ToString(), 1f, IsNumeric: true),
        new("Amount", r => r.Amount.ToString("N2"), 1.5f, IsNumeric: true, Total: r => r.Amount),
    ];
}
