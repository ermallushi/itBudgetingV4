namespace ItBudgetingV4.Web.Domain;

public class BudgetMonthlyValue
{
    public int Id { get; set; }

    public int BudgetItemId { get; set; }
    public BudgetItem BudgetItem { get; set; } = null!;

    public int Year { get; set; }
    public int Month { get; set; }
    public decimal PlannedAmount { get; set; }

    public bool IsLocked { get; set; }
}
