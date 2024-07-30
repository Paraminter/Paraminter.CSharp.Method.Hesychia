namespace Paraminter.CSharp.Method.Hesychia;

using Moq;

using Paraminter.Associators.Queries;

using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.CSharp.Method.Queries.Handlers;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>>> paramsArgumentIdentifierMock = new();

        SyntacticCSharpMethodAssociator sut = new(paramsArgumentIdentifierMock.Object);

        return new Fixture(sut, paramsArgumentIdentifierMock);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>, IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler> Sut;

        private readonly Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>>> ParamsArgumentIdentifierMock;

        public Fixture(
            IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>, IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler> sut,
            Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>>> paramsArgumentIdentifierMock)
        {
            Sut = sut;

            ParamsArgumentIdentifierMock = paramsArgumentIdentifierMock;
        }

        IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>, IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler> IFixture.Sut => Sut;

        Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>>> IFixture.ParamsArgumentIdentifierMock => ParamsArgumentIdentifierMock;
    }
}
