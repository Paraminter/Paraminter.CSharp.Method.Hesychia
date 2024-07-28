namespace Paraminter.CSharp.Method.Hesychia.Queries;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

/// <summary>Represents data used to associate syntactic C# method arguments.</summary>
public interface IAssociateSyntacticCSharpMethodData
{
    /// <summary>The method parameters.</summary>
    public abstract IReadOnlyList<IParameterSymbol> Parameters { get; }

    /// <summary>The syntactic C# method arguments.</summary>
    public abstract IReadOnlyList<ArgumentSyntax> SyntacticArguments { get; }

    /// <summary>A semantic model describing the syntactic arguments.</summary>
    public abstract SemanticModel SemanticModel { get; }
}
