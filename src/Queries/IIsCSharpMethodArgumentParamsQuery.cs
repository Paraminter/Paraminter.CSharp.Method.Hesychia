namespace Paraminter.Associating.CSharp.Method.Hesychia.Queries;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Cqs;

/// <summary>Represents a query for whether a syntactic C# method argument is a <see langword="params"/> argument.</summary>
public interface IIsCSharpMethodArgumentParamsQuery
    : IQuery
{
    /// <summary>The method parameter.</summary>
    public abstract IParameterSymbol Parameter { get; }

    /// <summary>The syntactic C# method argument.</summary>
    public abstract ArgumentSyntax SyntacticArgument { get; }

    /// <summary>A semantic model describing the syntactic argument.</summary>
    public abstract SemanticModel SemanticModel { get; }
}
