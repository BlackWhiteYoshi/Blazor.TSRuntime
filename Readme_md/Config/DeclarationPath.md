# Config - Declaration Path

## Configure declaration-files (.d.ts) directory

The generated declaration files do not hide behind your .razor files and can make your folders look dirty/clunky.
To solve that problem you can configure a declaration directory in tsconfig.json and tsconfig.tsruntime.json.  
If the name of your folder starts with a ".", the folder does not even show up in Visual Studio.

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


<br></br>
## [declaration path] in depth
The value for [declaration path] is actually much more than a string. Setting it to the string ".typescript-declarations" is just a shorthand for:

```json
{
  "declaration path": [
    {
      "include": ".typescript-declarations",
      "excludes": [],
      "file module path": null
    }
  ]
}
```

So, you can have a list of include paths. Each path can be a folder or a file.  
Each include path can have a list of exclude paths, each can be a folder or a file.
An exclude path must start with the same as include path in order to match.

**Example**:  
```json
{
  "declaration path": [
    {
      "include": ".typescript-declarations",
      "excludes": [
        ".typescript-declarations/private",
        ".typescript-declarations/wwwroot/service-worker.d.ts"
      ]
    },
    "otherDeclarationFolder"
  ]
}
```

The preceding configuration has two include paths ".typescript-declarations" and "otherDeclarationFolder" and inside ".typescript-declarations" the folder "private" and inside "wwwroot" the file "service-worker.d.ts" will not be included.

<br></br>
If your include path is a file, the module path will be the same as your include path.
If that path does not fit, you can set it explicit with [file module path].

**Example**:  
```json
{
  "declaration path": {
    "include": "scripts/declarations/shared.d.ts",
    "file module path": "/scripts/shared.js"
  }
}
```

The preceding configuration only reads in one module: "shared.d.ts".
If [file module path] would be not set, the module path would be "/scripts/declarations/shared.js".
Because the actual script is served on the URL "/scripts/shared.js", it has to be set explicitly.

Setting explicit module path for folders is not supported.


<br></br>
## Troubleshooting

Setting in *tsconfig.json* [declarationDir] does **not work** if you only have 1 module.
You need to have **at least 2 modules in different folders** before this method is working properly.  
The problem is, that the TS-compiler does not preserve the folder structure as long as there is only 1 folder path involved.
If there is only 1 path, the declaration files are just created at the root level, so the generator will determine the wrong module path (except your module is in the wwwroot folder).
As soon as you have 2 modules in different folders, the TS-compiler creates the corresponding folder structure and put the declaration files properly, so the generator can work properly.

If you get an error like "*Type 'ITSRuntime' already defines a member called '...' with the same parameter types*", you have most likely duplicate files in your declaration folder.
Check your declaration folder and remove any exceeding files.
