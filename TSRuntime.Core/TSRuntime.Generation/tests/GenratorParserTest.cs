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
    public void RawText_ConvertsTo_YieldReturnRawText() {
        string result = new Parser().ParseAndGetContent("Example asdf Text");

        Assert.Equal(""""
                    yield return """
                        Example asdf Text
                        """;

            """", result);
    }

    [Fact]
    public void SingleTick_ConvertsTo_YieldReturnRawCode() {
        string result = new Parser().ParseAndGetContent("`ExampleVariable`");

        Assert.Equal("""
                    yield return ExampleVariable;

            """, result);
    }

    [Fact]
    public void DoubleTick_ConvertsTo_RawCode() {
        string result = new Parser().ParseAndGetContent("``My Example Code``");

        Assert.Equal("""
                    My Example Code

            """, result);
    }

    [Fact]
    public void LinebreakAfterDoubleTick_IsIgnored() {
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
    public void DoubleTickPlus_ConvertsTo_RawCodeAndIncreasesIndent() {
        string result = new Parser().ParseAndGetContent("``My Example Code`+asdf");

        Assert.Equal(""""
                    My Example Code
                        yield return """
                            asdf
                            """;

            """", result);
    }

    [Fact]
    public void DoubleTickMinus_ConvertsTo_RawCodeAndDecreasesIndent() {
        string result = new Parser().ParseAndGetContent("``My Example Code`-asdf");

        Assert.Equal(""""
                My Example Code
                yield return """
                    asdf
                    """;

            """", result);
    }

    [Fact]
    public void Emptry_ConvertsTo_Empty() {
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
    public void Combination_ConvertsTo_Concatenation(string input, string expected) {
        string result = new Parser().ParseAndGetContent(input);

        Assert.Equal(expected, result);
    }
}