using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using TSRuntime.Core.Configs;
using TSRuntime.Core.Configs.NamePattern;
using Xunit;

namespace TSRuntime.Core.Tests;

public sealed class CoreConfigTest {
    
    [Fact]
    public void Config_FieldsHaveDefaultValues() {
        Config config = new();

        foreach (PropertyInfo property in typeof(Config).GetProperties()) {
            object? value = property.GetValue(config);
            Assert.NotNull(value);
            if (value is IEnumerable<object?> enumerable)
                Assert.NotEmpty(enumerable);
        }
    }

    [Fact]
    public void Config_EmptyJsonHaveDefaultValues() {
        Config config = Config.FromJson("{}");

        foreach (PropertyInfo property in typeof(Config).GetProperties()) {
            object? value = property.GetValue(config);
            Assert.NotNull(value);
            if (value is IEnumerable<object?> enumerable)
                Assert.NotEmpty(enumerable);
        }
    }

    [Fact]
    public void Config_ToJsonSavesAllProperties() {
        string configAsJson = new Config().ToJson();
        JsonNode root = JsonNode.Parse(configAsJson)!;
        JsonObject jsonObject = root.AsObject();

        // maps to one property but has leaf nodes
        jsonObject.Remove("declaration path");
        jsonObject.Remove("type map");
        jsonObject.Remove("function name pattern");
        jsonObject.Remove("preload name pattern");

        int numberOfLeafNodes = 4;
        foreach (KeyValuePair<string, JsonNode?> node in jsonObject)
            numberOfLeafNodes += NumberOfLeafNodes(node.Value!);
            
        Assert.Equal(typeof(Config).GetProperties().Length, numberOfLeafNodes);
        

        static int NumberOfLeafNodes(JsonNode node) {
            if (node is not JsonObject jsonObject)
                return 1;


            int numberOfLeafNodes = 0;
            
            foreach (KeyValuePair<string, JsonNode?> child in jsonObject) {
                numberOfLeafNodes += NumberOfLeafNodes(child.Value!);
            }
            
            return numberOfLeafNodes;
        }
    }


    [Fact]
    public void Config_FromJson_EmptyPathWorks() {
        string json = """
            {
                "declaration path": ""
            }
            """;
        Config config = Config.FromJson(json);
        
        Assert.Equal(string.Empty, config.DeclarationPath[0].Include);
    }


    [Theory]
    [InlineData("""{ "declaration path": "" }""", new string?[3] { "", "", null })]
    [InlineData("""{ "declaration path": "\\test" }""", new string?[3] { "/test", "", null })]
    [InlineData("""{ "declaration path": { "include": "\\test" } }""", new string?[3] { "/test", "", null })]
    [InlineData("""{ "declaration path": [{ "include": "\\test" }] }""", new string?[3] { "/test", "", null })]
    [InlineData("""{ "declaration path": { "include": "\\test", "excludes": "as\\df" } }""", new string?[3] { "/test", "as/df", null })]
    [InlineData("""{ "declaration path": { "include": "\\test", "excludes": [ "as\\df", "ghjk" ] } }""", new string?[3] { "/test", "as/df,ghjk", null })]
    [InlineData("""{ "declaration path": { "include": "\\test", "file module path": "yx\\cv" } }""", new string?[3] { "/test", "", "yx/cv" })]
    [InlineData("""{ "declaration path": { "include": "\\test", "excludes": "as\\df", "file module path": "yx\\cv" } }""", new string?[3] { "/test", "as/df", "yx/cv" })]
    [InlineData("""{ "declaration path": [{ "include": "\\test", "excludes": "as\\df", "file module path": "yx\\cv" }, { "include": "q", "excludes": ["w", "ww" ] } ] }""", new string?[6] { "/test", "as/df", "yx/cv", "q", "w,ww", null })]
    public void Config_FromJson_DeclarationPathWorks(string json, string?[] expected) {
        Config config = Config.FromJson(json);

        string?[] result = new string[config.DeclarationPath.Length * 3];
        for (int i = 0; i < config.DeclarationPath.Length; i++) {
            DeclarationPath declarationPath = config.DeclarationPath[i];

            result[i * 3 + 0] = declarationPath.Include;
            result[i * 3 + 1] = string.Join(",", declarationPath.Excludes);
            result[i * 3 + 2] = declarationPath.FileModulePath;
        }

        for (int i = 0; i < result.Length; i++)
            Assert.Equal(expected[i], result[i]);
    }

