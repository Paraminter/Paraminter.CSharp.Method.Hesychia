namespace Paraminter.Associating.CSharp.Method.Hesychia;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Associating.Commands;
using Paraminter.Associating.CSharp.Method.Hesychia.Errors;
using Paraminter.Associating.CSharp.Method.Hesychia.Models;
using Paraminter.Associating.CSharp.Method.Hesychia.Queries;
using Paraminter.Pairing.Commands;
using Paraminter.Parameters.Method.Models;

internal interface IFixture
{
    public abstract ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>> Sut { get; }

    public abstract Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> NormalPairerMock { get; }
    public abstract Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> ParamsPairerMock { get; }
    public abstract Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> DefaultPairerMock { get; }

    public abstract Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> ParamsArgumentDistinguisherMock { get; }

    public abstract Mock<ICSharpMethodAssociatorErrorHandler> ErrorHandlerMock { get; }
}
