namespace Paraminter.CSharp.Method.Hesychia.Common;

using Microsoft.CodeAnalysis;

using Paraminter.CSharp.Method.Commands;

internal sealed class AddDefaultCSharpMethodCommand
    : IAddDefaultCSharpMethodCommand
{
    private readonly IParameterSymbol Parameter;

    public AddDefaultCSharpMethodCommand(
        IParameterSymbol parameter)
    {
        Parameter = parameter;
    }

    IParameterSymbol IAddDefaultCSharpMethodCommand.Parameter => Parameter;
}
