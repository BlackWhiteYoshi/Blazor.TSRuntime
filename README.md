# TSRuntime
A typed JSRuntime for automatic js-module management and more guidance.

# Getting Started
still in development. At the moment there is a ConsoleApp entryPoint and SourceGenerator.

# Structure
The Solution contains a core library and 3 entryPoints (ConsoleApp, SourceGenerator, VS-extension).  
The Core itself uses a SourceGenerator ("TSRuntime.Generation") to produce the code for the function "GetITSRuntimeContent".  
The library "TSRuntime.FileWatcher" contains shared code from the entrypoints SourceGenerator and VS-extension.

# TODO
 * Config
   - generate on save (generate-files, source-generate)
   - include/exclude folder, include-file <- list of folder pathes (each has a list of exclude pathes), list of file pathes
   - function list (module, non-module) <- manual declaring functions directliy in the config
   - Function (non-module) parsing
   - (non-module) include/exclude folder, include-file <- list of folder pathes (each has a list of exclude pathes), list of file pathes
 * EntryPoints
   - Extension
 * Documentation (readme.md -> hero section, getting started (setup typescript -> entrypoints), explaining configs)
 * GenericSupport (INumber<T>)
 * Function (non-module) parsing
 * TypeMapDefault add types: https://learn.microsoft.com/en-us//aspnet/core/blazor/javascript-interoperability/import-export-interop?view=aspnetcore-7.0
   - Uint8Array -> byte[]
   - DotNetStreamReference -> DotNetStreamReference
 * add config option "append async at promise"
