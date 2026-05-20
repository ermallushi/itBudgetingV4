using Microsoft.EntityFrameworkCore;
using ItBudgetingV4.Web.Domain;

namespace ItBudgetingV4.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<BudgetVersion> BudgetVersions => Set<BudgetVersion>();
    public DbSet<BudgetItem> BudgetItems => Set<BudgetItem>();
    public DbSet<BudgetMonthlyValue> BudgetMonthlyValues => Set<BudgetMonthlyValue>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BudgetVersion>()
            .HasOne(v => v.ParentVersion)
            .WithMany(v => v.Revisions)
            .HasForeignKey(v => v.ParentVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BudgetVersion>()
            .HasIndex(v => new { v.Year, v.Status });

        modelBuilder.Entity<BudgetMonthlyValue>()
            .HasIndex(x => new { x.BudgetItemId, x.Year, x.Month })
            .IsUnique();

        modelBuilder.Entity<BudgetMonthlyValue>()
            .ToTable(x => x.HasCheckConstraint("CK_BudgetMonthlyValue_Month", "[Month] >= 1 AND [Month] <= 12"));

        modelBuilder.Entity<BudgetItem>()
            .HasMany(x => x.MonthlyValues)
            .WithOne(x => x.BudgetItem)
            .HasForeignKey(x => x.BudgetItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BudgetItem>()
            .HasMany(x => x.FinancialTransactions)
            .WithOne(x => x.BudgetItem)
            .HasForeignKey(x => x.BudgetItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
