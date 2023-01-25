namespace TSRuntime.FileWatcher.Tests;

public static class TestFileContent {
    public const string CONFIG_JSON = """
        {
          "declaration path": ".typescript-declarations/",
          "file output": {
            "class": "TSRuntime.cs",
            "interface": "ITSRuntime.cs"
          },
          "module": {
            "invoke enabled": true,
            "trysync enabled": true,
            "async enabled": true
          },
          "js runtime": {
            "invoke enabled": true,
            "trysync enabled": true,
            "async enabled": true
          },
          "function name pattern": {
            "pattern": "#function#_#module#_#action#",
            "function transform": "first upper case",
            "module transform": "none",
            "action transform": "none"
          },
          "preload name pattern": {
            "pattern": "PreLoad#module#",
            "module transform": "none",
            "all modules name": "AllModules"
          },
          "using statements": [ "Microsoft.AspNetCore.Components" ],
          "type map": {
            "number": "double",
            "boolean": "bool",
            "bigint": "long",
            "HTMLObjectElement": "ElementReference"
          }
        }

        """;

    public const string TS_DECLARATION = """
        export declare function setPointerCapture(targetElement: HTMLObjectElement, pointerId: number): void;

        export declare function releasePointerCapture(targetElement: HTMLObjectElement, pointerId: number): void;

        export declare function getCookies(): string;

        export declare function setCookie(key: string, value: string, days: number): void;

        export declare function scrollIntoView(elementId: string): void;

        export declare function mathJaxRender(): void;

        """;
}
