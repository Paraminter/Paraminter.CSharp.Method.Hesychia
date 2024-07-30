namespace Paraminter.CSharp.Method.Hesychia.Common;

using Paraminter.Queries.Values.Commands;

internal sealed class SetQueryResponseValueCommand<TValue>
    : ISetQueryResponseValueCommand<TValue>
{
    private readonly TValue Value;

    public SetQueryResponseValueCommand(
        TValue value)
    {
        Value = value;
    }

    TValue ISetQueryResponseValueCommand<TValue>.Value => Value;
}
