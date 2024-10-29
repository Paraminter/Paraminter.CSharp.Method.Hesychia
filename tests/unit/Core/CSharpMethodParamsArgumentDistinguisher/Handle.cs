namespace Paraminter.Associating.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Associating.CSharp.Method.Hesychia.Queries;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public async Task NullQuery_ThrowsArgumentNullException()
    {
        var result = await Record.ExceptionAsync(() => Target(null!, CancellationToken.None));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public async Task NotParamsParameter_ReturnsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(false);

        Mock<IIsCSharpMethodArgumentParamsQuery> queryMock = new();

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        var result = await Target(queryMock.Object, CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task NotArraySymbol_ReturnsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(true);

        Mock<IIsCSharpMethodArgumentParamsQuery> queryMock = new();

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        var result = await Target(queryMock.Object, CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task ArgumentNotOfElementType_ReturnsFalse()
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

        await ReturnsValue(source, false, CancellationToken.None);
    }

    [Fact]
    public async Task ArgumentOfElementType_ReturnsTrue()
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

        await ReturnsValue(source, true, CancellationToken.None);
    }

    private async Task<bool> Target(
        IIsCSharpMethodArgumentParamsQuery query,
        CancellationToken cancellationToken)
    {
        return await Fixture.Sut.Handle(query, cancellationToken);
    }

    private async Task ReturnsValue(
        string source,
        bool expected,
        CancellationToken cancellationToken)
    {
        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var method = type.GetMembers().OfType<IMethodSymbol>().Single((symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var invokeMethod = (await syntaxTree.GetRootAsync(CancellationToken.None)).DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IIsCSharpMethodArgumentParamsQuery> queryMock = new();

        queryMock.Setup(static (query) => query.Parameter).Returns(parameters[0]);
        queryMock.Setup(static (query) => query.SyntacticArgument).Returns(syntacticArguments[0]);
        queryMock.Setup(static (query) => query.SemanticModel).Returns(semanticModel);

        var result = await Target(queryMock.Object, cancellationToken);

        Assert.Equal(expected, result);
    }
}
