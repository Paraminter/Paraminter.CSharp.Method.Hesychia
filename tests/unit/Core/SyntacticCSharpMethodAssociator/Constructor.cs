namespace Paraminter.CSharp.Method.Hesychia;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Commands.Handlers;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Parameters.Method.Models;
using Paraminter.Queries.Handlers;
using Paraminter.Recorders.Commands;

using System;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void NullNormalRecorder_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            null!,
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            Mock.Of<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullParamsRecorder_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            null!,
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            Mock.Of<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullDefaultRecorder_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            null!,
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            Mock.Of<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullParamsArgumentIdentifier_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            null!,
            Mock.Of<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullInvalidator_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void ValidArguments_ReturnsAssociator()
    {
        var result = Target(
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>>(),
            Mock.Of<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>>());

        Assert.NotNull(result);
    }

    private static SyntacticCSharpMethodAssociator Target(
        ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalRecorder,
        ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsRecorder,
        ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultRecorder,
        IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentIdentifier,
        ICommandHandler<IInvalidateArgumentAssociationsRecordCommand> invalidator)
    {
        return new SyntacticCSharpMethodAssociator(normalRecorder, paramsRecorder, defaultRecorder, paramsArgumentIdentifier, invalidator);
    }
}
