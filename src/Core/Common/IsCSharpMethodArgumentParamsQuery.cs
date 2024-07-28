namespace Paraminter.CSharp.Method.Hesychia.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.CSharp.Method.Hesychia.Queries;

internal sealed class IsCSharpMethodArgumentParamsQuery
    : IIsCSharpMethodArgumentParamsQuery
{
    private readonly IParameterSymbol Parameter;
    private readonly ArgumentSyntax SyntacticArgument;
    private readonly SemanticModel SemanticModel;

    public IsCSharpMethodArgumentParamsQuery(
        IParameterSymbol parameter,
        ArgumentSyntax syntacticArgument,
        SemanticModel semanticModel)
    {
        Parameter = parameter;
        SyntacticArgument = syntacticArgument;
        SemanticModel = semanticModel;
    }

    IParameterSymbol IIsCSharpMethodArgumentParamsQuery.Parameter => Parameter;
    ArgumentSyntax IIsCSharpMethodArgumentParamsQuery.SyntacticArgument => SyntacticArgument;
    SemanticModel IIsCSharpMethodArgumentParamsQuery.SemanticModel => SemanticModel;
}
