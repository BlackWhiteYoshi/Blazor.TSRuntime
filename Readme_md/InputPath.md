# Config - Webroot Path and Input Path

## Webroot Path

The relative path to the web root from where the path gets resolved.  
Normally the relative path is your project root directory, then .js-files attached at razor components get resolved correctly
and .js files in the wwwroot-folder have also the right path, because the starting 'wwwroot' folder is ignored.  
When you place your tsruntime.json file in the project root directory, then you can just let this value alone.
But if you want to move this file somewhere else, e.g. in a 'config' folder, you can set *web root path* to "..",
so the root path still starts at your project root directory.


<br></br>
## Config - Input Path

You can set "input path" just to a string

```json
{
  "input path": "/jsFolder"
}
```

This will include all files inside "/jsFolder".

But if you want you can also be more accurate. The previous example is just a shorthand for:

```json
{
  "input path": [
    {
      "include": "/jsFolder",
      "excludes": [],
      "module files": true,
      "module path": null
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
  "input path": [
    {
      "include": "/jsFolder",
      "excludes": [
        "/jsFolder/private",
        "/jsFolder/wwwroot/service-worker.js"
      ]
    },
    "/otherInputFolder"
  ]
}
```

The preceding configuration has two include paths "/jsFolder" and "/otherInputFolder"
and inside "/jsFolder" the folder "private" and inside "wwwroot" the file "service-worker.js" will not be included.


### Module Files

A flag that can be set to false to read in a folder/file where global scripts are located
(files that are placed in html with the &lt;script&gt; tag).

**Example**:
```html
<head>
  <script src="/js/site.js" defer></script>
</head>
```

```json
{
  "input path": {
    "include": "/wwwroot/js",
    "module files": false
  }
}
```

In the preceding example all files located in the *wwwroot/js* folder are included as global scripts.

Note:  
To recognize a function in a global script as callable function, the line must start with "function", other types of declarations are ignored.  
If you have multiple *input path* and they intersect, the first one in the list has priority.
So put the specific rules at the top and the general rules at the bottom. Or make sure to exclude sections that intersect.


### Module Path

If your include path is a file, the module path will be the same as your include path.
If that path does not fit, you can set it explicit with [module path].

**Example**:
```json
{
  "input path": {
    "include": "/scripts/declarations/shared.js",
    "module path": "/scripts/shared.js"
  }
}
```

The preceding configuration only reads in one module: "shared.js".
If [module path] would be not set, the module path would be "/scripts/declarations/shared.js".
Because the actual script is served on the URL "/scripts/shared.js", it has to be set explicitly.  
If a value is provided for [module path], it should end with ".js", regardless of the include type (*.js*, *.ts*, *.d.ts*).

Setting explicit module path for folders is not supported and will result in errors (duplicate hintNames).
