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
        Config config = new("{}");

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

        #region maps to one property but has leaf nodes

        int numberOfLeafNodes = 0;

        jsonObject.Remove("declaration path");
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
        // expected = [include1, exclude1, fileModulePath1, include2, exclude2, fileModulePath2, ...]

        Config config = new(json);

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
            DeclarationPath = [new DeclarationPath(include, excludes, fileModulePath)]
        };
        string json = config.ToJson();

        Assert.Contains($""" "declaration path": {expected},""", json);
    }

    [Fact]
    public void Config_ToJson_MultipleDeclarationPath() {
        Config config = new() {
            DeclarationPath = [
                new DeclarationPath("qwer") { Excludes = ["asdf"], FileModulePath = "yxcv" },
                new DeclarationPath("rewq") { Excludes = ["fdsa", "kjhg"], FileModulePath = "vcxy" }
            ]
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
            DeclarationPath = []
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
        Config config = new(json);

        for (int i = 0; i < config.UsingStatements.Length; i++)
            Assert.Equal(expected[i], config.UsingStatements[i]);
    }

    [Theory]
    [InlineData(new string[0], """[]""")]
    [InlineData(new string[1] { "Microsoft.AspNetCore.Components" }, """[ "Microsoft.AspNetCore.Components" ]""")]
    [InlineData(new string[3] { "qwer", "asdf", "yxcv" }, """
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
    [InlineData(""" "test": "Test" """, "test", "Test", new string?[0])]
    [InlineData(""" "test": { "type": "Test" } """, "test", "Test", new string?[0])]
    [InlineData(""" "test": { "type": "Test", "generic types": [] } """, "test", "Test", new string?[0])]
    [InlineData(""" "test": { "type": "Test", "generic types": "TTest" } """, "test", "Test", new string?[2] { "TTest", null })]
    [InlineData(""" "test": { "type": "Test", "generic types": { "name": "TTest" } } """, "test", "Test", new string?[2] { "TTest", null })]
    [InlineData(""" "test": { "type": "Test", "generic types": { "name": "TTest", "constraint": "ITest" } } """, "test", "Test", new string?[2] { "TTest", "ITest" })]
    [InlineData(""" "test": { "type": "Test", "generic types": [ { "name": "TTest", "constraint": "ITest" } ] } """, "test", "Test", new string?[2] { "TTest", "ITest" })]
    [InlineData(""" "test": { "type": "Test", "generic types": [ { "name": "TTest1", "constraint": "ITest1" }, { "name": "TTest2", "constraint": "ITest2" } ] } """, "test", "Test", new string?[4] { "TTest1", "ITest1", "TTest2", "ITest2" })]
    public void Config_FromJson_TypeMapWorks(string typeMapItemJson, string expectedKey, string expectedType, string?[] expectedGenericTypes) {
        // expected = [genericType1, constraint1, genericType2, constraint2, ...]

        string json = $$"""
            {
                "invoke function": {
                  "type map": { {{typeMapItemJson}} }
                }
            }
            """;
        Config config = new(json);

        Assert.Equal(expectedKey, config.TypeMap.Keys.First());
        Assert.Equal(expectedType, config.TypeMap[expectedKey].Type);
        for (int i = 0; i < config.TypeMap[expectedKey].GenericTypes.Length; i++) {
            Assert.Equal(expectedGenericTypes[2 * i], config.TypeMap[expectedKey].GenericTypes[i].Name);
            Assert.Equal(expectedGenericTypes[2 * i + 1], config.TypeMap[expectedKey].GenericTypes[i].Constraint);
        }
    }

    [Theory]
    [InlineData(new string[0], """{ }""")]
    [InlineData(new string[2] { "key", "value" }, """
                                                {
                                                      "key": "value"
                                                    }
                                                """)]
    [InlineData(new string[6] { "a", "b", "c", "d", "e", "f" }, """
                                                            {
                                                                  "a": "b",
                                                                  "c": "d",
                                                                  "e": "f"
                                                                }
                                                            """)]
    public void Config_ToJson_TypeMapSimpleMappingWorks(string[] types , string expected) {
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
    [InlineData("test", "Test", new string?[2] { "TTest", null }, """
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
    [InlineData("test", "Test", new string?[2] { "TTest", "ITest" }, """
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
    [InlineData("test", "Test", new string?[4] { "TTest1", "ITest1", "TTest2", "ITest2" }, """
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
    public void Config_ToJson_TypeMapComplexMappingWorks(string key, string type, string?[] genericTypes, string expected) {
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
    [InlineData("""true""", true, true, "I#module#Module", NameTransform.FirstUpperCase)]
    [InlineData("""false""", false, true, "I#module#Module", NameTransform.FirstUpperCase)]
    [InlineData("""{ "enabled": true }""", true, true, "I#module#Module", NameTransform.FirstUpperCase)]
    [InlineData("""{ "enabled": true, "service extension": false }""", true, false, "I#module#Module", NameTransform.FirstUpperCase)]
    [InlineData("""{ "enabled": true, "service extension": false, "interface name pattern": { "pattern": "test", "module transform": "none" } }""", true, false, "test", NameTransform.None)]
    public void Config_FromJson_ModuleGroupingWorks(string moduleGroupingJson, bool expectedModuleGrouping, bool expectedServiceExtension, string expectedNamePattern, NameTransform expectedModuleTransform) {
        string json = $$"""
            {
              "module grouping": {{moduleGroupingJson}}
            }
            """;
        Config config = new(json);

        Assert.Equal(expectedModuleGrouping, config.ModuleGrouping);
        Assert.Equal(expectedServiceExtension, config.ModuleGroupingServiceExtension);
        Assert.Equal(expectedNamePattern, config.ModuleGroupingNamePattern.NamePattern);
        Assert.Equal(expectedModuleTransform, config.ModuleGroupingNamePattern.ModuleTransform);
    }

    [Theory]
    [InlineData(true, false, "", NameTransform.None, """
                                                    {
                                                        "enabled": true,
                                                        "service extension": false,
                                                        "interface name pattern": {
                                                          "pattern": "",
                                                          "module transform": "None"
                                                        }
                                                      }
                                                    """)]
    [InlineData(false, true, "", NameTransform.None, """
                                                    {
                                                        "enabled": false,
                                                        "service extension": true,
                                                        "interface name pattern": {
                                                          "pattern": "",
                                                          "module transform": "None"
                                                        }
                                                      }
                                                    """)]
    [InlineData(false, false, "test", NameTransform.FirstUpperCase, """
                                                    {
                                                        "enabled": false,
                                                        "service extension": false,
                                                        "interface name pattern": {
                                                          "pattern": "test",
                                                          "module transform": "FirstUpperCase"
                                                        }
                                                      }
                                                    """)]
    public void Config_ToJson_ModuleGroupingWorks(bool enabled, bool serviceExtension, string interfaceNamePattern, NameTransform moduleTransform, string expected) {
        Config config = new() {
            ModuleGrouping = enabled,
            ModuleGroupingServiceExtension = serviceExtension,
            ModuleGroupingNamePattern = new ModuleNamePattern(interfaceNamePattern, moduleTransform)
        };
        string json = config.ToJson();

        Assert.Contains($""" "module grouping": {expected}""", json);
    }


    [Fact]
    public void StructureTreeEquals() {
        Config configA = new();
        Config configB = new();

        Assert.True(configA.StructureTreeEquals(configB));


        configB = configA with { UsingStatements = [] };
        Assert.False(configA.StructureTreeEquals(configB));


        configB = configA with { InvokeFunctionSyncEnabled = true };
        Assert.False(configA.StructureTreeEquals(configB));

        configB = configA with { InvokeFunctionTrySyncEnabled = false };
        Assert.False(configA.StructureTreeEquals(configB));

        configB = configA with { InvokeFunctionAsyncEnabled = true };
        Assert.False(configA.StructureTreeEquals(configB));


        configB = configA with { InvokeFunctionNamePattern = new FunctionNamePattern("test", NameTransform.None, NameTransform.None, NameTransform.None) };
        Assert.False(configA.StructureTreeEquals(configB));


        configB = configA with { PromiseOnlyAsync = false };
        Assert.False(configA.StructureTreeEquals(configB));

        configB = configA with { PromiseAppendAsync = true };
        Assert.False(configA.StructureTreeEquals(configB));


        configB = configA with { TypeMap = [] };
        Assert.False(configA.StructureTreeEquals(configB));


        configB = configA with { PreloadNamePattern = new ModuleNamePattern("test", NameTransform.None) };
        Assert.False(configA.StructureTreeEquals(configB));

        configB = configA with { PreloadAllModulesName = "test" };
        Assert.False(configA.StructureTreeEquals(configB));


        configB = configA with { JSRuntimeSyncEnabled = true };
        Assert.False(configA.StructureTreeEquals(configB));

        configB = configA with { JSRuntimeTrySyncEnabled = true };
        Assert.False(configA.StructureTreeEquals(configB));

        configB = configA with { JSRuntimeAsyncEnabled = true };
        Assert.False(configA.StructureTreeEquals(configB));
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
        foreach (string str in functionNaming.GetNaming(MODULE, FUNCTION, ACTION))
            builder.Append(str);
        string result = builder.ToString();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(NameTransform.None, NameTransform.None, NameTransform.None, "#module##function##action#", $"{MODULE}{FUNCTION}{ACTION}")]
    [InlineData(NameTransform.None, NameTransform.UpperCase, NameTransform.None, "#function#", "FUNCTION")]
    [InlineData(NameTransform.LowerCase, NameTransform.None, NameTransform.None, "#module#", "module")]
    [InlineData(NameTransform.None, NameTransform.FirstUpperCase, NameTransform.None, "#function#", "Function")]
    [InlineData(NameTransform.FirstLowerCase, NameTransform.None, NameTransform.None, "#module#", "module")]
    [InlineData(NameTransform.None, NameTransform.None, NameTransform.FirstLowerCase, "#action#", "action")]
    public void FunctionNamePattern_TransformWorks(NameTransform module, NameTransform function, NameTransform action, string naming, string expected) {
        FunctionNamePattern functionNaming = new(naming, module, function, action);

        StringBuilder builder = new();
        foreach (string str in functionNaming.GetNaming(MODULE, FUNCTION, ACTION))
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
        ModuleNamePattern preLoadNaming = new(naming, NameTransform.None);

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
        ModuleNamePattern preLoadNaming = new(naming, module);

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
            ModuleNamePattern preLoadNaming = new(naming, NameTransform.None);
            Assert.Fail("No Exception happened");
        }
        catch (ArgumentException) { }
    }

    #endregion
}
