namespace Paraminter.CSharp.Method.Hesychia.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.CSharp.Method.Commands;

internal sealed class AddNormalCSharpMethodAssociationCommand
    : IAddNormalCSharpMethodAssociationCommand
{
    private readonly IParameterSymbol Parameter;
    private readonly ArgumentSyntax SyntacticArgument;

    public AddNormalCSharpMethodAssociationCommand(
        IParameterSymbol parameter,
        ArgumentSyntax syntacticArgument)
    {
        Parameter = parameter;
        SyntacticArgument = syntacticArgument;
    }

    IParameterSymbol IAddNormalCSharpMethodAssociationCommand.Parameter => Parameter;
    ArgumentSyntax IAddNormalCSharpMethodAssociationCommand.SyntacticArgument => SyntacticArgument;
}
