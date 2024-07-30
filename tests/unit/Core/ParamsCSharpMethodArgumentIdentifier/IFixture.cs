namespace Paraminter.CSharp.Method.Hesychia;

using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

internal interface IFixture
{
    public abstract IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>> Sut { get; }
}
