using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChain.Application.Authorization;
using SupplyChain.Application.Reports;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Reports.MonthlyStatement;
using SupplyChain.Application.Reports.PartyStatement;
using SupplyChain.Application.Reports.Registers;
using SupplyChain.Application.Reports.TransactionStatement;
using SupplyChain.Web.Reporting;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// Read-only reports.
/// </summary>
/// <remarks>
/// <para>
/// Wave 3 is the first module of the parallel run described in
/// <c>docs/03-Database-Migration-Plan.md</c> §5.2: output is compared against the live VB6
/// application while that system remains authoritative for writes.
/// </para>
/// <para>
/// Each report has one action serving both media. The screen and the PDF share a query and a
/// column definition, and differ only in presentation.
/// </para>
/// </remarks>
[Authorize]
public class ReportsController : Controller
{
    private readonly ITransactionStatementQueryService _transactionStatement;
    private readonly IPartyStatementQueryService _partyStatement;
    private readonly IPartyStatementSummaryQueryService _partyStatementSummary;
    private readonly IMonthlyStatementQueryService _monthlyStatement;
    private readonly ISupplierService _suppliers;
    private readonly IPartyService _parties;
    private readonly IAccountsHeadService _heads;
    private readonly IBillRegisterQueryService _billRegister;
    private readonly IChequeRegisterQueryService _chequeRegister;
    private readonly IAuditQueryService _audit;
    private readonly IPurchaseOrderRegisterQueryService _orderRegister;
    private readonly IMrrRegisterQueryService _mrrRegister;
    private readonly IItemMovementQueryService _itemMovement;
    private readonly IItemService _items;
    private readonly IItemTypeService _itemTypes;
    private readonly SupplyChain.Application.Purchasing.IPurchaseOrderService _orders;
    private readonly SupplyChain.Application.Accounts.IChequeBookService _chequeBooks;
    private readonly IPdfRenderer _pdf;

    /// <summary>Initialises a new instance.</summary>
    public ReportsController(
        ITransactionStatementQueryService transactionStatement,
        IPartyStatementQueryService partyStatement,
        IPartyStatementSummaryQueryService partyStatementSummary,
        IMonthlyStatementQueryService monthlyStatement,
        ISupplierService suppliers,
        IPartyService parties,
        IAccountsHeadService heads,
        IBillRegisterQueryService billRegister,
        IChequeRegisterQueryService chequeRegister,
        IAuditQueryService audit,
        IPurchaseOrderRegisterQueryService orderRegister,
        IMrrRegisterQueryService mrrRegister,
        IItemMovementQueryService itemMovement,
        IItemService items,
        IItemTypeService itemTypes,
        SupplyChain.Application.Purchasing.IPurchaseOrderService orders,
        SupplyChain.Application.Accounts.IChequeBookService chequeBooks,
        IPdfRenderer pdf)
    {
        _parties = parties;
        _heads = heads;
        _billRegister = billRegister;
        _chequeRegister = chequeRegister;
        _audit = audit;
        _orderRegister = orderRegister;
        _mrrRegister = mrrRegister;
        _itemMovement = itemMovement;
        _items = items;
        _itemTypes = itemTypes;
        _orders = orders;
        _chequeBooks = chequeBooks;
        _transactionStatement = transactionStatement;
        _partyStatement = partyStatement;
        _partyStatementSummary = partyStatementSummary;
        _monthlyStatement = monthlyStatement;
        _suppliers = suppliers;
        _pdf = pdf;
    }

    /// <summary>
    /// The party statement, in detail or summarised.
    /// </summary>
    /// <remarks>
    /// Replaces the densest form in the legacy application: 752 lines containing a hundred SQL
    /// statements, plus the shared scratch table it used to union across eighteen databases.
    /// </remarks>
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> PartyStatement(
        PartyStatementViewModel model,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        var canRunAllCompanies = User.HasClaim(Permissions.ClaimType, Permissions.Reports.AllCompanies);
        model.CanRunAllCompanies = canRunAllCompanies;

        if (model.AllCompanies && !canRunAllCompanies)
        {
            return Forbid();
        }

        var suppliers = await _suppliers.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        model.Suppliers = suppliers.Items.Select(s => new NamedItemDto(s.Id, s.Name)).ToList();

        if (!model.HasRun)
        {
            return View(model);
        }

        var parameters = new PartyStatementParams(
            model.SupplierId, model.DateBasis, model.FromDate, model.ToDate, model.Type, model.AllCompanies);

        var wantsPdf = string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase);

