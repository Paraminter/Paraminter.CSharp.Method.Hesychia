namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;

/// <summary>Represents a command to handle an error encountered when associating syntactic C# method arguments with parameters, caused by there being multiple parameters with the same name.</summary>
public interface IHandleDuplicateParameterNamesCommand
    : ICommand
{
    /// <summary>The name of the parameters.</summary>
    public abstract string ParameterName { get; }
}
