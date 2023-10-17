# Get Started - Programmatically Usage

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
Config config = new(json);

// parse the .d.ts-files
TSStructureTree structureTree = await TSStructureTree.ParseFiles(config.DeclarationPath);

// generate ITSRuntime into file
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
