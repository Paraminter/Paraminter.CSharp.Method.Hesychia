namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Method.Hesychia.Queries;
using Paraminter.CSharp.Method.Queries.Collectors;

using System.Collections.Generic;
using System.Linq;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void MethodInvocation_NormalArguments_AssociatesAll()
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
        var method = type.GetMembers().OfType<IMethodSymbol>().Single((symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        queryMock.Setup((query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(parameters[0], syntacticArguments[0]), Times.Once());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(parameters[1], syntacticArguments[1]), Times.Once());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(parameters[2], syntacticArguments[2]), Times.Once());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(It.IsAny<IParameterSymbol>(), It.IsAny<ArgumentSyntax>()), Times.Exactly(3));

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Params.Add(It.IsAny<IParameterSymbol>(), It.IsAny<IReadOnlyList<ArgumentSyntax>>()), Times.Never());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Default.Add(It.IsAny<IParameterSymbol>()), Times.Never());

        queryResponseCollectorMock.Verify((collector) => collector.Invalidator.Invalidate(), Times.Never());
    }

    [Fact]
    public void AttributeUsage_ParamsArguments_AssociatesAll()
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
        var method = type.GetMembers().OfType<IMethodSymbol>().Single((symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        queryMock.Setup((query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Params.Add(parameters[0], syntacticArguments), Times.Once());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Params.Add(It.IsAny<IParameterSymbol>(), It.IsAny<IReadOnlyList<ArgumentSyntax>>()), Times.Exactly(1));

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(It.IsAny<IParameterSymbol>(), It.IsAny<ArgumentSyntax>()), Times.Never());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Default.Add(It.IsAny<IParameterSymbol>()), Times.Never());

        queryResponseCollectorMock.Verify((collector) => collector.Invalidator.Invalidate(), Times.Never());
    }

    [Fact]
    public void AttributeUsage_DefaultArgument_AssociatesAll()
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
        var method = type.GetMembers().OfType<IMethodSymbol>().Single((symbol) => symbol.Name == "Method");
        var parameters = method.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var invokeMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(static (method) => method.Identifier.Text == "Invoke");
        var methodInvocation = invokeMethod.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

        var syntacticArguments = methodInvocation.ArgumentList.Arguments;

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        queryMock.Setup((query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Default.Add(parameters[0]), Times.Once());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Default.Add(It.IsAny<IParameterSymbol>()), Times.Exactly(1));

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Params.Add(It.IsAny<IParameterSymbol>(), It.IsAny<IReadOnlyList<ArgumentSyntax>>()), Times.Never());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(It.IsAny<IParameterSymbol>(), It.IsAny<ArgumentSyntax>()), Times.Never());

        queryResponseCollectorMock.Verify((collector) => collector.Invalidator.Invalidate(), Times.Never());
    }

    private void Target(
        IAssociateArgumentsQuery<IAssociateSyntacticCSharpMethodData> query,
        IInvalidatingAssociateSyntacticCSharpMethodQueryResponseCollector queryResponseCollector)
    {
        Fixture.Sut.Handle(query, queryResponseCollector);
    }
}
