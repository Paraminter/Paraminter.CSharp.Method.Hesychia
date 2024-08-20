namespace Paraminter.CSharp.Method.Hesychia.Errors.Commands;

using Paraminter.Cqs;

/// <summary>Represents a command to handle an error encountered when associating C# method arguments with parameters, caused by there being multiple parameters with the same name.</summary>
public interface IHandleDuplicateParameterNamesCommand
    : ICommand
{
    /// <summary>The name of the parameters.</summary>
    public abstract string ParameterName { get; }
}
