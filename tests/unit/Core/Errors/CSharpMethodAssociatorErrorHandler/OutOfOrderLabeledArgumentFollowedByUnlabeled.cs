namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors;

using Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;
using Paraminter.Cqs;

using Xunit;

public sealed class OutOfOrderLabeledArgumentFollowedByUnlabeled
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void ReturnsHandler()
    {
        var result = Target();

        Assert.Same(Fixture.OutOfOrderLabeledArgumentFollowedByUnlabeledMock.Object, result);
    }

    private ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand> Target() => Fixture.Sut.OutOfOrderLabeledArgumentFollowedByUnlabeled;
}
