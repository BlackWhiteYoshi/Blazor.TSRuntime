# Get Started - Source Generator

TSRuntime included as source generator generates C#-methods on the fly:

- you write a TS-function
- saving .ts-file triggers the TS-compiler to generate files
- source generator detects file changes and generate C#-methods
- methods are available in your code with IntelliSense support

## 1. Setup TypeScript - tsconfig.json

If you want to use TSRuntime you have to use a TS-compiler.
There are many different compilers and ways to get this done, but if you are using Visual Studio, you get one out of the box.
You only need to add a tsconfig.json file.  
Create a **tsconfig.json** file in the same folder as your .csproj-file.  
Make sure you enable output for declaration-files: **"declaration": true**.

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


## 2. Add TSRuntime.SourceGenerator NuGet package

In your .csproj-file put a package reference to *TSRuntime.SourceGenerator*.

```xml
<ItemGroup>
  <PackageReference Include="TSRuntime.SourceGenerator" Version="{latest version}" PrivateAssets="all" />
</ItemGroup>
```


## 3. Add tsconfig.tsruntime.json

In your .csproj-file put an additional file directive to *tsconfig.tsruntime.json*.

```xml
<ItemGroup>
  <PackageReference Include="TSRuntime.SourceGenerator" Version="{latest version}" PrivateAssets="all" />
  <AdditionalFiles Include="tsconfig.tsruntime.json" />
</ItemGroup>
```

Create a *tsconfig.tsruntime.json*-file in the same folder as your .csproj-file.  
Your .csproj-file, tsconfig.json, tsconfig.tsruntime.json should be all in the same folder.

```json
{
  "invoke function": {
    "sync enabled": false,
    "trysync enabled": true,
    "async enabled": false,
    "name pattern": {
      "pattern": "#function#",
      "module transform": "first upper case",
      "function transform": "first upper case",
      "action transform": "none"
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
  }
}
```


## 4. Add ITSRuntime as dependency

If everything is set up correctly, the generator should already be generating the 2 files *TSRuntime*, *ITSRuntime*.  
Register them in your dependency container.

```csharp
using Microsoft.JSInterop;

services.AddScoped<ITSRuntime, TSRuntime>();
```

## 5. Use It

Now you are ready to rumble, to make a "Hello World" test you can create 2 files:

- Example.razor

```razor
<button @onclick="InvokeJS">

@code {
    [Inject]
    public required ITSRuntime TsRuntime { private get; init; }
    
    private Task InvokeJS() => TsRuntime.Example();
}
```

- Example.razor.ts

```js
export function example() {
    console.log("Hello World");
}
```


## Troubleshooting

make sure

- TypeScript is working correctly
- you are generating decalaration(.d.ts) files
- you have *&lt;PackageReference TSRuntime.SourceGenerator&gt;* in .csproj
- you have *&lt;AdditionalFiles&gt;* in .csproj
- you have a *tsconfig.tsruntime.json*-file
- you are using *Microsoft.JSInterop* namespace
- restart Visual Studio to reload the generator
