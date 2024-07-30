namespace Paraminter.CSharp.Method.Hesychia;

using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

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
        private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>> Sut;

        public Fixture(
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>> sut)
        {
            Sut = sut;
        }

        IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>> IFixture.Sut => Sut;
    }
}