    [Theory]
    [InlineData("", new string[0], null, """
        [
            {
              "include": "",
              "excludes": []
            }
          ]
        """)]
    [InlineData("", new string[0], "", """
        [
            {
              "include": "",
              "excludes": [],
              "file module path": ""
            }
          ]
        """)]
    [InlineData("qwer", new string[1] { "asdf" }, "yxcv", """
        [
            {
              "include": "qwer",
              "excludes": [ "asdf" ],
              "file module path": "yxcv"
            }
          ]
        """)]
    [InlineData("qwer", new string[2] { "asdf", "ghjk" }, "yxcv", """
        [
            {
              "include": "qwer",
              "excludes": [
                "asdf",
                "ghjk"
              ],
              "file module path": "yxcv"
            }
          ]
        """)]
    public void Config_ToJson_DeclarationPathWorks(string include, string[] excludes, string fileModulePath, string expected) {
        Config config = new() {
            DeclarationPath = new DeclarationPath[1] { new(include, excludes, fileModulePath) }
        };
        string json = config.ToJson();

        Assert.Contains($""" "declaration path": {expected},""", json);
    }

    [Fact]
    public void Config_ToJson_MultipleDeclarationPath() {
        Config config = new() {
            DeclarationPath = new DeclarationPath[2] { new("qwer", new string[1] { "asdf" }, "yxcv"), new("rewq", new string[2] { "fdsa", "kjhg" }, "vcxy") }
        };
        string json = config.ToJson();

        const string expected = """
            "declaration path": [
                {
                  "include": "qwer",
                  "excludes": [ "asdf" ],
                  "file module path": "yxcv"
                },
                {
                  "include": "rewq",
                  "excludes": [
                    "fdsa",
                    "kjhg"
                  ],
                  "file module path": "vcxy"
                }
              ],
            """;
        Assert.Contains(expected, json);
    }

    [Fact]
    public void Config_ToJson_EmptyDeclarationPath() {
        Config config = new() {
            DeclarationPath = Array.Empty<DeclarationPath>()
        };
        string json = config.ToJson();

        Assert.Contains($""" "declaration path": [],""", json);
    }


    [Theory]
    [InlineData(""" "" """, new string[1] { "" })]
    [InlineData(""" "Something" """, new string[1] { "Something" })]
    [InlineData(""" [] """, new string[0])]
    [InlineData(""" ["Something"] """, new string[1] { "Something" })]
    [InlineData(""" ["Something", "More"] """, new string[2] { "Something", "More" })]
    public void Config_FromJson_UsingStatementsWork(string usingStatementsValue, string[] expected) {
        string json = $$"""
            {
                "using statements": {{usingStatementsValue}}
            }
            """;
        Config config = Config.FromJson(json);

        for (int i = 0; i < config.UsingStatements.Length; i++)
            Assert.Equal(expected[i], config.UsingStatements[i]);
    }

    [Theory]
    [InlineData(new string[] { }, """[]""")]
    [InlineData(new string[] { "Microsoft.AspNetCore.Components" }, """[ "Microsoft.AspNetCore.Components" ]""")]
    [InlineData(new string[] { "qwer", "asdf", "yxcv" }, """
                                                        [
                                                            "qwer",
                                                            "asdf",
                                                            "yxcv"
                                                          ]
                                                        """)]
    public void Config_ToJson_UsingStatementsWorks(string[] usingStatements, string expected) {
        Config config = new() {
            UsingStatements = usingStatements
        };
        string json = config.ToJson();

        Assert.Contains($""" "using statements": {expected},""", json);
    }


    [Theory]
    [InlineData(new string[] { }, """{ }""")]
    [InlineData(new string[] { "key", "value" }, """
                                                {
                                                    "key": "value"
                                                  }
                                                """)]
    [InlineData(new string[] { "a", "b", "c", "d", "e", "f" }, """
                                                            {
                                                                "a": "b",
                                                                "c": "d",
                                                                "e": "f"
                                                              }
                                                            """)]
    public void Config_ToJson_TypeMapWorks(string[] types , string expected) {
        Dictionary<string, string> map = new(types.Length / 2);
        for (int i = 0; i < types.Length; i += 2)
            map.Add(types[i], types[i + 1]);

        Config config = new() {
            TypeMap = map
        };
        string json = config.ToJson();

        Assert.Contains($""" "type map": {expected}""", json);
    }


