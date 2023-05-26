using Xunit;

namespace TSRuntime.Generation.Tests;

public sealed class GenratorParserTest {
    [Fact]
    public void RawText_ConvertsTo_YieldReturnRawText() {
        string result = Parser.Parse("Example asdf Text");

        Assert.Equal(""""
                    yield return """
                        Example asdf Text
                        """;
            """", result);
    }

    [Fact]
    public void SingleTick_ConvertsTo_YieldReturnRawCode() {
        string result = Parser.Parse("`ExampleVariable`");

        Assert.Equal("""
                    yield return ExampleVariable;
            """, result);
    }

    [Fact]
    public void DoubleTick_ConvertsTo_RawCode() {
        string result = Parser.Parse("``My Example Code``");

        Assert.Equal("""
                    My Example Code
            """, result);
    }

    [Fact]
    public void LinebreakAfterDoubleTick_IsIgnored() {
        string result = Parser.Parse("""
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
        string result = Parser.Parse("``My Example Code`+asdf");

        Assert.Equal(""""
                    My Example Code
                        yield return """
                            asdf
                            """;
            """", result);
    }

    [Fact]
    public void DoubleTickMinus_ConvertsTo_RawCodeAndDecreasesIndent() {
        string result = Parser.Parse("``My Example Code`-asdf");

        Assert.Equal(""""
                My Example Code
                yield return """
                    asdf
                    """;
            """", result);
    }

    [Fact]
    public void Empty_ConvertsTo_Empty() {
        string result = Parser.Parse(string.Empty);

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
        string result = Parser.Parse(input);

        Assert.Equal(expected, result);
    }
}
