namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Method.Hesychia.Errors;
using Paraminter.CSharp.Method.Hesychia.Errors.Commands;
using Paraminter.CSharp.Method.Hesychia.Models;
using Paraminter.CSharp.Method.Hesychia.Queries;
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
    public void NullCommand_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void MissingRequiredArgument_HandlesError()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(HandleMissingRequiredArgumentExpression(parameterSymbolMock.Object), Times.Once());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());
    }

    [Fact]
    public void OutOfOrderLabelledArgumentFollowedByUnlabelled_HandlesError()
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

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(HandleOutOfOrderLabeledArgumentFollowedByUnlabeledExpression(syntacticArguments[1]), Times.Once());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());
    }

    [Fact]
    public void ArgumentForNonExistingParameter_HandlesError()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.NameColon("_1"), SyntaxFactory.Token(SyntaxKind.None), SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(HandleUnrecognizedLabeledArgumentExpression(syntacticArguments[0]), Times.Once());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());
    }

    [Fact]
    public void DuplicateParameter_HandlesError()
    {
        var parameterName = "Foo";

        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns(parameterName);
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);

        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns(parameterName);
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(HandleDuplicateParameterNamesExpression(parameterName), Times.Once());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());
    }

    [Fact]
    public void MultipleArgumentsForSameParameter_HandlesError()
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

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(HandleDuplicateArgumentsExpression(parameter1SymbolMock.Object, syntacticArguments[1]), Times.Once());
    }

    [Fact]
    public void UnspecifiedOptionalArguments_AssociatesDefaultArguments()
    {
        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_1");
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_2");
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.DefaultIndividualAssociatorMock.Verify(AssociateIndividualDefaultExpression(parameter1SymbolMock.Object), Times.Once());
        Fixture.DefaultIndividualAssociatorMock.Verify(AssociateIndividualDefaultExpression(parameter2SymbolMock.Object), Times.Once());
        Fixture.DefaultIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Exactly(2));

        Fixture.NormalIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Never());
        Fixture.ParamsIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Never());
    }

    [Fact]
    public void ValidNormalArgument_AssociatesNormalArgument()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.NormalIndividualAssociatorMock.Verify(AssociateIndividualNormalExpression(parameterSymbolMock.Object, syntacticArguments[0]), Times.Once());
        Fixture.NormalIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Once());

        Fixture.ParamsIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
    }

    [Fact]
    public void UnspecifiedParamsArgument_AssocitesEmptyArray()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.ParamsIndividualAssociatorMock.Verify(AssociateIndividualParamsExpression(parameterSymbolMock.Object, []), Times.Once());
        Fixture.ParamsIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Once());

        Fixture.NormalIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
    }

    [Fact]
    public void MultipleParamsArguments_AssociatesParamsArguments()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42")),
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.ParamsIndividualAssociatorMock.Verify(AssociateIndividualParamsExpression(parameterSymbolMock.Object, syntacticArguments.Take(2).ToList()), Times.Once());
        Fixture.ParamsIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Once());

        Fixture.NormalIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
    }

    [Fact]
    public void SingleArgumentOfParamsParameters_IsParamsArgument_AssociatesParamsArgument()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);
        commandMock.Setup(static (command) => command.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentDistinguisherMock.Setup(IsParamsExpression(parameterSymbolMock.Object, syntacticArguments[0], semanticModel)).Returns(true);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.ParamsIndividualAssociatorMock.Verify(AssociateIndividualParamsExpression(parameterSymbolMock.Object, syntacticArguments), Times.Once());
        Fixture.ParamsIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Once());

        Fixture.NormalIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
    }

    [Fact]
    public void SingleArgumentOfParamsParameter_IsNotParamsArgument_AssociatesNormalArgument()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpMethodArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);
        commandMock.Setup(static (command) => command.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentDistinguisherMock.Setup(IsParamsExpression(parameterSymbolMock.Object, syntacticArguments[0], semanticModel)).Returns(false);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.NormalIndividualAssociatorMock.Verify(AssociateIndividualNormalExpression(parameterSymbolMock.Object, syntacticArguments[0]), Times.Once());
        Fixture.NormalIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Once());

        Fixture.ParamsIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
    }

    private static Expression<Func<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>, bool>> IsParamsExpression(
        IParameterSymbol parameter,
        ArgumentSyntax argument,
        SemanticModel semanticModel)
    {
        return (handler) => handler.Handle(It.Is(MatchIsParamsQuery(parameter, argument, semanticModel)));
    }

    private static Expression<Func<IIsCSharpMethodArgumentParamsQuery, bool>> MatchIsParamsQuery(
        IParameterSymbol parameter,
        ArgumentSyntax argument,
        SemanticModel semanticModel)
    {
        return (query) => ReferenceEquals(query.Parameter, parameter) && ReferenceEquals(query.SyntacticArgument, argument) && ReferenceEquals(query.SemanticModel, semanticModel);
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

    private static Expression<Action<ICSharpMethodAssociatorErrorHandler>> HandleMissingRequiredArgumentExpression(
        IParameterSymbol parameterSymbol)
    {
        return (handler) => handler.MissingRequiredArgument.Handle(It.Is(MatchHandleMissingRequiredArgumentCommand(parameterSymbol)));
    }

    private static Expression<Func<IHandleMissingRequiredArgumentCommand, bool>> MatchHandleMissingRequiredArgumentCommand(
        IParameterSymbol parameterSymbol)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter);
    }

    private static Expression<Action<ICSharpMethodAssociatorErrorHandler>> HandleOutOfOrderLabeledArgumentFollowedByUnlabeledExpression(
        ArgumentSyntax syntacticUnlabeledArgument)
    {
        return (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.Is(MatchHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(syntacticUnlabeledArgument)));
    }

    private static Expression<Func<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand, bool>> MatchHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(
        ArgumentSyntax syntacticUnlabeledArgument)
    {
        return (command) => Equals(syntacticUnlabeledArgument, command.SyntacticUnlabeledArgument);
    }

    private static Expression<Action<ICSharpMethodAssociatorErrorHandler>> HandleUnrecognizedLabeledArgumentExpression(
        ArgumentSyntax syntacticArgument)
    {
        return (handler) => handler.UnrecognizedLabeledArgument.Handle(It.Is(MatchHandleUnrecognizedLabeledArgumentCommand(syntacticArgument)));
    }

    private static Expression<Func<IHandleUnrecognizedLabeledArgumentCommand, bool>> MatchHandleUnrecognizedLabeledArgumentCommand(
        ArgumentSyntax syntacticArgument)
    {
        return (command) => Equals(syntacticArgument, command.SyntacticArgument);
    }

    private static Expression<Action<ICSharpMethodAssociatorErrorHandler>> HandleDuplicateParameterNamesExpression(
        string parameterName)
    {
        return (handler) => handler.DuplicateParameterNames.Handle(It.Is(MatchHandleDuplicateParameterNamesCommand(parameterName)));
    }

    private static Expression<Func<IHandleDuplicateParameterNamesCommand, bool>> MatchHandleDuplicateParameterNamesCommand(
        string parameterName)
    {
        return (command) => Equals(parameterName, command.ParameterName);
    }

    private static Expression<Action<ICSharpMethodAssociatorErrorHandler>> HandleDuplicateArgumentsExpression(
        IParameterSymbol parameterSymbol,
        ArgumentSyntax syntacticArgument)
    {
        return (handler) => handler.DuplicateArguments.Handle(It.Is(MatchHandleDuplicateArgumentsCommand(parameterSymbol, syntacticArgument)));
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

    private void Target(
        IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpMethodArgumentsData> command)
    {
        Fixture.Sut.Handle(command);
    }
}
