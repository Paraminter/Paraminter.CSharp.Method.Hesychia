﻿namespace Paraminter.Associating.CSharp.Method.Hesychia.Errors;

using Paraminter.Associating.CSharp.Method.Hesychia.Errors.Commands;

using Xunit;

public sealed class MissingRequiredArgument
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void ReturnsHandler()
    {
        var result = Target();

        Assert.Same(Fixture.MissingRequiredArgumentMock.Object, result);
    }

    private ICommandHandler<IHandleMissingRequiredArgumentCommand> Target() => Fixture.Sut.MissingRequiredArgument;
}
