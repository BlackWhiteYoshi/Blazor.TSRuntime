# Config - Promise Function

**[only async enabled]**: Whenever a module function returns a promise, the *[invoke function].[sync enabled]*, *[invoke function].[trysync enabled]* and *[invoke function].[async enabled]* flags will be ignored
and instead, only the async invoke method will be generated.  
Asynchronous JS-functions will only be awaited with the async invoke method, so this value should always be true.
Set it only to false when you know what you are doing.

**[append Async]**: Whenever a module function returns a promise, the string "Async" is appended.  
If your pattern ends already with "Async", for example with the #action# variable, this will result in a double: "AsyncAsync".
