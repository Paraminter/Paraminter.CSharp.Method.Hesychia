namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal sealed class HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand
    : IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand
{
    private readonly ArgumentSyntax SyntacticUnlabeledArgument;

    public HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(
        ArgumentSyntax syntacticUnlabeledArgument)
    {
        SyntacticUnlabeledArgument = syntacticUnlabeledArgument;
    }

    ArgumentSyntax IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand.SyntacticUnlabeledArgument => SyntacticUnlabeledArgument;
}
