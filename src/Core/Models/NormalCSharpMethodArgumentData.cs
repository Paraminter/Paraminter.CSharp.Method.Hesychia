namespace Paraminter.Associating.CSharp.Method.Hesychia.Models;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Arguments.CSharp.Method.Models;

internal sealed class NormalCSharpMethodArgumentData
    : INormalCSharpMethodArgumentData
{
    private readonly ArgumentSyntax SyntacticArgument;

    public NormalCSharpMethodArgumentData(
        ArgumentSyntax syntacticArgument)
    {
        SyntacticArgument = syntacticArgument;
    }

    ArgumentSyntax INormalCSharpMethodArgumentData.SyntacticArgument => SyntacticArgument;
}
