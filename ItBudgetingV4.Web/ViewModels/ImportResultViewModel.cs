using ItBudgetingV4.Web.Domain;

namespace ItBudgetingV4.Web.ViewModels;

public class ImportResultViewModel
{
    public BudgetCategory Category { get; set; }
    public int ImportedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
