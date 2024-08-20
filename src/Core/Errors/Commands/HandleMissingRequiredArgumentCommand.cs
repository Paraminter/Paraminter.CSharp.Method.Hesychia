namespace Paraminter.CSharp.Method.Hesychia.Errors.Commands;

using Paraminter.Parameters.Method.Models;

internal sealed class HandleMissingRequiredArgumentCommand
    : IHandleMissingRequiredArgumentCommand
{
    private readonly IMethodParameter Parameter;

    public HandleMissingRequiredArgumentCommand(
        IMethodParameter parameter)
    {
        Parameter = parameter;
    }

    IMethodParameter IHandleMissingRequiredArgumentCommand.Parameter => Parameter;
}
