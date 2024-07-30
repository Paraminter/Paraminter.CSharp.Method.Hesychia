namespace Paraminter.CSharp.Method.Hesychia.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.CSharp.Method.Commands;

using System.Collections.Generic;

internal sealed class AddParamsCSharpMethodAssociationCommand
    : IAddParamsCSharpMethodAssociationCommand
{
    private readonly IParameterSymbol Parameter;
    private readonly IReadOnlyList<ArgumentSyntax> SyntacticArguments;

    public AddParamsCSharpMethodAssociationCommand(
        IParameterSymbol parameter,
        IReadOnlyList<ArgumentSyntax> syntacticArguments)
    {
        Parameter = parameter;
        SyntacticArguments = syntacticArguments;
    }

    IParameterSymbol IAddParamsCSharpMethodAssociationCommand.Parameter => Parameter;
    IReadOnlyList<ArgumentSyntax> IAddParamsCSharpMethodAssociationCommand.SyntacticArguments => SyntacticArguments;
}
