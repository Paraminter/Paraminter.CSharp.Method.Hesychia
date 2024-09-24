namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal sealed class HandleUnrecognizedLabeledArgumentCommand
    : IHandleUnrecognizedLabeledArgumentCommand
{
    private readonly ArgumentSyntax SyntacticArgument;

    public HandleUnrecognizedLabeledArgumentCommand(
        ArgumentSyntax syntacticArgument)
    {
        SyntacticArgument = syntacticArgument;
    }

    ArgumentSyntax IHandleUnrecognizedLabeledArgumentCommand.SyntacticArgument => SyntacticArgument;
}
