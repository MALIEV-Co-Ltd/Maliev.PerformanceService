using Maliev.PerformanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.PerformanceService.Infrastructure.Data;

/// <summary>
/// Database context for the Performance Management Service.
/// </summary>
public class PerformanceDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceDbContext"/> class.
    /// </summary>
    public PerformanceDbContext(DbContextOptions<PerformanceDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the performance reviews table.
    /// </summary>
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();

    /// <summary>
    /// Gets or sets the performance goals table.
    /// </summary>
    public DbSet<Goal> Goals => Set<Goal>();

    /// <summary>
    /// Gets or sets the review feedback table.
    /// </summary>
    public DbSet<ReviewFeedback> ReviewFeedback => Set<ReviewFeedback>();

    /// <summary>
    /// Gets or sets the performance improvement plans table.
    /// </summary>
    public DbSet<PerformanceImprovementPlan> PerformanceImprovementPlans => Set<PerformanceImprovementPlan>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PerformanceDbContext).Assembly);

        // Apply snake_case naming convention
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Replace table names
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

            // Replace column names
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            // Replace keys and indexes
            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName()!));
            }

            foreach (var key in entity.GetForeignKeys())
            {
                key.SetConstraintName(ToSnakeCase(key.GetConstraintName()!));
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var startUnderscore = input.StartsWith("_");
        var res = System.Text.RegularExpressions.Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        return startUnderscore ? "_" + res : res;
    }
}