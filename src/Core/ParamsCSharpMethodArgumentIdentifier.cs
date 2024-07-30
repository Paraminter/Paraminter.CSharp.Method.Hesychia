namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;

using Paraminter.CSharp.Method.Hesychia.Common;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

using System;

/// <summary>Identifies <see langword="params"/> C# method arguments.</summary>
public sealed class ParamsCSharpMethodArgumentIdentifier
    : IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>>
{
    /// <summary>Instantiates a <see cref="ParamsCSharpMethodArgumentIdentifier"/>, identifying <see langword="params"/> C# method arguments.</summary>
    public ParamsCSharpMethodArgumentIdentifier() { }

    void IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>>.Handle(
        IIsCSharpMethodArgumentParamsQuery query,
        IValuedQueryResponseHandler<bool> queryResponseHandler)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (queryResponseHandler is null)
        {
            throw new ArgumentNullException(nameof(queryResponseHandler));
        }

        var result = Handle(query);

        var command = new SetQueryResponseValueCommand<bool>(result);

        queryResponseHandler.Value.Handle(command);
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
