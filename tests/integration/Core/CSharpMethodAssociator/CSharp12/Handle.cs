namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Method.Hesychia.Errors.Commands;
using Paraminter.CSharp.Method.Hesychia.Models;
using Paraminter.Parameters.Method.Models;

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
        var method = type.GetMembers().OfType<IMethodSymbol>().Single(static (symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.NormalIndividualAssociatorMock.Verify(AssociateIndividualNormalExpression(parameters[0], syntacticArguments[0]), Times.Once());
        Fixture.NormalIndividualAssociatorMock.Verify(AssociateIndividualNormalExpression(parameters[1], syntacticArguments[1]), Times.Once());
        Fixture.NormalIndividualAssociatorMock.Verify(AssociateIndividualNormalExpression(parameters[2], syntacticArguments[2]), Times.Once());
        Fixture.NormalIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Exactly(3));

        Fixture.ParamsIndividualAssociatorMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultIndividualAssociatorMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
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
        var method = type.GetMembers().OfType<IMethodSymbol>().Single(static (symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.ParamsIndividualAssociatorMock.Verify(AssociateIndividualParamsExpression(parameters[0], syntacticArguments), Times.Once());
        Fixture.ParamsIndividualAssociatorMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Exactly(1));

        Fixture.NormalIndividualAssociatorMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultIndividualAssociatorMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
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
        var method = type.GetMembers().OfType<IMethodSymbol>().Single(static (symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.DefaultIndividualAssociatorMock.Verify(AssociateIndividualDefaultExpression(parameters[0]), Times.Once());
        Fixture.DefaultIndividualAssociatorMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Exactly(1));

        Fixture.NormalIndividualAssociatorMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Never());
        Fixture.ParamsIndividualAssociatorMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Never());
    }

    private static Expression<Action<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>> AssociateIndividualNormalExpression(
        IParameterSymbol parameterSymbol,
        ArgumentSyntax syntacticArgument)
    {
        return (recorder) => recorder.Handle(It.Is(MatchAssociateIndividualNormalCommand(parameterSymbol, syntacticArgument)));
    }

    private static Expression<Func<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>, bool>> MatchAssociateIndividualNormalCommand(
        IParameterSymbol parameterSymbol,
        ArgumentSyntax syntacticArgument)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchNormalArgumentData(syntacticArgument, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>> AssociateIndividualParamsExpression(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<ArgumentSyntax> syntacticArguments)
    {
        return (recorder) => recorder.Handle(It.Is(MatchAssociateIndividualParamsCommand(parameterSymbol, syntacticArguments)));
    }

    private static Expression<Func<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>, bool>> MatchAssociateIndividualParamsCommand(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<ArgumentSyntax> syntacticArguments)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchParamsArgumentData(syntacticArguments, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>> AssociateIndividualDefaultExpression(
        IParameterSymbol parameterSymbol)
    {
        return (recorder) => recorder.Handle(It.Is(MatchAssociateIndividualDefaultCommand(parameterSymbol)));
    }

    private static Expression<Func<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>, bool>> MatchAssociateIndividualDefaultCommand(
        IParameterSymbol parameterSymbol)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter);
    }

    private static bool MatchParameter(
        IParameterSymbol parameterSymbol,
        IMethodParameter parameter)
    {
        return ReferenceEquals(parameterSymbol, parameter.Symbol);
    }

    private static bool MatchNormalArgumentData(
        ArgumentSyntax syntacticArgument,
        INormalCSharpMethodArgumentData argumentData)
    {
        return ReferenceEquals(syntacticArgument, argumentData.SyntacticArgument);
    }

    private static bool MatchParamsArgumentData(
        IReadOnlyList<ArgumentSyntax> syntacticArguments,
        IParamsCSharpMethodArgumentData argumentData)
    {
        return Enumerable.SequenceEqual(syntacticArguments, argumentData.SyntacticArguments);
    }

    private void Target(
        IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData> command)
    {
        Fixture.Sut.Handle(command);
    }
}
