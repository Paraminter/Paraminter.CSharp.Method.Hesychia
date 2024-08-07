namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Associators.Commands;
using Paraminter.Commands.Handlers;
using Paraminter.CSharp.Method.Hesychia.Common;
using Paraminter.CSharp.Method.Hesychia.Models;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Parameters.Method.Models;
using Paraminter.Queries.Handlers;
using Paraminter.Recorders.Commands;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Associates syntactic C# method arguments.</summary>
public sealed class SyntacticCSharpMethodAssociator
    : ICommandHandler<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>>
{
    private readonly ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>> NormalRecorder;
    private readonly ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> ParamsRecorder;
    private readonly ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> DefaultRecorder;

    private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> ParamsArgumentIdentifier;

    private readonly ICommandHandler<IInvalidateArgumentAssociationsRecordCommand> Invalidator;

    /// <summary>Instantiates a <see cref="SyntacticCSharpMethodAssociator"/>, associating syntactic C# method arguments.</summary>
    /// <param name="normalRecorder">Record associated normal C# method arguments.</param>
    /// <param name="paramsRecorder">Record associated <see langword="params"/> C# method arguments.</param>
    /// <param name="defaultRecorder">Record associated default C# method arguments.</param>
    /// <param name="paramsArgumentIdentifier">Identifies <see langword="params"/> arguments.</param>
    /// <param name="invalidator">Invalidates the record of associated syntactic C# method arguments.</param>
    public SyntacticCSharpMethodAssociator(
        ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalRecorder,
        ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsRecorder,
        ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultRecorder,
        IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentIdentifier,
        ICommandHandler<IInvalidateArgumentAssociationsRecordCommand> invalidator)
    {
        NormalRecorder = normalRecorder ?? throw new ArgumentNullException(nameof(normalRecorder));
        ParamsRecorder = paramsRecorder ?? throw new ArgumentNullException(nameof(paramsRecorder));
        DefaultRecorder = defaultRecorder ?? throw new ArgumentNullException(nameof(defaultRecorder));

        ParamsArgumentIdentifier = paramsArgumentIdentifier ?? throw new ArgumentNullException(nameof(paramsArgumentIdentifier));

        Invalidator = invalidator ?? throw new ArgumentNullException(nameof(invalidator));
    }

    void ICommandHandler<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>>.Handle(
        IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData> command)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        Associator.Associate(NormalRecorder, ParamsRecorder, DefaultRecorder, ParamsArgumentIdentifier, Invalidator, command);
    }

    private sealed class Associator
    {
        public static void Associate(
            ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalRecorder,
            ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsRecorder,
            ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultRecorder,
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentIdentifier,
            ICommandHandler<IInvalidateArgumentAssociationsRecordCommand> invalidator,
            IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData> command)
        {
            var associator = new Associator(normalRecorder, paramsRecorder, defaultRecorder, paramsArgumentIdentifier, invalidator, command);

            associator.Associate();
        }

        private readonly ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>> NormalRecorder;
        private readonly ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> ParamsRecorder;
        private readonly ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> DefaultRecorder;

        private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> ParamsArgumentIdentifier;

        private readonly ICommandHandler<IInvalidateArgumentAssociationsRecordCommand> Invalidator;

        private readonly IAssociateSyntacticCSharpMethodData UnassociatedInvocationData;

        private readonly IDictionary<string, IParameterSymbol> UnparsedParameterSymbolsByName;

        private bool HasEncounteredOutOfOrderLabelledArgument;
        private bool HasEncounteredParamsArgument;
        private bool HasEncounteredError;

        private Associator(
            ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalRecorder,
            ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsRecorder,
            ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultRecorder,
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentIdentifier,
            ICommandHandler<IInvalidateArgumentAssociationsRecordCommand> invalidator,
            IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData> command)
        {
            NormalRecorder = normalRecorder;
            ParamsRecorder = paramsRecorder;
            DefaultRecorder = defaultRecorder;

            ParamsArgumentIdentifier = paramsArgumentIdentifier;

            Invalidator = invalidator;

            UnassociatedInvocationData = command.Data;

            UnparsedParameterSymbolsByName = new Dictionary<string, IParameterSymbol>(command.Data.Parameters.Count, StringComparer.Ordinal);
        }

        private void Associate()
        {
            ResetUnparsedParameterSymbolsByNameDictionary();

            AssociateSpecifiedArguments();
            ValidateUnspecifiedArguments();

            if (HasEncounteredError)
            {
                Invalidator.Handle(InvalidateArgumentAssociationsRecordCommand.Instance);
            }
        }

        private void AssociateSpecifiedArguments()
        {
            if (HasEncounteredError)
            {
                return;
            }

            var maximumNumberOfSpecifiedArguments = Math.Min(UnassociatedInvocationData.Parameters.Count, UnassociatedInvocationData.SyntacticArguments.Count);

            for (var i = 0; i < maximumNumberOfSpecifiedArguments; i++)
            {
                AssociateArgument(i);

                if (HasEncounteredParamsArgument || HasEncounteredError)
                {
                    break;
                }
            }
        }

        private void ValidateUnspecifiedArguments()
        {
            foreach (var parameterSymbol in UnparsedParameterSymbolsByName.Values)
            {
                if (parameterSymbol.IsOptional)
                {
                    var parameter = new MethodParameter(parameterSymbol);

                    var recordCommand = RecordCSharpMethodAssociationCommandFactory.Create(parameter, DefaultCSharpMethodArgumentData.Instance);

                    DefaultRecorder.Handle(recordCommand);

                    continue;
                }

                if (parameterSymbol.IsParams)
                {
                    var parameter = new MethodParameter(parameterSymbol);
                    var argumentData = new ParamsCSharpMethodArgumentData([]);

                    var recordCommand = RecordCSharpMethodAssociationCommandFactory.Create(parameter, argumentData);

                    ParamsRecorder.Handle(recordCommand);

                    continue;
                }

                HasEncounteredError = true;

                return;
            }
        }

        private void AssociateArgument(
            int index)
        {
            if (UnassociatedInvocationData.SyntacticArguments[index].NameColon is NameColonSyntax nameColonSyntax)
            {
                AssociateNameColonArgument(index, nameColonSyntax);

                return;
            }

            if (HasEncounteredOutOfOrderLabelledArgument)
            {
                HasEncounteredError = true;

                return;
            }

            UnparsedParameterSymbolsByName.Remove(UnassociatedInvocationData.Parameters[index].Name);

            if (UnassociatedInvocationData.Parameters[index].IsParams)
            {
                AssociateParamsParameterArgument(index);

                return;
            }

            AssociateNormalArgument(index);
        }

        private void AssociateNormalArgument(
            int index)
        {
            var parameter = new MethodParameter(UnassociatedInvocationData.Parameters[index]);
            var argumentData = new NormalCSharpMethodArgumentData(UnassociatedInvocationData.SyntacticArguments[index]);

            var recordCommand = RecordCSharpMethodAssociationCommandFactory.Create(parameter, argumentData);

            NormalRecorder.Handle(recordCommand);
        }

        private void AssociateNameColonArgument(
            int index,
            NameColonSyntax nameColonSyntax)
        {
            if (nameColonSyntax.Name.Identifier.Text != UnassociatedInvocationData.Parameters[index].Name)
            {
                HasEncounteredOutOfOrderLabelledArgument = true;
            }

            if (UnparsedParameterSymbolsByName.TryGetValue(nameColonSyntax.Name.Identifier.Text, out var parameterSymbol) is false)
            {
                HasEncounteredError = true;

                return;
            }

            UnparsedParameterSymbolsByName.Remove(nameColonSyntax.Name.Identifier.Text);

            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = new NormalCSharpMethodArgumentData(UnassociatedInvocationData.SyntacticArguments[index]);

            var recordCommand = RecordCSharpMethodAssociationCommandFactory.Create(parameter, argumentData);

            NormalRecorder.Handle(recordCommand);
        }

        private void AssociateParamsParameterArgument(
            int index)
        {
            if (HasAtLeastConstructorArguments(index + 2))
            {
                var syntacticArguments = CollectSyntacticParamsArgument(index);

                AssociateParamsArgument(index, syntacticArguments);

                return;
            }

            if (IsParamsArgument(index) is false)
            {
                AssociateNormalArgument(index);

                return;
            }

            AssociateParamsArgument(index, [UnassociatedInvocationData.SyntacticArguments[index]]);
        }

        private void AssociateParamsArgument(
            int index,
            IReadOnlyList<ArgumentSyntax> syntacticArguments)
        {
            var parameter = new MethodParameter(UnassociatedInvocationData.Parameters[index]);
            var argumentData = new ParamsCSharpMethodArgumentData(syntacticArguments);

            var recordCommand = RecordCSharpMethodAssociationCommandFactory.Create(parameter, argumentData);

            ParamsRecorder.Handle(recordCommand);

            HasEncounteredParamsArgument = true;
        }

        private void ResetUnparsedParameterSymbolsByNameDictionary()
        {
            UnparsedParameterSymbolsByName.Clear();

            foreach (var parameterSymbol in UnassociatedInvocationData.Parameters)
            {
                if (UnparsedParameterSymbolsByName.ContainsKey(parameterSymbol.Name))
                {
                    HasEncounteredError = true;

                    return;
                }

                UnparsedParameterSymbolsByName.Add(parameterSymbol.Name, parameterSymbol);
            }
        }

        private IReadOnlyList<ArgumentSyntax> CollectSyntacticParamsArgument(
            int index)
        {
            var syntacticParamsArguments = new List<ArgumentSyntax>(UnassociatedInvocationData.SyntacticArguments.Count - index);

            foreach (var syntacticArgument in UnassociatedInvocationData.SyntacticArguments.Skip(index))
            {
                syntacticParamsArguments.Add(syntacticArgument);
            }

            return syntacticParamsArguments;
        }

        private bool HasAtLeastConstructorArguments(
            int amount)
        {
            return UnassociatedInvocationData.SyntacticArguments.Count >= amount;
        }

        private bool IsParamsArgument(
            int index)
        {
            var query = new IsCSharpMethodArgumentParamsQuery(UnassociatedInvocationData.Parameters[index], UnassociatedInvocationData.SyntacticArguments[index], UnassociatedInvocationData.SemanticModel);

            return ParamsArgumentIdentifier.Handle(query);
        }
    }
}
