# TSRuntime

An improved JSRuntime with

- automatic JS-module loading and caching
- compile time errors instead of runtime errors
- IntelliSense guidance

![InlineComposition Example](README_IMAGE.png)


<br></br>
## Available Methods

### Invoke

Each "export function" in typescript will generate up to 3 C#-methods:
- **Invoke** - interops synchronous
- **InvokeTrySync** - interops synchronous if possible, otherwise asynchronous
- **InvokeAsync** - interops asynchronous

```csharp
// "function name pattern" is configured: "#function##action#"
// ts-function is saveNumber(string name, int myNumber)

TsRuntime.SaveNumberInvoke("save1", 5); // will invoke sync
await TsRuntime.SaveNumberInvokeTrySync("save1", 5); // invokes sync if possible, otherwise async
await TsRuntime.SaveNumberInvokeAsync("save1", 5); // invokes async
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
```


<br></br>
## Get Started - Source Generator

TSRuntime included as source generator generates C#-methods on the fly:

- you write a TS-function
- saving .ts-file triggers the TS-compiler to generate files
- source generator detects file changes and generate C#-methods
- methods are available in your code with IntelliSense support

### 1. Setup TypeScript

If you want to use TSRuntime you have to use a TS-compiler.
There are many ways to set this up, but when you are using Visual Studio, you only have to do 2 steps.

  * **Microsoft.TypeScript.MSBuild** NuGet package -> add the latest version to your project.

```xml
<PackageReference Include="Microsoft.TypeScript.MSBuild" Version="x.x.x">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

  * **tsconfig.json** -> create the config file in the same folder as your .csproj-file.  
    Make sure you enable output for daclaration-files: **"declaration": true**.

```json
{
  "compileOnSave": true,
  "compilerOptions": {
    "noImplicitAny": true,
    "strictNullChecks": true,
    "noEmitOnError": true,
    "removeComments": false,
    "sourceMap": false,
    "declaration": true,
    "target": "es6",
    "lib": [
      "es6",
      "DOM"
    ]
  },
  "exclude": [
    "bin",
    "obj",
    "Properties",
    "**/*.js",
    "**/*.jsx"
  ]
}
```


### 2. Add TSRuntime.SourceGenerator NuGet package

In your .csproj-file put a package reference to *TSRuntime.SourceGenerator*.

```xml
<ItemGroup>
  <PackageReference Include="TSRuntime.SourceGenerator" Version="x.x.x" />
</ItemGroup>
```


### 3. Add tsconfig.tsruntime.json

In your .csproj-file put an additional file directive to *tsconfig.tsruntime.json*.

```xml
<ItemGroup>
  <PackageReference Include="TSRuntime.SourceGenerator" Version="x.x.x" />
  <AdditionalFiles Include="tsconfig.tsruntime.json" />
</ItemGroup>
```

Create a *tsconfig.tsruntime.json*-file in the same folder as your .csproj-file.  
Your .csproj-file, tsconfig.json, tsconfig.tsruntime.json should be all in the same folder.

```json
{
  "module": {
    "invoke enabled": false,
    "trysync enabled": true,
    "async enabled": false
  },
  "function name pattern": {
    "pattern": "#function#",
    "function transform": "first upper case",
    "module transform": "none",
    "action transform": "none"
  }
}
```


### 4. Add ITSRuntime as dependency

If everything is set up correctly, the generator should already be generating the 2 files *TSRuntime*, *ITSRuntime*.  
Register them in your dependency container.

```csharp
using Microsoft.JSInterop;

services.AddScoped<ITSRuntime, TSRuntime>();
```


### Troubleshooting

make sure

- typescript is working correctly
- you are generating decalaration(.d.ts) files
- you have *&lt;PackageReference&gt;* in .csproj
- you have *&lt;AdditionalFiles&gt;* in .csproj
- you have a *tsconfig.tsruntime.json*-file
- you are using *Microsoft.JSInterop* namespace
- restart Visual Studio to reload the generator


<br></br>
## Get Started - Visual Studio Extension

not available yet


<br></br>
## Get Started - Programmatically Usage

You can also generate the class and interface by using *TSRuntime.Core* directly.

```xml
<ItemGroup>
  <PackageReference Include="TSRuntime.Core" Version="x.x.x" />
</ItemGroup>
```

The content of TSRuntime is constant, so to "generate" the class you only need to read the string *Generator.TSRuntimeContent*.

To generate the interface you need to do 3 steps:

- create a config-object
- parse the .d.ts-files
- generate ITSRuntime

```csharp
using TSRuntime.Core.Parsing;
using TSRuntime.Core.Configs;
using TSRuntime.Core.Generation;

