using NetArchTest.Rules;

namespace MoveWell.ArchitectureTests;

/// <summary>Enforces the dependency direction from ADR-0001 and the foundation rule.</summary>
public class LayerDependencyTests
{
    private const string Application = "MoveWell.Application";
    private const string Infrastructure = "MoveWell.Infrastructure";
    private const string Api = "MoveWell.Api";
    private const string Web = "MoveWell.Web";

    [Fact]
    public void Domain_does_not_depend_on_outer_layers()
    {
        var result = Types.InAssembly(typeof(Domain.AssemblyReference).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(Application, Infrastructure, Api, Web, "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Application_does_not_depend_on_infrastructure_or_hosts()
    {
        var result = Types.InAssembly(typeof(Application.AssemblyReference).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(Infrastructure, Api, Web, "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Web_does_not_reference_domain_application_or_infrastructure()
    {
        var referenced = System.Reflection.Assembly.Load(Web).GetReferencedAssemblies().Select(a => a.Name).ToList();

        Assert.DoesNotContain("MoveWell.Domain", referenced);
        Assert.DoesNotContain(Application, referenced);
        Assert.DoesNotContain(Infrastructure, referenced);
    }

    private static string Describe(TestResult result) =>
        result.IsSuccessful ? string.Empty : "Violations: " + string.Join(", ", result.FailingTypeNames ?? []);
}
