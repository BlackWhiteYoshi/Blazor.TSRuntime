using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using TSRuntime.Configs;
using TSRuntime.Configs.NamePattern;

namespace TSRuntime.Tests;

public sealed class ConfigTests {
    [Test]
    public async ValueTask Config_FieldsHaveDefaultValues() {
        Config config = new();

        foreach (PropertyInfo property in typeof(Config).GetProperties()) {
            if (property.Name == "ErrorList")
                continue;

            object? value = property.GetValue(config);
            await Assert.That(value).IsNotNull();
            if (value is IEnumerable<object?> enumerable)
                await Assert.That(enumerable).IsNotEmpty();
        }
    }

    [Test]
    public async ValueTask Config_EmptyJsonHaveDefaultValues() {
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, "{}");

        foreach (PropertyInfo property in typeof(Config).GetProperties()) {
            if (property.Name == "ErrorList")
                continue;

            object? value = property.GetValue(config);
            await Assert.That(value).IsNotNull();
            if (value is IEnumerable<object?> enumerable)
                await Assert.That(enumerable).IsNotEmpty();
        }
    }

    [Test]
    public async ValueTask Config_ToJsonSavesAllProperties() {
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

        await Assert.That(numberOfLeafNodes).IsEqualTo(propertyCount);


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

    [Test]
    public async ValueTask Config_FromJson_InputPath_Empty() {
        const string JSON = """
            {
                "input path": ""
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        await Assert.That(config.InputPath).HasSingleItem();
        InputPath inputPath = config.InputPath[0];

        await Assert.That(inputPath.Include).IsEqualTo("");
        await Assert.That(inputPath.Excludes).IsEmpty();
        await Assert.That(inputPath.ModuleFiles).IsTrue();
        await Assert.That(inputPath.ModulePath).IsNull();
    }

    [Test]
    public async ValueTask Config_FromJson_InputPath_IncludeShorthand() {
        const string JSON = """
            {
                "input path": "\\test"
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        await Assert.That(config.InputPath).HasSingleItem();
        InputPath inputPath = config.InputPath[0];

        await Assert.That(inputPath.Include).IsEqualTo("/test");
        await Assert.That(inputPath.Excludes).IsEmpty();
        await Assert.That(inputPath.ModuleFiles).IsTrue();
        await Assert.That(inputPath.ModulePath).IsNull();
    }

    [Test]
    public async ValueTask Config_FromJson_InputPath_IncludeSingle() {
        const string JSON = """
            {
                "input path": {
                    "include": "\\test"
                }
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);

        await Assert.That(config.InputPath).HasSingleItem();
        InputPath inputPath = config.InputPath[0];

        await Assert.That(inputPath.Include).IsEqualTo("/test");
        await Assert.That(inputPath.Excludes).IsEmpty();
        await Assert.That(inputPath.ModuleFiles).IsTrue();
        await Assert.That(inputPath.ModulePath).IsNull();
    }

    [Test]
    public async ValueTask Config_FromJson_InputPath_IncludeArray() {
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

        await Assert.That(config.InputPath).HasSingleItem();
        InputPath inputPath = config.InputPath[0];

        await Assert.That(inputPath.Include).IsEqualTo("/test");
        await Assert.That(inputPath.Excludes).IsEmpty();
        await Assert.That(inputPath.ModuleFiles).IsTrue();
        await Assert.That(inputPath.ModulePath).IsNull();
    }

    [Test]
    public async ValueTask Config_FromJson_InputPath_ExcludesSingle() {
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

        await Assert.That(config.InputPath).HasSingleItem();
        InputPath inputPath = config.InputPath[0];

        await Assert.That(inputPath.Include).IsEqualTo("/test");
        await Assert.That(inputPath.Excludes[0]).IsEqualTo("as/df");
        await Assert.That(inputPath.ModuleFiles).IsTrue();
        await Assert.That(inputPath.ModulePath).IsNull();
    }

    [Test]
    public async ValueTask Config_FromJson_InputPath_ExcludesMultiple() {
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

        await Assert.That(config.InputPath).HasSingleItem();
        InputPath inputPath = config.InputPath[0];

        await Assert.That(inputPath.Include).IsEqualTo("/test");
        await Assert.That(inputPath.Excludes.SequenceEqual(["as/df", "ghjk"])).IsTrue();
        await Assert.That(inputPath.ModuleFiles).IsTrue();
        await Assert.That(inputPath.ModulePath).IsNull();
    }

    [Test]
    public async ValueTask Config_FromJson_InputPath_ModuleFiles() {
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

        await Assert.That(config.InputPath).HasSingleItem();
        InputPath inputPath = config.InputPath[0];

        await Assert.That(inputPath.Include).IsEqualTo("/test");
        await Assert.That(inputPath.Excludes).IsEmpty();
        await Assert.That(inputPath.ModuleFiles).IsFalse();
        await Assert.That(inputPath.ModulePath).IsNull();
    }

    [Test]
    public async ValueTask Config_FromJson_InputPath_ModulePath() {
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

        await Assert.That(config.InputPath).HasSingleItem();
        InputPath inputPath = config.InputPath[0];

        await Assert.That(inputPath.Include).IsEqualTo("/test");
        await Assert.That(inputPath.Excludes).IsEmpty();
        await Assert.That(inputPath.ModuleFiles).IsTrue();
        await Assert.That(inputPath.ModulePath).IsEqualTo("yx/cv");
    }

    [Test]
    public async ValueTask Config_FromJson_InputPath_ExcludeAndModuleFileAndModulePath() {
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

        await Assert.That(config.InputPath).HasSingleItem();
        InputPath inputPath = config.InputPath[0];

        await Assert.That(inputPath.Include).IsEqualTo("/test");
        await Assert.That(inputPath.Excludes[0]).IsEqualTo("as/df");
        await Assert.That(inputPath.ModuleFiles).IsFalse();
        await Assert.That(inputPath.ModulePath).IsEqualTo("yx/cv");
    }

    [Test]
    public async ValueTask Config_FromJson_InputPath_Multiple() {
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

        await Assert.That(config.InputPath.Length).IsEqualTo(2);
        {
            InputPath inputPath = config.InputPath[0];
            await Assert.That(inputPath.Include).IsEqualTo("/test");
            await Assert.That(inputPath.Excludes[0]).IsEqualTo("as/df");
            await Assert.That(inputPath.ModuleFiles).IsTrue();
            await Assert.That(inputPath.ModulePath).IsEqualTo("yx/cv");
        }
        {
            InputPath inputPath = config.InputPath[1];
            await Assert.That(inputPath.Include).IsEqualTo("q");
            await Assert.That(inputPath.Excludes.SequenceEqual(["w", "ww"])).IsTrue();
            await Assert.That(inputPath.ModuleFiles).IsFalse();
            await Assert.That(inputPath.ModulePath).IsNull();
        }
    }

    #endregion

    [Test]
    [Arguments("", (string[])[], true, null, """
        [
            {
              "include": "",
              "excludes": [],
              "module files": true
            }
          ]
        """)]
    [Arguments("", (string[])[], false, "", """
        [
            {
              "include": "",
              "excludes": [],
              "module files": false,
              "module path": ""
            }
          ]
        """)]
    [Arguments("qwer", (string[])["asdf"], true, "yxcv", """
        [
            {
              "include": "qwer",
              "excludes": [ "asdf" ],
              "module files": true,
              "module path": "yxcv"
            }
          ]
        """)]
    [Arguments("qwer", (string[])["asdf", "ghjk"], false, "yxcv", """
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
    public async ValueTask Config_ToJson_InputPathWorks(string include, string[] excludes, bool moduleFiles, string? fileModulePath, string expected) {
        Config config = new() {
            InputPath = [new InputPath(include, excludes, moduleFiles, fileModulePath)]
        };
        string json = config.ToJson();

        await Assert.That(json).Contains($""" "input path": {expected},""");
    }

    [Test]
    public async ValueTask Config_ToJson_MultipleInputPath() {
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
        await Assert.That(json).Contains(expected);
    }

    [Test]
    public async ValueTask Config_ToJson_EmptyInputPath() {
        Config config = new() {
            InputPath = []
        };
        string json = config.ToJson();

        await Assert.That(json).Contains($""" "input path": [],""");
    }


    [Test]
    [Arguments(""" "" """, (string[])[""])]
    [Arguments(""" "Something" """, (string[])["Something"])]
    [Arguments(""" [] """, (string[])[])]
    [Arguments(""" ["Something"] """, (string[])["Something"])]
    [Arguments(""" ["Something", "More"] """, (string[])["Something", "More"])]
    public async ValueTask Config_FromJson_UsingStatementsWork(string usingStatementsValue, string[] expected) {
        string json = $$"""
            {
                "using statements": {{usingStatementsValue}}
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, json);

        for (int i = 0; i < config.UsingStatements.Length; i++)
            await Assert.That(config.UsingStatements[i]).IsEqualTo(expected[i]);
    }

    [Test]
    [Arguments((string[])[], """[]""")]
    [Arguments((string[])["Microsoft.AspNetCore.Components"], """[ "Microsoft.AspNetCore.Components" ]""")]
    [Arguments((string[])["qwer", "asdf", "yxcv"], """
                                                        [
                                                            "qwer",
                                                            "asdf",
                                                            "yxcv"
                                                          ]
                                                        """)]
    public async ValueTask Config_ToJson_UsingStatementsWorks(string[] usingStatements, string expected) {
        Config config = new() {
            UsingStatements = usingStatements
        };
        string json = config.ToJson();

        await Assert.That(json).Contains($""" "using statements": {expected},""");
    }


    [Test]
    [Arguments(""" "test": "Test" """, "test", "Test", (string?[])[])]
    [Arguments(""" "test": { "type": "Test" } """, "test", "Test", (string?[])[])]
    [Arguments(""" "test": { "type": "Test", "generic types": [] } """, "test", "Test", (string?[])[])]
    [Arguments(""" "test": { "type": "Test", "generic types": "TTest" } """, "test", "Test", (string?[])["TTest", null])]
    [Arguments(""" "test": { "type": "Test", "generic types": { "name": "TTest" } } """, "test", "Test", (string?[])["TTest", null])]
    [Arguments(""" "test": { "type": "Test", "generic types": { "name": "TTest", "constraint": "ITest" } } """, "test", "Test", (string?[])["TTest", "ITest"])]
    [Arguments(""" "test": { "type": "Test", "generic types": [ { "name": "TTest", "constraint": "ITest" } ] } """, "test", "Test", (string?[])["TTest", "ITest"])]
    [Arguments(""" "test": { "type": "Test", "generic types": [ { "name": "TTest1", "constraint": "ITest1" }, { "name": "TTest2", "constraint": "ITest2" } ] } """, "test", "Test", (string?[])["TTest1", "ITest1", "TTest2", "ITest2"])]
    public async ValueTask Config_FromJson_TypeMapWorks(string typeMapItemJson, string expectedKey, string expectedType, string?[] expectedGenericTypes) {
        // expected = [genericType1, constraint1, genericType2, constraint2, ...]

        string json = $$"""
            {
                "invoke function": {
                  "type map": { {{typeMapItemJson}} }
                }
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, json);

        await Assert.That(config.TypeMap.Keys.First()).IsEqualTo(expectedKey);
        await Assert.That(config.TypeMap[expectedKey].Type).IsEqualTo(expectedType);
        for (int i = 0; i < config.TypeMap[expectedKey].GenericTypes.Length; i++) {
            await Assert.That(config.TypeMap[expectedKey].GenericTypes[i].Name).IsEqualTo(expectedGenericTypes[2 * i]);
            await Assert.That(config.TypeMap[expectedKey].GenericTypes[i].Constraint).IsEqualTo(expectedGenericTypes[2 * i + 1]);
        }
    }

    [Test]
    [Arguments((string?[])[], """{ }""")]
    [Arguments((string?[])["key", "value"], """
                                                {
                                                      "key": "value"
                                                    }
                                                """)]
    [Arguments((string?[])["a", "b", "c", "d", "e", "f"], """
                                                            {
                                                                  "a": "b",
                                                                  "c": "d",
                                                                  "e": "f"
                                                                }
                                                            """)]
    public async ValueTask Config_ToJson_TypeMapSimpleMappingWorks(string[] types, string expected) {
        // types = [key1, value1, key2, value2, ...]

        Dictionary<string, MappedType> map = new(types.Length / 2);
        for (int i = 0; i < types.Length; i += 2)
            map.Add(types[i], new MappedType(types[i + 1]));

        Config config = new() {
            TypeMap = map
        };
        string json = config.ToJson();

        await Assert.That(json).Contains($""" "type map": {expected}""");
    }

    [Test]
    [Arguments("test", "Test", (string?[])["TTest", null], """
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
    [Arguments("test", "Test", (string?[])["TTest", "ITest"], """
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
    [Arguments("test", "Test", (string?[])["TTest1", "ITest1", "TTest2", "ITest2"], """
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
    public async ValueTask Config_ToJson_TypeMapComplexMappingWorks(string key, string type, string?[] genericTypes, string expected) {
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

        await Assert.That(json).Contains($$"""
            "type map": {
            {{expected}}
                }
            """);
    }


    [Test]
    [Arguments("""true""", true, "I#module#Module", NameTransform.FirstUpperCase)]
    [Arguments("""false""", false, "I#module#Module", NameTransform.FirstUpperCase)]
    [Arguments("""{ "enabled": true }""", true, "I#module#Module", NameTransform.FirstUpperCase)]
    [Arguments("""{ "enabled": true, "interface name pattern": { "pattern": "test", "module transform": "none" } }""", true, "test", NameTransform.None)]
    public async ValueTask Config_FromJson_ModuleGroupingWorks(string moduleGroupingJson, bool expectedModuleGrouping, string expectedNamePattern, NameTransform expectedModuleTransform) {
        string json = $$"""
            {
              "module grouping": {{moduleGroupingJson}}
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, json);

        await Assert.That(config.ModuleGrouping).IsEqualTo(expectedModuleGrouping);
        await Assert.That(config.ModuleGroupingNamePattern.NamePattern).IsEqualTo(expectedNamePattern);
        await Assert.That(config.ModuleGroupingNamePattern.ModuleTransform).IsEqualTo(expectedModuleTransform);
    }

    [Test]
    [Arguments(true, "", NameTransform.None, """
                                                    {
                                                        "enabled": true,
                                                        "interface name pattern": {
                                                          "pattern": "",
                                                          "module transform": "None"
                                                        }
                                                      }
                                                    """)]
    [Arguments(false, "", NameTransform.None, """
                                                    {
                                                        "enabled": false,
                                                        "interface name pattern": {
                                                          "pattern": "",
                                                          "module transform": "None"
                                                        }
                                                      }
                                                    """)]
    [Arguments(false, "test", NameTransform.FirstUpperCase, """
                                                    {
                                                        "enabled": false,
                                                        "interface name pattern": {
                                                          "pattern": "test",
                                                          "module transform": "FirstUpperCase"
                                                        }
                                                      }
                                                    """)]
    public async ValueTask Config_ToJson_ModuleGroupingWorks(bool enabled, string interfaceNamePattern, NameTransform moduleTransform, string expected) {
        Config config = new() {
            ModuleGrouping = enabled,
            ModuleGroupingNamePattern = new ModuleNamePattern(interfaceNamePattern, moduleTransform, null!)
        };
        string json = config.ToJson();

        await Assert.That(json).Contains($""" "module grouping": {expected}""");
    }


    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async ValueTask Config_FromJson_ServiceExtensionWorks(bool expected) {
        string json = $$"""
            {
              "service extension": {{(expected ? "true" : "false")}}
            }
            """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, json);

        await Assert.That(config.ServiceExtension).IsEqualTo(expected);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async ValueTask Config_ToJson_ServiceExtensionWorks(bool expected) {
        Config config = new() {
            ServiceExtension = expected
        };
        string json = config.ToJson();
        await Assert.That(json).Contains($""" "service extension": {(expected ? "true" : "false")}""");
    }


    #region NamePattern

    private const string FUNCTION = "function";
    private const string MODULE = "Module";
    private const string ACTION = "Action";


    [Test]
    [Arguments("#function#_#module#_#action#", $"{FUNCTION}_{MODULE}_{ACTION}", 0)]
    [Arguments("#function##module##action#", $"{FUNCTION}{MODULE}{ACTION}", 0)]
    [Arguments("test#function##module##action#", $"test{FUNCTION}{MODULE}{ACTION}", 0)]
    [Arguments("test2", $"test2", 0)]
    [Arguments("#action#", $"{ACTION}", 0)]
    [Arguments("", "", 0)]
    [Arguments("#wrong#", "", 1)]
    [Arguments("test#", "test", 1)]
    [Arguments("#test", "", 1)]
    [Arguments("##", "", 1)]
    [Arguments("#modole#", "", 1)]
    [Arguments("test#fonction#", "test", 1)]
    [Arguments("###", "", 2)]
    public async ValueTask FunctionNamePattern_ParsingWorks(string naming, string expected, int errorCount) {
        List<Diagnostic> errorList = [];
        FunctionNamePattern functionNaming = new(naming, NameTransform.None, NameTransform.None, NameTransform.None, errorList);

        StringBuilder builder = new();
        functionNaming.AppendNaming(builder, MODULE, FUNCTION, ACTION);
        string result = builder.ToString();

        await Assert.That(result).IsEqualTo(expected);
        await Assert.That(errorList.Count).IsEqualTo(errorCount);
    }

    [Test]
    [Arguments(NameTransform.None, NameTransform.None, NameTransform.None, "#module##function##action#", $"{MODULE}{FUNCTION}{ACTION}")]
    [Arguments(NameTransform.None, NameTransform.UpperCase, NameTransform.None, "#function#", "FUNCTION")]
    [Arguments(NameTransform.LowerCase, NameTransform.None, NameTransform.None, "#module#", "module")]
    [Arguments(NameTransform.None, NameTransform.FirstUpperCase, NameTransform.None, "#function#", "Function")]
    [Arguments(NameTransform.FirstLowerCase, NameTransform.None, NameTransform.None, "#module#", "module")]
    [Arguments(NameTransform.None, NameTransform.None, NameTransform.FirstLowerCase, "#action#", "action")]
    public async ValueTask FunctionNamePattern_TransformWorks(NameTransform module, NameTransform function, NameTransform action, string naming, string expected) {
        FunctionNamePattern functionNaming = new(naming, module, function, action, null!);

        StringBuilder builder = new();
        functionNaming.AppendNaming(builder, MODULE, FUNCTION, ACTION);
        string result = builder.ToString();

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async ValueTask FunctionNamePattern_MissingActionWarning() {
        const string JSON = """{ "invoke function": { "sync enabled": true, "trysync enabled": true, "async enabled": true } } """;
        Config config = new(GenerateSourceTextExtension.CONFIG_FOLDER_PATH, JSON);
        await Assert.That(config.ErrorList).HasSingleItem();
        await Assert.That(config.ErrorList[0].GetMessage()).IsEqualTo("malformed config: '[invoke function].[name pattern].[pattern]' should contain '#action#' when 2 or more method types are enabled, otherwise it leads to duplicate method naming");
    }


    [Test]
    [Arguments("Preload#module#", $"Preload{MODULE}", 0)]
    [Arguments("#module#", $"{MODULE}", 0)]
    [Arguments("#module##module##module#", $"{MODULE}{MODULE}{MODULE}", 0)]
    [Arguments("test2", $"test2", 0)]
    [Arguments("", "", 0)]
    [Arguments("#wrong#", "", 1)]
    [Arguments("#function#", "", 1)]
    [Arguments("#action#", "", 1)]
    [Arguments("test#", "test", 1)]
    [Arguments("#test", "", 1)]
    [Arguments("##", "", 1)]
    [Arguments("#modole#", "", 1)]
    [Arguments("test#function#", "test", 1)]
    [Arguments("###", "", 2)]
    public async ValueTask PreloadNamePattern_ParsingWorks(string naming, string expected, int errorCount) {
        List<Diagnostic> errorList = [];
        ModuleNamePattern preLoadNaming = new(naming, NameTransform.None, errorList);

        StringBuilder builder = new();
        preLoadNaming.AppendNaming(builder, MODULE);
        string result = builder.ToString();

        await Assert.That(result).IsEqualTo(expected);
        await Assert.That(errorList.Count).IsEqualTo(errorCount);
    }

    [Test]
    [Arguments(NameTransform.None, "#module##module##module#", $"{MODULE}{MODULE}{MODULE}")]
    [Arguments(NameTransform.UpperCase, "#module#", "MODULE")]
    [Arguments(NameTransform.LowerCase, "#module#", "module")]
    [Arguments(NameTransform.FirstLowerCase, "#module#", "module")]
    public async ValueTask PreloadNamePattern_TransformWorks(NameTransform module, string naming, string expected) {
        ModuleNamePattern preLoadNaming = new(naming, module, null!);

        StringBuilder builder = new();
        preLoadNaming.AppendNaming(builder, MODULE);
        string result = builder.ToString();

        await Assert.That(result).IsEqualTo(expected);
    }

    #endregion
}
