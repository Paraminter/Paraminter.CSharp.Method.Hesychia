namespace Paraminter.Associating.CSharp.Method.Hesychia;

using Paraminter.Associating.CSharp.Method.Hesychia.Queries;
using Paraminter.Cqs;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        CSharpMethodParamsArgumentDistinguisher sut = new();

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
