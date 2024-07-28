namespace Paraminter.CSharp.Method.Hesychia;

using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Collectors;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        ParamsCSharpMethodArgumentIdentifier sut = new();

        return new Fixture(sut);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>> Sut;

        public Fixture(
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>> sut)
        {
            Sut = sut;
        }

        IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>> IFixture.Sut => Sut;
    }
}
