using ItBudgetingV4.Web.Data;
using ItBudgetingV4.Web.Domain;

namespace ItBudgetingV4.Web.Services;

public interface IAuditLogService
{
    Task LogAsync(string entityType, string entityId, string fieldName, string? oldValue, string? newValue, string changedBy, CancellationToken cancellationToken = default);
}

public class AuditLogService(ApplicationDbContext dbContext) : IAuditLogService
{
    public async Task LogAsync(string entityType, string entityId, string fieldName, string? oldValue, string? newValue, string changedBy, CancellationToken cancellationToken = default)
    {
        dbContext.AuditLogs.Add(new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
