namespace Paraminter.CSharp.Method.Hesychia;

using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Method.Hesychia.Queries;

internal interface IFixture
{
    public abstract IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> Sut { get; }
}
