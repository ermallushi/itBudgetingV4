using ItBudgetingV4.Web.Data;
using ItBudgetingV4.Web.Domain;
using ItBudgetingV4.Web.Services;
using ItBudgetingV4.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItBudgetingV4.Web.Controllers;

[Authorize]
public class DashboardController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await dbContext.BudgetItems
            .Include(x => x.MonthlyValues)
            .Include(x => x.FinancialTransactions)
            .ToListAsync(cancellationToken);

        var model = new DashboardViewModel
        {
            TotalBudget = items.Sum(BudgetCalculationService.GetPlanned),
            RemainingBudget = items.Sum(BudgetCalculationService.GetRemaining),
            CapExTotal = items.Where(x => x.Category == BudgetCategory.CapEx).Sum(BudgetCalculationService.GetPlanned),
            OpExTotal = items.Where(x => x.Category == BudgetCategory.OpEx).Sum(BudgetCalculationService.GetPlanned)
        };

        return View(model);
    }
}
