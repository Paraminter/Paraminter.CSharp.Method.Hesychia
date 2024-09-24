namespace Paraminter.Associating.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Associating.Commands;
using Paraminter.Associating.CSharp.Method.Hesychia.Commands;
using Paraminter.Associating.CSharp.Method.Hesychia.Errors;
using Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;
using Paraminter.Associating.CSharp.Method.Hesychia.Models;
using Paraminter.Associating.CSharp.Method.Hesychia.Queries;
using Paraminter.Cqs.Handlers;
using Paraminter.Pairing.Commands;
using Paraminter.Parameters.Method.Models;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Associates syntactic C# method arguments with parameters.</summary>
public sealed class CSharpMethodAssociator
    : ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>>
{
    private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> NormalPairer;
    private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> ParamsPairer;
    private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> DefaultPairer;

    private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> ParamsArgumentDistinguisher;

    private readonly ICSharpMethodAssociatorErrorHandler ErrorHandler;

    /// <summary>Instantiates an associator of syntactic C# method arguments with parameters.</summary>
    /// <param name="normalPairer">Pairs normal syntactic C# method arguments with parameters.</param>
    /// <param name="paramsPairer">Pairs <see langword="params"/> syntactic C# method arguments with parameters.</param>
    /// <param name="defaultPairer">Pairs default syntactic C# method arguments with parameters.</param>
    /// <param name="paramsArgumentDistinguisher">Distinguishes between <see langword="params"/> and non-<see langword="params"/> syntactic C# method arguments.</param>
    /// <param name="errorHandler">Handles encountered errors.</param>
    public CSharpMethodAssociator(
        ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalPairer,
        ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsPairer,
        ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultPairer,
        IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentDistinguisher,
        ICSharpMethodAssociatorErrorHandler errorHandler)
    {
        NormalPairer = normalPairer ?? throw new ArgumentNullException(nameof(normalPairer));
        ParamsPairer = paramsPairer ?? throw new ArgumentNullException(nameof(paramsPairer));
        DefaultPairer = defaultPairer ?? throw new ArgumentNullException(nameof(defaultPairer));

        ParamsArgumentDistinguisher = paramsArgumentDistinguisher ?? throw new ArgumentNullException(nameof(paramsArgumentDistinguisher));

        ErrorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    void ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>>.Handle(
        IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData> command)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        Associator.Associate(NormalPairer, ParamsPairer, DefaultPairer, ParamsArgumentDistinguisher, ErrorHandler, command);
    }

    private sealed class Associator
    {
        public static void Associate(
            ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalPairer,
            ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsPairer,
            ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultPairer,
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentDistinguisher,
            ICSharpMethodAssociatorErrorHandler errorHandler,
            IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData> command)
        {
            var associator = new Associator(normalPairer, paramsPairer, defaultPairer, paramsArgumentDistinguisher, errorHandler, command);

            associator.Associate();
        }

        private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> NormalPairer;
        private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> ParamsPairer;
        private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> DefaultPairer;

        private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> ParamsArgumentDistinguisher;

        private readonly ICSharpMethodAssociatorErrorHandler ErrorHandler;

        private readonly IAssociateCSharpMethodArgumentsData UnassociatedInvocationData;

        private readonly IDictionary<string, ParameterStatus> ParametersByName;

        private bool HasEncounteredOutOfOrderLabelledArgument;
        private bool HasEncounteredParamsArgument;

        private Associator(
            ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalPairer,
            ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsPairer,
            ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultPairer,
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentDistinguisher,
            ICSharpMethodAssociatorErrorHandler errorHandler,
            IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData> command)
        {
            NormalPairer = normalPairer;
            ParamsPairer = paramsPairer;
            DefaultPairer = defaultPairer;

            ParamsArgumentDistinguisher = paramsArgumentDistinguisher;

            ErrorHandler = errorHandler;

            UnassociatedInvocationData = command.Data;

            ParametersByName = new Dictionary<string, ParameterStatus>(command.Data.Parameters.Count, StringComparer.Ordinal);
        }

        private void Associate()
        {
            ResetParametersByNameDictionary();

            PairSpecifiedArguments();
            ValidateUnpairedArguments();
        }

        private void PairSpecifiedArguments()
        {
            var maximumNumberOfSpecifiedArguments = Math.Min(UnassociatedInvocationData.Parameters.Count, UnassociatedInvocationData.SyntacticArguments.Count);

            for (var i = 0; i < maximumNumberOfSpecifiedArguments; i++)
            {
                PairArgumentAtIndex(i);

                if (HasEncounteredParamsArgument)
                {
                    break;
                }
            }
        }

        private void ValidateUnpairedArguments()
        {
            var unpairedParameters = ParametersByName.Values.Where(static (parsableParameter) => parsableParameter.HasBeenPaired is false);

            foreach (var parameterSymbol in unpairedParameters.Select(static (parsableParameter) => parsableParameter.Symbol))
            {
                if (parameterSymbol.IsOptional)
                {
                    PairDefaultArgument(parameterSymbol);

                    continue;
                }

                if (parameterSymbol.IsParams)
                {
                    PairParamsArgument(parameterSymbol, []);

                    continue;
                }

                HandleMissingRequiredArgument(parameterSymbol);
            }
        }

        private void PairArgumentAtIndex(
            int index)
        {
            if (UnassociatedInvocationData.SyntacticArguments[index].NameColon is NameColonSyntax nameColonSyntax)
            {
                PairNameColonArgumentAtIndex(index, nameColonSyntax);

                return;
            }

            if (HasEncounteredOutOfOrderLabelledArgument)
            {
                HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(UnassociatedInvocationData.SyntacticArguments[index]);

                return;
            }

            if (UnassociatedInvocationData.Parameters[index].IsParams)
            {
                PairParamsParameterArgumentAtIndex(index);

                return;
            }

            PairNormalArgumentAtIndex(index);
        }

        private void PairNormalArgumentAtIndex(
            int index)
        {
            PairNormalArgument(UnassociatedInvocationData.Parameters[index], UnassociatedInvocationData.SyntacticArguments[index]);
        }

        private void PairNameColonArgumentAtIndex(
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

            if (parameterStatus.HasBeenPaired)
            {
                HandleDuplicateArgumentsCommand(parameterStatus.Symbol, UnassociatedInvocationData.SyntacticArguments[index]);

                return;
            }

            PairNormalArgument(parameterStatus.Symbol, UnassociatedInvocationData.SyntacticArguments[index]);
        }

        private void PairParamsParameterArgumentAtIndex(
            int index)
        {
            if (HasAtLeastConstructorArguments(index + 2))
            {
                var syntacticArguments = CollectSyntacticParamsArgument(index);

                PairParamsArgumentAtIndex(index, syntacticArguments);

                return;
            }

            if (IsParamsArgument(index) is false)
            {
                PairNormalArgumentAtIndex(index);

                return;
            }

            PairParamsArgumentAtIndex(index, [UnassociatedInvocationData.SyntacticArguments[index]]);
        }

        private void PairParamsArgumentAtIndex(
            int index,
            IReadOnlyList<ArgumentSyntax> syntacticArguments)
        {
            PairParamsArgument(UnassociatedInvocationData.Parameters[index], syntacticArguments);
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

        private void PairNormalArgument(
            IParameterSymbol parameterSymbol,
            ArgumentSyntax syntacticArgument)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = new NormalCSharpMethodArgumentData(syntacticArgument);

            var command = PairArgumentCommandFactory.Create(parameter, argumentData);

            NormalPairer.Handle(command);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);
        }

        private void PairParamsArgument(
            IParameterSymbol parameterSymbol,
            IReadOnlyList<ArgumentSyntax> syntacticArguments)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = new ParamsCSharpMethodArgumentData(syntacticArguments);

            var command = PairArgumentCommandFactory.Create(parameter, argumentData);

            ParamsPairer.Handle(command);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);

            HasEncounteredParamsArgument = true;
        }

        private void PairDefaultArgument(
            IParameterSymbol parameterSymbol)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = DefaultCSharpMethodArgumentData.Instance;

            var command = PairArgumentCommandFactory.Create(parameter, argumentData);

            DefaultPairer.Handle(command);

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
            public bool HasBeenPaired { get; }

            public ParameterStatus(
                IParameterSymbol symbol,
                bool hasBeenParsed)
            {
                Symbol = symbol;
                HasBeenPaired = hasBeenParsed;
            }
        }
    }
}
