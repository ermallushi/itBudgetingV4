using ClosedXML.Excel;
using ItBudgetingV4.Web.Data;
using ItBudgetingV4.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace ItBudgetingV4.Web.Services;

public interface IBudgetImportService
{
    Task<(int imported, List<string> errors)> ImportAsync(IFormFile file, int versionId, BudgetCategory category, string changedBy, CancellationToken cancellationToken = default);
}

public class BudgetImportService(ApplicationDbContext dbContext, IAuditLogService auditLogService) : IBudgetImportService
{
    private const string MustContractType = "Must";
    private static readonly string[] RequiredHeaders =
    [
        "WBS", "SAP Code", "SAP Description", "IT Domain", "IT Subdomain", "Commitment Code", "Hyperion Category", "Hyperion Code",
        "Hyperion Subcategory", "Project", "Vendor", "Requestor", "EPMO ID", "LRP Category", "Cost Center", "Fund Code", "Contract Type"
    ];

    public async Task<(int imported, List<string> errors)> ImportAsync(IFormFile file, int versionId, BudgetCategory category, string changedBy, CancellationToken cancellationToken = default)
    {
        var version = await dbContext.BudgetVersions.FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);
        if (version is null)
        {
            return (0, ["Budget version was not found."]);
        }

        if (version.Status != BudgetVersionStatus.Draft)
        {
            return (0, ["Only Draft versions can be imported."]);
        }

        if (file.Length == 0)
        {
            return (0, ["Uploaded file is empty."]);
        }

        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.First();

        var errors = new List<string>();
        var headers = sheet.Row(1).Cells().Select(c => c.GetString().Trim()).ToArray();
        foreach (var requiredHeader in RequiredHeaders)
        {
            if (!headers.Contains(requiredHeader, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"Missing required column: {requiredHeader}");
            }
        }

        if (errors.Count > 0)
        {
            return (0, errors);
        }

        var lockedMonths = version.Name.Split('+').FirstOrDefault() is { } part && int.TryParse(part, out var lockCount) ? Math.Clamp(lockCount, 0, 12) : 0;

        var imported = 0;
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var row = sheet.Row(rowNumber);
            if (row.Cell(1).IsEmpty() && row.Cell(2).IsEmpty())
            {
                continue;
            }

            var sapCode = row.Cell("B").GetString().Trim();
            var costCenter = row.Cell("O").GetString().Trim();
            if (string.IsNullOrWhiteSpace(sapCode))
            {
                errors.Add($"Row {rowNumber}: SAP Code is required.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(costCenter))
            {
                errors.Add($"Row {rowNumber}: Cost Center is required.");
                continue;
            }

            var item = new BudgetItem
            {
                BudgetVersionId = versionId,
                Wbs = row.Cell("A").GetString().Trim(),
                SapCode = sapCode,
                SapDescription = row.Cell("C").GetString().Trim(),
                ItDomain = row.Cell("D").GetString().Trim(),
                ItSubdomain = row.Cell("E").GetString().Trim(),
                CommitmentCode = row.Cell("F").GetString().Trim(),
                HyperionCategory = row.Cell("G").GetString().Trim(),
                HyperionCode = row.Cell("H").GetString().Trim(),
                HyperionSubcategory = row.Cell("I").GetString().Trim(),
                Project = row.Cell("J").GetString().Trim(),
                Vendor = row.Cell("K").GetString().Trim(),
                Requestor = row.Cell("L").GetString().Trim(),
                EpmoId = row.Cell("M").GetString().Trim(),
                LrpCategory = row.Cell("N").GetString().Trim(),
                CostCenter = costCenter,
                FundCode = row.Cell("P").GetString().Trim(),
                ContractType = ParseContractType(row.Cell("Q").GetString().Trim()),
                Category = category
            };

            for (var month = 1; month <= 12; month++)
            {
                var monthColumn = 17 + month;
                var value = row.Cell(monthColumn).TryGetValue<decimal>(out var amount) ? amount : 0m;
                if (value < 0)
                {
                    errors.Add($"Row {rowNumber}: Month {month} has invalid negative value.");
                    continue;
                }

                item.MonthlyValues.Add(new BudgetMonthlyValue
                {
                    Year = version.Year,
                    Month = month,
                    PlannedAmount = value,
                    IsLocked = month <= lockedMonths
                });
            }

            dbContext.BudgetItems.Add(item);
            imported++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogService.LogAsync("BudgetImport", versionId.ToString(), "Import", null, $"Imported {imported} rows for {category}", changedBy, cancellationToken);
        return (imported, errors);
    }

    private static ContractType ParseContractType(string value) =>
        string.Equals(value, MustContractType, StringComparison.OrdinalIgnoreCase) ? ContractType.Must : ContractType.Optional;
}
