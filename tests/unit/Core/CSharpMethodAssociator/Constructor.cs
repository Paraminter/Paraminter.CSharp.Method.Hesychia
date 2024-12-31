namespace Paraminter.Associating.CSharp.Method.Hesychia;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Associating.CSharp.Method.Hesychia.Errors;
using Paraminter.Associating.CSharp.Method.Hesychia.Queries;
using Paraminter.Pairing.Commands;
using Paraminter.Parameters.Method.Models;

using System;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void NullNormalPairer_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            null!,
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpMethodAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullParamsPairer_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            null!,
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpMethodAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullDefaultPairer_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            null!,
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpMethodAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullParamsArgumentDistinguisher_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            null!,
            Mock.Of<ICSharpMethodAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullErrorHandler_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void ValidArguments_ReturnsAssociator()
    {
        var result = Target(
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpMethodAssociatorErrorHandler>());

        Assert.NotNull(result);
    }

    private static CSharpMethodAssociator Target(
        ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalPairer,
        ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsPairer,
        ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultPairer,
        IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentDistinguisher,
        ICSharpMethodAssociatorErrorHandler errorHandler)
    {
        return new CSharpMethodAssociator(normalPairer, paramsPairer, defaultPairer, paramsArgumentDistinguisher, errorHandler);
    }
}
