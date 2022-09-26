using Xunit;

namespace TSRuntime.Generation.Tests;

internal static class ParserExtension {
    internal static string ParseAndGetContent(this Parser parser, string argument) {
        parser.Parse(argument);
        return parser.GetContent();
    }
}

public class GenratorParserTest {
    [Fact]
    public void Raw_Text_Converts_To_Yield_Return_Raw_Text() {
        string result = new Parser().ParseAndGetContent("Example asdf Text");

        Assert.Equal(""""
                    yield return """
                        Example asdf Text
                        """;

            """", result);
    }

    [Fact]
    public void Single_Tick_Converts_To_Yield_Return_Raw_Code() {
        string result = new Parser().ParseAndGetContent("`ExampleVariable`");

        Assert.Equal("""
                    yield return ExampleVariable;

            """, result);
    }

    [Fact]
    public void Double_Tick_Converts_To_Raw_Code() {
        string result = new Parser().ParseAndGetContent("``My Example Code``");

        Assert.Equal("""
                    My Example Code

            """, result);
    }

    [Fact]
    public void Linebreak_After_Double_Tick_Is_Ignored() {
        string result = new Parser().ParseAndGetContent("""
            ``My Example Code``
            asdf
            """);

        Assert.Equal(""""
                    My Example Code
                    yield return """
                        asdf
                        """;

            """", result);
    }

    [Fact]
    public void Double_Tick_Plus_Converts_To_Raw_Code_And_Increases_Indent() {
        string result = new Parser().ParseAndGetContent("``My Example Code`+asdf");

        Assert.Equal(""""
                    My Example Code
                        yield return """
                            asdf
                            """;

            """", result);
    }

    [Fact]
    public void Double_Tick_Minus_Converts_To_Raw_Code_And_Decreases_Indent() {
        string result = new Parser().ParseAndGetContent("``My Example Code`-asdf");

        Assert.Equal(""""
                My Example Code
                yield return """
                    asdf
                    """;

            """", result);
    }

    [Fact]
    public void Emptry_Converts_To_Empty() {
        string result = new Parser().ParseAndGetContent(string.Empty);

        Assert.Equal(string.Empty, result);
    }

    [Theory]
    [InlineData("asdf`var`qwer",
        """"
                yield return """
                    asdf
                    """;
                yield return var;
                yield return """
                    qwer
                    """;

        """")]
    [InlineData("""
        aaa
        ``
        indent+1
        `+
        bbb
        ``
        indent-1
        `-
        """,
        """"
                yield return """
                    aaa
                    
                    """;
                indent+1
                    yield return """
                        bbb
                        
                        """;
                indent-1

        """")]
    public void Combination_Converts_To_Concatenation(string input, string expected) {
        string result = new Parser().ParseAndGetContent(input);

        Assert.Equal(expected, result);
    }
}