using EternalCycle.Rules;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class PortableRulesContractBoundaryTests
{
    [Fact]
    public void PortableContractHasNoManagedRuntimeDependency()
    {
        var assembly = typeof(CompiledRulesArtifactContract).Assembly;
        var references = assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal("EternalCycle.Rules", assembly.GetName().Name);
        Assert.DoesNotContain("EternalCycle.Persistence.Mcp", references);
        Assert.DoesNotContain("Microsoft.Data.SqlClient", references);
        Assert.DoesNotContain("Microsoft.Extensions.Hosting", references);
        Assert.DoesNotContain("ModelContextProtocol", references);
    }
}
