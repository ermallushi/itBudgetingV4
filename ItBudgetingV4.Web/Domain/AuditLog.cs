using System.ComponentModel.DataAnnotations;

namespace ItBudgetingV4.Web.Domain;

public class AuditLog
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    [MaxLength(200)]
    public string ChangedBy { get; set; } = "system";

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
