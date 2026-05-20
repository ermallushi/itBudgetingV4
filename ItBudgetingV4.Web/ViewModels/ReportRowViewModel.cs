namespace ItBudgetingV4.Web.ViewModels;

public class ReportRowViewModel
{
    public string Project { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Planned { get; set; }
    public decimal Actual { get; set; }
    public decimal Remaining { get; set; }
}
