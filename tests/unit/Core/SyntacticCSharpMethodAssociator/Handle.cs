namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Method.Hesychia.Models;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Parameters.Method.Models;
using Paraminter.Recorders.Commands;

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
    public void DuplicateParameter_Invalidates()
    {
        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.AtLeastOnce());
    }

    [Fact]
    public void MissingRequiredArgument_Invalidates()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.AtLeastOnce());
    }

    [Fact]
    public void OutOfOrderLabelledArgumentFollowedByUnlabelled_Invalidates()
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

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.AtLeastOnce());
    }

    [Fact]
    public void MultipleArgumentsForParameter_Invalidates()
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

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.AtLeastOnce());
    }

    [Fact]
    public void ArgumentForNonExistingParameter_Invalidates()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.NameColon("_1"), SyntaxFactory.Token(SyntaxKind.None), SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.AtLeastOnce());
    }

    [Fact]
    public void UnspecifiedOptionalArguments_RecordsDefaultArguments()
    {
        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_1");
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_2");
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.DefaultRecorderMock.Verify(RecordDefaultExpression(parameter1SymbolMock.Object), Times.Once());
        Fixture.DefaultRecorderMock.Verify(RecordDefaultExpression(parameter2SymbolMock.Object), Times.Once());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Exactly(2));

        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Never());
        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Never());
    }

    [Fact]
    public void ValidNormalArgument_RecordsNormalArgument()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.NormalRecorderMock.Verify(RecordNormalExpression(parameterSymbolMock.Object, syntacticArguments[0]), Times.Once());
        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Once());

        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
    }

    [Fact]
    public void UnspecifiedParamsArgument_RecordsEmptyArray()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.ParamsRecorderMock.Verify(RecordParamsExpression(parameterSymbolMock.Object, []), Times.Once());
        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Once());

        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
    }

    [Fact]
    public void MultipleParamsArguments_RecordsParamsArguments()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42")),
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.ParamsRecorderMock.Verify(RecordParamsExpression(parameterSymbolMock.Object, syntacticArguments.Take(2).ToList()), Times.Once());
        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Once());

        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
    }

    [Fact]
    public void SingleArgumentOfParamsParameters_IsParamsArgument_RecordsParamsArgument()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);
        commandMock.Setup(static (command) => command.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentIdentifierMock.Setup(IsParamsExpression(parameterSymbolMock.Object, syntacticArguments[0], semanticModel)).Returns(true);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.ParamsRecorderMock.Verify(RecordParamsExpression(parameterSymbolMock.Object, syntacticArguments), Times.Once());
        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Once());

        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
    }

    [Fact]
    public void SingleArgumentOfParamsParameter_IsNotParamsArgument_RecordsNormalArgument()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);
        commandMock.Setup(static (command) => command.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentIdentifierMock.Setup(IsParamsExpression(parameterSymbolMock.Object, syntacticArguments[0], semanticModel)).Returns(false);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.NormalRecorderMock.Verify(RecordNormalExpression(parameterSymbolMock.Object, syntacticArguments[0]), Times.Once());
        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Once());

        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
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

    private static Expression<Action<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>> RecordNormalExpression(
        IParameterSymbol parameterSymbol,
        ArgumentSyntax syntacticArgument)
    {
        return (recorder) => recorder.Handle(It.Is(MatchRecordNormalCommand(parameterSymbol, syntacticArgument)));
    }

    private static Expression<Func<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>, bool>> MatchRecordNormalCommand(
        IParameterSymbol parameterSymbol,
        ArgumentSyntax syntacticArgument)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchNormalArgumentData(syntacticArgument, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>> RecordParamsExpression(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<ArgumentSyntax> syntacticArguments)
    {
        return (recorder) => recorder.Handle(It.Is(MatchRecordParamsCommand(parameterSymbol, syntacticArguments)));
    }

    private static Expression<Func<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>, bool>> MatchRecordParamsCommand(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<ArgumentSyntax> syntacticArguments)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchParamsArgumentData(syntacticArguments, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>> RecordDefaultExpression(
        IParameterSymbol parameterSymbol)
    {
        return (recorder) => recorder.Handle(It.Is(MatchRecordDefaultCommand(parameterSymbol)));
    }

    private static Expression<Func<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>, bool>> MatchRecordDefaultCommand(
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
        IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData> command)
    {
        Fixture.Sut.Handle(command);
    }
}
