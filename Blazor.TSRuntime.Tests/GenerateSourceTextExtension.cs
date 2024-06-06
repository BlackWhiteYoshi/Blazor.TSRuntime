using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;

namespace TSRuntime.Tests;

public static class GenerateSourceTextExtension {
    public const string CONFIG_FOLDER_PATH = @"C:\SomeAbsolutePath";

    /// <summary>
    /// <para>Takes additional files as input and outputs the generated source code based on the given input.</para>
    /// <para>The generated source code contains post-initialization-output code as well as source output code.</para>
    /// </summary>
    /// <param name="input"></param>
    /// <param name="outputCompilation"></param>
    /// <param name="diagnostics"></param>
    /// <returns></returns>
    public static string[] GenerateSourceText(this string config, (string path, string content)[] input, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics)
        => config.GenerateSourceResult(input, out outputCompilation, out diagnostics).Select((GeneratedSourceResult result) => result.SourceText.ToString()).ToArray();
    

    /// <summary>
    /// <para>Takes additional files as input and outputs the generated source code based on the given input.</para>
    /// <para>The generated source code contains post-initialization-output code as well as source output code.</para>
    /// </summary>
    /// <param name="input"></param>
    /// <param name="outputCompilation"></param>
    /// <param name="diagnostics"></param>
    /// <returns></returns>
    public static ImmutableArray<GeneratedSourceResult> GenerateSourceResult(this string config, (string path, string content)[] input, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics) {
        TSRuntimeGenerator generator = new();
        AdditionalText configFile = new InMemoryAdditionalText($"{CONFIG_FOLDER_PATH}/tsruntime.json", config);
        IEnumerable<AdditionalText> modules = input.Select<(string, string), AdditionalText>(((string path, string content) file) => new InMemoryAdditionalText(file.path, file.content));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.AddAdditionalTexts([configFile, .. modules]);
        driver = driver.RunGeneratorsAndUpdateCompilation(CreateCompilation(string.Empty), out outputCompilation, out diagnostics);

        GeneratorDriverRunResult runResult = driver.GetRunResult();
        GeneratorRunResult generatorResult = runResult.Results[0];
        return generatorResult.GeneratedSources;


        static CSharpCompilation CreateCompilation(string source) {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            PortableExecutableReference metadataReference = MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location);
            CSharpCompilationOptions compilationOptions = new(OutputKind.ConsoleApplication);

            return CSharpCompilation.Create("compilation", [syntaxTree], [metadataReference], compilationOptions);
        }
    }


    private sealed class InMemoryAdditionalText(string path, string text) : AdditionalText {
        private sealed class InMemorySourceText(string text) : SourceText {
            public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count) => text.CopyTo(sourceIndex, destination, destinationIndex, count);

            public override Encoding? Encoding => Encoding.Default;
            public override int Length => text.Length;

            public override char this[int position] => text[position];
        }

        public override string Path { get; } = path;

        public override SourceText? GetText(CancellationToken cancellationToken = default) => new InMemorySourceText(text);
    }
}
