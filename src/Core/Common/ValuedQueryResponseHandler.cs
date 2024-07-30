namespace Paraminter.CSharp.Method.Hesychia.Common;

using Paraminter.Commands.Handlers;
using Paraminter.Queries.Values.Commands;
using Paraminter.Queries.Values.Handlers;

internal sealed class ValuedQueryResponseHandler<TValue>
    : IValuedQueryResponseHandler<TValue>,
    ICommandHandler<ISetQueryResponseValueCommand<TValue>>
{
    public bool HasSetValue { get; private set; }

    private TValue Value = default!;

    public TValue GetValue() => Value;

    void ICommandHandler<ISetQueryResponseValueCommand<TValue>>.Handle(
        ISetQueryResponseValueCommand<TValue> command)
    {
        Value = command.Value;
        HasSetValue = true;
    }

    ICommandHandler<ISetQueryResponseValueCommand<TValue>> IValuedQueryResponseHandler<TValue>.Value => this;
}
