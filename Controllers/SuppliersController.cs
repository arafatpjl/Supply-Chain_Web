using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Authorization;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// Suppliers. Replaces <c>frmNewSupplier.frm</c>.
/// </summary>
/// <remarks>
/// The clearest single illustration of the migration: saving one supplier in the VB6 application
/// executed seventeen hardcoded INSERT statements, one per company database, untransacted.
/// </remarks>
[Authorize]
public class SuppliersController : Controller
{
    private readonly ISupplierService _suppliers;
    private readonly ICountryService _countries;

    /// <summary>Initialises a new instance.</summary>
    public SuppliersController(ISupplierService suppliers, ICountryService countries)
    {
        _suppliers = suppliers;
        _countries = countries;
    }

    /// <summary>Lists suppliers, filtered by name and country, paged.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MasterData.View)]
    public async Task<IActionResult> Index(
        string? search,
        int? countryId,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        return View(new SupplierIndexViewModel
        {
            Results = await _suppliers.SearchAsync(search, countryId, page, 25, cancellationToken),
            SearchTerm = search,
            CountryId = countryId,
            Countries = await _countries.ListAllAsync(cancellationToken),
        });
    }

    /// <summary>Shows the create form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MasterData.Create)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return View("Form", new SupplierFormViewModel
        {
            Countries = await _countries.ListAllAsync(cancellationToken),
        });
    }

    /// <summary>Creates a supplier.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.MasterData.Create)]
    public async Task<IActionResult> Create(SupplierFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await RedisplayAsync(model, cancellationToken);
        }

        var result = await _suppliers.CreateAsync(
            new SaveSupplierDto(model.Name, model.Address, model.CountryId), cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return await RedisplayAsync(model, cancellationToken);
        }

        TempData["Success"] = $"Supplier '{result.Value.Name}' was saved.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Shows the edit form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MasterData.Edit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await _suppliers.GetAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View("Form", new SupplierFormViewModel
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            Address = result.Value.Address,
            CountryId = result.Value.CountryId,
            Countries = await _countries.ListAllAsync(cancellationToken),
        });
    }

    /// <summary>Updates a supplier.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.MasterData.Edit)]
    public async Task<IActionResult> Edit(int id, SupplierFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            return await RedisplayAsync(model, cancellationToken);
        }

        var result = await _suppliers.UpdateAsync(
            id, new SaveSupplierDto(model.Name, model.Address, model.CountryId), cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return await RedisplayAsync(model, cancellationToken);
        }

        TempData["Success"] = $"Supplier '{result.Value.Name}' was updated.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Deletes a supplier, provided nothing still references it.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.MasterData.Delete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _suppliers.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Supplier was deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Repopulates the drop-down before redisplaying a form that failed validation.</summary>
    private async Task<IActionResult> RedisplayAsync(SupplierFormViewModel model, CancellationToken cancellationToken)
    {
        model.Countries = await _countries.ListAllAsync(cancellationToken);
        return View("Form", model);
    }
}
