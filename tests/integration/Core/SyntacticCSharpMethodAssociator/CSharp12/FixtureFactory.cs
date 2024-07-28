namespace Paraminter.CSharp.Method.Hesychia;

using Moq;

using Paraminter.Associators.Queries;

using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.CSharp.Method.Queries.Collectors;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Collectors;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>>> paramsArgumentIdentifierMock = new();

        SyntacticCSharpMethodAssociator sut = new(paramsArgumentIdentifierMock.Object);

        return new Fixture(sut, paramsArgumentIdentifierMock);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>, IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> Sut;

        private readonly Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>>> ParamsArgumentIdentifierMock;

        public Fixture(
            IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>, IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> sut,
            Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>>> paramsArgumentIdentifierMock)
        {
            Sut = sut;

            ParamsArgumentIdentifierMock = paramsArgumentIdentifierMock;
        }

        IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>, IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> IFixture.Sut => Sut;

        Mock<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>>> IFixture.ParamsArgumentIdentifierMock => ParamsArgumentIdentifierMock;
    }
}
