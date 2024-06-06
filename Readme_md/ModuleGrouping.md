# Config - Module Grouping And Service Extension

## Module Grouping

When your ITSRuntime interface gets big and complex, you can split it up into multiple interfaces, each represents a module.
To enable module grouping you can use a shorthand and set the \[module grouping] key directly to true:

```json
{
  "module grouping": true

  // - the same as
  // "module grouping": {
  //   "enabled": true,
  //   "interface name pattern": {
  //     "pattern": "I#module#Module",
  //     "module transform": "first upper case"
  //   }
  // }
}
```

This will result in setting \[module grouping].[enabled] = true, while \[module grouping].[interface name pattern] will have its default value.

With [interface name pattern] you can specify the naming of your module interfaces. For how it works see [Name Pattern].


## Service Extension

When [service extension] is enabled, you can use the generated extension method to register all generated module interfaces to your service collection.
This will register a scoped ITSRuntime with a TSRuntime instance as implementation and registers the module interfaces with the same TSRuntime-instance.
If module grouping is disabled and service extension is enabled, it will only register ITSRuntime as scoped dependency with a TSRuntime instance as implementation.
