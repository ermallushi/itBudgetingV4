namespace ItBudgetingV4.Web.ViewModels;

public class BudgetGridRowViewModel
{
    public int BudgetItemId { get; set; }
    public string Wbs { get; set; } = string.Empty;
    public string SapCode { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Planned { get; set; }
    public decimal Transactions { get; set; }
    public decimal Remaining { get; set; }
    public Dictionary<int, decimal> MonthlyValues { get; set; } = new();
    public HashSet<int> LockedMonths { get; set; } = new();
}
