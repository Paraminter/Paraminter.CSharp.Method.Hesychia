namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors;

using Moq;

using Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;
using Paraminter.Cqs.Handlers;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        Mock<ICommandHandler<IHandleMissingRequiredArgumentCommand>> missingRequiredArgumentMock = new();
        Mock<ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>> outOfOrderLabeledArgumentFollowedByUnlabeledMock = new();
        Mock<ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand>> unrecognizedLabeledArgumentMock = new();
        Mock<ICommandHandler<IHandleDuplicateParameterNamesCommand>> duplicateParameterNamesMock = new();
        Mock<ICommandHandler<IHandleDuplicateArgumentsCommand>> duplicateArgumentsMock = new();

        CSharpMethodAssociatorErrorHandler sut = new(missingRequiredArgumentMock.Object, outOfOrderLabeledArgumentFollowedByUnlabeledMock.Object, unrecognizedLabeledArgumentMock.Object, duplicateParameterNamesMock.Object, duplicateArgumentsMock.Object);

        return new Fixture(sut, missingRequiredArgumentMock, outOfOrderLabeledArgumentFollowedByUnlabeledMock, unrecognizedLabeledArgumentMock, duplicateParameterNamesMock, duplicateArgumentsMock);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly ICSharpMethodAssociatorErrorHandler Sut;

        private readonly Mock<ICommandHandler<IHandleMissingRequiredArgumentCommand>> MissingRequiredArgumentMock;
        private readonly Mock<ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>> OutOfOrderLabeledArgumentFollowedByUnlabeledMock;
        private readonly Mock<ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand>> UnrecognizedLabeledArgumentMock;
        private readonly Mock<ICommandHandler<IHandleDuplicateParameterNamesCommand>> DuplicateParameterNamesMock;
        private readonly Mock<ICommandHandler<IHandleDuplicateArgumentsCommand>> DuplicateArgumentsMock;

        public Fixture(
            ICSharpMethodAssociatorErrorHandler sut,
            Mock<ICommandHandler<IHandleMissingRequiredArgumentCommand>> missingRequiredArgumentMock,
            Mock<ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>> outOfOrderLabeledArgumentFollowedByUnlabeledMock,
            Mock<ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand>> unrecognizedLabeledArgumentMock,
            Mock<ICommandHandler<IHandleDuplicateParameterNamesCommand>> duplicateParameterNamesMock,
            Mock<ICommandHandler<IHandleDuplicateArgumentsCommand>> duplicateArgumentsMock)
        {
            Sut = sut;

            MissingRequiredArgumentMock = missingRequiredArgumentMock;
            OutOfOrderLabeledArgumentFollowedByUnlabeledMock = outOfOrderLabeledArgumentFollowedByUnlabeledMock;
            UnrecognizedLabeledArgumentMock = unrecognizedLabeledArgumentMock;
            DuplicateParameterNamesMock = duplicateParameterNamesMock;
            DuplicateArgumentsMock = duplicateArgumentsMock;
        }

        ICSharpMethodAssociatorErrorHandler IFixture.Sut => Sut;

        Mock<ICommandHandler<IHandleMissingRequiredArgumentCommand>> IFixture.MissingRequiredArgumentMock => MissingRequiredArgumentMock;
        Mock<ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>> IFixture.OutOfOrderLabeledArgumentFollowedByUnlabeledMock => OutOfOrderLabeledArgumentFollowedByUnlabeledMock;
        Mock<ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand>> IFixture.UnrecognizedLabeledArgumentMock => UnrecognizedLabeledArgumentMock;
        Mock<ICommandHandler<IHandleDuplicateParameterNamesCommand>> IFixture.DuplicateParameterNamesMock => DuplicateParameterNamesMock;
        Mock<ICommandHandler<IHandleDuplicateArgumentsCommand>> IFixture.DuplicateArgumentsMock => DuplicateArgumentsMock;
    }
}
