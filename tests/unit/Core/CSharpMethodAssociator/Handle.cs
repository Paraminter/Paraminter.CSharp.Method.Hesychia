namespace Paraminter.Associating.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Associating.Commands;
using Paraminter.Associating.CSharp.Method.Hesychia.Errors;
using Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;
using Paraminter.Associating.CSharp.Method.Hesychia.Models;
using Paraminter.Associating.CSharp.Method.Hesychia.Queries;
using Paraminter.Cqs;
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
    public async Task NullCommand_ThrowsArgumentNullException()
    {
        var result = await Record.ExceptionAsync(() => Target(null!, CancellationToken.None));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public async Task MissingRequiredArgument_HandlesError()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(HandleMissingRequiredArgumentExpression(parameterSymbolMock.Object, It.IsAny<CancellationToken>()), Times.Once());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task OutOfOrderLabelledArgumentFollowedByUnlabelled_HandlesError()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.NameColon("_2"), SyntaxFactory.Token(SyntaxKind.None), SyntaxFactory.ParseExpression("42")),
            SyntaxFactory.Argument(null, SyntaxFactory.Token(SyntaxKind.None), SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_1");
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_2");
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(HandleOutOfOrderLabeledArgumentFollowedByUnlabeledExpression(syntacticArguments[1], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task ArgumentForNonExistingParameter_HandlesError()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.NameColon("_1"), SyntaxFactory.Token(SyntaxKind.None), SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(HandleUnrecognizedLabeledArgumentExpression(syntacticArguments[0], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task DuplicateParameter_HandlesError()
    {
        var parameterName = "Foo";

        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns(parameterName);
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);

        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns(parameterName);
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(HandleDuplicateParameterNamesExpression(parameterName, It.IsAny<CancellationToken>()), Times.Once());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task MultipleArgumentsForSameParameter_HandlesError()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.NameColon("_1"), SyntaxFactory.Token(SyntaxKind.None), SyntaxFactory.ParseExpression("42")),
            SyntaxFactory.Argument(SyntaxFactory.NameColon("_1"), SyntaxFactory.Token(SyntaxKind.None), SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_1");
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_2");
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(HandleDuplicateArgumentsExpression(parameter1SymbolMock.Object, syntacticArguments[1], It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task UnspecifiedOptionalArguments_PairsDefaultArguments()
    {
        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_1");
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_2");
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());

        Fixture.DefaultPairerMock.Verify(PairDefaultArgumentExpression(parameter1SymbolMock.Object, It.IsAny<CancellationToken>()), Times.Once());
        Fixture.DefaultPairerMock.Verify(PairDefaultArgumentExpression(parameter2SymbolMock.Object, It.IsAny<CancellationToken>()), Times.Once());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task ValidNormalArgument_PairsNormalArgument()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());

        Fixture.NormalPairerMock.Verify(PairNormalArgumentExpression(parameterSymbolMock.Object, syntacticArguments[0], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Once());

        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task UnspecifiedParamsArgument_PairsEmptyArray()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());

        Fixture.ParamsPairerMock.Verify(PairParamsArgumentExpression(parameterSymbolMock.Object, [], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Once());

        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task MultipleParamsArguments_PairsParamsArguments()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42")),
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());

        Fixture.ParamsPairerMock.Verify(PairParamsArgumentExpression(parameterSymbolMock.Object, syntacticArguments.Take(2).ToList(), It.IsAny<CancellationToken>()), Times.Once());
        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Once());

        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task SingleArgumentOfParamsParameters_IsParamsArgument_PairsParamsArgument()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);
        commandMock.Setup(static (command) => command.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentDistinguisherMock.Setup(IsParamsExpression(parameterSymbolMock.Object, syntacticArguments[0], semanticModel, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());

        Fixture.ParamsPairerMock.Verify(PairParamsArgumentExpression(parameterSymbolMock.Object, syntacticArguments, It.IsAny<CancellationToken>()), Times.Once());
        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Once());

        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task SingleArgumentOfParamsParameter_IsNotParamsArgument_PairsNormalArgument()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);
        commandMock.Setup(static (command) => command.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentDistinguisherMock.Setup(IsParamsExpression(parameterSymbolMock.Object, syntacticArguments[0], semanticModel, It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());

        Fixture.NormalPairerMock.Verify(PairNormalArgumentExpression(parameterSymbolMock.Object, syntacticArguments[0], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Once());

        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    private static Expression<Func<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>, Task<bool>>> IsParamsExpression(
        IParameterSymbol parameter,
        ArgumentSyntax argument,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return (handler) => handler.Handle(It.Is(MatchIsParamsQuery(parameter, argument, semanticModel)), cancellationToken);
    }

    private static Expression<Func<IIsCSharpMethodArgumentParamsQuery, bool>> MatchIsParamsQuery(
        IParameterSymbol parameter,
        ArgumentSyntax argument,
        SemanticModel semanticModel)
    {
        return (query) => ReferenceEquals(query.Parameter, parameter) && ReferenceEquals(query.SyntacticArgument, argument) && ReferenceEquals(query.SemanticModel, semanticModel);
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

    private static Expression<Func<ICSharpMethodAssociatorErrorHandler, Task>> HandleMissingRequiredArgumentExpression(
        IParameterSymbol parameterSymbol,
        CancellationToken cancellationToken)
    {
        return (handler) => handler.MissingRequiredArgument.Handle(It.Is(MatchHandleMissingRequiredArgumentCommand(parameterSymbol)), cancellationToken);
    }

    private static Expression<Func<IHandleMissingRequiredArgumentCommand, bool>> MatchHandleMissingRequiredArgumentCommand(
        IParameterSymbol parameterSymbol)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter);
    }

    private static Expression<Func<ICSharpMethodAssociatorErrorHandler, Task>> HandleOutOfOrderLabeledArgumentFollowedByUnlabeledExpression(
        ArgumentSyntax syntacticUnlabeledArgument,
        CancellationToken cancellationToken)
    {
        return (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.Is(MatchHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(syntacticUnlabeledArgument)), cancellationToken);
    }

    private static Expression<Func<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand, bool>> MatchHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(
        ArgumentSyntax syntacticUnlabeledArgument)
    {
        return (command) => Equals(syntacticUnlabeledArgument, command.SyntacticUnlabeledArgument);
    }

    private static Expression<Func<ICSharpMethodAssociatorErrorHandler, Task>> HandleUnrecognizedLabeledArgumentExpression(
        ArgumentSyntax syntacticArgument,
        CancellationToken cancellationToken)
    {
        return (handler) => handler.UnrecognizedLabeledArgument.Handle(It.Is(MatchHandleUnrecognizedLabeledArgumentCommand(syntacticArgument)), cancellationToken);
    }

    private static Expression<Func<IHandleUnrecognizedLabeledArgumentCommand, bool>> MatchHandleUnrecognizedLabeledArgumentCommand(
        ArgumentSyntax syntacticArgument)
    {
        return (command) => Equals(syntacticArgument, command.SyntacticArgument);
    }

    private static Expression<Func<ICSharpMethodAssociatorErrorHandler, Task>> HandleDuplicateParameterNamesExpression(
        string parameterName,
        CancellationToken cancellationToken)
    {
        return (handler) => handler.DuplicateParameterNames.Handle(It.Is(MatchHandleDuplicateParameterNamesCommand(parameterName)), cancellationToken);
    }

    private static Expression<Func<IHandleDuplicateParameterNamesCommand, bool>> MatchHandleDuplicateParameterNamesCommand(
        string parameterName)
    {
        return (command) => Equals(parameterName, command.ParameterName);
    }

    private static Expression<Func<ICSharpMethodAssociatorErrorHandler, Task>> HandleDuplicateArgumentsExpression(
        IParameterSymbol parameterSymbol,
        ArgumentSyntax syntacticArgument,
        CancellationToken cancellationToken)
    {
        return (handler) => handler.DuplicateArguments.Handle(It.Is(MatchHandleDuplicateArgumentsCommand(parameterSymbol, syntacticArgument)), cancellationToken);
    }

    private static Expression<Func<IHandleDuplicateArgumentsCommand, bool>> MatchHandleDuplicateArgumentsCommand(
        IParameterSymbol parameterSymbol,
        ArgumentSyntax syntacticArgument)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && Equals(syntacticArgument, command.SyntacticArgument);
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
