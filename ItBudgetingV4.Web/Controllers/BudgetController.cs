using ItBudgetingV4.Web.Data;
using ItBudgetingV4.Web.Domain;
using ItBudgetingV4.Web.Services;
using ItBudgetingV4.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItBudgetingV4.Web.Controllers;

[Authorize]
public class BudgetController(ApplicationDbContext dbContext, IAuditLogService auditLogService, ICurrentUserService currentUserService) : Controller
{
    public async Task<IActionResult> Index(int? versionId, CancellationToken cancellationToken)
    {
        var version = versionId.HasValue
            ? await dbContext.BudgetVersions.FirstOrDefaultAsync(x => x.Id == versionId, cancellationToken)
            : await dbContext.BudgetVersions.OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);

        if (version is null)
        {
            return View(new List<BudgetGridRowViewModel>());
        }

        ViewBag.Version = version;

        var rows = await dbContext.BudgetItems
            .Where(x => x.BudgetVersionId == version.Id)
            .Include(x => x.MonthlyValues)
            .Include(x => x.FinancialTransactions)
            .Select(x => new BudgetGridRowViewModel
            {
                BudgetItemId = x.Id,
                Wbs = x.Wbs,
                SapCode = x.SapCode,
                Project = x.Project,
                Category = x.Category.ToString(),
                Planned = x.MonthlyValues.Sum(m => m.PlannedAmount),
                Transactions = x.FinancialTransactions.Sum(t => t.Amount),
                Remaining = x.MonthlyValues.Sum(m => m.PlannedAmount) - x.FinancialTransactions.Sum(t => t.Amount),
                MonthlyValues = x.MonthlyValues.ToDictionary(m => m.Month, m => m.PlannedAmount),
                LockedMonths = x.MonthlyValues.Where(m => m.IsLocked).Select(m => m.Month).ToHashSet()
            })
            .ToListAsync(cancellationToken);

        return View(rows);
    }

    [HttpPost]
    [Authorize(Roles = "CIO Secretary")]
    public async Task<IActionResult> UpdateMonthlyValue(int budgetItemId, int month, decimal plannedAmount, CancellationToken cancellationToken)
    {
        if (month is < 1 or > 12)
        {
            return BadRequest("Invalid month");
        }

        var item = await dbContext.BudgetItems
            .Include(i => i.BudgetVersion)
            .Include(i => i.MonthlyValues)
            .FirstOrDefaultAsync(i => i.Id == budgetItemId, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        if (item.BudgetVersion.Status != BudgetVersionStatus.Draft)
        {
            return BadRequest("Final versions are immutable.");
        }

        var monthly = item.MonthlyValues.FirstOrDefault(m => m.Month == month && m.Year == item.BudgetVersion.Year)
            ?? throw new InvalidOperationException("Monthly value not found.");

        if (monthly.IsLocked)
        {
            return BadRequest("Locked month cannot be edited.");
        }

        var oldValue = monthly.PlannedAmount;
        monthly.PlannedAmount = plannedAmount;
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogService.LogAsync("BudgetMonthlyValue", monthly.Id.ToString(), nameof(monthly.PlannedAmount), oldValue.ToString(), plannedAmount.ToString(), currentUserService.GetUsername(User), cancellationToken);
        return Ok();
    }
}
