namespace Paraminter.CSharp.Method.Hesychia.Common;

using Paraminter.Queries.Values.Collectors;

internal sealed class ValuedQueryResponseCollector<TValue>
    : IValuedQueryResponseCollector<TValue>,
    IQueryHandlerResponseValueCollector<TValue>
{
    public bool HasSetValue { get; private set; }

    private TValue Value = default!;

    public TValue GetValue() => Value;

    void IQueryHandlerResponseValueCollector<TValue>.Set(
        TValue value)
    {
        Value = value;
        HasSetValue = true;
    }

    IQueryHandlerResponseValueCollector<TValue> IValuedQueryResponseCollector<TValue>.Value => this;
}
