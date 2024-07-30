namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Queries.Values.Commands;
using Paraminter.Queries.Values.Handlers;

using System;
using System.Linq;
using System.Linq.Expressions;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void NullQuery_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(null!, Mock.Of<IValuedQueryResponseHandler<bool>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullQueryResponseHandler_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(Mock.Of<IIsCSharpMethodArgumentParamsQuery>(), null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NotParamsParameter_SetsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(false);

        Mock<IIsCSharpMethodArgumentParamsQuery> queryMock = new();
        Mock<IValuedQueryResponseHandler<bool>> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Value.Handle(It.IsAny<ISetQueryResponseValueCommand<bool>>()), Times.Once());
        queryResponseHandlerMock.Verify(SetValueExpression(false), Times.Once());
    }

    [Fact]
    public void NotArraySymbol_SetsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(true);

        Mock<IIsCSharpMethodArgumentParamsQuery> queryMock = new();
        Mock<IValuedQueryResponseHandler<bool>> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Value.Handle(It.IsAny<ISetQueryResponseValueCommand<bool>>()), Times.Once());
        queryResponseHandlerMock.Verify(SetValueExpression(false), Times.Once());
    }

    [Fact]
    public void ArgumentNotOfElementType_SetsFalse()
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

        SetsValue(source, false);
    }

    [Fact]
    public void ArgumentOfElementType_SetsTrue()
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

        SetsValue(source, true);
    }

    private static Expression<Action<IValuedQueryResponseHandler<TValue>>> SetValueExpression<TValue>(
        TValue value)
    {
        return (handler) => handler.Value.Handle(It.Is(MatchSetValueCommand(value)));
    }

    private static Expression<Func<ISetQueryResponseValueCommand<TValue>, bool>> MatchSetValueCommand<TValue>(
        TValue value)
    {
        return (command) => Equals(command.Value, value);
    }

    private void Target(
        IIsCSharpMethodArgumentParamsQuery query,
        IValuedQueryResponseHandler<bool> queryResponseHandler)
    {
        Fixture.Sut.Handle(query, queryResponseHandler);
    }

    private void SetsValue(
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
        Mock<IValuedQueryResponseHandler<bool>> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Parameter).Returns(parameters[0]);
        queryMock.Setup(static (query) => query.SyntacticArgument).Returns(syntacticArguments[0]);
        queryMock.Setup(static (query) => query.SemanticModel).Returns(semanticModel);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Value.Handle(It.IsAny<ISetQueryResponseValueCommand<bool>>()), Times.Once());
        queryResponseHandlerMock.Verify(SetValueExpression(expected), Times.Once());
    }
}
