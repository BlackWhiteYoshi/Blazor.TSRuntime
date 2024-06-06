# Config - Name Pattern

\[name pattern\] describes the naming of the generated methods.
For example, if you provide for the key [pattern] the value "MyMethod", all generated methods will have the name "MyMethod", which will result in a compile error.
That is why there are variables provided to customize your method-naming. For the invoke methods there are 3 variables:

- #module#
- #function#
- #action#

<br></br>
Let's say we have a module named "Example" and a function "saveNumber":

- "pattern": "#function##Example##action#":  
  -> saveNumberExampleInvoke(...)  
  -> saveNumberExampleInvokeTrySync(...)  
  -> saveNumberExampleInvokeAsync(...)

- "pattern": "#action#_text#function#":  
  -> Invoke_textsaveNumber(...)  
  -> InvokeTrySync_textsaveNumber(...)  
  -> InvokeAsync_textsaveNumber(...)

<br></br>
Like in the example JS-functions are normally lower case and in C# most things are upper case.
To handle that you can apply lower/upper case transformation for each variable.  
NameTransform can be one of 5 different values:

- **"none"**: identity, changes nothing
- **"first upper case"**: first letter is uppercase
- **"first lower case"**: first letter is lowercase
- **"upper case"**: all letters are uppercase
- **"lower case"**: all letters are lowercase

With [function transform] set to "first upper case" you get:

- "pattern": "#function##Example##action#":  
  -> SaveNumberExampleInvoke(...)  
  -> SaveNumberExampleInvokeTrySync(...)  
  -> SaveNumberExampleInvokeAsync(...)

- "pattern": "#action#_text#function#":  
  -> Invoke_textSaveNumber(...)  
  -> InvokeTrySync_textSaveNumber(...)  
  -> InvokeAsync_textSaveNumber(...)

<br></br>
The \[name pattern\] for preload or module grouping works pretty much the same, except there is only 1 variable:

- #module#
