namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors;

using Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;
using Paraminter.Cqs;

/// <summary>Handles errors encountered when associating syntactic C# method arguments with parameters.</summary>
public interface ICSharpMethodAssociatorErrorHandler
{
    /// <summary>Handles a missing required argument.</summary>
    public abstract ICommandHandler<IHandleMissingRequiredArgumentCommand> MissingRequiredArgument { get; }

    /// <summary>Handles an out-of-order labeled argument followed by an unlabeled argument.</summary>
    public abstract ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand> OutOfOrderLabeledArgumentFollowedByUnlabeled { get; }

    /// <summary>Handles a labeled argument of an unrecognized parameter.</summary>
    public abstract ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand> UnrecognizedLabeledArgument { get; }

    /// <summary>Handles multiple parameters having the same name.</summary>
    public abstract ICommandHandler<IHandleDuplicateParameterNamesCommand> DuplicateParameterNames { get; }

    /// <summary>Handles multiple arguments for the same parameter.</summary>
    public abstract ICommandHandler<IHandleDuplicateArgumentsCommand> DuplicateArguments { get; }
}
