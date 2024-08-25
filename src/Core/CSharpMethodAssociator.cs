namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Method.Hesychia.Commands;
using Paraminter.CSharp.Method.Hesychia.Errors;
using Paraminter.CSharp.Method.Hesychia.Errors.Commands;
using Paraminter.CSharp.Method.Hesychia.Models;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Parameters.Method.Models;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Associates syntactic C# method arguments with parameters.</summary>
public sealed class CSharpMethodAssociator
    : ICommandHandler<IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData>>
{
    private readonly ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> NormalIndividualAssociator;
    private readonly ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> ParamsIndividualAssociator;
    private readonly ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> DefaultIndividualAssociator;

    private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> ParamsArgumentDistinguisher;

    private readonly ICSharpMethodAssociatorErrorHandler ErrorHandler;

    /// <summary>Instantiates an associator of syntactic C# method arguments with parameters.</summary>
    /// <param name="normalIndividualAssociator">Associates individual normal syntactic C# method arguments with parameters.</param>
    /// <param name="paramsIndividualAssociator">Associates individual <see langword="params"/> syntactic C# method arguments with parameters.</param>
    /// <param name="defaultIndividualAssociator">Associates individual default syntactic C# method arguments with parameters.</param>
    /// <param name="paramsArgumentDistinguisher">Distinguishes between <see langword="params"/> and non-<see langword="params"/> syntactic C# method arguments.</param>
    /// <param name="errorHandler">Handles encountered errors.</param>
    public CSharpMethodAssociator(
        ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalIndividualAssociator,
        ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsIndividualAssociator,
        ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultIndividualAssociator,
        IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentDistinguisher,
        ICSharpMethodAssociatorErrorHandler errorHandler)
    {
        NormalIndividualAssociator = normalIndividualAssociator ?? throw new ArgumentNullException(nameof(normalIndividualAssociator));
        ParamsIndividualAssociator = paramsIndividualAssociator ?? throw new ArgumentNullException(nameof(paramsIndividualAssociator));
        DefaultIndividualAssociator = defaultIndividualAssociator ?? throw new ArgumentNullException(nameof(defaultIndividualAssociator));

        ParamsArgumentDistinguisher = paramsArgumentDistinguisher ?? throw new ArgumentNullException(nameof(paramsArgumentDistinguisher));

        ErrorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    void ICommandHandler<IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData>>.Handle(
        IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData> command)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        Associator.Associate(NormalIndividualAssociator, ParamsIndividualAssociator, DefaultIndividualAssociator, ParamsArgumentDistinguisher, ErrorHandler, command);
    }

    private sealed class Associator
    {
        public static void Associate(
            ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalIndividualAssociator,
            ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsIndividualAssociator,
            ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultIndividualAssociator,
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentDistinguisher,
            ICSharpMethodAssociatorErrorHandler errorHandler,
            IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData> command)
        {
            var associator = new Associator(normalIndividualAssociator, paramsIndividualAssociator, defaultIndividualAssociator, paramsArgumentDistinguisher, errorHandler, command);

            associator.Associate();
        }

        private readonly ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> NormalIndividualAssociator;
        private readonly ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> ParamsIndividualAssociator;
        private readonly ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> DefaultIndividualAssociator;

        private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> ParamsArgumentDistinguisher;

        private readonly ICSharpMethodAssociatorErrorHandler ErrorHandler;

        private readonly IAssociateAllCSharpMethodArgumentsData UnassociatedInvocationData;

        private readonly IDictionary<string, ParameterStatus> ParametersByName;

        private bool HasEncounteredOutOfOrderLabelledArgument;
        private bool HasEncounteredParamsArgument;

        private Associator(
            ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalIndividualAssociator,
            ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsIndividualAssociator,
            ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultIndividualAssociator,
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentDistinguisher,
            ICSharpMethodAssociatorErrorHandler errorHandler,
            IAssociateAllArgumentsCommand<IAssociateAllCSharpMethodArgumentsData> command)
        {
            NormalIndividualAssociator = normalIndividualAssociator;
            ParamsIndividualAssociator = paramsIndividualAssociator;
            DefaultIndividualAssociator = defaultIndividualAssociator;

            ParamsArgumentDistinguisher = paramsArgumentDistinguisher;

            ErrorHandler = errorHandler;

            UnassociatedInvocationData = command.Data;

            ParametersByName = new Dictionary<string, ParameterStatus>(command.Data.Parameters.Count, StringComparer.Ordinal);
        }

        private void Associate()
        {
            ResetParametersByNameDictionary();

            AssociateSpecifiedArguments();
            ValidateUnassociatedArguments();
        }

        private void AssociateSpecifiedArguments()
        {
            var maximumNumberOfSpecifiedArguments = Math.Min(UnassociatedInvocationData.Parameters.Count, UnassociatedInvocationData.SyntacticArguments.Count);

            for (var i = 0; i < maximumNumberOfSpecifiedArguments; i++)
            {
                AssociateArgumentAtIndex(i);

                if (HasEncounteredParamsArgument)
                {
                    break;
                }
            }
        }

        private void ValidateUnassociatedArguments()
        {
            var unassociatedParameters = ParametersByName.Values.Where(static (parsableParameter) => parsableParameter.HasBeenAssociated is false);

            foreach (var parameterSymbol in unassociatedParameters.Select(static (parsableParameter) => parsableParameter.Symbol))
            {
                if (parameterSymbol.IsOptional)
                {
                    AssociateDefaultArgument(parameterSymbol);

                    continue;
                }

                if (parameterSymbol.IsParams)
                {
                    AssociateParamsArgument(parameterSymbol, []);

                    continue;
                }

                HandleMissingRequiredArgument(parameterSymbol);
            }
        }

        private void AssociateArgumentAtIndex(
            int index)
        {
            if (UnassociatedInvocationData.SyntacticArguments[index].NameColon is NameColonSyntax nameColonSyntax)
            {
                AssociateNameColonArgumentAtIndex(index, nameColonSyntax);

                return;
            }

            if (HasEncounteredOutOfOrderLabelledArgument)
            {
                HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(UnassociatedInvocationData.SyntacticArguments[index]);

                return;
            }

            if (UnassociatedInvocationData.Parameters[index].IsParams)
            {
                AssociateParamsParameterArgumentAtIndex(index);

                return;
            }

            AssociateNormalArgumentAtIndex(index);
        }

        private void AssociateNormalArgumentAtIndex(
            int index)
        {
            AssociateNormalArgument(UnassociatedInvocationData.Parameters[index], UnassociatedInvocationData.SyntacticArguments[index]);
        }

        private void AssociateNameColonArgumentAtIndex(
            int index,
            NameColonSyntax nameColonSyntax)
        {
            if (nameColonSyntax.Name.Identifier.Text != UnassociatedInvocationData.Parameters[index].Name)
            {
                HasEncounteredOutOfOrderLabelledArgument = true;
            }

            if (ParametersByName.TryGetValue(nameColonSyntax.Name.Identifier.Text, out var parameterStatus) is false)
            {
                HandleUnrecognizedLabeledArgumentCommand(UnassociatedInvocationData.SyntacticArguments[index]);

                return;
            }

            if (parameterStatus.HasBeenAssociated)
            {
                HandleDuplicateArgumentsCommand(parameterStatus.Symbol, UnassociatedInvocationData.SyntacticArguments[index]);

                return;
            }

            AssociateNormalArgument(parameterStatus.Symbol, UnassociatedInvocationData.SyntacticArguments[index]);
        }

        private void AssociateParamsParameterArgumentAtIndex(
            int index)
        {
            if (HasAtLeastConstructorArguments(index + 2))
            {
                var syntacticArguments = CollectSyntacticParamsArgument(index);

                AssociateParamsArgumentAtIndex(index, syntacticArguments);

                return;
            }

            if (IsParamsArgument(index) is false)
            {
                AssociateNormalArgumentAtIndex(index);

                return;
            }

            AssociateParamsArgumentAtIndex(index, [UnassociatedInvocationData.SyntacticArguments[index]]);
        }

        private void AssociateParamsArgumentAtIndex(
            int index,
            IReadOnlyList<ArgumentSyntax> syntacticArguments)
        {
            AssociateParamsArgument(UnassociatedInvocationData.Parameters[index], syntacticArguments);
        }

        private void ResetParametersByNameDictionary()
        {
            ParametersByName.Clear();

            foreach (var parameterSymbol in UnassociatedInvocationData.Parameters)
            {
                if (ParametersByName.ContainsKey(parameterSymbol.Name))
                {
                    HandleDuplicateParameterNamesCommand(parameterSymbol.Name);

                    continue;
                }

                var parameterStatus = new ParameterStatus(parameterSymbol, false);

                ParametersByName.Add(parameterSymbol.Name, parameterStatus);
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

            return ParamsArgumentDistinguisher.Handle(query);
        }

        private void AssociateNormalArgument(
            IParameterSymbol parameterSymbol,
            ArgumentSyntax syntacticArgument)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = new NormalCSharpMethodArgumentData(syntacticArgument);

            var command = AssociateSingleArgumentCommandFactory.Create(parameter, argumentData);

            NormalIndividualAssociator.Handle(command);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);
        }

        private void AssociateParamsArgument(
            IParameterSymbol parameterSymbol,
            IReadOnlyList<ArgumentSyntax> syntacticArguments)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = new ParamsCSharpMethodArgumentData(syntacticArguments);

            var command = AssociateSingleArgumentCommandFactory.Create(parameter, argumentData);

            ParamsIndividualAssociator.Handle(command);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);

            HasEncounteredParamsArgument = true;
        }

        private void AssociateDefaultArgument(
            IParameterSymbol parameterSymbol)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = DefaultCSharpMethodArgumentData.Instance;

            var command = AssociateSingleArgumentCommandFactory.Create(parameter, argumentData);

            DefaultIndividualAssociator.Handle(command);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);
        }

        private void HandleMissingRequiredArgument(
            IParameterSymbol parameterSymbol)
        {
            var parameter = new MethodParameter(parameterSymbol);

            var command = new HandleMissingRequiredArgumentCommand(parameter);

            ErrorHandler.MissingRequiredArgument.Handle(command);
        }

        private void HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(
            ArgumentSyntax syntacticUnlabeledArgument)
        {
            var command = new HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(syntacticUnlabeledArgument);

            ErrorHandler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(command);
        }

        private void HandleUnrecognizedLabeledArgumentCommand(
            ArgumentSyntax syntacticArgument)
        {
            var command = new HandleUnrecognizedLabeledArgumentCommand(syntacticArgument);

            ErrorHandler.UnrecognizedLabeledArgument.Handle(command);
        }

        private void HandleDuplicateParameterNamesCommand(
           string parameterName)
        {
            var command = new HandleDuplicateParameterNamesCommand(parameterName);

            ErrorHandler.DuplicateParameterNames.Handle(command);
        }

        private void HandleDuplicateArgumentsCommand(
            IParameterSymbol parameterSymbol,
            ArgumentSyntax syntacticArgument)
        {
            var parameter = new MethodParameter(parameterSymbol);

            var command = new HandleDuplicateArgumentsCommand(parameter, syntacticArgument);

            ErrorHandler.DuplicateArguments.Handle(command);
        }

        private readonly struct ParameterStatus
        {
            public IParameterSymbol Symbol { get; }
            public bool HasBeenAssociated { get; }

            public ParameterStatus(
                IParameterSymbol symbol,
                bool hasBeenParsed)
            {
                Symbol = symbol;
                HasBeenAssociated = hasBeenParsed;
            }
        }
    }
}