// create a config-object
string json = File.ReadAllText("tsconfig.tsruntime.json");
Config config = Config.FromJson(json);

// parse the .d.ts-files
TSStructureTree structureTree = new();
await structureTree.ParseModules(config.DeclarationPath);

// generate ITSRuntime
using FileStream fileStream = new(config.FileOutputinterface, FileMode.Create, FileAccess.Write);
using StreamWriter streamWriter = new(fileStream);
foreach (string fragment in Generator.GetITSRuntimeContent(structureTree, config))
    await streamWriter.WriteAsync(fragment);

// save TSRuntime to file
await File.WriteAllTextAsync(config.FileOutputClass, Generator.TSRuntimeContent);
```

**Note**: *Generator.GetITSRuntimeContent* is an IEnumerable&lt;string&gt;, each enumeration executes the next part of the generation.
So, enumerating it to the end will give you the whole content of ITSRuntime.
The benefit of this design is that you can decide where to store the generation without the overhead of having a character-buffer.  
In the example it is directly written to disk, but you can also use a StringBuilder to save the generation to memory.


<br></br>
## Configure declaration-files (.d.ts) directory.

The generated declaration files do not hide behind your .razor files and make your folders dirty/clunky.
To solve that problem you can configure a declaration directory in tsconfig.json and tsconfig.tsruntime.json.  
If the name of your folder starts with a '.', the folder does not even show up in Visual Studio.

```json
// tsconfig.json
{
  "compilerOptions": {
    "declaration": true,
    "declarationDir": ".typescript-declarations",
    ...
  },
  ...
}
```

```json
// tsconfig.tsruntime.json
{
  "declaration path": ".typescript-declarations",
  ...
}
```

**Note**:  
This **does not work** if you only have 1 module.
You need to have **at least 2 modules in different folders** before this method is working properly.  
The problem is, that the TS-compiler does not preserve the folder structure as long as there is only 1 folder path involved.
If there is only 1 path, the declaration files are just created at the root level, so the generator will determine the wrong module path (except your module is in the wwwroot folder).
As soon as you have 2 modules in different folders, the TS-compiler creates the corresponding folder structure and put the declaration files properly, so the generator can work properly.

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
  "module": {
    "invoke enabled": false,
    "trysync enabled": true,
    "async enabled": false
  },
  "js runtime": {
    "invoke enabled": false,
    "trysync enabled": false,
    "async enabled": false
  },
  "promise function": {
    "only async enabled": true,
    "append Async": false
  },
  "function name pattern": {
    "pattern": "#function#",
    "function transform": "first upper case",
    "module transform": "none",
    "action transform": "none"
  },
  "preload name pattern": {
    "pattern": "Preload#module#",
    "module transform": "none"
  },
  "preload all modules name": "PreloadAllModules",
  "using statements": ["Microsoft.AspNetCore.Components"],
  "type map": {
    "number": "double",
    "boolean": "bool",
    "bigint": "long",
    "HTMLObjectElement": "ElementReference"
  }
}
```

- **[declaration path]**: Folder where to locate the d.ts declaration files. Path relative to this file and no starting or ending slash.
- **[file output].[class]**: File-path of TSRuntime. Path relative to json-file and no starting slash. Not used in source generator.
- **[file output].[interface]**: File-path of ITSRuntime. Path relative to json-file and no starting slash. Not used in source generator.
- **[module].[invoke enabled]**: Toggles whether sync invoke methods should be generated for modules.
- **[module].[trysync enabled]**: Toggles whether try-sync invoke methods should be generated for modules.
- **[module].[async enabled]**: Toggles whether async invoke methods should be generated for modules.
- **[js runtime].[invoke enabled]**: Toggles whether generic JSRuntime sync invoke method should be generated.
- **[js runtime].[trysync enabled]**: Toggles whether generic JSRuntime try-sync invoke method should be generated.
- **[js runtime].[async enabled]**: Toggles whether generic JSRuntime async invoke method should be generated.
- **[promise function].[only async enabled]**: If true, whenever a module function returns a promise, the *[module].[invoke enabled]*, *[module].[trysync enabled]* and *[module].[async enabled]* flags will be ignored and instead only the async invoke method will be generated.  
This value should always be true. Set it only to false when you know what you are doing.
- **[promise function].[append Async]**: If true, whenever a module function returns a promise, the string "Async" is appended.  
If your pattern ends already with "Async", for example with the #action# variable, this will result in a double: "AsyncAsync"
- **[function name pattern].[pattern]**: Naming of the generated methods that invoke module functions.
- **[function name pattern].[function transform]**: Lower/Upper case transform for the variable #function#. See Notes below.
- **[function name pattern].[module transform]**: Lower/Upper case transform for the variable #module#. See Notes below.
- **[function name pattern].[action transform]**: Lower/Upper case transform for the variable #action#.. See Notes below.
- **[preload name pattern].[pattern]**: Naming of the generated methods that preloads a specific module.
- **[preload name pattern].[module transform]**: Lower/Upper case transform for the variable #module#.. See Notes below.
- **[preload all modules name]**: Naming of the method that preloads all modules.
- **[using statements]**: List of generated using statements at the top of ITSRuntime.
- **[type map]**: Mapping of typescript-types (key) to C#-types (value). Not listed types are mapped unchanged (Identity function).

