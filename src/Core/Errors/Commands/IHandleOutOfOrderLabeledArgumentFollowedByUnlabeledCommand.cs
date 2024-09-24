namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Cqs;

/// <summary>Represents a command to handle an error encountered when associating syntactic C# method arguments with parameters, caused by an out-of-order labeled argument being followed by an unlabeled argument.</summary>
public interface IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand
    : ICommand
{
    /// <summary>The unlabeled syntactic C# method argument.</summary>
    public abstract ArgumentSyntax SyntacticUnlabeledArgument { get; }
}
