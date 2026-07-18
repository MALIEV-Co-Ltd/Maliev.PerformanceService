namespace Maliev.PerformanceService.Tests.Workflows;

using System;
using System.IO;

using Xunit;

public sealed class WorkflowContractTests
{
    private static readonly string Root = FindRoot();
    private static readonly string Workflows = Path.Combine(Root, ".github", "workflows");

    [Fact]
    public void PullRequests_UseReadOnlyReusableValidation()
    {
        var text = Read("pr-validation.yml");

        Assert.Contains("pull_request:", text);
        Assert.Contains("contents: read", text);
        Assert.Contains("uses: ./.github/workflows/_validate.yml", text);
        Assert.DoesNotContain("paths:", text);
        AssertSafe(text);
    }

    [Theory]
    [InlineData("ci-main.yml", "main")]
    [InlineData("ci-develop.yml", "develop")]
    [InlineData("ci-staging.yml", "release/v*")]
    public void BranchAndTagWorkflows_AreValidationOnly(string file, string trigger)
    {
        var text = Read(file);

        Assert.Contains(trigger, text);
        Assert.Contains("uses: ./.github/workflows/_validate.yml", text);
        AssertSafe(text);
    }

    [Fact]
    public void ReusableValidation_IsCredentialFreeAndUsesImmutablePublicSources()
    {
        var text = Read("_validate.yml");

        Assert.Contains("workflow_call:", text);
        Assert.Contains("name: validate", text);
        Assert.Contains("actions/checkout@9c091bb21b7c1c1d1991bb908d89e4e9dddfe3e0", text);
        Assert.Contains("actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68", text);
        Assert.Contains("repository: MALIEV-Co-Ltd/Maliev.Aspire", text);
        Assert.Contains("ref: 01d506203763b914e237268a8746f1406423df86", text);
        Assert.Contains("repository: MALIEV-Co-Ltd/Maliev.MessagingContracts", text);
        Assert.Contains("ref: 559a00db0c7920a5247fdff60d4476ad23a9a501", text);
        Assert.Equal(3, text.Split("/p:GITHUB_ACTIONS=false", StringSplitOptions.None).Length - 1);
        AssertSafe(text);
    }

    [Fact]
    public void ProjectGraph_DoesNotRequireEmployeeServiceOrPrivatePackagesForValidation()
    {
        var api = File.ReadAllText(Path.Combine(Root, "Maliev.PerformanceService.Api", "Maliev.PerformanceService.Api.csproj"));
        var infrastructure = File.ReadAllText(Path.Combine(Root, "Maliev.PerformanceService.Infrastructure", "Maliev.PerformanceService.Infrastructure.csproj"));

        Assert.Contains("$(SharedSourceRoot)", api);
        Assert.Contains("$(SharedSourceRoot)", infrastructure);
        Assert.DoesNotContain("Maliev.EmployeeService", infrastructure);
    }

    [Fact]
    public void AllWorkflows_ForbidSecretsAndDeployment()
    {
        foreach (var file in Directory.GetFiles(Workflows, "*.yml"))
        {
            AssertSafe(File.ReadAllText(file));
        }
    }

    [Fact]
    public void IntegrationTests_SharingProcessEnvironment_AreSerialized()
    {
        var integrationRoot = Path.Combine(Root, "Maliev.PerformanceService.Tests", "Integration");
        var collection = File.ReadAllText(Path.Combine(integrationRoot, "IntegrationTestCollection.cs"));

        Assert.Contains("DisableParallelization = true", collection);
        foreach (var file in new[]
        {
            "FeedbackControllerTests.cs", "GoalsControllerTests.cs",
            "PerformanceReviewsControllerTests.cs", "PIPsControllerTests.cs",
        })
        {
            Assert.Contains("[Collection(IntegrationTestCollection.Name)]", File.ReadAllText(Path.Combine(integrationRoot, file)));
        }
    }

    private static void AssertSafe(string text)
    {
        foreach (var value in new[]
        {
            "secrets.", "GITOPS_PAT", "GCP_SA_KEY", "NUGET_PASSWORD", "id-token: write",
            "credentials_json", "google-github-actions/auth", "gcloud auth", "docker push",
            "maliev-gitops", "kustomize edit", "gh pr create", "pull_request_target",
        })
        {
            Assert.DoesNotContain(value, text, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string Read(string file)
    {
        var path = Path.Combine(Workflows, file);
        Assert.True(File.Exists(path), $"Required workflow is missing: {file}");
        return File.ReadAllText(path);
    }

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Maliev.PerformanceService.sln"))) return directory.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate PerformanceService repository root.");
    }
}
