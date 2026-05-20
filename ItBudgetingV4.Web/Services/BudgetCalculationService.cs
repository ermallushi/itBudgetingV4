using ItBudgetingV4.Web.Domain;

namespace ItBudgetingV4.Web.Services;

public static class BudgetCalculationService
{
    public static decimal GetPlanned(BudgetItem item) => item.MonthlyValues.Sum(x => x.PlannedAmount);

    public static decimal GetCommitted(BudgetItem item) => item.FinancialTransactions.Sum(x => x.Amount);

    public static decimal GetRemaining(BudgetItem item) => GetPlanned(item) - GetCommitted(item);

    public static int GetLockedMonthCount(string versionName)
    {
        var parts = versionName.Split('+', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 && int.TryParse(parts[0], out var locked) ? Math.Clamp(locked, 0, 12) : 0;
    }
}
