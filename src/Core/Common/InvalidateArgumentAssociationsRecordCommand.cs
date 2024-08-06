namespace Paraminter.CSharp.Method.Hesychia.Common;

using Paraminter.Associators.Commands;

internal sealed class InvalidateArgumentAssociationsRecordCommand
    : IInvalidateArgumentAssociationsRecordCommand
{
    public static IInvalidateArgumentAssociationsRecordCommand Instance { get; } = new InvalidateArgumentAssociationsRecordCommand();

    private InvalidateArgumentAssociationsRecordCommand() { }
}
