namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;

using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Collectors;

using System;

/// <summary>Identifies <see langword="params"/> C# method arguments.</summary>
public sealed class ParamsCSharpMethodArgumentIdentifier
    : IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>>
{
    /// <summary>Instantiates a <see cref="ParamsCSharpMethodArgumentIdentifier"/>, identifying <see langword="params"/> C# method arguments.</summary>
    public ParamsCSharpMethodArgumentIdentifier() { }

    void IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>>.Handle(
        IIsCSharpMethodArgumentParamsQuery query,
        IValuedQueryResponseCollector<bool> queryResponseCollector)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (queryResponseCollector is null)
        {
            throw new ArgumentNullException(nameof(queryResponseCollector));
        }

        var result = Handle(query);

        queryResponseCollector.Value.Set(result);
    }

    private bool Handle(
        IIsCSharpMethodArgumentParamsQuery query)
    {
        if (query.Parameter.IsParams is false)
        {
            return false;
        }

        if (query.Parameter.Type is not IArrayTypeSymbol arrayType)
        {
            return false;
        }

        var expressedType = query.SemanticModel.GetTypeInfo(query.SyntacticArgument.Expression);

        return SymbolEqualityComparer.Default.Equals(expressedType.ConvertedType, arrayType.ElementType);
    }
}
