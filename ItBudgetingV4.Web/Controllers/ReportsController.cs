using ItBudgetingV4.Web.Data;
using ItBudgetingV4.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItBudgetingV4.Web.Controllers;

[Authorize]
public class ReportsController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var data = await dbContext.BudgetItems
            .Include(x => x.MonthlyValues)
            .Include(x => x.FinancialTransactions)
            .Select(x => new ReportRowViewModel
            {
                Project = x.Project,
                Category = x.Category.ToString(),
                Planned = x.MonthlyValues.Sum(m => m.PlannedAmount),
                Actual = x.FinancialTransactions.Sum(t => t.Amount),
                Remaining = x.MonthlyValues.Sum(m => m.PlannedAmount) - x.FinancialTransactions.Sum(t => t.Amount)
            })
            .OrderBy(x => x.Project)
            .ToListAsync(cancellationToken);

        return View(data);
    }
}
