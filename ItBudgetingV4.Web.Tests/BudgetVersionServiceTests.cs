using ItBudgetingV4.Web.Data;
using ItBudgetingV4.Web.Domain;
using ItBudgetingV4.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace ItBudgetingV4.Web.Tests;

public class BudgetVersionServiceTests
{
    private static ApplicationDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name)
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateDraftRevisionFromFinal_ClonesAndLocksHistoricalMonths()
    {
        await using var db = CreateDb(Guid.NewGuid().ToString());
        var finalVersion = new BudgetVersion { Name = "0+12", Year = 2026, Status = BudgetVersionStatus.Final };
        var item = new BudgetItem { Project = "Project A", SapCode = "SAP1", CostCenter = "CC1", Category = BudgetCategory.CapEx, ContractType = ContractType.Must };
        for (var month = 1; month <= 12; month++)
        {
            item.MonthlyValues.Add(new BudgetMonthlyValue { Year = 2026, Month = month, PlannedAmount = month * 100 });
        }

        finalVersion.BudgetItems.Add(item);
        db.BudgetVersions.Add(finalVersion);
        await db.SaveChangesAsync();

        var service = new BudgetVersionService(db, new AuditLogService(db));
        var cloned = await service.CreateDraftRevisionFromFinalAsync(finalVersion.Id, "3+9", "tester");

        Assert.Equal(BudgetVersionStatus.Draft, cloned.Status);
        Assert.Equal(finalVersion.Id, cloned.ParentVersionId);
        Assert.Single(cloned.BudgetItems);
        var monthlyValues = cloned.BudgetItems.Single().MonthlyValues;
        Assert.Equal(3, monthlyValues.Count(x => x.IsLocked));
        Assert.True(monthlyValues.Single(x => x.Month == 1).IsLocked);
        Assert.False(monthlyValues.Single(x => x.Month == 4).IsLocked);
    }

    [Fact]
    public void RemainingBudget_UsesPlannedMinusTransactions()
    {
        var item = new BudgetItem
        {
            MonthlyValues =
            [
                new BudgetMonthlyValue { Month = 1, Year = 2026, PlannedAmount = 100 },
                new BudgetMonthlyValue { Month = 2, Year = 2026, PlannedAmount = 50 }
            ],
            FinancialTransactions =
            [
                new FinancialTransaction { Amount = 60, Type = FinancialTransactionType.PR },
                new FinancialTransaction { Amount = 20, Type = FinancialTransactionType.PO }
            ]
        };

        Assert.Equal(70, BudgetCalculationService.GetRemaining(item));
    }

    [Fact]
    public async Task EnsureDraftExists_OnlyCreatesSingleDraftPerYear()
    {
        await using var db = CreateDb(Guid.NewGuid().ToString());
        var service = new BudgetVersionService(db, new AuditLogService(db));

        var first = await service.EnsureDraftExistsAsync(2026, "tester");
        var second = await service.EnsureDraftExistsAsync(2026, "tester");

        Assert.Equal(first.Id, second.Id);
        Assert.Single(db.BudgetVersions.Where(v => v.Year == 2026 && v.Status == BudgetVersionStatus.Draft));
    }
}
