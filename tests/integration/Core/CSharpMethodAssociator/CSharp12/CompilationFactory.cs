namespace Paraminter.CSharp.Method.Hesychia;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal static class CompilationFactory
{
    private static readonly CSharpCompilation EmptyCompilation = CreateEmptyCompilation();

    private static readonly CSharpCompilationOptions CompilationOptions = new(OutputKind.DynamicallyLinkedLibrary);

    public static CSharpCompilation Create(
        string source)
    {
        CSharpParseOptions parseOptions = new(languageVersion: LanguageVersion.CSharp12);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

        return EmptyCompilation.AddSyntaxTrees(syntaxTree);
    }

    private static CSharpCompilation CreateEmptyCompilation()
    {
        var references = ListAssemblies()
            .Where(static (assembly) => assembly.IsDynamic is false)
            .Select(static (assembly) => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        return CSharpCompilation.Create("TestAssembly", references: references, options: CompilationOptions);
    }

    private static List<Assembly> ListAssemblies()
    {
        List<Assembly> resolvedAssemblies = [];

        resolvedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());

        return resolvedAssemblies;
    }
}
