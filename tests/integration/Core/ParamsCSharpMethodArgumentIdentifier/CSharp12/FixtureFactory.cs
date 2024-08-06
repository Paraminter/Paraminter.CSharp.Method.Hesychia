namespace Paraminter.CSharp.Method.Hesychia;

using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Queries.Handlers;

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
        private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> Sut;

        public Fixture(
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> sut)
        {
            Sut = sut;
        }

        IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> IFixture.Sut => Sut;
    }
}
