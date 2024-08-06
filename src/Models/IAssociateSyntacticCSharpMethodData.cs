namespace Paraminter.CSharp.Method.Hesychia.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Associators.Models;

using System.Collections.Generic;

/// <summary>Represents data used to associate syntactic C# method arguments.</summary>
public interface IAssociateSyntacticCSharpMethodData
    : IAssociateArgumentsData
{
    /// <summary>The method parameters.</summary>
    public abstract IReadOnlyList<IParameterSymbol> Parameters { get; }

    /// <summary>The syntactic C# method arguments.</summary>
    public abstract IReadOnlyList<ArgumentSyntax> SyntacticArguments { get; }

    /// <summary>A semantic model describing the syntactic arguments.</summary>
    public abstract SemanticModel SemanticModel { get; }
}
