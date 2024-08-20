namespace Paraminter.CSharp.Method.Hesychia;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void ReturnsDistinguisher()
    {
        var result = Target();

        Assert.NotNull(result);
    }

    private static CSharpMethodParamsArgumentDistinguisher Target() => new();
}
