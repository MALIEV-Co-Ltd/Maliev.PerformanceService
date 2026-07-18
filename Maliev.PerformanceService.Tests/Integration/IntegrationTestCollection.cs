namespace Maliev.PerformanceService.Tests.Integration;

using Xunit;

/// <summary>
/// Serializes integration fixtures because they configure process-wide connection-string environment variables.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class IntegrationTestCollection
{
    /// <summary>
    /// Shared xUnit collection name for container-backed integration tests.
    /// </summary>
    public const string Name = "PerformanceService integration tests";
}
