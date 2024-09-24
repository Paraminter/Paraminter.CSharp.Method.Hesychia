namespace Paraminter.Associating.CSharp.Method.Hesychia;

using Paraminter.Associating.CSharp.Method.Hesychia.Queries;
using Paraminter.Cqs.Handlers;

internal interface IFixture
{
    public abstract IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> Sut { get; }
}
