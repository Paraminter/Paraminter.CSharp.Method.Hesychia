namespace Paraminter.CSharp.Method.Hesychia;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Method.Hesychia.Errors;
using Paraminter.CSharp.Method.Hesychia.Models;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Parameters.Method.Models;

internal interface IFixture
{
    public abstract ICommandHandler<IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData>> Sut { get; }

    public abstract Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> NormalIndividualAssociatorMock { get; }
    public abstract Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> ParamsIndividualAssociatorMock { get; }
    public abstract Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> DefaultIndividualAssociatorMock { get; }

    public abstract Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> ParamsArgumentDistinguisherMock { get; }

    public abstract Mock<ICSharpMethodAssociatorErrorHandler> ErrorHandlerMock { get; }
}
