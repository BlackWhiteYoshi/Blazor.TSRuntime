# TSRuntime

An improved JSRuntime with

- automatic JS-module loading and caching
- compile time errors instead of runtime errors
- IntelliSense guidance

![InlineComposition Example](README_IMAGE.png)


<br></br>
## Available Methods

### Invoke

Each "export function" in TypeScript will generate up to 3 C#-methods:
- **Invoke** - interops synchronous
- **InvokeTrySync** - interops synchronous if possible, otherwise asynchronous
- **InvokeAsync** - interops asynchronous

```csharp
// "function name pattern" is configured: "#function##action#"
// ts-function is saveNumber(string name, int myNumber)

TsRuntime.SaveNumberInvoke("key1", 5); // will invoke sync
await TsRuntime.SaveNumberInvokeTrySync("key1", 5); // invokes sync if possible, otherwise async
await TsRuntime.SaveNumberInvokeAsync("key1", 5); // invokes async
```

**Note**:
- *InvokeTrySync* checks if IJSInProcessRuntime is available and if available, executes the call synchronous.
So, if the module is already be downloaded and IJSInProcessRuntime is available, this method executes synchronous.
- Asynchronous JavaScript-functions (JS-functions that return a promise) should be called with *InvokeAsync* (not *Invoke* or *InvokeTrySync*), otherwise the promise will not be awaited.
- *Invoke*-interop fails with an exception when module is not loaded.
So make sure to await the corresponding preload-method beforehand.

### Preload

Each module will generate a method to preload the module.
Additionaly, there is a *PreloadAllModules* method, that preloads all modules.
Preloading will start the download of the JS-module and the task completes when the module is downloaded and cached.  
If a JS-function is called before or while preloading, the download task will first be awaited before executing the function (A sync-call throws an exception).
Therefore, it is recommended to call this method as "fire and forget".
```csharp
_ = PreloadExample(); // loads and caches Example module in the background
_ = PreloadAllModules(); // loads and caches all modules in the background
await PreloadAllModules(); // awaits the loading of all modules, recommended when using sync-interop
```

Furthermore you can prefetch your modules into JavaScript, so the Preload-methods will only get a reference to the module.
```html
<head>
  ...
  <link rel="modulepreload" href="Page/Example.razor.js" />
</head>
```


<br></br>
## Get Started
 - [Source Generator](Readme_md/GetStarted/SourceGenerator.md)
 - [Visual Studio Extension](Readme_md/GetStarted/VisualStudioExtension.md)
 - [Programmatically Usage](Readme_md/GetStarted/ProgrammaticallyUsage.md)


<br></br>
## Config - tsconfig.tsruntime.json

All available config keys with its default value:

```json
{
  "declaration path": "",
  "file output": {
    "class": "TSRuntime/TSRuntime.cs",
    "interface": "TSRuntime/ITSRuntime.cs"
  },
  "generate on save": true,
  "using statements": ["Microsoft.AspNetCore.Components"],
  "invoke function": {
    "sync enabled": false,
    "trysync enabled": true,
    "async enabled": false,
    "name pattern": {
      "pattern": "#function#",
      "module transform": "first upper case",
      "function transform": "first upper case",
      "action transform": "none",
      "action name": {
        "sync": "Invoke",
        "trysync": "InvokeTrySync",
        "async": "InvokeAsync"
      }
    },
    "promise": {
      "only async enabled": true,
      "append Async": false
    },
    "type map": {
      "number": {
        "type": "TNumber",
        "generic types": {
          "name": "TNumber",
          "constraint": "INumber<TNumber>"
        }
      },
      "boolean": "bool",
      "Uint8Array": "byte[]",
      "HTMLObjectElement": "ElementReference"
    }
  },
  "preload function": {
    "name pattern": {
      "pattern": "Preload#module#",
      "module transform": "first upper case"
    },
    "all modules name": "PreloadAllModules",
  },
  "module grouping": {
    "enabled": false,
    "service extension": true,
    "interface name pattern": {
      "pattern": "I#module#Module",
      "module transform": "first upper case"
    }
  },
  "js runtime": {
    "sync enabled": false,
    "trysync enabled": false,
    "async enabled": false
  }
}
```

- **[\[declaration path\]](Readme_md/Config/DeclarationPath.md)**:
 Folder where to locate the .d.ts declaration files. Path relative to json-file and no starting or ending slash.
- **[\[file output\].\[class\]](Readme_md/Config/FileSave.md)**:
 File-path of TSRuntime. Path relative to json-file and no starting slash. Not used in source generator.
- **[\[file output\].\[interface\]](Readme_md/Config/FileSave.md)**:
 File-path of ITSRuntime. Path relative to json-file and no starting slash. Not used in source generator.
- **[\[generate on save\]](Readme_md/Config/FileSave.md)**:
 Every time a .d.ts-file is changed, ITSRuntime is generated. Not used in source generator.
- **[\[using statements\]](Readme_md/Config/UsingStatements.md)**:
 List of generated using statements at the top of ITSRuntime.
