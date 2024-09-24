namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;

using Paraminter.Cqs;
using Paraminter.Parameters.Method.Models;

/// <summary>Represents a command to handle an error encountered when associating syntactic C# method arguments with parameters, caused by missing a required argument.</summary>
public interface IHandleMissingRequiredArgumentCommand
    : ICommand
{
    /// <summary>The parameter.</summary>
    public abstract IMethodParameter Parameter { get; }
}
