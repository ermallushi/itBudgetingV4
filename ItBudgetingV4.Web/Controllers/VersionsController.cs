using ItBudgetingV4.Web.Data;
using ItBudgetingV4.Web.Domain;
using ItBudgetingV4.Web.Services;
using ItBudgetingV4.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItBudgetingV4.Web.Controllers;

[Authorize]
public class VersionsController(ApplicationDbContext dbContext, IBudgetVersionService budgetVersionService, ICurrentUserService currentUserService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new VersionIndexViewModel
        {
            Versions = await dbContext.BudgetVersions.OrderByDescending(v => v.Year).ThenByDescending(v => v.CreatedAt).ToListAsync(cancellationToken),
            CurrentYear = DateTime.UtcNow.Year
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "CIO Secretary")]
    public async Task<IActionResult> EnsureDraft(int year, CancellationToken cancellationToken)
    {
        await budgetVersionService.EnsureDraftExistsAsync(year, currentUserService.GetUsername(User), cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "CIO")]
    public async Task<IActionResult> FinalizeVersion(int versionId, CancellationToken cancellationToken)
    {
        await budgetVersionService.FinalizeAsync(versionId, currentUserService.GetUsername(User), cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "CIO Secretary")]
    public async Task<IActionResult> CloneRevision(int sourceFinalVersionId, string revisionName, CancellationToken cancellationToken)
    {
        await budgetVersionService.CreateDraftRevisionFromFinalAsync(sourceFinalVersionId, revisionName, currentUserService.GetUsername(User), cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
