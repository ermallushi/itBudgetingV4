using ItBudgetingV4.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItBudgetingV4.Web.Controllers;

[Authorize]
public class AuditLogsController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var logs = await dbContext.AuditLogs.OrderByDescending(x => x.ChangedAt).Take(500).ToListAsync(cancellationToken);
        return View(logs);
    }
}
