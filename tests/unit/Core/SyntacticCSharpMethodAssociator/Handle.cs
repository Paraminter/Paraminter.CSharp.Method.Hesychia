namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.CSharp.Method.Queries.Collectors;
using Paraminter.Queries.Values.Collectors;

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void NullQuery_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(null!, Mock.Of<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullQueryResponseCollector_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(Mock.Of<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>>(), null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void DuplicateParameter_InvalidatesResponse()
    {
        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns([]);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify(static (collector) => collector.Invalidator.Invalidate(), Times.Once());
    }

    [Fact]
    public void MissingRequiredArgument_InvalidatesResponse()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns([]);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify(static (collector) => collector.Invalidator.Invalidate(), Times.Once());
    }

    [Fact]
    public void OutOfOrderLabelledArgumentFollowedByUnlabelled_InvalidatesResponse()
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

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify(static (collector) => collector.Invalidator.Invalidate(), Times.Once());
    }

    [Fact]
    public void MultipleArgumentsForParameter_InvalidatesResponse()
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

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify(static (collector) => collector.Invalidator.Invalidate(), Times.Once());
    }

    [Fact]
    public void ArgumentForNonExistingParameter_InvalidatesResponse()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.NameColon("_1"), SyntaxFactory.Token(SyntaxKind.None), SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify(static (collector) => collector.Invalidator.Invalidate(), Times.Once());
    }

    [Fact]
    public void SingleArgumentOfParamsParameter_UndeterminedIfParamsArgument_InvalidatesResponse()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);
        queryMock.Setup(static (query) => query.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentIdentifierMock.Setup(static (identifier) => identifier.Handle(It.IsAny<IIsCSharpMethodArgumentParamsQuery>(), It.IsAny<IValuedQueryResponseCollector<bool>>()))
            .Callback<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>>((query, queryResponseCollector) => { });

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify(static (collector) => collector.Invalidator.Invalidate(), Times.Once());
    }

    [Fact]
    public void SingleArgumentOfParamsParameter_MakesCorrectQueryToIdentifier()
    {
        var syntacticArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);
        queryMock.Setup(static (query) => query.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentIdentifierMock.Setup(static (identifier) => identifier.Handle(It.IsAny<IIsCSharpMethodArgumentParamsQuery>(), It.IsAny<IValuedQueryResponseCollector<bool>>()))
            .Callback<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>>((query, queryResponseCollector) => { });

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        var verifyQueryDelegate = createVerifyQueryDelegate();

        Fixture.ParamsArgumentIdentifierMock.Verify((identifier) => identifier.Handle(It.Is<IIsCSharpMethodArgumentParamsQuery>((query) => verifyQueryDelegate(query)), It.IsAny<IValuedQueryResponseCollector<bool>>()), Times.Once());

        Func<IIsCSharpMethodArgumentParamsQuery, bool> createVerifyQueryDelegate()
        {
            return (query) =>
            {
                return ReferenceEquals(parameterSymbolMock.Object, query.Parameter) && ReferenceEquals(syntacticArguments[0], query.SyntacticArgument) && ReferenceEquals(semanticModel, query.SemanticModel);
            };
        }
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

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns([]);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Default.Add(parameter1SymbolMock.Object), Times.Once());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Default.Add(parameter2SymbolMock.Object), Times.Once());
        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Default.Add(It.IsAny<IParameterSymbol>()), Times.Exactly(2));

        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Normal.Add(It.IsAny<IParameterSymbol>(), It.IsAny<ArgumentSyntax>()), Times.Never());
        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Params.Add(It.IsAny<IParameterSymbol>(), It.IsAny<IReadOnlyList<ArgumentSyntax>>()), Times.Never());

        queryResponseCollectorMock.Verify(static (collector) => collector.Invalidator.Invalidate(), Times.Never());
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

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(parameterSymbolMock.Object, syntacticArguments[0]), Times.Once());
        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Normal.Add(It.IsAny<IParameterSymbol>(), It.IsAny<ArgumentSyntax>()), Times.Once());

        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Params.Add(It.IsAny<IParameterSymbol>(), It.IsAny<IReadOnlyList<ArgumentSyntax>>()), Times.Never());
        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Default.Add(It.IsAny<IParameterSymbol>()), Times.Never());

        queryResponseCollectorMock.Verify(static (collector) => collector.Invalidator.Invalidate(), Times.Never());
    }

    [Fact]
    public void UnspecifiedParamsArgument_AssociatesEmptyArray()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns([]);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Params.Add(parameterSymbolMock.Object, Array.Empty<ArgumentSyntax>()), Times.Once());
        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Params.Add(It.IsAny<IParameterSymbol>(), It.IsAny<IReadOnlyList<ArgumentSyntax>>()), Times.Once());

        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Normal.Add(It.IsAny<IParameterSymbol>(), It.IsAny<ArgumentSyntax>()), Times.Never());
        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Default.Add(It.IsAny<IParameterSymbol>()), Times.Never());

        queryResponseCollectorMock.Verify(static (collector) => collector.Invalidator.Invalidate(), Times.Never());
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

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Params.Add(parameterSymbolMock.Object, syntacticArguments.Take(2).ToList()), Times.Once());
        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Params.Add(It.IsAny<IParameterSymbol>(), It.IsAny<IReadOnlyList<ArgumentSyntax>>()), Times.Once());

        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Normal.Add(It.IsAny<IParameterSymbol>(), It.IsAny<ArgumentSyntax>()), Times.Never());
        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Default.Add(It.IsAny<IParameterSymbol>()), Times.Never());

        queryResponseCollectorMock.Verify(static (collector) => collector.Invalidator.Invalidate(), Times.Never());
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

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);
        queryMock.Setup(static (query) => query.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentIdentifierMock.Setup(static (identifier) => identifier.Handle(It.IsAny<IIsCSharpMethodArgumentParamsQuery>(), It.IsAny<IValuedQueryResponseCollector<bool>>()))
            .Callback<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>>((query, queryResponseCollector) =>
            {
                queryResponseCollector.Value.Set(true);
            });

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Params.Add(parameterSymbolMock.Object, syntacticArguments), Times.Once());
        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Params.Add(It.IsAny<IParameterSymbol>(), It.IsAny<IReadOnlyList<ArgumentSyntax>>()), Times.Once());

        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Normal.Add(It.IsAny<IParameterSymbol>(), It.IsAny<ArgumentSyntax>()), Times.Never());
        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Default.Add(It.IsAny<IParameterSymbol>()), Times.Never());

        queryResponseCollectorMock.Verify(static (collector) => collector.Invalidator.Invalidate(), Times.Never());
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

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);
        queryMock.Setup(static (query) => query.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentIdentifierMock.Setup(static (identifier) => identifier.Handle(It.IsAny<IIsCSharpMethodArgumentParamsQuery>(), It.IsAny<IValuedQueryResponseCollector<bool>>()))
            .Callback<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>>((query, queryResponseCollector) =>
            {
                queryResponseCollector.Value.Set(false);
            });

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(parameterSymbolMock.Object, syntacticArguments[0]), Times.Once());
        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Normal.Add(It.IsAny<IParameterSymbol>(), It.IsAny<ArgumentSyntax>()), Times.Once());

        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Params.Add(It.IsAny<IParameterSymbol>(), It.IsAny<IReadOnlyList<ArgumentSyntax>>()), Times.Never());
        queryResponseCollectorMock.Verify(static (collector) => collector.Associations.Default.Add(It.IsAny<IParameterSymbol>()), Times.Never());

        queryResponseCollectorMock.Verify(static (collector) => collector.Invalidator.Invalidate(), Times.Never());
    }

    private void Target(
        IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData> query,
        IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector queryResponseCollector)
    {
        Fixture.Sut.Handle(query, queryResponseCollector);
    }
}
