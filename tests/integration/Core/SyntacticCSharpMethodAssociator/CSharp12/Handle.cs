namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Method.Commands;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.CSharp.Method.Queries.Handlers;
using Paraminter.Queries.Invalidation.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void MethodInvocation_NormalArguments_AssociatesAll()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(1, "", false);
                }

                public void Method(int a, string b, bool c) { }
            }
            """;

        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var method = type.GetMembers().OfType<IMethodSymbol>().Single((symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        queryMock.Setup((query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(NormalAssociationExpression(parameters[0], syntacticArguments[0]), Times.Once());
        queryResponseHandlerMock.Verify(NormalAssociationExpression(parameters[1], syntacticArguments[1]), Times.Once());
        queryResponseHandlerMock.Verify(NormalAssociationExpression(parameters[2], syntacticArguments[2]), Times.Once());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Normal.Handle(It.IsAny<IAddNormalCSharpMethodAssociationCommand>()), Times.Exactly(3));

        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Params.Handle(It.IsAny<IAddParamsCSharpMethodAssociationCommand>()), Times.Never());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Default.Handle(It.IsAny<IAddDefaultCSharpMethodCommand>()), Times.Never());

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Never());
    }

    [Fact]
    public void AttributeUsage_ParamsArguments_AssociatesAll()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(1, 2, 3);
                }

                public void Method(params int[] a) { }
            }
            """;

        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var method = type.GetMembers().OfType<IMethodSymbol>().Single((symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        queryMock.Setup((query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(ParamsAssociationExpression(parameters[0], syntacticArguments), Times.Once());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Params.Handle(It.IsAny<IAddParamsCSharpMethodAssociationCommand>()), Times.Exactly(1));

        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Normal.Handle(It.IsAny<IAddNormalCSharpMethodAssociationCommand>()), Times.Never());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Default.Handle(It.IsAny<IAddDefaultCSharpMethodCommand>()), Times.Never());

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Never());
    }

    [Fact]
    public void AttributeUsage_DefaultArgument_AssociatesAll()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method();
                }

                public void Method(int a = 3) { }
            }
            """;

        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var method = type.GetMembers().OfType<IMethodSymbol>().Single((symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        queryMock.Setup((query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(DefaultAssociationExpression(parameters[0]), Times.Once());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Default.Handle(It.IsAny<IAddDefaultCSharpMethodCommand>()), Times.Exactly(1));

        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Normal.Handle(It.IsAny<IAddNormalCSharpMethodAssociationCommand>()), Times.Never());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Params.Handle(It.IsAny<IAddParamsCSharpMethodAssociationCommand>()), Times.Never());

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Never());
    }

    private static Expression<Action<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler>> NormalAssociationExpression(
        IParameterSymbol parameter,
        ArgumentSyntax syntacticArgument)
    {
        return (handler) => handler.AssociationCollector.Normal.Handle(It.Is(MatchNormalAssociationCommand(parameter, syntacticArgument)));
    }

    private static Expression<Func<IAddNormalCSharpMethodAssociationCommand, bool>> MatchNormalAssociationCommand(
        IParameterSymbol parameter,
        ArgumentSyntax syntacticArgument)
    {
        return (command) => ReferenceEquals(command.Parameter, parameter) && ReferenceEquals(command.SyntacticArgument, syntacticArgument);
    }

    private static Expression<Action<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler>> ParamsAssociationExpression(
        IParameterSymbol parameter,
        IReadOnlyList<ArgumentSyntax> syntacticArguments)
    {
        return (handler) => handler.AssociationCollector.Params.Handle(It.Is(MatchParamsAssociationCommand(parameter, syntacticArguments)));
    }

    private static Expression<Func<IAddParamsCSharpMethodAssociationCommand, bool>> MatchParamsAssociationCommand(
        IParameterSymbol parameter,
        IReadOnlyList<ArgumentSyntax> syntacticArguments)
    {
        return (command) => ReferenceEquals(command.Parameter, parameter) && Enumerable.SequenceEqual(command.SyntacticArguments, syntacticArguments);
    }

    private static Expression<Action<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler>> DefaultAssociationExpression(
        IParameterSymbol parameter)
    {
        return (handler) => handler.AssociationCollector.Default.Handle(It.Is(MatchDefaultAssociationCommand(parameter)));
    }

    private static Expression<Func<IAddDefaultCSharpMethodCommand, bool>> MatchDefaultAssociationCommand(
        IParameterSymbol parameter)
    {
        return (command) => ReferenceEquals(command.Parameter, parameter);
    }

    private void Target(
        IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData> query,
        IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler queryResponseHandler)
    {
        Fixture.Sut.Handle(query, queryResponseHandler);
    }
}
