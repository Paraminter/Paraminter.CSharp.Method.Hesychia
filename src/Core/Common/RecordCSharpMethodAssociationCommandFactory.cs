namespace Paraminter.CSharp.Method.Hesychia.Common;

using Paraminter.Associators.Commands;
using Paraminter.Associators.Models;
using Paraminter.Parameters.Method.Models;

internal static class RecordCSharpMethodAssociationCommandFactory
{
    public static IRecordArgumentAssociationCommand<IMethodParameter, TArgumentData> Create<TArgumentData>(
        IMethodParameter parameter,
        TArgumentData argumentData)
        where TArgumentData : IArgumentData
    {
        return new RecordCSharpMethodAssociationCommand<TArgumentData>(parameter, argumentData);
    }

    private sealed class RecordCSharpMethodAssociationCommand<TArgumentData>
        : IRecordArgumentAssociationCommand<IMethodParameter, TArgumentData>
        where TArgumentData : IArgumentData
    {
        private readonly IMethodParameter Parameter;
        private readonly TArgumentData ArgumentData;

        public RecordCSharpMethodAssociationCommand(
            IMethodParameter parameter,
            TArgumentData argumentData)
        {
            Parameter = parameter;
            ArgumentData = argumentData;
        }

        IMethodParameter IRecordArgumentAssociationCommand<IMethodParameter, TArgumentData>.Parameter => Parameter;
        TArgumentData IRecordArgumentAssociationCommand<IMethodParameter, TArgumentData>.ArgumentData => ArgumentData;
    }
}
