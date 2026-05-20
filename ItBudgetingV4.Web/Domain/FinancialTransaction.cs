using System.ComponentModel.DataAnnotations;

namespace ItBudgetingV4.Web.Domain;

public class FinancialTransaction
{
    public int Id { get; set; }

    public int BudgetItemId { get; set; }
    public BudgetItem BudgetItem { get; set; } = null!;

    public FinancialTransactionType Type { get; set; }

    [MaxLength(100)]
    public string ReferenceNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    [MaxLength(200)]
    public string Vendor { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Link { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Open";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
