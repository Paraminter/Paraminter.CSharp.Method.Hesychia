namespace Paraminter.CSharp.Method.Hesychia;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Method.Hesychia.Errors;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Parameters.Method.Models;

using System;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void NullNormalIndividualAssociator_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            null!,
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpMethodAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullParamsIndividualAssociator_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            null!,
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpMethodAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullDefaultIndividualAssociator_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            null!,
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpMethodAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullParamsArgumentDistinguisher_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            null!,
            Mock.Of<ICSharpMethodAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullErrorHandler_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void ValidArguments_ReturnsAssociator()
    {
        var result = Target(
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpMethodAssociatorErrorHandler>());

        Assert.NotNull(result);
    }

    private static CSharpMethodAssociator Target(
        ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalIndividualAssocitor,
        ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsIndividualAssociator,
        ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultIndividualAssociator,
        IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentDistinguisher,
        ICSharpMethodAssociatorErrorHandler errorHandler)
    {
        return new CSharpMethodAssociator(normalIndividualAssocitor, paramsIndividualAssociator, defaultIndividualAssociator, paramsArgumentDistinguisher, errorHandler);
    }
}
