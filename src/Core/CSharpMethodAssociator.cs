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
using Paraminter.Pairing.Commands;
using Paraminter.Parameters.Method.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    async Task ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData>>.Handle(
        IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData> command,
        CancellationToken cancellationToken)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        await Associator.Associate(NormalPairer, ParamsPairer, DefaultPairer, ParamsArgumentDistinguisher, ErrorHandler, command, cancellationToken).ConfigureAwait(false);
    }

    private sealed class Associator
    {
        public static async Task Associate(
            ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalPairer,
            ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsPairer,
            ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultPairer,
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentDistinguisher,
            ICSharpMethodAssociatorErrorHandler errorHandler,
            IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData> command,
            CancellationToken cancellationToken)
        {
            var associator = new Associator(normalPairer, paramsPairer, defaultPairer, paramsArgumentDistinguisher, errorHandler, command, cancellationToken);

            await associator.Associate().ConfigureAwait(false);
        }

        private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> NormalPairer;
        private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> ParamsPairer;
        private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> DefaultPairer;

        private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> ParamsArgumentDistinguisher;

        private readonly ICSharpMethodAssociatorErrorHandler ErrorHandler;

        private readonly IAssociateCSharpMethodArgumentsData UnassociatedInvocationData;

        private readonly IDictionary<string, ParameterStatus> ParametersByName;

        private readonly CancellationToken CancellationToken;

        private bool HasEncounteredOutOfOrderLabelledArgument;
        private bool HasEncounteredParamsArgument;

        private Associator(
            ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpMethodArgumentData>> normalPairer,
            ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpMethodArgumentData>> paramsPairer,
            ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>> defaultPairer,
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, bool> paramsArgumentDistinguisher,
            ICSharpMethodAssociatorErrorHandler errorHandler,
            IAssociateArgumentsCommand<IAssociateCSharpMethodArgumentsData> command,
            CancellationToken cancellationToken)
        {
            NormalPairer = normalPairer;
            ParamsPairer = paramsPairer;
            DefaultPairer = defaultPairer;

            ParamsArgumentDistinguisher = paramsArgumentDistinguisher;

            ErrorHandler = errorHandler;

            UnassociatedInvocationData = command.Data;

            ParametersByName = new Dictionary<string, ParameterStatus>(command.Data.Parameters.Count, StringComparer.Ordinal);

            CancellationToken = cancellationToken;
        }

        private async Task Associate()
        {
            await ResetParametersByNameDictionary().ConfigureAwait(false);

            await PairSpecifiedArguments().ConfigureAwait(false);
            await ValidateUnpairedArguments().ConfigureAwait(false);
        }

        private async Task PairSpecifiedArguments()
        {
            var maximumNumberOfSpecifiedArguments = Math.Min(UnassociatedInvocationData.Parameters.Count, UnassociatedInvocationData.SyntacticArguments.Count);

            for (var i = 0; i < maximumNumberOfSpecifiedArguments; i++)
            {
                await PairArgumentAtIndex(i).ConfigureAwait(false);

                if (HasEncounteredParamsArgument)
                {
                    break;
                }
            }
        }

        private async Task ValidateUnpairedArguments()
        {
            var unpairedParameters = ParametersByName.Values.Where(static (parsableParameter) => parsableParameter.HasBeenPaired is false);

            foreach (var parameterSymbol in unpairedParameters.Select(static (parsableParameter) => parsableParameter.Symbol))
            {
                if (parameterSymbol.IsOptional)
                {
                    await PairDefaultArgument(parameterSymbol).ConfigureAwait(false);

                    continue;
                }

                if (parameterSymbol.IsParams)
                {
                    await PairParamsArgument(parameterSymbol, []).ConfigureAwait(false);

                    continue;
                }

                await HandleMissingRequiredArgument(parameterSymbol).ConfigureAwait(false);
            }
        }

        private async Task PairArgumentAtIndex(
            int index)
        {
            if (UnassociatedInvocationData.SyntacticArguments[index].NameColon is NameColonSyntax nameColonSyntax)
            {
                await PairNameColonArgumentAtIndex(index, nameColonSyntax).ConfigureAwait(false);

                return;
            }

            if (HasEncounteredOutOfOrderLabelledArgument)
            {
                await HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(UnassociatedInvocationData.SyntacticArguments[index]).ConfigureAwait(false);

                return;
            }

            if (UnassociatedInvocationData.Parameters[index].IsParams)
            {
                await PairParamsParameterArgumentAtIndex(index).ConfigureAwait(false);

                return;
            }

            await PairNormalArgumentAtIndex(index).ConfigureAwait(false);
        }

        private async Task PairNormalArgumentAtIndex(
            int index)
        {
            await PairNormalArgument(UnassociatedInvocationData.Parameters[index], UnassociatedInvocationData.SyntacticArguments[index]).ConfigureAwait(false);
        }

        private async Task PairNameColonArgumentAtIndex(
            int index,
            NameColonSyntax nameColonSyntax)
        {
            if (nameColonSyntax.Name.Identifier.Text != UnassociatedInvocationData.Parameters[index].Name)
            {
                HasEncounteredOutOfOrderLabelledArgument = true;
            }

            if (ParametersByName.TryGetValue(nameColonSyntax.Name.Identifier.Text, out var parameterStatus) is false)
            {
                await HandleUnrecognizedLabeledArgumentCommand(UnassociatedInvocationData.SyntacticArguments[index]).ConfigureAwait(false);

                return;
            }

            if (parameterStatus.HasBeenPaired)
            {
                await HandleDuplicateArgumentsCommand(parameterStatus.Symbol, UnassociatedInvocationData.SyntacticArguments[index]).ConfigureAwait(false);

                return;
            }

            await PairNormalArgument(parameterStatus.Symbol, UnassociatedInvocationData.SyntacticArguments[index]).ConfigureAwait(false);
        }

        private async Task PairParamsParameterArgumentAtIndex(
            int index)
        {
            if (HasAtLeastConstructorArguments(index + 2))
            {
                var syntacticArguments = CollectSyntacticParamsArgument(index);

                await PairParamsArgumentAtIndex(index, syntacticArguments).ConfigureAwait(false);

                return;
            }

            if (await IsParamsArgument(index).ConfigureAwait(false) is false)
            {
                await PairNormalArgumentAtIndex(index).ConfigureAwait(false);

                return;
            }

            await PairParamsArgumentAtIndex(index, [UnassociatedInvocationData.SyntacticArguments[index]]).ConfigureAwait(false);
        }

        private async Task PairParamsArgumentAtIndex(
            int index,
            IReadOnlyList<ArgumentSyntax> syntacticArguments)
        {
            await PairParamsArgument(UnassociatedInvocationData.Parameters[index], syntacticArguments).ConfigureAwait(false);
        }

        private async Task ResetParametersByNameDictionary()
        {
            ParametersByName.Clear();

            foreach (var parameterSymbol in UnassociatedInvocationData.Parameters)
            {
                if (ParametersByName.ContainsKey(parameterSymbol.Name))
                {
                    await HandleDuplicateParameterNamesCommand(parameterSymbol.Name).ConfigureAwait(false);

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

        private async Task<bool> IsParamsArgument(
            int index)
        {
            var query = new IsCSharpMethodArgumentParamsQuery(UnassociatedInvocationData.Parameters[index], UnassociatedInvocationData.SyntacticArguments[index], UnassociatedInvocationData.SemanticModel);

            return await ParamsArgumentDistinguisher.Handle(query, CancellationToken).ConfigureAwait(false);
        }

        private async Task PairNormalArgument(
            IParameterSymbol parameterSymbol,
            ArgumentSyntax syntacticArgument)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = new NormalCSharpMethodArgumentData(syntacticArgument);

            var command = PairArgumentCommandFactory.Create(parameter, argumentData);

            await NormalPairer.Handle(command, CancellationToken).ConfigureAwait(false);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);
        }

        private async Task PairParamsArgument(
            IParameterSymbol parameterSymbol,
            IReadOnlyList<ArgumentSyntax> syntacticArguments)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = new ParamsCSharpMethodArgumentData(syntacticArguments);

            var command = PairArgumentCommandFactory.Create(parameter, argumentData);

            await ParamsPairer.Handle(command, CancellationToken).ConfigureAwait(false);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);

            HasEncounteredParamsArgument = true;
        }

        private async Task PairDefaultArgument(
            IParameterSymbol parameterSymbol)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = DefaultCSharpMethodArgumentData.Instance;

            var command = PairArgumentCommandFactory.Create(parameter, argumentData);

            await DefaultPairer.Handle(command, CancellationToken).ConfigureAwait(false);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);
        }

        private async Task HandleMissingRequiredArgument(
            IParameterSymbol parameterSymbol)
        {
            var parameter = new MethodParameter(parameterSymbol);

            var command = new HandleMissingRequiredArgumentCommand(parameter);

            await ErrorHandler.MissingRequiredArgument.Handle(command, CancellationToken).ConfigureAwait(false);
        }

        private async Task HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(
            ArgumentSyntax syntacticUnlabeledArgument)
        {
            var command = new HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(syntacticUnlabeledArgument);

            await ErrorHandler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(command, CancellationToken).ConfigureAwait(false);
        }

        private async Task HandleUnrecognizedLabeledArgumentCommand(
            ArgumentSyntax syntacticArgument)
        {
            var command = new HandleUnrecognizedLabeledArgumentCommand(syntacticArgument);

            await ErrorHandler.UnrecognizedLabeledArgument.Handle(command, CancellationToken).ConfigureAwait(false);
        }

        private async Task HandleDuplicateParameterNamesCommand(
           string parameterName)
        {
            var command = new HandleDuplicateParameterNamesCommand(parameterName);

            await ErrorHandler.DuplicateParameterNames.Handle(command, CancellationToken).ConfigureAwait(false);
        }

        private async Task HandleDuplicateArgumentsCommand(
            IParameterSymbol parameterSymbol,
            ArgumentSyntax syntacticArgument)
        {
            var parameter = new MethodParameter(parameterSymbol);

            var command = new HandleDuplicateArgumentsCommand(parameter, syntacticArgument);

            await ErrorHandler.DuplicateArguments.Handle(command, CancellationToken).ConfigureAwait(false);
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
