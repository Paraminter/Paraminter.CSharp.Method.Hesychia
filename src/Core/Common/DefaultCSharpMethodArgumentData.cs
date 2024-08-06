namespace Paraminter.CSharp.Method.Hesychia.Common;

using Paraminter.Arguments.CSharp.Method.Models;

internal sealed class DefaultCSharpMethodArgumentData
    : IDefaultCSharpMethodArgumentData
{
    public static IDefaultCSharpMethodArgumentData Instance { get; } = new DefaultCSharpMethodArgumentData();

    private DefaultCSharpMethodArgumentData() { }
}
