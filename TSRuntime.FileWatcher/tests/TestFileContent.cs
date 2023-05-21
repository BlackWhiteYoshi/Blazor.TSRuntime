namespace TSRuntime.FileWatcher.Tests;

public static class TestFileContent {
    public const string CONFIG_JSON = """
        {
          "invoke function": {
            "sync enabled": false,
            "trysync enabled": true,
            "async enabled": false,
            "name pattern": {
              "pattern": "#module#",
              "module transform": "first upper case",
              "function transform": "first upper case",
              "action transform": "none"
            }
          }
        }

        """;

    public const string TS_DECLARATION = """
        export declare function setPointerCapture(targetElement: HTMLObjectElement, pointerId: number): void;

        export declare function releasePointerCapture(targetElement: HTMLObjectElement, pointerId: number): void;

        export declare function getCookies(): string;

        export declare function setCookie(key: string, value: string, days: number): void;

        export declare function scrollIntoView(elementId: string): void;

        """;
}
