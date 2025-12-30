using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.PerformanceService.Infrastructure.Data;

/// <summary>
/// Design-time factory for the <see cref="PerformanceDbContext"/>.
/// </summary>
public class PerformanceDbContextFactory : IDesignTimeDbContextFactory<PerformanceDbContext>
{
    /// <inheritdoc/>
    public PerformanceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PerformanceDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=performance_db;Username=postgres;Password=postgres");

        return new PerformanceDbContext(optionsBuilder.Options);
    }
}
