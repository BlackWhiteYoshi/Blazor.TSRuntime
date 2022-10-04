# TSRuntime
A typed JSRuntime for automatic js-module management and more guidance.

# Getting Started
still in development. At the moment there is only a ConsoleApp entryPoint.

# Structure
The Solution contains a core library and 3 entryPoints (ConsoleApp, SourceGenerator, VS-extension).  
The Core itself uses a SourceGenerator ("TSRuntime.Generation") to produce the code for the function "GetITSRuntimeContent".

# TODO
 * Config
   - function-naming (PascalCase, CamelCase)
   - include/exclude folder, include-file
   - function list (module, non-module)
   - Function (non-module) parsing
   - include/exclude folder, include-file for non-module
 * EntryPoints (Dictionary for ModuleList (key is filePath, value is index of ModuleList))
   - ConsoleApp
   - SourceGenerator
   - Extension
 * Config json-file (tsconfig.tsruntime.json)
 * Documentation (readme.md -> hero section, getting started (setup typescript -> entrypoints), explaining configs)
 * GenericSupport (INumber<T>)
 * Function (non-module) parsing

# Contribute
See TODO
