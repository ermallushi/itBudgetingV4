using ItBudgetingV4.Web.Data;
using ItBudgetingV4.Web.Domain;
using ItBudgetingV4.Web.Services;
using ItBudgetingV4.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ItBudgetingV4.Web.Controllers;

[Authorize]
public class ImportController(ApplicationDbContext dbContext, IBudgetImportService budgetImportService, ICurrentUserService currentUserService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Versions = new SelectList(await dbContext.BudgetVersions.Where(v => v.Status == BudgetVersionStatus.Draft).OrderByDescending(v => v.CreatedAt).ToListAsync(cancellationToken), nameof(BudgetVersion.Id), nameof(BudgetVersion.Name));
        return View(new ImportResultViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "CIO Secretary")]
    public async Task<IActionResult> Upload(IFormFile file, int versionId, BudgetCategory category, CancellationToken cancellationToken)
    {
        var (imported, errors) = await budgetImportService.ImportAsync(file, versionId, category, currentUserService.GetUsername(User), cancellationToken);
        ViewBag.Versions = new SelectList(await dbContext.BudgetVersions.Where(v => v.Status == BudgetVersionStatus.Draft).OrderByDescending(v => v.CreatedAt).ToListAsync(cancellationToken), nameof(BudgetVersion.Id), nameof(BudgetVersion.Name));
        return View("Index", new ImportResultViewModel
        {
            Category = category,
            ImportedCount = imported,
            Errors = errors
        });
    }
}
