namespace Paraminter.Associating.CSharp.Method.Hesychia.Commands;

using Paraminter.Arguments.Models;
using Paraminter.Pairing.Commands;
using Paraminter.Parameters.Method.Models;

internal static class PairArgumentCommandFactory
{
    public static IPairArgumentCommand<IMethodParameter, TArgumentData> Create<TArgumentData>(
        IMethodParameter parameter,
        TArgumentData argumentData)
        where TArgumentData : IArgumentData
    {
        return new PairArgumentCommand<TArgumentData>(parameter, argumentData);
    }

    private sealed class PairArgumentCommand<TArgumentData>
        : IPairArgumentCommand<IMethodParameter, TArgumentData>
        where TArgumentData : IArgumentData
    {
        private readonly IMethodParameter Parameter;
        private readonly TArgumentData ArgumentData;

        public PairArgumentCommand(
            IMethodParameter parameter,
            TArgumentData argumentData)
        {
            Parameter = parameter;
            ArgumentData = argumentData;
        }

        IMethodParameter IPairArgumentCommand<IMethodParameter, TArgumentData>.Parameter => Parameter;
        TArgumentData IPairArgumentCommand<IMethodParameter, TArgumentData>.ArgumentData => ArgumentData;
    }
}
