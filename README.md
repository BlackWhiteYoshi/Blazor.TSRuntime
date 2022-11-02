# TSRuntime
A typed JSRuntime for automatic js-module management and more guidance.

# Getting Started
still in development. At the moment there is only a ConsoleApp entryPoint.

# Structure
The Solution contains a core library and 3 entryPoints (ConsoleApp, SourceGenerator, VS-extension).  
The Core itself uses a SourceGenerator ("TSRuntime.Generation") to produce the code for the function "GetITSRuntimeContent".  
The library "TSRuntime.FileWatcher" contains shared code from the entrypoints SourceGenerator and VS-extension.

# TODO
 * Config
   - generate on save
   - include/exclude folder, include-file <- list of folder pathes (each has a list of exclude pathes), list of file pathes
   - function list (module, non-module) <- manual declaring functions directliy in the config
   - Function (non-module) parsing
   - (non-module) include/exclude folder, include-file <- list of folder pathes (each has a list of exclude pathes), list of file pathes
 * EntryPoints
   - SourceGenerator
   - Extension
 * Documentation (readme.md -> hero section, getting started (setup typescript -> entrypoints), explaining configs)
 * GenericSupport (INumber<T>)
 * Function (non-module) parsing
