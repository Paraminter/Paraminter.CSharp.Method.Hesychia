namespace Paraminter.CSharp.Method.Hesychia.Commands;

using Paraminter.Arguments.Models;
using Paraminter.Commands;
using Paraminter.Parameters.Method.Models;

internal static class AssociateSingleArgumentCommandFactory
{
    public static IAssociateSingleArgumentCommand<IMethodParameter, TArgumentData> Create<TArgumentData>(
        IMethodParameter parameter,
        TArgumentData argumentData)
        where TArgumentData : IArgumentData
    {
        return new AssociateSingleArgumentCommand<TArgumentData>(parameter, argumentData);
    }

    private sealed class AssociateSingleArgumentCommand<TArgumentData>
        : IAssociateSingleArgumentCommand<IMethodParameter, TArgumentData>
        where TArgumentData : IArgumentData
    {
        private readonly IMethodParameter Parameter;
        private readonly TArgumentData ArgumentData;

        public AssociateSingleArgumentCommand(
            IMethodParameter parameter,
            TArgumentData argumentData)
        {
            Parameter = parameter;
            ArgumentData = argumentData;
        }

        IMethodParameter IAssociateSingleArgumentCommand<IMethodParameter, TArgumentData>.Parameter => Parameter;
        TArgumentData IAssociateSingleArgumentCommand<IMethodParameter, TArgumentData>.ArgumentData => ArgumentData;
    }
}
