namespace Paraminter.Associating.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Associating.Commands;
using Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;
using Paraminter.Associating.CSharp.Method.Hesychia.Models;
using Paraminter.Pairing.Commands;
using Paraminter.Parameters.Method.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public async Task MethodInvocation_NormalArguments_PairsAll()
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

        var invokeMethod = (await syntaxTree.GetRootAsync(CancellationToken.None)).DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());

        Fixture.NormalPairerMock.Verify(PairNormalArgumentExpression(parameters[0], syntacticArguments[0], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.NormalPairerMock.Verify(PairNormalArgumentExpression(parameters[1], syntacticArguments[1], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.NormalPairerMock.Verify(PairNormalArgumentExpression(parameters[2], syntacticArguments[2], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task AttributeUsage_ParamsArguments_PairsAll()
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

        var invokeMethod = (await syntaxTree.GetRootAsync(CancellationToken.None)).DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());

        Fixture.ParamsPairerMock.Verify(PairParamsArgumentExpression(parameters[0], syntacticArguments, It.IsAny<CancellationToken>()), Times.Once());
        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task AttributeUsage_DefaultArgument_PairsAll()
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

        var invokeMethod = (await syntaxTree.GetRootAsync(CancellationToken.None)).DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());

        Fixture.DefaultPairerMock.Verify(PairDefaultArgumentExpression(parameters[0], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    private static Expression<Func<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>, Task>> PairNormalArgumentExpression(
        IParameterSymbol parameterSymbol,
        ArgumentSyntax syntacticArgument,
        CancellationToken cancellationToken)
    {
        return (handler) => handler.Handle(It.Is(MatchPairNormalArgumentCommand(parameterSymbol, syntacticArgument)), cancellationToken);
    }

    private static Expression<Func<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>, bool>> MatchPairNormalArgumentCommand(
        IParameterSymbol parameterSymbol,
        ArgumentSyntax syntacticArgument)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchNormalArgumentData(syntacticArgument, command.ArgumentData);
    }

    private static Expression<Func<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>, Task>> PairParamsArgumentExpression(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<ArgumentSyntax> syntacticArguments,
        CancellationToken cancellationToken)
    {
        return (handler) => handler.Handle(It.Is(MatchPairParamsArgumentCommand(parameterSymbol, syntacticArguments)), cancellationToken);
    }

    private static Expression<Func<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>, bool>> MatchPairParamsArgumentCommand(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<ArgumentSyntax> syntacticArguments)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchParamsArgumentData(syntacticArguments, command.ArgumentData);
    }

    private static Expression<Func<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>, Task>> PairDefaultArgumentExpression(
        IParameterSymbol parameterSymbol,
        CancellationToken cancellationToken)
    {
        return (handler) => handler.Handle(It.Is(MatchPairDefaultArgumentCommand(parameterSymbol)), cancellationToken);
    }

    private static Expression<Func<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>, bool>> MatchPairDefaultArgumentCommand(
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

    private async Task Target(
        IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData> command,
        CancellationToken cancellationToken)
    {
        await Fixture.Sut.Handle(command, cancellationToken);
    }
}
