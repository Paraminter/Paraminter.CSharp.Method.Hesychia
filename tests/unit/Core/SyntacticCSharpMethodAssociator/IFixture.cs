namespace Paraminter.CSharp.Method.Hesychia;

using Moq;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.CSharp.Method.Queries.Handlers;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

internal interface IFixture
{
    public abstract IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>, IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler> Sut { get; }

    public abstract Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>>> ParamsArgumentIdentifierMock { get; }
}
