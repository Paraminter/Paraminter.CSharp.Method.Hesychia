﻿namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.Queries.Values.Collectors;

using System.Linq;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void Params_ImplicitlyConverted_RespondsWithTrue()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(4);
                }

                public void Method(params double[] values) { }
            }
            """;

        RespondsWithTrue(source);
    }

    [Fact]
    public void Params_ExactType_RespondsWithTrue()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(4.2);
                }

                public void Method(params double[] values) { }
            }
            """;

        RespondsWithTrue(source);
    }

    [Fact]
    public void Params_SameTypeExceptNullability_RespondsWithTrue()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(4.2);
                }

                public void Method(params double?[] values) { }
            }
            """;

        RespondsWithTrue(source);
    }

    [Fact]
    public void Params_Null_RespondsTrue()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(null, null);
                }

                public void Method(params object?[]? values) { }
            }
            """;

        RespondsWithTrue(source);
    }

    [Fact]
    public void NonParams_RespondsFalse()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method([4.2]);
                }

                public void Method(params double[] values) { }
            }
            """;

        ResponseWithFalse(source);
    }

    [Fact]
    public void NonParams_Null_RespondsFalse()
    {
        var source = """
            public class Foo
            {
                public void Invoke()
                {
                    Method(null);
                }

                public void Method(params double[] values) { }
            }
            """;

        ResponseWithFalse(source);
    }

    private void Target(
        IIsCSharpMethodArgumentParamsQuery query,
        IValuedQueryResponseCollector<bool> queryResponseCollector)
    {
        Fixture.Sut.Handle(query, queryResponseCollector);
    }

    private void RespondsWithTrue(
        string source)
    {
        RespondsWithValue(source, true);
    }

    private void ResponseWithFalse(
        string source)
    {
        RespondsWithValue(source, false);
    }

    private void RespondsWithValue(
        string source,
        bool expected)
    {
        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var method = type.GetMembers().OfType<IMethodSymbol>().Single((symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IIsCSharpMethodArgumentParamsQuery> queryMock = new();
        Mock<IValuedQueryResponseCollector<bool>> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Parameter).Returns(parameters[0]);
        queryMock.Setup(static (query) => query.SyntacticArgument).Returns(syntacticArguments[0]);
        queryMock.Setup(static (query) => query.SemanticModel).Returns(semanticModel);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Value.Set(expected), Times.Once());
        queryResponseCollectorMock.Verify(static (collector) => collector.Value.Set(It.IsAny<bool>()), Times.Once());
    }
}
