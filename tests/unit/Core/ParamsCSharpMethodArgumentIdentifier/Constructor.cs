namespace Paraminter.CSharp.Method.Hesychia;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void ReturnsIdentifier()
    {
        var result = Target();

        Assert.NotNull(result);
    }

    private static ParamsCSharpMethodArgumentIdentifier Target() => new();
}
