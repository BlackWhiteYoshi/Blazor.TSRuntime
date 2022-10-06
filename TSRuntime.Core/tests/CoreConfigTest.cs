using System.Text;
using TSRuntime.Core.Configs;
using Xunit;

namespace TSRuntime.Core.Tests;

public sealed class CoreConfigTest {
    // TODO after config is established

    [Fact]
    public void Config_FieldsAreNotEmpty() {
        Config config = new();

        Assert.NotNull(config.FileOutputClass);
        Assert.NotNull(config.FileOutputinterface);
        Assert.NotEmpty(config.TypeMap);
        Assert.NotEmpty(config.UsingStatements);
    }


    #region FunctionNaming

    private const string FUNCTION = "function";
    private const string MODULE = "Module";
    private const string ACTION = "Action";

    [Theory]
    [InlineData("$function$_$module$_$action$", $"{FUNCTION}_{MODULE}_{ACTION}")]
    [InlineData("$function$$module$$action$", $"{FUNCTION}{MODULE}{ACTION}")]
    [InlineData("test$function$$module$$action$", $"test{FUNCTION}{MODULE}{ACTION}")]
    [InlineData("test2", $"test2")]
    [InlineData("$action$", $"{ACTION}")]
    [InlineData("", "")]
    public void FunctionNaming_ParsingWorks(string naming, string expected) {
        FunctionNamePattern functionNaming = new(naming, NameTransform.None, NameTransform.None, NameTransform.None);

        StringBuilder builder = new();
        foreach (string str in functionNaming.GetNaming(FUNCTION, MODULE, ACTION))
            builder.Append(str);
        string result = builder.ToString();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(NameTransform.None, NameTransform.None, NameTransform.None, "$function$$module$$action$", $"{FUNCTION}{MODULE}{ACTION}")]
    [InlineData(NameTransform.UpperCase, NameTransform.None, NameTransform.None, "$function$", "FUNCTION")]
    [InlineData(NameTransform.None, NameTransform.LowerCase, NameTransform.None, "$module$", "module")]
    [InlineData(NameTransform.FirstUpperCase, NameTransform.None, NameTransform.None, "$function$", "Function")]
    [InlineData(NameTransform.None, NameTransform.FirstLowerCase, NameTransform.None, "$module$", "module")]
    public void FunctionNaming_TransformWorks(NameTransform function, NameTransform module, NameTransform action, string naming, string expected) {
        FunctionNamePattern functionNaming = new(naming, function, module, action);

        StringBuilder builder = new();
        foreach (string str in functionNaming.GetNaming(FUNCTION, MODULE, ACTION))
            builder.Append(str);
        string result = builder.ToString();

        Assert.Equal(expected, result);
    }

    #endregion
}
