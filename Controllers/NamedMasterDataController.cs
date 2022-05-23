using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChain.Application.MasterData;
using SupplyChain.Application.Authorization;
using SupplyChain.Web.ViewModels;

namespace SupplyChain.Web.Controllers;

/// <summary>
/// Shared CRUD controller for master-data entities identified by a unique name.
/// </summary>
/// <remarks>
/// <para>
/// Four legacy forms — <c>frmNewCountry</c>, <c>frmNewPFNo</c>, <c>frmParty</c> and
/// <c>frmNewAccountsHead</c> — reduce to one controller and one set of Razor views. Each
/// subclass supplies only its service and its labels.
/// </para>
/// <para>
/// Actions stay thin: bind, call the service, translate the result. All rules live in the
/// Application layer, which is what makes them testable without a browser.
/// </para>
/// </remarks>
[Authorize]
public abstract class NamedMasterDataController : Controller
{
    private const string IndexView = "~/Views/Shared/MasterData/Index.cshtml";
    private const string FormView = "~/Views/Shared/MasterData/Form.cshtml";

    private readonly INamedMasterDataService _service;

    /// <summary>Initialises a new instance.</summary>
    protected NamedMasterDataController(INamedMasterDataService service) => _service = service;

    /// <summary>Labels and route information for this screen.</summary>
    protected abstract MasterDataDescriptor Descriptor { get; }

    /// <summary>Lists records, filtered and paged.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MasterData.View)]
    public async Task<IActionResult> Index(
        string? search,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var results = await _service.SearchAsync(search, page, pageSize: 25, cancellationToken);

        return View(IndexView, new NamedMasterDataIndexViewModel
        {
            Descriptor = Descriptor,
            Results = results,
            SearchTerm = search,
        });
    }

    /// <summary>Shows the create form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MasterData.Create)]
    public IActionResult Create() =>
        View(FormView, new NamedMasterDataFormViewModel { Descriptor = Descriptor });

    /// <summary>Creates a record.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.MasterData.Create)]
    public async Task<IActionResult> Create(
        NamedMasterDataFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Descriptor = Descriptor;

        var result = await _service.CreateAsync(new SaveNamedItemDto(model.Name), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return View(FormView, model);
        }

        TempData["Success"] = $"{Descriptor.SingularTitle} '{result.Value.Name}' was saved.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Shows the edit form.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MasterData.Edit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View(FormView, new NamedMasterDataFormViewModel
        {
            Descriptor = Descriptor,
            Id = result.Value.Id,
            Name = result.Value.Name,
        });
    }

    /// <summary>Updates a record.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.MasterData.Edit)]
    public async Task<IActionResult> Edit(
        int id,
        NamedMasterDataFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Descriptor = Descriptor;
        model.Id = id;

        var result = await _service.UpdateAsync(id, new SaveNamedItemDto(model.Name), cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return View(FormView, model);
        }

        TempData["Success"] = $"{Descriptor.SingularTitle} '{result.Value.Name}' was updated.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Deletes a record, provided nothing still references it.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.MasterData.Delete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = $"{Descriptor.SingularTitle} was deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}
