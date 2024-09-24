namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors;

using Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;
using Paraminter.Cqs.Handlers;

using Xunit;

public sealed class DuplicateParameterNames
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void ReturnsHandler()
    {
        var result = Target();

        Assert.Same(Fixture.DuplicateParameterNamesMock.Object, result);
    }

    private ICommandHandler<IHandleDuplicateParameterNamesCommand> Target() => Fixture.Sut.DuplicateParameterNames;
}
