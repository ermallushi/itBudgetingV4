using System.ComponentModel.DataAnnotations;

namespace ItBudgetingV4.Web.Domain;

public class BudgetVersion
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string Name { get; set; } = "0+12";

    public int Year { get; set; }

    public BudgetVersionStatus Status { get; set; } = BudgetVersionStatus.Draft;

    public int? ParentVersionId { get; set; }
    public BudgetVersion? ParentVersion { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BudgetVersion> Revisions { get; set; } = new List<BudgetVersion>();
    public ICollection<BudgetItem> BudgetItems { get; set; } = new List<BudgetItem>();
}
