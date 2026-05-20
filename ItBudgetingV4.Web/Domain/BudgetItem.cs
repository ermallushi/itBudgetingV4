using System.ComponentModel.DataAnnotations;

namespace ItBudgetingV4.Web.Domain;

public class BudgetItem
{
    public int Id { get; set; }

    public int BudgetVersionId { get; set; }
    public BudgetVersion BudgetVersion { get; set; } = null!;

    [MaxLength(100)] public string Wbs { get; set; } = string.Empty;
    [MaxLength(100)] public string SapCode { get; set; } = string.Empty;
    [MaxLength(200)] public string SapDescription { get; set; } = string.Empty;
    [MaxLength(100)] public string ItDomain { get; set; } = string.Empty;
    [MaxLength(100)] public string ItSubdomain { get; set; } = string.Empty;
    [MaxLength(100)] public string CommitmentCode { get; set; } = string.Empty;
    [MaxLength(100)] public string HyperionCategory { get; set; } = string.Empty;
    [MaxLength(100)] public string HyperionCode { get; set; } = string.Empty;
    [MaxLength(100)] public string HyperionSubcategory { get; set; } = string.Empty;
    [MaxLength(200)] public string Project { get; set; } = string.Empty;
    [MaxLength(200)] public string Vendor { get; set; } = string.Empty;
    [MaxLength(200)] public string Requestor { get; set; } = string.Empty;
    [MaxLength(100)] public string EpmoId { get; set; } = string.Empty;
    [MaxLength(100)] public string LrpCategory { get; set; } = string.Empty;
    [MaxLength(100)] public string CostCenter { get; set; } = string.Empty;
    [MaxLength(100)] public string FundCode { get; set; } = string.Empty;

    public ContractType ContractType { get; set; }
    public BudgetCategory Category { get; set; }

    public ICollection<BudgetMonthlyValue> MonthlyValues { get; set; } = new List<BudgetMonthlyValue>();
    public ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();
}
