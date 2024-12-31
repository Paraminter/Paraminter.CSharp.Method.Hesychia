namespace Paraminter.Associating.CSharp.Method.Hesychia;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Associating.Commands;
using Paraminter.Associating.CSharp.Method.Hesychia.Errors;
using Paraminter.Associating.CSharp.Method.Hesychia.Models;
using Paraminter.Associating.CSharp.Method.Hesychia.Queries;
using Paraminter.Pairing.Commands;
using Paraminter.Parameters.Method.Models;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> normalPairerMock = new();
        Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> paramsPairerMock = new();
        Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> defaultPairerMock = new();

        Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> paramsArgumentDistinguisherMock = new();

        Mock<ICSharpMethodAssociatorErrorHandler> errorHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        CSharpMethodAssociator sut = new(normalPairerMock.Object, paramsPairerMock.Object, defaultPairerMock.Object, paramsArgumentDistinguisherMock.Object, errorHandlerMock.Object);

        return new Fixture(sut, normalPairerMock, paramsPairerMock, defaultPairerMock, paramsArgumentDistinguisherMock, errorHandlerMock);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> Sut;

        private readonly Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> NormalPairerMock;
        private readonly Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> ParamsPairerMock;
        private readonly Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> DefaultPairerMock;

        private readonly Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> ParamsArgumentDistinguisherMock;

        private readonly Mock<ICSharpMethodAssociatorErrorHandler> ErrorHandlerMock;

        public Fixture(
            ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> sut,
            Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> normalPairerMock,
            Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> paramsPairerMock,
            Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> defaultPairerMock,
            Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> paramsArgumentDistinguisherMock,
            Mock<ICSharpMethodAssociatorErrorHandler> errorHandlerMock)
        {
            Sut = sut;

            NormalPairerMock = normalPairerMock;
            ParamsPairerMock = paramsPairerMock;
            DefaultPairerMock = defaultPairerMock;

            ParamsArgumentDistinguisherMock = paramsArgumentDistinguisherMock;

            ErrorHandlerMock = errorHandlerMock;
        }

        ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> IFixture.Sut => Sut;

        Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> IFixture.NormalPairerMock => NormalPairerMock;
        Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> IFixture.ParamsPairerMock => ParamsPairerMock;
        Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> IFixture.DefaultPairerMock => DefaultPairerMock;

        Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> IFixture.ParamsArgumentDistinguisherMock => ParamsArgumentDistinguisherMock;

        Mock<ICSharpMethodAssociatorErrorHandler> IFixture.ErrorHandlerMock => ErrorHandlerMock;
    }
}
