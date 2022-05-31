using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChain.Application.Authorization;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Statements;
using SupplyChain.Domain.Enums;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// Cash-and-credit statements.
/// </summary>
/// <remarks>
/// Replaces <c>frmStatementCheque.frm</c>. Wave 3 built <c>CashCreditStatement</c> read-only
/// because the reports needed it first; this completes it.
/// </remarks>
[Authorize]
public class StatementsController : Controller
{
    private readonly IStatementService _statements;
    private readonly ISupplierService _suppliers;

    /// <summary>Initialises a new instance.</summary>
    public StatementsController(IStatementService statements, ISupplierService suppliers)
    {
        _statements = statements;
        _suppliers = suppliers;
    }

    /// <summary>Lists statements.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Accounts.View)]
    public async Task<IActionResult> Index(
        string? search,
        TransactionType? type,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        return View(new StatementIndexViewModel
        {
            Results = await _statements.SearchAsync(search, type, page, 25, cancellationToken),
            SearchTerm = search,
            Type = type,
        });
    }

    /// <summary>Shows one statement with its entries.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Accounts.View)]
    public async Task<IActionResult> Details(
        int statementNo,
        TransactionType type,
        DateOnly transactionDate,
        CancellationToken cancellationToken)
    {
        var result = await _statements.GetAsync(statementNo, type, transactionDate, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    /// <summary>Shows the create form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Accounts.Create)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new StatementFormViewModel { Suppliers = await LoadSuppliersAsync(cancellationToken) };
        return View("Form", model);
    }

    /// <summary>Creates a statement and its entries.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.Accounts.Create)]
    public async Task<IActionResult> Create(StatementFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Suppliers = await LoadSuppliersAsync(cancellationToken);
            return View("Form", model);
        }

        var result = await _statements.CreateAsync(ToDto(model), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.Suppliers = await LoadSuppliersAsync(cancellationToken);
            return View("Form", model);
        }

        TempData["Success"] = "Data saved successfully";

        return RedirectToAction(nameof(Details), new
        {
            statementNo = result.Value.StatementNo,
            type = result.Value.Type,
            transactionDate = result.Value.TransactionDate.ToString("yyyy-MM-dd"),
        });
    }

    /// <summary>Shows the edit form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Accounts.Edit)]
    public async Task<IActionResult> Edit(
        int statementNo,
        TransactionType type,
        DateOnly transactionDate,
        CancellationToken cancellationToken)
    {
        var result = await _statements.GetAsync(statementNo, type, transactionDate, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var statement = result.Value;

        return View("Form", new StatementFormViewModel
        {
            StatementNo = statement.StatementNo,
            Type = statement.Type,
            TransactionDate = statement.TransactionDate,
            Currency = statement.Currency,
            Suppliers = await LoadSuppliersAsync(cancellationToken),
            Lines = statement.Lines
                .Select(l => new StatementLineInputModel
                {
                    SupplierId = l.SupplierId,
                    PartyName = l.PartyName ?? string.Empty,
                    BillNo = l.BillNo,
                    BillDate = l.BillDate,
                    MrrNo = l.MrrNo,
                    MrrDate = l.MrrDate,
                    ChallanNo = l.ChallanNo,
                    ChallanDate = l.ChallanDate,
                    Remark = l.Remark,
                    Amount = l.Amount,
                })
                .ToList(),
        });
    }

    /// <summary>Replaces a statement's entries, marking the previous set superseded.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.Accounts.Edit)]
    public async Task<IActionResult> Edit(
        int statementNo,
        TransactionType originalType,
        DateOnly originalDate,
        StatementFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.StatementNo = statementNo;

        if (!ModelState.IsValid)
        {
            model.Suppliers = await LoadSuppliersAsync(cancellationToken);
            return View("Form", model);
        }

        var result = await _statements.ReplaceAsync(
            statementNo, originalType, originalDate, ToDto(model), cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.Suppliers = await LoadSuppliersAsync(cancellationToken);
            return View("Form", model);
        }

        TempData["Success"] = "Data saved successfully";

        return RedirectToAction(nameof(Details), new
        {
            statementNo,
            type = result.Value.Type,
            transactionDate = result.Value.TransactionDate.ToString("yyyy-MM-dd"),
        });
    }

    /// <summary>Deletes a statement.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.Accounts.Delete)]
    public async Task<IActionResult> Delete(
        int statementNo,
        TransactionType type,
        DateOnly transactionDate,
        CancellationToken cancellationToken)
    {
        var result = await _statements.DeleteAsync(statementNo, type, transactionDate, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Deletion complete successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static SaveStatementDto ToDto(StatementFormViewModel model) => new(
        model.Type,
        model.TransactionDate,
        model.Currency,
        model.Lines
            .Select(l => new SaveStatementLineDto(
                l.SupplierId, l.PartyName, l.BillNo, l.BillDate, l.MrrNo, l.MrrDate,
                l.ChallanNo, l.ChallanDate, l.Remark, l.Amount))
            .ToList());

    private async Task<IReadOnlyList<NamedItemDto>> LoadSuppliersAsync(CancellationToken cancellationToken)
    {
        var suppliers = await _suppliers.SearchAsync(null, null, 1, int.MaxValue, cancellationToken);
        return suppliers.Items.Select(s => new NamedItemDto(s.Id, s.Name)).ToList();
    }
}
