namespace Paraminter.Associating.CSharp.Method.Hesychia.Models;

using Paraminter.Arguments.CSharp.Method.Models;

internal sealed class DefaultCSharpMethodArgumentData
    : IDefaultCSharpMethodArgumentData
{
    public static IDefaultCSharpMethodArgumentData Instance { get; } = new DefaultCSharpMethodArgumentData();

    private DefaultCSharpMethodArgumentData() { }
}
