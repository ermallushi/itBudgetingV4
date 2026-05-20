using ItBudgetingV4.Web.Domain;

namespace ItBudgetingV4.Web.ViewModels;

public class VersionIndexViewModel
{
    public List<BudgetVersion> Versions { get; set; } = new();
    public string RevisionName { get; set; } = "3+9";
    public int SourceFinalVersionId { get; set; }
    public int CurrentYear { get; set; } = DateTime.UtcNow.Year;
}
