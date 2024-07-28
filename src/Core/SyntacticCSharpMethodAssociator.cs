namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Method.Hesychia.Common;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.CSharp.Method.Queries.Collectors;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Collectors;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Associates syntactic C# method arguments.</summary>
public sealed class SyntacticCSharpMethodAssociator
    : IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>, IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector>
{
    private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>> ParamsArgumentIdentifier;

    /// <summary>Instantiates a <see cref="SyntacticCSharpMethodAssociator"/>, associating syntactic C# method arguments.</summary>
    /// <param name="paramsArgumentIdentifier">Identifies <see langword="params"/> arguments.</param>
    public SyntacticCSharpMethodAssociator(
        IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>> paramsArgumentIdentifier)
    {
        ParamsArgumentIdentifier = paramsArgumentIdentifier ?? throw new ArgumentNullException(nameof(paramsArgumentIdentifier));
    }

    void IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>, IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector>.Handle(
        IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData> query,
        IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector queryResponseCollector)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (queryResponseCollector is null)
        {
            throw new ArgumentNullException(nameof(queryResponseCollector));
        }

        Associator.Associate(ParamsArgumentIdentifier, query, queryResponseCollector);
    }

    private sealed class Associator
    {
        public static void Associate(
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>> paramsArgumentIdentifier,
            IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData> query,
            IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector queryResponseCollector)
        {
            var associator = new Associator(paramsArgumentIdentifier, query, queryResponseCollector);

            associator.Associate();
        }

        private readonly IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>> ParamsArgumentIdentifier;

        private readonly IAssociateSyntacticCSharpMethodData UnassociatedInvocationData;
        private readonly IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector QueryResponseCollector;

        private readonly IDictionary<string, IParameterSymbol> UnparsedParametersByName;

        private bool HasEncounteredOutOfOrderLabelledArgument;
        private bool HasEncounteredParamsArgument;
        private bool HasEncounteredError;

        private Associator(
            IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseCollector<bool>> paramsArgumentIdentifier,
            IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData> query,
            IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector queryResponseCollector)
        {
            ParamsArgumentIdentifier = paramsArgumentIdentifier;
            UnassociatedInvocationData = query.Data;
            QueryResponseCollector = queryResponseCollector;

            UnparsedParametersByName = new Dictionary<string, IParameterSymbol>(query.Data.Parameters.Count, StringComparer.Ordinal);
        }

        private void Associate()
        {
            ResetUnparsedParametersByNameDictionary();

            AssociateSpecifiedArguments();
            ValidateUnspecifiedArguments();

            if (HasEncounteredError)
            {
                QueryResponseCollector.Invalidator.Invalidate();
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
                    QueryResponseCollector.Associations.Default.Add(parameter);

                    continue;
                }

                if (parameter.IsParams)
                {
                    QueryResponseCollector.Associations.Params.Add(parameter, []);

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
            QueryResponseCollector.Associations.Normal.Add(UnassociatedInvocationData.Parameters[index], UnassociatedInvocationData.SyntacticArguments[index]);
        }

        private void AssociateNameColonArgument(
            int index,
            NameColonSyntax nameColonSyntax)
        {
            if (nameColonSyntax.Name.Identifier.Text != UnassociatedInvocationData.Parameters[index].Name)
            {
                HasEncounteredOutOfOrderLabelledArgument = true;
            }

            if (UnparsedParametersByName.TryGetValue(nameColonSyntax.Name.Identifier.Text, out var parameterSymbol) is false)
            {
                HasEncounteredError = true;

                return;
            }

            UnparsedParametersByName.Remove(nameColonSyntax.Name.Identifier.Text);

            QueryResponseCollector.Associations.Normal.Add(parameterSymbol, UnassociatedInvocationData.SyntacticArguments[index]);
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
            QueryResponseCollector.Associations.Params.Add(UnassociatedInvocationData.Parameters[index], syntacticArguments);

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
            var queryResponseCollector = new ValuedQueryResponseCollector<bool>();

            ParamsArgumentIdentifier.Handle(query, queryResponseCollector);

            if (queryResponseCollector.HasSetValue is false)
            {
                HasEncounteredError = true;

                return false;
            }

            return queryResponseCollector.GetValue();
        }
    }
}
