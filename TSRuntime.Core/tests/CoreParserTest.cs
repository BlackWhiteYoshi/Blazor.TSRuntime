using TSRuntime.Core.Parsing;
using Xunit;

namespace TSRuntime.Core.Tests;

public class CoreParserTest {
    #region TSParamter

    [Theory]
    [InlineData("number", "number", false, false, false)]
    [InlineData("string", "string", false, false, false)]
    [InlineData("asdf", "asdf", false, false, false)]
    [InlineData("number | null", "number", true, false, false)]
    [InlineData("number | undefined", "number", true, false, false)]
    [InlineData("number[]", "number", false, true, false)]
    [InlineData("Array<number>", "number", false, true, false)]
    [InlineData("number[] | null", "number", false, true, true)]
    [InlineData("Array<number> | null", "number", false, true, true)]
    [InlineData("(number | null)[]", "number", true, true, false)]
    [InlineData("(number | null)[] | null", "number", true, true, true)]
    public void Parsing_Parameter_Works(string input, string type, bool typeNullable, bool array, bool arrayNullable) {
        TSParameter parameter = new();

        parameter.ParseType(input);

        Assert.Equal(type, parameter.Type);
        Assert.Equal(typeNullable, parameter.TypeNullable);
        Assert.Equal(array, parameter.Array);
        Assert.Equal(arrayNullable, parameter.ArrayNullable);
    }

    #endregion


    #region TSFunction

    [Fact]
    public void Parsing_Function__Wrong_Start_Retunrs_Null() {
        TSFunction? result = TSFunction.Parse("asdf");
        Assert.Null(result);
    }

    [Fact]
    public void Parsing_Function__Right_Start_But_Wrong_Syntax_Throws() {
        Assert.Throws<Exception>(() => TSFunction.Parse("export declare function "));
    }

    [Theory]
    [InlineData("export declare function getCookies(): string;", "getCookies", "string", false)]
    [InlineData("export declare function asdf(): voidy;", "asdf", "voidy", false)]
    [InlineData("export declare function longRunningTask(): Promise<void>;", "longRunningTask", "void", true)]
    [InlineData("export declare function longRunningTask2(): Promise<something>;", "longRunningTask2", "something", true)]
    public void Parsing_Function_Works(string input, string name, string returnType, bool promise) {
        TSFunction function = TSFunction.Parse(input)!;

        Assert.Equal(name, function.Name);
        Assert.Equal(returnType, function.ReturnType.Type);
        Assert.Equal(promise, function.ReturnPromise);
    }

    #endregion


    #region TSModule

    [Fact]
    public void Parsing_Mdule__Wrong_FilePath_Throws() {
        Assert.Throws<ArgumentException>(() => TSModule.Parse("", string.Empty));
        Assert.Throws<FileNotFoundException>(() => TSModule.Parse("#", string.Empty));
    }

    // TODO after config is established

    #endregion


    #region TSSyntaxTree

    // TODO after config is established

    #endregion
}
