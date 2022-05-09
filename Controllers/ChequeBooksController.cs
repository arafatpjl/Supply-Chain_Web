using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChain.Application.Accounts;
using SupplyChain.Application.Authorization;
using SupplyChain.Application.MasterData;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// Cheque books and the cheques drawn from them.
/// </summary>
/// <remarks>Replaces <c>frmChequeInfo.frm</c>.</remarks>
[Authorize]
public class ChequeBooksController : Controller
{
    private readonly IChequeBookService _books;
    private readonly IBankService _banks;

    /// <summary>Initialises a new instance.</summary>
    public ChequeBooksController(IChequeBookService books, IBankService banks)
    {
        _books = books;
        _banks = banks;
    }

    /// <summary>Lists cheque books.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Accounts.View)]
    public async Task<IActionResult> Index(
        string? search,
        int? bankId,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        return View(new ChequeBookIndexViewModel
        {
            Results = await _books.SearchAsync(search, bankId, page, 25, cancellationToken),
            SearchTerm = search,
            BankId = bankId,
            Banks = await _banks.ListAllAsync(cancellationToken),
        });
    }

    /// <summary>Shows one book with its cheques.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Accounts.View)]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var result = await _books.GetAsync(id, cancellationToken);
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
        var model = new ChequeBookFormViewModel { Banks = await _banks.ListAllAsync(cancellationToken) };
        return View("Form", model);
    }

    /// <summary>Creates a cheque book and its cheques.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.Accounts.Create)]
    public async Task<IActionResult> Create(ChequeBookFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Banks = await _banks.ListAllAsync(cancellationToken);
            return View("Form", model);
        }

        var result = await _books.CreateAsync(ToDto(model), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.Banks = await _banks.ListAllAsync(cancellationToken);
            return View("Form", model);
        }

        TempData["Success"] = "Data saved successfully.";
        return RedirectToAction(nameof(Details), new { id = result.Value.Id });
    }

    /// <summary>Shows the edit form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Accounts.Edit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await _books.GetAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var book = result.Value;

        return View("Form", new ChequeBookFormViewModel
        {
            Id = book.Id,
            BankId = book.BankId,
            BookNo = book.BookNo,
            StartNo = book.StartNo,
            EndNo = book.EndNo,
            Prefix = book.Prefix,
            Currency = book.Currency,
            Banks = await _banks.ListAllAsync(cancellationToken),
            Cheques = book.Cheques
                .Select(c => new ChequeInputModel
                {
                    ChequeNo = c.ChequeNo,
                    IssueDate = c.IssueDate,
                    Value = c.Value,
                    Beneficiary = c.Beneficiary,
                    PaymentParticulars = c.PaymentParticulars,
                    EncashmentDate = c.EncashmentDate,
                    Remarks = c.Remarks,
                })
                .ToList(),
        });
    }

    /// <summary>Updates a book and replaces its cheques.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.Accounts.Edit)]
    public async Task<IActionResult> Edit(
        int id,
        ChequeBookFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            model.Banks = await _banks.ListAllAsync(cancellationToken);
            return View("Form", model);
        }

        var result = await _books.UpdateAsync(id, ToDto(model), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.Banks = await _banks.ListAllAsync(cancellationToken);
            return View("Form", model);
        }

        TempData["Success"] = "Data updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Deletes a cheque book.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.Accounts.Delete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _books.DeleteAsync(id, cancellationToken);

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

    private static SaveChequeBookDto ToDto(ChequeBookFormViewModel model) => new(
        model.BankId,
        model.BookNo,
        model.StartNo,
        model.EndNo,
        model.Prefix,
        model.Currency,
        model.Cheques
            .Select(c => new SaveChequeDto(
                c.ChequeNo, c.IssueDate, c.Value, c.Beneficiary,
                c.PaymentParticulars, c.EncashmentDate, c.Remarks))
            .ToList());
}
