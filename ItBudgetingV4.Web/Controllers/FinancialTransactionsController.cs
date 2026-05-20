using ItBudgetingV4.Web.Data;
using ItBudgetingV4.Web.Domain;
using ItBudgetingV4.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ItBudgetingV4.Web.Controllers;

[Authorize]
public class FinancialTransactionsController(ApplicationDbContext dbContext, IAuditLogService auditLogService, ICurrentUserService currentUserService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.BudgetItems = new SelectList(await dbContext.BudgetItems.OrderBy(x => x.Project).ToListAsync(cancellationToken), nameof(BudgetItem.Id), nameof(BudgetItem.Project));
        var items = await dbContext.FinancialTransactions.Include(x => x.BudgetItem).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        return View(items);
    }

    [HttpPost]
    [Authorize(Roles = "CIO Secretary")]
    public async Task<IActionResult> Create(FinancialTransaction transaction, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index));
        }

        transaction.CreatedAt = DateTime.UtcNow;
        dbContext.FinancialTransactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogService.LogAsync("FinancialTransaction", transaction.Id.ToString(), "Create", null, transaction.Type.ToString(), currentUserService.GetUsername(User), cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