        if (model.Summary)
        {
            var summary = await _partyStatementSummary.ExecuteAsync(parameters, cancellationToken);

            if (wantsPdf)
            {
                return Pdf(_pdf.Render(summary, PartyStatementColumns.Summary), summary);
            }

            model.SummaryReport = summary;
            return View(model);
        }

        var detail = await _partyStatement.ExecuteAsync(parameters, cancellationToken);

        if (wantsPdf)
        {
            return Pdf(_pdf.Render(detail, PartyStatementColumns.Detail), detail);
        }

        model.Detail = detail;
        return View(model);
    }

    /// <summary>The monthly statement.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> MonthlyStatement(
        MonthlyStatementViewModel model,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        var canRunAllCompanies = User.HasClaim(Permissions.ClaimType, Permissions.Reports.AllCompanies);
        model.CanRunAllCompanies = canRunAllCompanies;

        if (model.AllCompanies && !canRunAllCompanies)
        {
            return Forbid();
        }

        if (!model.HasRun)
        {
            return View(model);
        }

        var report = await _monthlyStatement.ExecuteAsync(
            new MonthlyStatementParams(model.Year, model.Type, model.AllCompanies), cancellationToken);

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Pdf(_pdf.Render(report, MonthlyStatementColumns.Definition), report);
        }

        model.Report = report;
        return View(model);
    }


    /// <summary>
    /// The bill register.
    /// </summary>
    /// <remarks>
    /// Replaces sixteen Crystal files: eight filter modes in a cash set and a credit set, driven
    /// by two forms. The mode and the type are parameters here.
    /// </remarks>
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> BillRegister(
        BillRegisterViewModel model,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        model.Parties = await _parties.ListAllAsync(cancellationToken);
        model.AccountsHeads = await _heads.ListAllAsync(cancellationToken);

        if (!model.HasRun)
        {
            return View(model);
        }

        var report = await _billRegister.ExecuteAsync(
            new BillRegisterParams(
                model.Mode, model.Kind, model.Reference, model.FromDate, model.ToDate,
                model.PartyId, model.AccountsHeadId),
            cancellationToken);

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Pdf(_pdf.Render(report, BillRegisterColumns.Definition), report);
        }

        model.Report = report;
        return View(model);
    }

    /// <summary>The cheque register. Replaces three Crystal files.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> ChequeRegister(
        ChequeRegisterViewModel model,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        var books = await _chequeBooks.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        model.ChequeBooks = books.Items
            .Select(b => new NamedItemDto(b.Id, $"{b.BookNo} — {b.BankName}"))
            .ToList();

        if (!model.HasRun)
        {
            return View(model);
        }

        var report = await _chequeRegister.ExecuteAsync(
            new ChequeRegisterParams(
                model.Mode, model.ChequeBookId, model.FromDate, model.ToDate,
                model.Beneficiary, model.OutstandingOnly),
            cancellationToken);

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Pdf(_pdf.Render(report, ChequeRegisterColumns.Definition), report);
        }

        model.Report = report;
        return View(model);
    }

    /// <summary>
    /// The audit report.
    /// </summary>
    /// <remarks>
    /// Replaces four Crystal files across three forms. The legacy reports could say a document had
    /// been deleted but not who deleted it — the schema recorded only the flag.
    /// </remarks>
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.Audit)]
    public async Task<IActionResult> Audit(
        AuditReportViewModel model,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        if (!model.HasRun)
        {
            return View(model);
        }

        var report = await _audit.ExecuteAsync(
            new AuditParams(model.Subject, model.FromDate, model.ToDate, model.DeletedOnly),
            cancellationToken);

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Pdf(_pdf.Render(report, AuditColumns.Definition), report);
        }

        model.Report = report;
        return View(model);
    }


    /// <summary>The purchase order register. Replaces thirteen Crystal files.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> PurchaseOrderRegister(
        PurchaseOrderRegisterViewModel model,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        var orders = await _orders.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        model.PurchaseOrders = orders.Items.Select(o => new NamedItemDto(o.Id, o.Number)).ToList();

        var suppliers = await _suppliers.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        model.Suppliers = suppliers.Items.Select(s => new NamedItemDto(s.Id, s.Name)).ToList();

        model.ItemTypes = await _itemTypes.ListAllAsync(cancellationToken);

        if (!model.HasRun)
        {
            return View(model);
        }

        var report = await _orderRegister.ExecuteAsync(
            new PurchaseOrderRegisterParams(
                model.Mode, model.PurchaseOrderId, model.SupplierId, model.ItemTypeId,
                model.FromDate, model.ToDate),
            cancellationToken);

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Pdf(_pdf.Render(report, PurchaseOrderRegisterColumns.Definition), report);
        }

        model.Report = report;
        return View(model);
    }

    /// <summary>The goods receipt register. Replaces twelve Crystal files.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> MrrRegister(
        MrrRegisterViewModel model,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        var suppliers = await _suppliers.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        model.Suppliers = suppliers.Items.Select(s => new NamedItemDto(s.Id, s.Name)).ToList();

        var items = await _items.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        model.Items = items.Items.Select(i => new NamedItemDto(i.Id, i.Name)).ToList();

        if (!model.HasRun)
        {
            return View(model);
        }

        var report = await _mrrRegister.ExecuteAsync(
            new MrrRegisterParams(
                model.Mode, model.Reference, model.SupplierId, model.ItemId,
                model.FromDate, model.ToDate),
            cancellationToken);

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Pdf(_pdf.Render(report, MrrRegisterColumns.Definition), report);
        }

        model.Report = report;
        return View(model);
    }

    /// <summary>Item movement: ordered against received. Replaces five Crystal files.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> ItemMovement(
        ItemMovementViewModel model,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        var canRunAllCompanies = User.HasClaim(Permissions.ClaimType, Permissions.Reports.AllCompanies);
        model.CanRunAllCompanies = canRunAllCompanies;

        if (model.AllCompanies && !canRunAllCompanies)
        {
            return Forbid();
        }

        var items = await _items.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        model.Items = items.Items.Select(i => new NamedItemDto(i.Id, i.Name)).ToList();

        if (!model.HasRun)
        {
            return View(model);
        }

        var report = await _itemMovement.ExecuteAsync(
            new ItemMovementParams(
                model.Subject, model.ItemId, model.FromDate, model.ToDate, model.AllCompanies),
            cancellationToken);

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Pdf(_pdf.Render(report, ItemMovementColumns.Definition), report);
        }

        model.Report = report;
        return View(model);
    }

    /// <summary>Returns a rendered PDF with a file name derived from the report.</summary>
    private FileContentResult Pdf<TRow>(byte[] bytes, ReportResult<TRow> report) =>
        File(
            bytes,
            "application/pdf",
            $"{report.Title.Replace(' ', '_').Replace("(", string.Empty).Replace(")", string.Empty)}"
            + $"_{report.GeneratedAt:yyyyMMdd_HHmm}.pdf");

    /// <summary>Lists the available reports.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public IActionResult Index() => View();

    /// <summary>
    /// The transaction statement, in one of its four modes.
    /// </summary>
    /// <param name="model">Filters, bound from the query string so a report URL can be shared.</param>
    /// <param name="format">Pass <c>pdf</c> to download rather than view.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> TransactionStatement(
        TransactionStatementViewModel model,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        var canRunAllCompanies = User.HasClaim(Permissions.ClaimType, Permissions.Reports.AllCompanies);
        model.CanRunAllCompanies = canRunAllCompanies;

        // The legacy forms offered "All Company" to everyone. Refusing it here rather than hiding
        // the checkbox is the point: a posted value cannot grant what the user does not hold.
        if (model.AllCompanies && !canRunAllCompanies)
        {
            return Forbid();
        }

        if (!model.HasRun)
        {
            return View(model);
        }

        var report = await _transactionStatement.ExecuteAsync(
            new TransactionStatementParams(
                model.Mode,
                model.Reference,
                model.FromDate,
                model.ToDate,
                model.Type,
                model.StatementNo,
                model.AllCompanies),
            cancellationToken);

        if (!string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            model.Report = report;
            return View(model);
        }

        return Pdf(_pdf.Render(report, TransactionStatementColumns.Definition), report);
    }
}
