namespace Paraminter.CSharp.Method.Hesychia.Errors.Commands;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Cqs;
using Paraminter.Parameters.Method.Models;

/// <summary>Represents a command to handle an error encountered when associating syntactic C# method arguments with parameters, caused by there being multiple arguments for the same parameter.</summary>
public interface IHandleDuplicateArgumentsCommand
    : ICommand
{
    /// <summary>The parameter.</summary>
    public abstract IMethodParameter Parameter { get; }

    /// <summary>The syntactic C# method argument.</summary>
    public abstract ArgumentSyntax SyntacticArgument { get; }
}
