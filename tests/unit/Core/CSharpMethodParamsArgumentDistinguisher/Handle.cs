namespace Paraminter.Associating.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Associating.CSharp.Method.Hesychia.Queries;

using System;
using System.Linq;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void NullQuery_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NotParamsParameter_ReturnsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(false);

        Mock<IIsCSharpMethodArgumentParamsQuery> queryMock = new();

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        var result = Target(queryMock.Object);

        Assert.False(result);
    }

    [Fact]
    public void NotArraySymbol_ReturnsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(true);

        Mock<IIsCSharpMethodArgumentParamsQuery> queryMock = new();

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        var result = Target(queryMock.Object);

        Assert.False(result);
    }

    [Fact]
    public void ArgumentNotOfElementType_ReturnsFalse()
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

        ReturnsValue(source, false);
    }

    [Fact]
    public void ArgumentOfElementType_ReturnsTrue()
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

        ReturnsValue(source, true);
    }

    private bool Target(
        IIsCSharpMethodArgumentParamsQuery query)
    {
        return Fixture.Sut.Handle(query);
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
