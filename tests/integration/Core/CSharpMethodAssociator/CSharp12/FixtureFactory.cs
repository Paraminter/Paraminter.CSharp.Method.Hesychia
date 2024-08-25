namespace Paraminter.CSharp.Method.Hesychia;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Method.Hesychia.Errors;
using Paraminter.CSharp.Method.Hesychia.Models;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Parameters.Method.Models;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> normalIndividualAssociatorMock = new();
        Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> paramsIndividualAssociatorMock = new();
        Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> defaultIndividualAssociatorMock = new();

        Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> paramsArgumentDistinguisherMock = new();

        Mock<ICSharpMethodAssociatorErrorHandler> errorHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        CSharpMethodAssociator sut = new(normalIndividualAssociatorMock.Object, paramsIndividualAssociatorMock.Object, defaultIndividualAssociatorMock.Object, paramsArgumentDistinguisherMock.Object, errorHandlerMock.Object);

        return new Fixture(sut, normalIndividualAssociatorMock, paramsIndividualAssociatorMock, defaultIndividualAssociatorMock, paramsArgumentDistinguisherMock, errorHandlerMock);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly ICommandHandler<IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData>> Sut;

        private readonly Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> NormalIndividualAssociatorMock;
        private readonly Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> ParamsIndividualAssociatorMock;
        private readonly Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> DefaultIndividualAssociatorMock;

        private readonly Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> ParamsArgumentDistinguisherMock;

        private readonly Mock<ICSharpMethodAssociatorErrorHandler> ErrorHandlerMock;

        public Fixture(
            ICommandHandler<IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData>> sut,
            Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> normalIndividualAssociatorMock,
            Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> paramsIndividualAssociatorMock,
            Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> defaultIndividualAssocitorMock,
            Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> paramsArgumentDistinguisherMock,
            Mock<ICSharpMethodAssociatorErrorHandler> errorHandlerMock)
        {
            Sut = sut;

            NormalIndividualAssociatorMock = normalIndividualAssociatorMock;
            ParamsIndividualAssociatorMock = paramsIndividualAssociatorMock;
            DefaultIndividualAssociatorMock = defaultIndividualAssocitorMock;

            ParamsArgumentDistinguisherMock = paramsArgumentDistinguisherMock;

            ErrorHandlerMock = errorHandlerMock;
        }

        ICommandHandler<IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData>> IFixture.Sut => Sut;

        Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> IFixture.NormalIndividualAssociatorMock => NormalIndividualAssociatorMock;
        Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> IFixture.ParamsIndividualAssociatorMock => ParamsIndividualAssociatorMock;
        Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> IFixture.DefaultIndividualAssociatorMock => DefaultIndividualAssociatorMock;

        Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> IFixture.ParamsArgumentDistinguisherMock => ParamsArgumentDistinguisherMock;

        Mock<ICSharpMethodAssociatorErrorHandler> IFixture.ErrorHandlerMock => ErrorHandlerMock;
    }
}
