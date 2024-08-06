namespace Paraminter.CSharp.Method.Hesychia;

using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Queries.Handlers;

internal interface IFixture
{
    public abstract IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> Sut { get; }
}
