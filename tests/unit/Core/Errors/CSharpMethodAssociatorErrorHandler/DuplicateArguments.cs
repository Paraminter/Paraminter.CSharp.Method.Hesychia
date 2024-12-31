namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors;

using Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;

using Xunit;

public sealed class DuplicateArguments
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void ReturnsHandler()
    {
        var result = Target();

        Assert.Same(Fixture.DuplicateArgumentsMock.Object, result);
    }

    private ICommandHandler<IHandleDuplicateArgumentsCommand> Target() => Fixture.Sut.DuplicateArguments;
}
