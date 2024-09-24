namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Cqs;

/// <summary>Represents a command to handle an error encountered when associating syntactic C# method arguments with parameters, caused by a labeled argument of an unrecognized parameter.</summary>
public interface IHandleUnrecognizedLabeledArgumentCommand
    : ICommand
{
    /// <summary>The syntactic C# method argument.</summary>
    public abstract ArgumentSyntax SyntacticArgument { get; }
}
