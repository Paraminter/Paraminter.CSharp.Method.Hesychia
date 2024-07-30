namespace Paraminter.CSharp.Method.Hesychia;

using Moq;

using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

using System;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void NullParamsArgumentIdentifier_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void ValidArguments_ReturnsAssociator()
    {
        var result = Target(Mock.Of<IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>>>());

        Assert.NotNull(result);
    }

    private static SyntacticCSharpMethodAssociator Target(
        IQueryHandler<IIsCSharpMethodArgumentParamsQuery, IValuedQueryResponseHandler<bool>> paramsArgumentIdentifier)
    {
        return new SyntacticCSharpMethodAssociator(paramsArgumentIdentifier);
    }
}
