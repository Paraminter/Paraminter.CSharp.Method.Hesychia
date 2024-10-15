﻿namespace Paraminter.Associating.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;

using Paraminter.Associating.CSharp.Method.Hesychia.Queries;
using Paraminter.Cqs;

using System;

/// <summary>Distinguishes between <see langword="params"/> and non-<see langword="params"/> syntactic C# method arguments.</summary>
public sealed class CSharpMethodParamsArgumentDistinguisher
    : IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>
{
    /// <summary>Instantiates a <see cref="CSharpMethodParamsArgumentDistinguisher"/>, distinguishing between <see langword="params"/> and non-<see langword="params"/> syntactic C# method arguments.</summary>
    public CSharpMethodParamsArgumentDistinguisher() { }

    bool IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool>.Handle(
        IIsCSharpMethodArgumentParamsQuery query)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

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
