using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using TSRuntime.Configs;
using TSRuntime.Configs.NamePattern;

namespace TSRuntime.Tests;

public static class ConfigTests {
    
    [Fact]
    public static void Config_FieldsHaveDefaultValues() {
        Config config = new();

        foreach (PropertyInfo property in typeof(Config).GetProperties()) {
            if (property.Name == "ErrorList")
                continue;

            object? value = property.GetValue(config);
            Assert.NotNull(value);
            if (value is IEnumerable<object?> enumerable)
                Assert.NotEmpty(enumerable);
        }
    }

    [Fact]
    public static void Config_EmptyJsonHaveDefaultValues() {
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, "{}");

        foreach (PropertyInfo property in typeof(Config).GetProperties()) {
            if (property.Name == "ErrorList")
                continue;

            object? value = property.GetValue(config);
            Assert.NotNull(value);
            if (value is IEnumerable<object?> enumerable)
                Assert.NotEmpty(enumerable);
        }
    }

    [Fact]
    public static void Config_ToJsonSavesAllProperties() {
        string configAsJson = new Config().ToJson();
        JsonNode root = JsonNode.Parse(configAsJson)!;
        JsonObject jsonObject = root.AsObject();

        #region maps to one property but has leaf nodes

        int numberOfLeafNodes = 0;

        jsonObject.Remove("input path");
        numberOfLeafNodes++;

        jsonObject["invoke function"]!["name pattern"]!.AsObject().Remove("pattern");
        jsonObject["invoke function"]!["name pattern"]!.AsObject().Remove("module transform");
        jsonObject["invoke function"]!["name pattern"]!.AsObject().Remove("function transform");
        jsonObject["invoke function"]!["name pattern"]!.AsObject().Remove("action transform");
        numberOfLeafNodes++;

        jsonObject["invoke function"]!.AsObject().Remove("type map");
        numberOfLeafNodes++;

        jsonObject["preload function"]!.AsObject().Remove("name pattern");
        numberOfLeafNodes++;

        jsonObject["module grouping"]!.AsObject().Remove("interface name pattern");
        numberOfLeafNodes++;

        #endregion

        foreach (KeyValuePair<string, JsonNode?> node in jsonObject)
            numberOfLeafNodes += NumberOfLeafNodes(node.Value!);

        int propertyCount = typeof(Config).GetProperties().Length - 1; // subtract ErrorList

        Assert.Equal(propertyCount, numberOfLeafNodes);
        

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


    #region InputPath

    [Fact]
    public static void Config_FromJson_InputPath_Empty() {
        const string JSON = """
            {
                "input path": ""
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        Assert.Single(config.InputPath);
        InputPath inputPath = config.InputPath[0];

        Assert.Equal("", inputPath.Include);
        Assert.Empty(inputPath.Excludes);
        Assert.True(inputPath.ModuleFiles);
        Assert.Null(inputPath.ModulePath);
    }

    [Fact]
    public static void Config_FromJson_InputPath_IncludeShorthand() {
        const string JSON = """
            {
                "input path": "\\test"
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        Assert.Single(config.InputPath);
        InputPath inputPath = config.InputPath[0];

        Assert.Equal("/test", inputPath.Include);
        Assert.Empty(inputPath.Excludes);
        Assert.True(inputPath.ModuleFiles);
        Assert.Null(inputPath.ModulePath);
    }

    [Fact]
    public static void Config_FromJson_InputPath_IncludeSingle() {
        const string JSON = """
            {
                "input path": {
                    "include": "\\test"
                }
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        Assert.Single(config.InputPath);
        InputPath inputPath = config.InputPath[0];

        Assert.Equal("/test", inputPath.Include);
        Assert.Empty(inputPath.Excludes);
        Assert.True(inputPath.ModuleFiles);
        Assert.Null(inputPath.ModulePath);
    }

    [Fact]
    public static void Config_FromJson_InputPath_IncludeArray() {
        const string JSON = """
            {
                "input path": [
                    {
                        "include": "\\test"
                    }
                ]
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        Assert.Single(config.InputPath);
        InputPath inputPath = config.InputPath[0];

        Assert.Equal("/test", inputPath.Include);
        Assert.Empty(inputPath.Excludes);
        Assert.True(inputPath.ModuleFiles);
        Assert.Null(inputPath.ModulePath);
    }

    [Fact]
    public static void Config_FromJson_InputPath_ExcludesSingle() {
        const string JSON = """
            {
                "input path": [
                    {
                        "include": "\\test",
                        "excludes": "as\\df"
                    }
                ]
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        Assert.Single(config.InputPath);
        InputPath inputPath = config.InputPath[0];

        Assert.Equal("/test", inputPath.Include);
        Assert.Equal("as/df", inputPath.Excludes[0]);
        Assert.True(inputPath.ModuleFiles);
        Assert.Null(inputPath.ModulePath);
    }

    [Fact]
    public static void Config_FromJson_InputPath_ExcludesMultiple() {
        const string JSON = """
            {
                "input path": [
                    {
                        "include": "\\test",
                        "excludes": [ "as\\df", "ghjk" ]
                    }
                ]
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        Assert.Single(config.InputPath);
        InputPath inputPath = config.InputPath[0];

        Assert.Equal("/test", inputPath.Include);
        Assert.True(inputPath.Excludes.SequenceEqual(["as/df", "ghjk"]));
        Assert.True(inputPath.ModuleFiles);
        Assert.Null(inputPath.ModulePath);
    }

    [Fact]
    public static void Config_FromJson_InputPath_ModuleFiles() {
        const string JSON = """
            {
                "input path": [
                    {
                        "include": "\\test",
                        "module files": false
                    }
                ]
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        Assert.Single(config.InputPath);
        InputPath inputPath = config.InputPath[0];

        Assert.Equal("/test", inputPath.Include);
        Assert.Empty(inputPath.Excludes);
        Assert.False(inputPath.ModuleFiles);
        Assert.Null(inputPath.ModulePath);
    }

    [Fact]
    public static void Config_FromJson_InputPath_ModulePath() {
        const string JSON = """
            {
                "input path": [
                    {
                        "include": "\\test",
                        "module path": "yx\\cv"
                    }
                ]
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        Assert.Single(config.InputPath);
        InputPath inputPath = config.InputPath[0];

        Assert.Equal("/test", inputPath.Include);
        Assert.Empty(inputPath.Excludes);
        Assert.True(inputPath.ModuleFiles);
        Assert.Equal("yx/cv", inputPath.ModulePath);
    }

    [Fact]
    public static void Config_FromJson_InputPath_ExcludeAndModuleFileAndModulePath() {
        const string JSON = """
            {
                "input path": [
                    {
                        "include": "\\test",
                        "excludes": "as\\df",
                        "module files": false,
                        "module path": "yx\\cv"
                    }
                ]
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        Assert.Single(config.InputPath);
        InputPath inputPath = config.InputPath[0];

        Assert.Equal("/test", inputPath.Include);
        Assert.Equal("as/df", inputPath.Excludes[0]);
        Assert.False(inputPath.ModuleFiles);
        Assert.Equal("yx/cv", inputPath.ModulePath);
    }

    [Fact]
    public static void Config_FromJson_InputPath_Multiple() {
        const string JSON = """
            {
                "input path": [
                    {
                        "include": "\\test",
                        "excludes": "as\\df",
                        "module path": "yx\\cv"
                    },
                    {
                        "include": "q",
                        "excludes": ["w", "ww" ],
                        "module files": false
                    }
                ]
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        Assert.Equal(2, config.InputPath.Length);
        {
            InputPath inputPath = config.InputPath[0];
            Assert.Equal("/test", inputPath.Include);
            Assert.Equal("as/df", inputPath.Excludes[0]);
            Assert.True(inputPath.ModuleFiles);
            Assert.Equal("yx/cv", inputPath.ModulePath);
        }
        {
            InputPath inputPath = config.InputPath[1];
            Assert.Equal("q", inputPath.Include);
            Assert.True(inputPath.Excludes.SequenceEqual(["w", "ww"]));
            Assert.False(inputPath.ModuleFiles);
            Assert.Null(inputPath.ModulePath);
        }
    }

    #endregion

    [Theory]
    [InlineData("", (string[])[], true, null, """
        [
            {
              "include": "",
              "excludes": [],
              "module files": true
            }
          ]
        """)]
    [InlineData("", (string[])[], false, "", """
        [
            {
              "include": "",
              "excludes": [],
              "module files": false,
              "module path": ""
            }
          ]
        """)]
    [InlineData("qwer", (string[])["asdf"], true, "yxcv", """
        [
            {
              "include": "qwer",
              "excludes": [ "asdf" ],
              "module files": true,
              "module path": "yxcv"
            }
          ]
        """)]
    [InlineData("qwer", (string[])["asdf", "ghjk"], false, "yxcv", """
        [
            {
              "include": "qwer",
              "excludes": [
                "asdf",
                "ghjk"
              ],
              "module files": false,
              "module path": "yxcv"
            }
          ]
        """)]
    public static void Config_ToJson_InputPathWorks(string include, string[] excludes, bool moduleFiles, string? fileModulePath, string expected) {
        Config config = new() {
            InputPath = [new InputPath(include, excludes, moduleFiles, fileModulePath)]
        };
        string json = config.ToJson();

        Assert.Contains($""" "input path": {expected},""", json);
    }

    [Fact]
    public static void Config_ToJson_MultipleInputPath() {
        Config config = new() {
            InputPath = [
                new InputPath("qwer") { Excludes = ["asdf"], ModulePath = "yxcv" },
                new InputPath("rewq") { Excludes = ["fdsa", "kjhg"], ModulePath = "vcxy" }
            ]
        };
        string json = config.ToJson();

        const string expected = """
            "input path": [
                {
                  "include": "qwer",
                  "excludes": [ "asdf" ],
                  "module files": true,
                  "module path": "yxcv"
                },
                {
                  "include": "rewq",
                  "excludes": [
                    "fdsa",
                    "kjhg"
                  ],
                  "module files": true,
                  "module path": "vcxy"
                }
              ],
            """;
        Assert.Contains(expected, json);
    }

    [Fact]
    public static void Config_ToJson_EmptyInputPath() {
        Config config = new() {
            InputPath = []
        };
        string json = config.ToJson();

        Assert.Contains($""" "input path": [],""", json);
    }


    [Theory]
    [InlineData(""" "" """, (string[])[""])]
    [InlineData(""" "Something" """, (string[])["Something"])]
    [InlineData(""" [] """, (string[])[])]
    [InlineData(""" ["Something"] """, (string[])["Something"])]
    [InlineData(""" ["Something", "More"] """, (string[])["Something", "More"])]
    public static void Config_FromJson_UsingStatementsWork(string usingStatementsValue, string[] expected) {
        string json = $$"""
            {
                "using statements": {{usingStatementsValue}}
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, json);

        for (int i = 0; i < config.UsingStatements.Length; i++)
            Assert.Equal(expected[i], config.UsingStatements[i]);
    }

    [Theory]
    [InlineData((string[])[], """[]""")]
    [InlineData((string[])["Microsoft.AspNetCore.Components"], """[ "Microsoft.AspNetCore.Components" ]""")]
    [InlineData((string[])["qwer", "asdf", "yxcv"], """
                                                        [
                                                            "qwer",
                                                            "asdf",
                                                            "yxcv"
                                                          ]
                                                        """)]
    public static void Config_ToJson_UsingStatementsWorks(string[] usingStatements, string expected) {
        Config config = new() {
            UsingStatements = usingStatements
        };
        string json = config.ToJson();

        Assert.Contains($""" "using statements": {expected},""", json);
    }


    [Theory]
    [InlineData(""" "test": "Test" """, "test", "Test", (string?[])[])]
    [InlineData(""" "test": { "type": "Test" } """, "test", "Test", (string?[])[])]
    [InlineData(""" "test": { "type": "Test", "generic types": [] } """, "test", "Test", (string?[])[])]
    [InlineData(""" "test": { "type": "Test", "generic types": "TTest" } """, "test", "Test", (string?[])["TTest", null])]
    [InlineData(""" "test": { "type": "Test", "generic types": { "name": "TTest" } } """, "test", "Test", (string?[])["TTest", null])]
    [InlineData(""" "test": { "type": "Test", "generic types": { "name": "TTest", "constraint": "ITest" } } """, "test", "Test", (string?[])["TTest", "ITest"])]
    [InlineData(""" "test": { "type": "Test", "generic types": [ { "name": "TTest", "constraint": "ITest" } ] } """, "test", "Test", (string?[])["TTest", "ITest"])]
    [InlineData(""" "test": { "type": "Test", "generic types": [ { "name": "TTest1", "constraint": "ITest1" }, { "name": "TTest2", "constraint": "ITest2" } ] } """, "test", "Test", (string?[])["TTest1", "ITest1", "TTest2", "ITest2"])]
    public static void Config_FromJson_TypeMapWorks(string typeMapItemJson, string expectedKey, string expectedType, string?[] expectedGenericTypes) {
        // expected = [genericType1, constraint1, genericType2, constraint2, ...]

        string json = $$"""
            {
                "invoke function": {
                  "type map": { {{typeMapItemJson}} }
                }
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, json);

        Assert.Equal(expectedKey, config.TypeMap.Keys.First());
        Assert.Equal(expectedType, config.TypeMap[expectedKey].Type);
        for (int i = 0; i < config.TypeMap[expectedKey].GenericTypes.Length; i++) {
            Assert.Equal(expectedGenericTypes[2 * i], config.TypeMap[expectedKey].GenericTypes[i].Name);
            Assert.Equal(expectedGenericTypes[2 * i + 1], config.TypeMap[expectedKey].GenericTypes[i].Constraint);
        }
    }

    [Theory]
    [InlineData((string?[])[], """{ }""")]
    [InlineData((string?[])["key", "value"], """
                                                {
                                                      "key": "value"
                                                    }
                                                """)]
    [InlineData((string?[])["a", "b", "c", "d", "e", "f"], """
                                                            {
                                                                  "a": "b",
                                                                  "c": "d",
                                                                  "e": "f"
                                                                }
                                                            """)]
    public static void Config_ToJson_TypeMapSimpleMappingWorks(string[] types, string expected) {
        // types = [key1, value1, key2, value2, ...]

        Dictionary<string, MappedType> map = new(types.Length / 2);
        for (int i = 0; i < types.Length; i += 2)
            map.Add(types[i], new MappedType(types[i + 1]));

        Config config = new() {
            TypeMap = map
        };
        string json = config.ToJson();

        Assert.Contains($""" "type map": {expected}""", json);
    }

    [Theory]
    [InlineData("test", "Test", (string?[])["TTest", null], """
                                                                      "test": {
                                                                        "type": "Test",
                                                                        "generic types": [
                                                                          {
                                                                            "name": "TTest",
                                                                            "constraint": null
                                                                          }
                                                                        ]
                                                                      }
                                                                """)]
    [InlineData("test", "Test", (string?[])["TTest", "ITest"], """
                                                                      "test": {
                                                                        "type": "Test",
                                                                        "generic types": [
                                                                          {
                                                                            "name": "TTest",
                                                                            "constraint": "ITest"
                                                                          }
                                                                        ]
                                                                      }
                                                                """)]
    [InlineData("test", "Test", (string?[])["TTest1", "ITest1", "TTest2", "ITest2"], """
                                                                                              "test": {
                                                                                                "type": "Test",
                                                                                                "generic types": [
                                                                                                  {
                                                                                                    "name": "TTest1",
                                                                                                    "constraint": "ITest1"
                                                                                                  },
                                                                                                  {
                                                                                                    "name": "TTest2",
                                                                                                    "constraint": "ITest2"
                                                                                                  }
                                                                                                ]
                                                                                              }
                                                                                        """)]
    public static void Config_ToJson_TypeMapComplexMappingWorks(string key, string type, string?[] genericTypes, string expected) {
        // genericTypes = [genericType1, constraint1, genericType2, constraint2, ...]

        GenericType[] generics = new GenericType[genericTypes.Length / 2];
        for (int i = 0; i < generics.Length; i++)
            generics[i] = new GenericType(genericTypes[2 * i]!) { Constraint = genericTypes[2 * i + 1] };

        Dictionary<string, MappedType> map = new(1) {
            [key] = new MappedType(type, generics)
        };

        Config config = new() {
            TypeMap = map
        };
        string json = config.ToJson();

        Assert.Contains($$"""
            "type map": {
            {{expected}}
                }
            """, json);
    }


    [Theory]
    [InlineData("""true""", true, "I#module#Module", NameTransform.FirstUpperCase)]
    [InlineData("""false""", false, "I#module#Module", NameTransform.FirstUpperCase)]
    [InlineData("""{ "enabled": true }""", true, "I#module#Module", NameTransform.FirstUpperCase)]
    [InlineData("""{ "enabled": true, "interface name pattern": { "pattern": "test", "module transform": "none" } }""", true, "test", NameTransform.None)]
    public static void Config_FromJson_ModuleGroupingWorks(string moduleGroupingJson, bool expectedModuleGrouping, string expectedNamePattern, NameTransform expectedModuleTransform) {
        string json = $$"""
            {
              "module grouping": {{moduleGroupingJson}}
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, json);

        Assert.Equal(expectedModuleGrouping, config.ModuleGrouping);
        Assert.Equal(expectedNamePattern, config.ModuleGroupingNamePattern.NamePattern);
        Assert.Equal(expectedModuleTransform, config.ModuleGroupingNamePattern.ModuleTransform);
    }

    [Theory]
    [InlineData(true, "", NameTransform.None, """
                                                    {
                                                        "enabled": true,
                                                        "interface name pattern": {
                                                          "pattern": "",
                                                          "module transform": "None"
                                                        }
                                                      }
                                                    """)]
    [InlineData(false, "", NameTransform.None, """
                                                    {
                                                        "enabled": false,
                                                        "interface name pattern": {
                                                          "pattern": "",
                                                          "module transform": "None"
                                                        }
                                                      }
                                                    """)]
    [InlineData(false, "test", NameTransform.FirstUpperCase, """
                                                    {
                                                        "enabled": false,
                                                        "interface name pattern": {
                                                          "pattern": "test",
                                                          "module transform": "FirstUpperCase"
                                                        }
                                                      }
                                                    """)]
    public static void Config_ToJson_ModuleGroupingWorks(bool enabled, string interfaceNamePattern, NameTransform moduleTransform, string expected) {
        Config config = new() {
            ModuleGrouping = enabled,
            ModuleGroupingNamePattern = new ModuleNamePattern(interfaceNamePattern, moduleTransform, null!)
        };
        string json = config.ToJson();

        Assert.Contains($""" "module grouping": {expected}""", json);
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public static void Config_FromJson_ServiceExtensionWorks(bool expected) {
        string json = $$"""
            {
              "service extension": {{(expected ? "true" : "false")}}
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, json);

        Assert.Equal(expected, config.ServiceExtension);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public static void Config_ToJson_ServiceExtensionWorks(bool expected) {
        Config config = new() {
            ServiceExtension = expected
        };
        string json = config.ToJson();
        Assert.Contains($""" "service extension": {(expected ? "true" : "false")}""", json);
    }


    #region NamePattern

    private const string FUNCTION = "function";
    private const string MODULE = "Module";
    private const string ACTION = "Action";


    [Theory]
    [InlineData("#function#_#module#_#action#", $"{FUNCTION}_{MODULE}_{ACTION}", 0)]
    [InlineData("#function##module##action#", $"{FUNCTION}{MODULE}{ACTION}", 0)]
    [InlineData("test#function##module##action#", $"test{FUNCTION}{MODULE}{ACTION}", 0)]
    [InlineData("test2", $"test2", 0)]
    [InlineData("#action#", $"{ACTION}", 0)]
    [InlineData("", "", 0)]
    [InlineData("#wrong#", "", 1)]
    [InlineData("test#", "test", 1)]
    [InlineData("#test", "", 1)]
    [InlineData("##", "", 1)]
    [InlineData("#modole#", "", 1)]
    [InlineData("test#fonction#", "test", 1)]
    [InlineData("###", "", 2)]
    public static void FunctionNamePattern_ParsingWorks(string naming, string expected, int errorCount) {
        List<Diagnostic> errorList = [];
        FunctionNamePattern functionNaming = new(naming, NameTransform.None, NameTransform.None, NameTransform.None, errorList);

        StringBuilder builder = new();
        functionNaming.AppendNaming(builder, MODULE, FUNCTION, ACTION);
        string result = builder.ToString();

        Assert.Equal(expected, result);
        Assert.Equal(errorCount, errorList.Count);
    }

    [Theory]
    [InlineData(NameTransform.None, NameTransform.None, NameTransform.None, "#module##function##action#", $"{MODULE}{FUNCTION}{ACTION}")]
    [InlineData(NameTransform.None, NameTransform.UpperCase, NameTransform.None, "#function#", "FUNCTION")]
    [InlineData(NameTransform.LowerCase, NameTransform.None, NameTransform.None, "#module#", "module")]
    [InlineData(NameTransform.None, NameTransform.FirstUpperCase, NameTransform.None, "#function#", "Function")]
    [InlineData(NameTransform.FirstLowerCase, NameTransform.None, NameTransform.None, "#module#", "module")]
    [InlineData(NameTransform.None, NameTransform.None, NameTransform.FirstLowerCase, "#action#", "action")]
    public static void FunctionNamePattern_TransformWorks(NameTransform module, NameTransform function, NameTransform action, string naming, string expected) {
        FunctionNamePattern functionNaming = new(naming, module, function, action, null!);

        StringBuilder builder = new();
        functionNaming.AppendNaming(builder, MODULE, FUNCTION, ACTION);
        string result = builder.ToString();

        Assert.Equal(expected, result);
    }


    [Theory]
    [InlineData("Preload#module#", $"Preload{MODULE}", 0)]
    [InlineData("#module#", $"{MODULE}", 0)]
    [InlineData("#module##module##module#", $"{MODULE}{MODULE}{MODULE}", 0)]
    [InlineData("test2", $"test2", 0)]
    [InlineData("", "", 0)]
    [InlineData("#wrong#", "", 1)]
    [InlineData("#function#", "", 1)]
    [InlineData("#action#", "", 1)]
    [InlineData("test#", "test", 1)]
    [InlineData("#test", "", 1)]
    [InlineData("##", "", 1)]
    [InlineData("#modole#", "", 1)]
    [InlineData("test#function#", "test", 1)]
    [InlineData("###", "", 2)]
    public static void PreloadNamePattern_ParsingWorks(string naming, string expected, int errorCount) {
        List<Diagnostic> errorList = [];
        ModuleNamePattern preLoadNaming = new(naming, NameTransform.None, errorList);

        StringBuilder builder = new();
        preLoadNaming.AppendNaming(builder, MODULE);
        string result = builder.ToString();

        Assert.Equal(expected, result);
        Assert.Equal(errorCount, errorList.Count);
    }

    [Theory]
    [InlineData(NameTransform.None, "#module##module##module#", $"{MODULE}{MODULE}{MODULE}")]
    [InlineData(NameTransform.UpperCase, "#module#", "MODULE")]
    [InlineData(NameTransform.LowerCase, "#module#", "module")]
    [InlineData(NameTransform.FirstLowerCase, "#module#", "module")]
    public static void PreloadNamePattern_TransformWorks(NameTransform module, string naming, string expected) {
        ModuleNamePattern preLoadNaming = new(naming, module, null!);

        StringBuilder builder = new();
        preLoadNaming.AppendNaming(builder, MODULE);
        string result = builder.ToString();

        Assert.Equal(expected, result);
    }

    #endregion
}