    #region NamePattern

    private const string FUNCTION = "function";
    private const string MODULE = "Module";
    private const string ACTION = "Action";


    [Theory]
    [InlineData("#function#_#module#_#action#", $"{FUNCTION}_{MODULE}_{ACTION}")]
    [InlineData("#function##module##action#", $"{FUNCTION}{MODULE}{ACTION}")]
    [InlineData("test#function##module##action#", $"test{FUNCTION}{MODULE}{ACTION}")]
    [InlineData("test2", $"test2")]
    [InlineData("#action#", $"{ACTION}")]
    [InlineData("", "")]
    public void FunctionNamePattern_ParsingWorks(string naming, string expected) {
        FunctionNamePattern functionNaming = new(naming, NameTransform.None, NameTransform.None, NameTransform.None);

        StringBuilder builder = new();
        foreach (string str in functionNaming.GetNaming(FUNCTION, MODULE, ACTION))
            builder.Append(str);
        string result = builder.ToString();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(NameTransform.None, NameTransform.None, NameTransform.None, "#function##module##action#", $"{FUNCTION}{MODULE}{ACTION}")]
    [InlineData(NameTransform.UpperCase, NameTransform.None, NameTransform.None, "#function#", "FUNCTION")]
    [InlineData(NameTransform.None, NameTransform.LowerCase, NameTransform.None, "#module#", "module")]
    [InlineData(NameTransform.FirstUpperCase, NameTransform.None, NameTransform.None, "#function#", "Function")]
    [InlineData(NameTransform.None, NameTransform.FirstLowerCase, NameTransform.None, "#module#", "module")]
    public void FunctionNamePattern_TransformWorks(NameTransform function, NameTransform module, NameTransform action, string naming, string expected) {
        FunctionNamePattern functionNaming = new(naming, function, module, action);

        StringBuilder builder = new();
        foreach (string str in functionNaming.GetNaming(FUNCTION, MODULE, ACTION))
            builder.Append(str);
        string result = builder.ToString();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("#wrong#")]
    [InlineData("test#")]
    [InlineData("#test")]
    [InlineData("##")]
    [InlineData("#modole#")]
    [InlineData("test#fonction#")]
    public void FunctionNamePattern_ThrowsException_WhenWrongNamingPattern(string naming) {
        try {
            FunctionNamePattern functionNaming = new(naming, NameTransform.None, NameTransform.None, NameTransform.None);
            Assert.Fail("No Exception happened");
        }
        catch (ArgumentException) { }
    }


    [Theory]
    [InlineData("Preload#module#", $"Preload{MODULE}")]
    [InlineData("#module#", $"{MODULE}")]
    [InlineData("#module##module##module#", $"{MODULE}{MODULE}{MODULE}")]
    [InlineData("test2", $"test2")]
    [InlineData("", "")]
    public void PreloadNamePattern_ParsingWorks(string naming, string expected) {
        PreloadNamePattern preLoadNaming = new(naming, NameTransform.None);

        StringBuilder builder = new();
        foreach (string str in preLoadNaming.GetNaming(MODULE))
            builder.Append(str);
        string result = builder.ToString();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(NameTransform.None, "#module##module##module#", $"{MODULE}{MODULE}{MODULE}")]
    [InlineData(NameTransform.UpperCase, "#module#", "MODULE")]
    [InlineData(NameTransform.LowerCase, "#module#", "module")]
    [InlineData(NameTransform.FirstLowerCase, "#module#", "module")]
    public void PreloadNamePattern_TransformWorks(NameTransform module, string naming, string expected) {
        PreloadNamePattern preLoadNaming = new(naming, module);

        StringBuilder builder = new();
        foreach (string str in preLoadNaming.GetNaming(MODULE))
            builder.Append(str);
        string result = builder.ToString();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("#wrong#")]
    [InlineData("#function#")]
    [InlineData("#action#")]
    [InlineData("test#")]
    [InlineData("#test")]
    [InlineData("##")]
    [InlineData("#modole#")]
    [InlineData("test#function#")]
    public void PreloadNamePattern_ThrowsException_WhenWrongNamingPattern(string naming) {
        try {
            PreloadNamePattern preLoadNaming = new(naming, NameTransform.None);
            Assert.Fail("No Exception happened");
        }
        catch (ArgumentException) { }
    }

    #endregion
}
