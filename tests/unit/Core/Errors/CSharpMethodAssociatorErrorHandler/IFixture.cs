namespace Paraminter.CSharp.Method.Hesychia.Errors;

using Moq;

using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Method.Hesychia.Errors.Commands;

internal interface IFixture
{
    public abstract ICSharpMethodAssociatorErrorHandler Sut { get; }

    public abstract Mock<ICommandHandler<IHandleMissingRequiredArgumentCommand>> MissingRequiredArgumentMock { get; }
    public abstract Mock<ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>> OutOfOrderLabeledArgumentFollowedByUnlabeledMock { get; }
    public abstract Mock<ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand>> UnrecognizedLabeledArgumentMock { get; }
    public abstract Mock<ICommandHandler<IHandleDuplicateParameterNamesCommand>> DuplicateParameterNamesMock { get; }
    public abstract Mock<ICommandHandler<IHandleDuplicateArgumentsCommand>> DuplicateArgumentsMock { get; }
}
