namespace Paraminter.Associating.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Associating.CSharp.Method.Hesychia.Queries;

using System.Linq;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void Params_ImplicitlyConverted_ReturnsTrue()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(4);
                }

                public void Method(params double[] values) { }
            }
            """;

        ReturnsTrue(source);
    }

    [Fact]
    public void Params_ExactType_ReturnsTrue()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(4.2);
                }

                public void Method(params double[] values) { }
            }
            """;

        ReturnsTrue(source);
    }

    [Fact]
    public void Params_SameTypeExceptNullability_ReturnsTrue()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(4.2);
                }

                public void Method(params double?[] values) { }
            }
            """;

        ReturnsTrue(source);
    }

    [Fact]
    public void Params_Null_ReturnsTrue()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(null, null);
                }

                public void Method(params object?[]? values) { }
            }
            """;

        ReturnsTrue(source);
    }

    [Fact]
    public void NonParams_ReturnsFalse()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method([4.2]);
                }

                public void Method(params double[] values) { }
            }
            """;

        ReturnsFalse(source);
    }

    [Fact]
    public void NonParams_Null_ReturnsFalse()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(null);
                }

                public void Method(params double[] values) { }
            }
            """;

        ReturnsFalse(source);
    }

    private bool Target(
        IIsCSharpMethodArgumentParamsQuery query)
    {
        return Fixture.Sut.Handle(query);
    }

    private void ReturnsTrue(
        string source)
    {
        ReturnsValue(source, true);
    }

    private void ReturnsFalse(
        string source)
    {
        ReturnsValue(source, false);
    }

    private void ReturnsValue(
        string source,
        bool expected)
    {
        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var method = type.GetMembers().OfType<IMethodSymbol>().Single((symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IIsCSharpMethodArgumentParamsQuery> queryMock = new();

        queryMock.Setup(static (query) => query.Parameter).Returns(parameters[0]);
        queryMock.Setup(static (query) => query.SyntacticArgument).Returns(syntacticArguments[0]);
        queryMock.Setup(static (query) => query.SemanticModel).Returns(semanticModel);

        var result = Target(queryMock.Object);

        Assert.Equal(expected, result);
    }
}
