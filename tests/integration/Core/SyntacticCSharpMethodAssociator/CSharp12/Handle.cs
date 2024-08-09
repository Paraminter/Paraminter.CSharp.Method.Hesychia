namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Arguments.CSharp.Method.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Method.Hesychia.Models;
using Paraminter.Parameters.Method.Models;
using Paraminter.Recorders.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void MethodInvocation_NormalArguments_RecordsAll()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(1, "", false);
                }

                public void Method(int a, string b, bool c) { }
            }
            """;

        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var method = type.GetMembers().OfType<IMethodSymbol>().Single(static (symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.NormalRecorderMock.Verify(RecordNormalExpression(parameters[0], syntacticArguments[0]), Times.Once());
        Fixture.NormalRecorderMock.Verify(RecordNormalExpression(parameters[1], syntacticArguments[1]), Times.Once());
        Fixture.NormalRecorderMock.Verify(RecordNormalExpression(parameters[2], syntacticArguments[2]), Times.Once());
        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Exactly(3));

        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
    }

    [Fact]
    public void AttributeUsage_ParamsArguments_RecordsAll()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(1, 2, 3);
                }

                public void Method(params int[] a) { }
            }
            """;

        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var method = type.GetMembers().OfType<IMethodSymbol>().Single(static (symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.ParamsRecorderMock.Verify(RecordParamsExpression(parameters[0], syntacticArguments), Times.Once());
        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Exactly(1));

        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Never());
    }

    [Fact]
    public void AttributeUsage_DefaultArgument_RecordsAll()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method();
                }

                public void Method(int a = 3) { }
            }
            """;

        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var method = type.GetMembers().OfType<IMethodSymbol>().Single(static (symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.DefaultRecorderMock.Verify(RecordDefaultExpression(parameters[0]), Times.Once());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>()), Times.Exactly(1));

        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>()), Times.Never());
        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>()), Times.Never());
    }

    private static Expression<Action<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>>>> RecordNormalExpression(
        IParameterSymbol parameterSymbol,
        ArgumentSyntax syntacticArgument)
    {
        return (recorder) => recorder.Handle(It.Is(MatchRecordNormalCommand(parameterSymbol, syntacticArgument)));
    }

    private static Expression<Func<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpMethodArgumentData>, bool>> MatchRecordNormalCommand(
        IParameterSymbol parameterSymbol,
        ArgumentSyntax syntacticArgument)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchNormalArgumentData(syntacticArgument, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>>>> RecordParamsExpression(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<ArgumentSyntax> syntacticArguments)
    {
        return (recorder) => recorder.Handle(It.Is(MatchRecordParamsCommand(parameterSymbol, syntacticArguments)));
    }

    private static Expression<Func<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpMethodArgumentData>, bool>> MatchRecordParamsCommand(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<ArgumentSyntax> syntacticArguments)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchParamsArgumentData(syntacticArguments, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>>>> RecordDefaultExpression(
        IParameterSymbol parameterSymbol)
    {
        return (recorder) => recorder.Handle(It.Is(MatchRecordDefaultCommand(parameterSymbol)));
    }

    private static Expression<Func<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpMethodArgumentData>, bool>> MatchRecordDefaultCommand(
        IParameterSymbol parameterSymbol)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter);
    }

    private static bool MatchParameter(
        IParameterSymbol parameterSymbol,
        IMethodParameter parameter)
    {
        return ReferenceEquals(parameterSymbol, parameter.Symbol);
    }

    private static bool MatchNormalArgumentData(
        ArgumentSyntax syntacticArgument,
        INormalCSharpMethodArgumentData argumentData)
    {
        return ReferenceEquals(syntacticArgument, argumentData.SyntacticArgument);
    }

    private static bool MatchParamsArgumentData(
        IReadOnlyList<ArgumentSyntax> syntacticArguments,
        IParamsCSharpMethodArgumentData argumentData)
    {
        return Enumerable.SequenceEqual(syntacticArguments, argumentData.SyntacticArguments);
    }

    private void Target(
        IAssociateArgumentsCommand<IAssociateSyntacticCSharpMethodData> command)
    {
        Fixture.Sut.Handle(command);
    }
}
