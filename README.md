# TSRuntime 
A typed JSRuntime for automatic js-module management and more guidance.

# Getting Started
still in development. At the moment there is only a ConsoleApp entryPoint.

# Structure
The Solution contains a core library and 3 entryPoints (ConsoleApp, SourceGenerator, VS-extension).  
The Core itself uses a SourceGenerator ("TSRuntime.Generation") to produce the code for the function "GetITSRuntimeContent".

# TODO
 * Tests (Config, Parser, Generator)
 * Config
   - include/exclude path
   - Invoke/TrySync/Async enable/disable
   - function-naming ($function$_$module$_$action$ -> getCookies_Shared_InvokeTrySync)
   - Function (non-module) parsing
   - JsRuntime-functions enable/disable)
 * EntryPoints (Dictionary for ModuleList (key is filePath, value is index of ModuleList))
   - ConsoleApp
   - SourceGenerator
   - Extension
 * Documentation (readme.md)
 * GenericSupport (INumber<T>)
 * Function (non-module) parsing

# Contribute
See TODO
