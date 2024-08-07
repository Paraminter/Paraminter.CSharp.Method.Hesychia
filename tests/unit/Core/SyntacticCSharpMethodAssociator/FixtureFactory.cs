namespace Paraminter.CSharp.Method.Hesychia;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Associators.Commands;
using Paraminter.Commands.Handlers;
using Paraminter.CSharp.Method.Hesychia.Models;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Parameters.Method.Models;
using Paraminter.Queries.Handlers;
using Paraminter.Recorders.Commands;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> normalRecorderMock = new();
        Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> paramsRecorderMock = new();
        Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> defaultRecorderMock = new();

        Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> paramsArgumentIdentifierMock = new();

        Mock<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>> invalidatorMock = new();

        SyntacticCSharpMethodAssociator sut = new(normalRecorderMock.Object, paramsRecorderMock.Object, defaultRecorderMock.Object, paramsArgumentIdentifierMock.Object, invalidatorMock.Object);

        return new Fixture(sut, normalRecorderMock, paramsRecorderMock, defaultRecorderMock, paramsArgumentIdentifierMock, invalidatorMock);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly ICommandHandler<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> Sut;

        private readonly Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> NormalRecorderMock;
        private readonly Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> ParamsRecorderMock;
        private readonly Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> DefaultRecorderMock;

        private readonly Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> ParamsArgumentIdentifierMock;

        private readonly Mock<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>> InvalidatorMock;

        public Fixture(
            ICommandHandler<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> sut,
            Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> normalRecorderMock,
            Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> paramsRecorderMock,
            Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> defaultRecorderMock,
            Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> paramsArgumentIdentifierMock,
            Mock<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>> invalidatorMock)
        {
            Sut = sut;

            NormalRecorderMock = normalRecorderMock;
            ParamsRecorderMock = paramsRecorderMock;
            DefaultRecorderMock = defaultRecorderMock;

            ParamsArgumentIdentifierMock = paramsArgumentIdentifierMock;

            InvalidatorMock = invalidatorMock;
        }

        ICommandHandler<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> IFixture.Sut => Sut;

        Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> IFixture.NormalRecorderMock => NormalRecorderMock;
        Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> IFixture.ParamsRecorderMock => ParamsRecorderMock;
        Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> IFixture.DefaultRecorderMock => DefaultRecorderMock;

        Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> IFixture.ParamsArgumentIdentifierMock => ParamsArgumentIdentifierMock;

        Mock<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>> IFixture.InvalidatorMock => InvalidatorMock;
    }
}
