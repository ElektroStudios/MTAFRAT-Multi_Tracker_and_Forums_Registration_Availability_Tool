# 👩‍💻 Documentation for Developers

(_This content is best viewed in a Markdown-compatible reader._ 👀)

## 📚 Basic Fundamentals

**MTAFRAT** uses the **Selenium WebDriver** API to automate interaction with websites.

The structure of the plugin configuration JSON file is as follows:

```json
{
  "Name":        "PLUGIN NAME",
  "Description": "DESCRIPTION OR CATEGORY",
  "Url":         "LOGIN OR SIGNUP URL",
  "IconPath":    "RELATIVE PATH TO IMAGE FILE",
  "VbCodeFile":  "RELATIVE PATH TO VB.NET SOURCE CODE FILE"
}
```

A plugin is implemented through a VB.NET class inheriting from **MTAFRAT.DynamicPlugin** and overloading the asynchronous function `RunAsync()`, which returns a value of type `Task(Of RegistrationStatus)`, as shown in the following simplified example:

```vbnet
Class MyPlugin : Inherits DynamicPlugin

    Overloads Async Function RunAsync() As Task(Of RegistrationStatus)
      
      ' Plugin's logic here.
    End Function

End Class
```

Note that `RegistrationStatus` is an enumeration used to indicate the status of the asynchronous operation. Its values are as follows:

 - **Open**:    Indicates that user registration on the website is open to the public.  
 - **Closed**:  Indicates that user registration on the website is closed to the public.  
 - **Unknown**: Registration status unknown. Can be used as an auxiliary value when the check cannot be determined.

It is the developer's responsibility to implement the logic for interacting with the website, error handling, and logging messages in the **MTAFRAT** user interface.

You can use any of the multiple built-in plugins found in the "plugins" folder as a starting point. Some of these plugins provide basic and limited support for pages protected by **Cloudflare**.

## 🛠️ Auxiliary Support

Plugin developers have at their disposal the **MTAFRAT.PluginSupport** module, designed with helper methods to simplify common tasks in development:

 - `CreateChromeDriver`
```vbnet
plugin As DynamicPlugin
ByRef refService As ChromeDriverService
headless As Boolean
ParamArray arguments As String()
```
Create an instance of type `ChromeDriver`, preconfigured with security and performance arguments. 

 - `NavigateTo`
```vbnet
driver As IWebDriver
url As String
```
Direct the `IWebDriver` to navigate to the specified URL, safely handling timeouts and exceptions.

 - `WaitForPageReady`
```vbnet
driver As IWebDriver
Optional timeoutSeconds As Integer = 10
```
Wait until the page fully loads (`document.readyState = "complete"`).  

 - `WaitForElement`
```vbnet
driver As IWebDriver
by As By
Optional timeoutSeconds As Integer = 10
```
Wait for an element matching the `By` selector to be present, visible, and interactable.  

 - `ClickElementJs`
```vbnet
driver As IWebDriver
element As IWebElement
```
Perform a click on the element using JavaScript, useful when the **Selenium** method `element.Click()` fails due to overlays or other issues.

 - `LogMessage`
```vbnet
plugin As DynamicPlugin
msg As String
```
Print a message in the `LogTextBox` control associated with the plugin.  
Ideal for displaying progress messages, results, or errors within the **MTAFRAT** interface.  

 - `LogMessageFormat`
```vbnet
plugin As DynamicPlugin
msgFormat As String
ParamArray args As Object()
```
Works similarly to `LogMessage`, but allows using formatted strings to dynamically construct the message, like `String.Format()`.

 - `NotifyMessage`
```vbnet
title As String
icon As MessageBoxIcon
msg As String
```
Unlike `LogMessage`, this method shows a `MessageBox` directly to the user. If the application form is minimized or hidden in the notification area, it will be shown.
It is used when you want to notify an important error or an action that requires immediate attention, for example, to notify the detection of open registration on a tracker.  

 - `NotifyMessageFormat`
```vbnet
title As String
icon As MessageBoxIcon
msgFormat As String
ParamArray args As Object()
```
Works similarly to `NotifyMessage`, but allows using formatted strings to dynamically construct the message, like `String.Format()`.

 - `ThrowIfStatusCode`
```vbnet
driver As IWebDriver
statusCode As Integer
afterDate As Date
```
Analyzes the browser log entries since the specified date to find any entry containing the specified HTTP status code error, throwing an `Exception` with the corresponding log entry message if found.

 - `ThrowIfAnyErrorStatusCode`
```vbnet
driver As IWebDriver
afterDate As Date
```
Analyzes the browser log entries since the specified date to find any entry containing any HTTP status code error, throwing an `Exception` with the corresponding log entry message if found. 

It also analyzes the current page source, applying special handling for Cloudflare-protected pages.

This method helps determine whether the currently loaded page returned an HTTP error status code.