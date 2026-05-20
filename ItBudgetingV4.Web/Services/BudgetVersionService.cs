using ItBudgetingV4.Web.Data;
using ItBudgetingV4.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace ItBudgetingV4.Web.Services;

public interface IBudgetVersionService
{
    Task<BudgetVersion> EnsureDraftExistsAsync(int year, string changedBy, CancellationToken cancellationToken = default);
    Task<BudgetVersion> CreateDraftRevisionFromFinalAsync(int finalVersionId, string revisionName, string changedBy, CancellationToken cancellationToken = default);
    Task FinalizeAsync(int versionId, string changedBy, CancellationToken cancellationToken = default);
}

public class BudgetVersionService(ApplicationDbContext dbContext, IAuditLogService auditLogService) : IBudgetVersionService
{
    public async Task<BudgetVersion> EnsureDraftExistsAsync(int year, string changedBy, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.BudgetVersions.FirstOrDefaultAsync(v => v.Year == year && v.Status == BudgetVersionStatus.Draft, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var version = new BudgetVersion
        {
            Year = year,
            Name = "0+12",
            Status = BudgetVersionStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.BudgetVersions.Add(version);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogService.LogAsync("BudgetVersion", version.Id.ToString(), "Create", null, version.Name, changedBy, cancellationToken);
        return version;
    }

    public async Task FinalizeAsync(int versionId, string changedBy, CancellationToken cancellationToken = default)
    {
        var version = await dbContext.BudgetVersions.FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken)
            ?? throw new InvalidOperationException("Version not found.");

        if (version.Status == BudgetVersionStatus.Final)
        {
            return;
        }

        version.Status = BudgetVersionStatus.Final;
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogService.LogAsync("BudgetVersion", version.Id.ToString(), "Status", BudgetVersionStatus.Draft.ToString(), BudgetVersionStatus.Final.ToString(), changedBy, cancellationToken);
    }

    public async Task<BudgetVersion> CreateDraftRevisionFromFinalAsync(int finalVersionId, string revisionName, string changedBy, CancellationToken cancellationToken = default)
    {
        var sourceVersion = await dbContext.BudgetVersions
            .Include(v => v.BudgetItems)
                .ThenInclude(i => i.MonthlyValues)
            .Include(v => v.BudgetItems)
                .ThenInclude(i => i.FinancialTransactions)
            .FirstOrDefaultAsync(v => v.Id == finalVersionId, cancellationToken)
            ?? throw new InvalidOperationException("Source version not found.");

        if (sourceVersion.Status != BudgetVersionStatus.Final)
        {
            throw new InvalidOperationException("Revisions can only be created from Final versions.");
        }

        var existingDraft = await dbContext.BudgetVersions.AnyAsync(v => v.Year == sourceVersion.Year && v.Status == BudgetVersionStatus.Draft, cancellationToken);
        if (existingDraft)
        {
            throw new InvalidOperationException("Only one Draft version is allowed per year.");
        }

        var newVersion = new BudgetVersion
        {
            Name = revisionName,
            Year = sourceVersion.Year,
            Status = BudgetVersionStatus.Draft,
            ParentVersionId = sourceVersion.Id,
            CreatedAt = DateTime.UtcNow
        };

        var lockedMonths = BudgetCalculationService.GetLockedMonthCount(revisionName);

        foreach (var sourceItem in sourceVersion.BudgetItems)
        {
            var newItem = new BudgetItem
            {
                Wbs = sourceItem.Wbs,
                SapCode = sourceItem.SapCode,
                SapDescription = sourceItem.SapDescription,
                ItDomain = sourceItem.ItDomain,
                ItSubdomain = sourceItem.ItSubdomain,
                CommitmentCode = sourceItem.CommitmentCode,
                HyperionCategory = sourceItem.HyperionCategory,
                HyperionCode = sourceItem.HyperionCode,
                HyperionSubcategory = sourceItem.HyperionSubcategory,
                Project = sourceItem.Project,
                Vendor = sourceItem.Vendor,
                Requestor = sourceItem.Requestor,
                EpmoId = sourceItem.EpmoId,
                LrpCategory = sourceItem.LrpCategory,
                CostCenter = sourceItem.CostCenter,
                FundCode = sourceItem.FundCode,
                ContractType = sourceItem.ContractType,
                Category = sourceItem.Category
            };

            foreach (var month in sourceItem.MonthlyValues)
            {
                newItem.MonthlyValues.Add(new BudgetMonthlyValue
                {
                    Year = month.Year,
                    Month = month.Month,
                    PlannedAmount = month.PlannedAmount,
                    IsLocked = month.Month <= lockedMonths
                });
            }

            newVersion.BudgetItems.Add(newItem);
        }

        dbContext.BudgetVersions.Add(newVersion);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogService.LogAsync("BudgetVersion", newVersion.Id.ToString(), "Clone", sourceVersion.Id.ToString(), revisionName, changedBy, cancellationToken);
        return newVersion;
    }
}
