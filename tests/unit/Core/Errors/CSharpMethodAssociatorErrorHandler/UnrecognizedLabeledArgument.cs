namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors;

using Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;
using Paraminter.Cqs;

using Xunit;

public sealed class UnrecognizedLabeledArgument
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void ReturnsHandler()
    {
        var result = Target();

        Assert.Same(Fixture.UnrecognizedLabeledArgumentMock.Object, result);
    }

    private ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand> Target() => Fixture.Sut.UnrecognizedLabeledArgument;
}
