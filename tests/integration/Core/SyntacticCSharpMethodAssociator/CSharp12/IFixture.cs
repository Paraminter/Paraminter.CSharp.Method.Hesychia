namespace Paraminter.CSharp.Method.Hesychia;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Associators.Commands;
using Paraminter.Commands.Handlers;
using Paraminter.CSharp.Method.Hesychia.Models;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Parameters.Method.Models;
using Paraminter.Queries.Handlers;

internal interface IFixture
{
    public abstract ICommandHandler<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> Sut { get; }

    public abstract Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>> NormalRecorderMock { get; }
    public abstract Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>> ParamsRecorderMock { get; }
    public abstract Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>> DefaultRecorderMock { get; }

    public abstract Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>> ParamsArgumentIdentifierMock { get; }

    public abstract Mock<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>> InvalidatorMock { get; }
}
