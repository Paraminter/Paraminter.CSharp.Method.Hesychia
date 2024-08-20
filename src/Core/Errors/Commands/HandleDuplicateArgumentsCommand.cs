namespace Paraminter.CSharp.Method.Hesychia.Errors.Commands;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Parameters.Method.Models;

internal sealed class HandleDuplicateArgumentsCommand
    : IHandleDuplicateArgumentsCommand
{
    private readonly IMethodParameter Parameter;
    private readonly ArgumentSyntax SyntacticArgument;

    public HandleDuplicateArgumentsCommand(
        IMethodParameter parameter,
        ArgumentSyntax syntacticArgument)
    {
        Parameter = parameter;
        SyntacticArgument = syntacticArgument;
    }

    IMethodParameter IHandleDuplicateArgumentsCommand.Parameter => Parameter;
    ArgumentSyntax IHandleDuplicateArgumentsCommand.SyntacticArgument => SyntacticArgument;
}
