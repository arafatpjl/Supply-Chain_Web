using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChain.Application.Accounts;
using SupplyChain.Application.Authorization;
using SupplyChain.Application.MasterData;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// Bills — batches of supplier invoices, cash or credit.
/// </summary>
/// <remarks>
/// Replaces <c>frmBillNO.frm</c> and <c>frmBillNOCash.frm</c>, roughly 3,300 lines across two forms
/// whose only functional difference was the character written to <c>BillMain.Status</c>.
/// See <c>docs/analysis/bill-cash-credit-variant-diff.md</c>.
/// </remarks>
[Authorize]
public class BillsController : Controller
{
    private readonly IBillService _bills;
    private readonly IPartyService _parties;
    private readonly IAccountsHeadService _heads;

    /// <summary>Initialises a new instance.</summary>
    public BillsController(IBillService bills, IPartyService parties, IAccountsHeadService heads)
    {
        _bills = bills;
        _parties = parties;
        _heads = heads;
    }

    /// <summary>Lists bills.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Accounts.View)]
    public async Task<IActionResult> Index(
        string? search,
        int? partyId,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        return View(new BillIndexViewModel
        {
            Results = await _bills.SearchAsync(search, partyId, page, 25, cancellationToken),
            SearchTerm = search,
            PartyId = partyId,
            Parties = await _parties.ListAllAsync(cancellationToken),
        });
    }

    /// <summary>Shows one bill with its invoices.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.Accounts.View)]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var result = await _bills.GetAsync(id, cancellationToken);
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
        var model = new BillFormViewModel();
        await PopulateAsync(model, cancellationToken);
        return View("Form", model);
    }

    /// <summary>Creates a bill and its invoices.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.Accounts.Create)]
    public async Task<IActionResult> Create(BillFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model, cancellationToken);
            return View("Form", model);
        }

        var result = await _bills.CreateAsync(ToDto(model), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateAsync(model, cancellationToken);
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
        var result = await _bills.GetAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var bill = result.Value;

        var model = new BillFormViewModel
        {
            Id = bill.Id,
            Number = bill.Number,
            IdentificationDate = bill.IdentificationDate,
            Kind = bill.Kind,
            Currency = bill.Currency,
            Lines = bill.Lines
                .Select(l => new BillLineInputModel
                {
                    AccountsHeadId = l.AccountsHeadId,
                    PartyId = l.PartyId,
                    BillNo = l.BillNo,
                    BillDate = l.BillDate,
                    Amount = l.Amount,
                    MrrNo = l.MrrNo,
                    MrrDate = l.MrrDate,
                    Remarks = l.Remarks,
                })
                .ToList(),
        };

        await PopulateAsync(model, cancellationToken);
        return View("Form", model);
    }

    /// <summary>Updates a bill and replaces its invoices.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.Accounts.Edit)]
    public async Task<IActionResult> Edit(int id, BillFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            await PopulateAsync(model, cancellationToken);
            return View("Form", model);
        }

        var result = await _bills.UpdateAsync(id, ToDto(model), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateAsync(model, cancellationToken);
            return View("Form", model);
        }

        TempData["Success"] = "Data updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Deletes a bill.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.Accounts.Delete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _bills.DeleteAsync(id, cancellationToken);

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

    private static SaveBillDto ToDto(BillFormViewModel model) => new(
        model.IdentificationDate,
        model.Kind,
        model.Currency,
        model.Lines
            .Select(l => new SaveBillLineDto(
                l.AccountsHeadId, l.PartyId, l.BillNo, l.BillDate, l.Amount, l.MrrNo, l.MrrDate, l.Remarks))
            .ToList());

    private async Task PopulateAsync(BillFormViewModel model, CancellationToken cancellationToken)
    {
        model.Parties = await _parties.ListAllAsync(cancellationToken);
        model.AccountsHeads = await _heads.ListAllAsync(cancellationToken);
    }
}