- **[\[invoke function\].\[sync enabled\]](#invoke)**:
 Toggles whether sync invoke methods should be generated for modules.
- **[\[invoke function\].\[trysync enabled\]](#invoke)**:
 Toggles whether try-sync invoke methods should be generated for modules.
- **[\[invoke function\].\[async enabled\]](#invoke)**:
 Toggles whether async invoke methods should be generated for modules.
- **[\[invoke function\].\[name pattern\].\[pattern\]](Readme_md/Config/NamePattern.md)**:
 Naming of the generated methods that invoke module functions.
- **[\[invoke function\].\[name pattern\].\[module transform\]](Readme_md/Config/NamePattern.md)**:
 Lower/Upper case transform for the variable #module#.
- **[\[invoke function\].\[name pattern\].\[function transform\]](Readme_md/Config/NamePattern.md)**:
 Lower/Upper case transform for the variable #function#.
- **[\[invoke function\].\[name pattern\].\[action transform\]](Readme_md/Config/NamePattern.md)**:
 Lower/Upper case transform for the variable #action#.. 
- **[\[invoke function\].\[name pattern\].\[action name\]\[sync\]](Readme_md/Config/NamePattern.md)**:
 Naming of the #action# variable for the invoke module functions name pattern when the action is synchronous.
- **[\[invoke function\].\[name pattern\].\[action name\]\[trysync\]](Readme_md/Config/NamePattern.md)**:
 Naming of the #action# variable for the invoke module functions name pattern when the action is try synchronous.
- **[\[invoke function\].\[name pattern\].\[action name\]\[async\]](Readme_md/Config/NamePattern.md)**:
 Naming of the #action# variable for the invoke module functions name pattern when the action is asynchronous.
- **[\[invoke function\].\[promise\].\[only async enabled\]](Readme_md/Config/PromiseFunction.md)**:
 Generates only async invoke method when return-type is promise.
- **[\[invoke function\].\[promise\].\[append Async\]](Readme_md/Config/PromiseFunction.md)**:
 Appends to the name 'Async' when return-type is promise.
- **[\[invoke function\].\[type map\]](Readme_md/Config/TypeMap.md)**:
 Mapping of TypeScript-types (key) to C#-types (value). Not listed types are mapped unchanged (Identity function).
- **[\[preload function\].\[name pattern\].\[pattern\]](Readme_md/Config/NamePattern.md)**:
 Naming of the generated methods that preloads a specific module.
- **[\[preload function\].\[name pattern\].\[module transform\]](Readme_md/Config/NamePattern.md)**:
 Lower/Upper case transform for the variable #module#.
- **[\[preload function\].\[all modules name\]](Readme_md/Config/NamePattern.md)**:
 Naming of the method that preloads all modules.
- **[\[module grouping\].\[enabled\]](Readme_md/Config/ModuleGrouping.md)**:
 Each module gets it own interface and the functions of that module are only available in that interface.
- **[\[module grouping\].\[service extension\]](Readme_md/Config/ModuleGrouping.md)**:
 A service extension method is generated, which registers ITSRuntime and if available, the module interfaces.
- **[\[module grouping\].\[interface name pattern\].\[pattern\]](Readme_md/Config/NamePattern.md)**:
 Naming of the generated module interfaces when *module grouping* is enabled.
- **[\[module grouping\].\[interface name pattern\].\[module transform\]](Readme_md/Config/NamePattern.md)**:
 Lower/Upper case transform for the variable #module#.
- **[\[js runtime\].\[sync enabled\]](Readme_md/Config/JSRuntime.md)**:
 Toggles whether generic JSRuntime sync invoke method should be generated.
- **[\[js runtime\].\[trysync enabled\]](Readme_md/Config/JSRuntime.md)**:
 Toggles whether generic JSRuntime try-sync invoke method should be generated.
- **[\[js runtime\].\[async enabled\]](Readme_md/Config/JSRuntime.md)**:
 Toggles whether generic JSRuntime async invoke method should be generated.


<br></br>
## Preview

This package is in preview and breaking changes may occur.

There are some features planned (no guarantees whatsoever):

- TSRuntime as VS-Extension
- map callbacks <-> delegates
- improved parser (summary, .ts, .js with JSDocs)
- support for non-module files
- Generic TS-Functions


<br></br>
## Release Notes

- 0.0.1  
  First version. Includes all basic functionalities for generating TSRuntime.
- 0.1  
  Improved declaration path: Instead of one include string, an array of objects { "include": string, "excludes": string[], "file module path": string } is now supported.
- 0.2  
  Optional parameters and default parameter values are now supported.
- 0.3  
  Breaking changes: changed config keys, defaults and properties in Config, changed Config.FromJson(string json) to new Config(string json).  
  Added key "generate on save" and "action name" keys to config.
- 0.4  
  Module grouping is now supported. Small breaking change: A namespace that contains IServiceCollection is required when serviceExtension is enabled and namespace *Microsoft.Extensions.DependencyInjection* was added to the defaults.
- 0.5  
  Generics in type map is supported now.
