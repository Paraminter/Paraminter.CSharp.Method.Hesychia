namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Method.Hesychia.Common;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.CSharp.Method.Queries.Handlers;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Associates syntactic C# method arguments.</summary>
public sealed class SyntacticCSharpMethodAssociator
    : IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>, IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler>
{
    private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>> ParamsArgumentIdentifier;

    /// <summary>Instantiates a <see cref="SyntacticCSharpMethodAssociator"/>, associating syntactic C# method arguments.</summary>
    /// <param name="paramsArgumentIdentifier">Identifies <see langword="params"/> arguments.</param>
    public SyntacticCSharpMethodAssociator(
        IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>> paramsArgumentIdentifier)
    {
        ParamsArgumentIdentifier = paramsArgumentIdentifier ?? throw new ArgumentNullException(nameof(paramsArgumentIdentifier));
    }

    void IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>, IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler>.Handle(
        IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData> query,
        IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler queryResponseHandler)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (queryResponseHandler is null)
        {
            throw new ArgumentNullException(nameof(queryResponseHandler));
        }

        Associator.Associate(ParamsArgumentIdentifier, query, queryResponseHandler);
    }

    private sealed class Associator
    {
        public static void Associate(
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>> paramsArgumentIdentifier,
            IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData> query,
            IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler queryResponseHandler)
        {
            var associator = new Associator(paramsArgumentIdentifier, query, queryResponseHandler);

            associator.Associate();
        }

        private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>> ParamsArgumentIdentifier;

        private readonly IAssociateSyntacticCSharpMethodData UnassociatedInvocationData;
        private readonly IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler QueryResponseHandler;

        private readonly IDictionary<string, IParameterSymbol> UnparsedParametersByName;

        private bool HasEncounteredOutOfOrderLabelledArgument;
        private bool HasEncounteredParamsArgument;
        private bool HasEncounteredError;

        private Associator(
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>> paramsArgumentIdentifier,
            IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData> query,
            IInvalidatingAssociateSyntacticCSharpMethodQueryResponseHandler queryResponseHandler)
        {
            ParamsArgumentIdentifier = paramsArgumentIdentifier;
            UnassociatedInvocationData = query.Data;
            QueryResponseHandler = queryResponseHandler;

            UnparsedParametersByName = new Dictionary<string, IParameterSymbol>(query.Data.Parameters.Count, StringComparer.Ordinal);
        }

        private void Associate()
        {
            ResetUnparsedParametersByNameDictionary();

            AssociateSpecifiedArguments();
            ValidateUnspecifiedArguments();

            if (HasEncounteredError)
            {
                QueryResponseHandler.Invalidator.Handle(InvalidateQueryResponseCommand.Instance);
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
            foreach (var parameter in UnparsedParametersByName.Values)
            {
                if (parameter.IsOptional)
                {
                    var command = new AddDefaultCSharpMethodCommand(parameter);

                    QueryResponseHandler.AssociationCollector.Default.Handle(command);

                    continue;
                }

                if (parameter.IsParams)
                {
                    var command = new AddParamsCSharpMethodAssociationCommand(parameter, []);

                    QueryResponseHandler.AssociationCollector.Params.Handle(command);

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

            UnparsedParametersByName.Remove(UnassociatedInvocationData.Parameters[index].Name);

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
            var command = new AddNormalCSharpMethodAssociationCommand(UnassociatedInvocationData.Parameters[index], UnassociatedInvocationData.SyntacticArguments[index]);

            QueryResponseHandler.AssociationCollector.Normal.Handle(command);
        }

        private void AssociateNameColonArgument(
            int index,
            NameColonSyntax nameColonSyntax)
        {
            if (nameColonSyntax.Name.Identifier.Text != UnassociatedInvocationData.Parameters[index].Name)
            {
                HasEncounteredOutOfOrderLabelledArgument = true;
            }

            if (UnparsedParametersByName.TryGetValue(nameColonSyntax.Name.Identifier.Text, out var parameter) is false)
            {
                HasEncounteredError = true;

                return;
            }

            UnparsedParametersByName.Remove(nameColonSyntax.Name.Identifier.Text);

            var command = new AddNormalCSharpMethodAssociationCommand(parameter, UnassociatedInvocationData.SyntacticArguments[index]);

            QueryResponseHandler.AssociationCollector.Normal.Handle(command);
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
            var command = new AddParamsCSharpMethodAssociationCommand(UnassociatedInvocationData.Parameters[index], syntacticArguments);

            QueryResponseHandler.AssociationCollector.Params.Handle(command);

            HasEncounteredParamsArgument = true;
        }

        private void ResetUnparsedParametersByNameDictionary()
        {
            UnparsedParametersByName.Clear();

            foreach (var parameter in UnassociatedInvocationData.Parameters)
            {
                if (UnparsedParametersByName.ContainsKey(parameter.Name))
                {
                    HasEncounteredError = true;

                    return;
                }

                UnparsedParametersByName.Add(parameter.Name, parameter);
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
            var queryResponseHandler = new ValuedQueryResponseHandler<bool>();

            ParamsArgumentIdentifier.Handle(query, queryResponseHandler);

            if (queryResponseHandler.HasSetValue is false)
            {
                HasEncounteredError = true;

                return false;
            }

            return queryResponseHandler.GetValue();
        }
    }
}