<br></br>
**Note: *Name Pattern***

[function name pattern] describes the naming of the generated invoke methods.
For example, if you provide for the key [pattern] the value "MyMethod", all generated methods will have the name "MyMethod", which will result in a compile error.
Therefore, there are 3 variables provided to customize your method-naming:

- #function#
- #module#
- #action#

Let's say we have a module named "Example" and a function "saveNumber":

- "pattern": "#function##Example##action#":  
  -> saveNumberExampleInvoke(...)  
  -> saveNumberExampleInvokeTrySync(...)  
  -> saveNumberExampleInvokeAsync(...)

- "pattern": "#action#_text#function#":  
  -> Invoke_textsaveNumber(...)  
  -> InvokeTrySync_textsaveNumber(...)  
  -> InvokeAsync_textsaveNumber(...)

Like in the example JS-functions are normally lower case and in C# most things are upper case.
To handle that you can apply lower/upper case transformation for each variable.  
NameTransform can be one of 5 different values:

- **"none"**: identity, changes nothing
- **"first upper case"**: first letter is uppercase
- **"first lower case"**: first letter is lowercase
- **"upper case"**: all letters are uppercase
- **"lower case"**: all letters are lowercase

With [function transform] set to "first upper case" you get:

- "pattern": "#function##Example##action#":  
  -> SaveNumberExampleInvoke(...)  
  -> SaveNumberExampleInvokeTrySync(...)  
  -> SaveNumberExampleInvokeAsync(...)

- "pattern": "#action#_text#function#":  
  -> Invoke_textSaveNumber(...)  
  -> InvokeTrySync_textSaveNumber(...)  
  -> InvokeAsync_textSaveNumber(...)

[preload name pattern] works pretty much the same, except there is only 1 variable:

- #module#

<br></br>
**Note: *Type Map***

Variants of nullable or optional are not concidered as different types.
If a TS-variable is nullable, it is also nullable in C#.
If a TS-variable is optional/undefined, it is also optional in C# by creating overload methods, but only if the last parameters are optional/undefined.

Assuming *bigint* is mapped to *long*

- (myParameter: bigint)                    -> (long myParameter)
- (myParameter: bigint | null)             -> (long? myParameter)
- (myParameter?: bigint)                   -> (), (long myParameter)
- (myParameter: bigint | undefined)        -> (), (long myParameter)
- (myParameter?: bigint | null)            -> (), (long? myParameter)
- (myParameter: bigint | null | undefined) -> (), (long? myParameter)

<br></br>
**Note**: The following using statements are always included

- using Microsoft.JSInterop.Infrastructure;
- using System.Diagnostics.CodeAnalysis;
- using System.Threading;
- using System.Threading.Tasks;

There is currently no way to change this default.
*[using statements]* will add additional using statements.


<br></br>
## Preview

This package is in preview and breaking changes may occur.

There are some features planned (no guarantees whatsoever):

* optional parameters -> function-overload (at the moment optional is ignored and undefined is mapped the same as null)
* more options in config
  - generate on save (not used in source generator)
  - include/exclude folder
* TSRuntime as VS-Extension
* GenericSupport (INumber&lt;T&gt; instead of double)
* Support for non-module JS-files
* TypeMapDefault more default types (e.g. Uint8Array -> byte[], DotNetStreamReference -> DotNetStreamReference)
* map callbacks <-> delegates
* option for grouping methods of a module in structs
* [JSImport]/[JSExport] interop
* autogenerate types to map to -> recursive figure out the structure (requires complex parser)
