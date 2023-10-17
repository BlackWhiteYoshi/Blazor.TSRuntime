# Config - Type Map

This map defines all types that are convertible between the languages. Types with the same name does not need to be listed.
Keep in mind that the JSRuntime conversion logic must support the mapping, otherwise you will end up with the wrong type.

<br></br>
To define a convertible pair, set the TS-type as key and the C#-type as value:

```json
"type map": {
  "number": "double" 
}
```

Generic types are not detected automatically and must be specified explicitly.
The above example is actually a shorthand:

```json
"type map": {
  "number": {
    "type": "double",
    "generic types": []
  }
}
```

So, if your type depends on one or more generic types, specify it in "generic types":

```json
"type map": {
  "number": {
    "type": "INumber<T>",
    "generic types": "T"
  }
}
```

This is once again a shorthand for:

```json
"type map": {
  "MyList": {
    "type": "INumber<TSelf>",
    "generic types": [
      {
        "name": "TSelf",
        "constraint": null
      }
    ]
  }
}
```

The above example will work on types like List&lt;T&gt;, but not INumber&lt;TSelf&gt;.
To use INumber&lt;TSelf&gt; properly, we need to use type constraint: 

```json
"type map": {
  "number": {
    "type": "TNumber",
    "generic types": {
      "name": "TNumber",
      "constraint": "INumber<TNumber>"
    }
  }
}
```

If you want to add multiple constraints on a type, just separate them with ','.  
Here a final complete example:

```json
"type map": {
  "JSType": {
    "type": "CSharpType<TType1, TType2>",
    "generic types": [
      {
        "name": "TType1",
        "constraint": "constraint1, constraint2, ..."
      },
      {
        "name": "TType2",
        "constraint": "IDisposable, new()"
      }
    ]
  }
}
```

**Note**: generic-type naming conflicts are not detected nor handled, so make sure your generic types are named uniquely.


<br></br>
## Nullable/Optional

Variants of nullable or optional are not concidered as different types.
If a TS-variable is nullable, it is also nullable in C#.
If a TS-variable is optional/undefined, it is also optional in C# by creating overload methods, but only if the last parameters are optional/undefined.
If an array item is undefined, it is treated like nullable.

Here are some examples:

| TypeScript                                               | C#                               |
| -------------------------------------------------------- | -------------------------------- |
| do(myParameter: string)                                  | Do(string myParameter)           |
| do(myParameter: string \| null)                          | Do(string? myParameter)          |
| do(myParameter?: string)                                 | Do(), Do(string myParameter)     |
| do(myParameter: string \| undefined)                     | Do(), Do(string myParameter)     |
| do(myParameter?: string \| undefined)                    | Do(), Do(string myParameter)     |
| do(myParameter?: string \| null)                         | Do(), Do(string? myParameter)    |
| do(myParameter: string \| null \| undefined)             | Do(), Do(string? myParameter)    |
| do(myParameter: (string \| null)[])                      | Do(string?[] myParameter)        |
| do(myParameter: (string \| undefined)[])                 | Do(string?[] myParameter)        |
| do(myParameter: (string \| null \| undefined)[])         | Do(string?[] myParameter)        |
| do(myParameter: (string \| null)[] \| null)              | Do(), Do(string?[]? myParameter) |
| do(myParameter: (string \| null)[] \| undefined)         | Do(), Do(string?[] myParameter)  |
| do(myParameter: (string \| null)[] \| null \| undefined) | Do(), Do(string?[]? myParameter) |

**Note**: default value parameters (e.g. do(myParameter = 5)) are automatically mapped to optional parameters in .d.ts-files, so they will work as expected.
