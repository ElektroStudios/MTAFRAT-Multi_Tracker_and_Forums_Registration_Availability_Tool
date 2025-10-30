# 👩‍💻 Documentación para programadores

(_Este contenido se visualiza mejor en un lector compatible con formato MarkDown._ 👀)

## 📚 Fundamentos básicos

**MTAFRAT** utiliza la API de **Selenium WebDriver** para automatizar la interacción con los sitios web. 

La estructura del archivo JSON de configuración del plugin, es la siguiente:

```json
{
  "Name":           "NOMBRE DEL PLUGIN",
  "Description":    "DESCRIPCIÓN O CATEGORIA DEL TRACKER/FORO",
  "UrlLogin":       "URL DE INICIO DE SESIÓN",
  "UrlRegister":    "URL DE REGISTRO DE CUENTA",
  "UrlApplication": "URL DE SOLICITUD DE MEMBRESÍA",
  "IconPath":       "RUTA RELATIVA DEL ARCHIVO DE IMAGEN/ICONO",
  "VbCodeFile":     "RUTA RELATIVA DEL ARCHIVO DE CÓDIGO FUENTE VB.NET"
}
```

Un plugin se implementa mediante una clase de VB.NET con herencia del tipo **MTAFRAT.DynamicPlugin** y sobrecarga de la función asíncrona `RunAsync()`, la cual devuelve un valor de tipo `Task(Of RegistrationFlags)`, como se muestra en el siguiente ejemplo simplificado:

```vbnet
Class MyPlugin : Inherits DynamicPlugin

    Overloads Async Function RunAsync() As Task(Of RegistrationFlags)
      
      ' Lógica del plugin aquí.
    End Function

End Class
```

Nótese que `RegistrationFlags` es una enumeración utilizada para indicar el estado de la operación asíncrona. Sus valores son los siguientes:

 - **RegistrationClosed**:  Indica que el formulario de registro está cerrado.
 - **RegistrationOpen**:    Indica que el formulario de registro está abierto.
 - **RegistrationUnknown**: El estado del formulario de registro es desconocido. Se puede usar como valor auxiliar cuando no se puede determinar el estado.
 - **ApplicationClosed**:  Indica que el formulario de solicitud de membresía está cerrado.
 - **ApplicationOpen**:    Indica que el formulario de solicitud de membresía está abierto.
 - **ApplicationUnknown**: El estado del formulario de solicitud de membresía es desconocido. Se puede usar como valor auxiliar cuando no se puede determinar el estado.
 - **Null**: Valor "nulo". Se puede usar como valor auxiliar cuando se produce un error o una condición inesperada.
  
Es responsabilidad del programador implementar la lógica de interacción con el sitio web, control de errores y registro de mensajes en la interfaz de usuario de **MTAFRAT**.

Puede tomar como punto de partida cualquiera de los múltiples plugins integrados que encontrará dentro de la carpeta "plugins". Algunos de estos plugins implementan soporte básico y limitado para páginas protegidas por **Cloudflare**.

## 🛠️ Soporte auxiliar

Los desarrolladores de plugins tienen a su disposición el módulo **MTAFRAT.PluginSupport**, diseñado con métodos auxiliares para facilitar tareas comunes en el desarrollo:

 - `CreateChromeDriver` As `ChromeDriver`
```vbnet
plugin As DynamicPlugin
ByRef refService As ChromeDriverService
headless As Boolean
ParamArray arguments As String()
```
Crea y devuelve una instancia  del tipo `ChromeDriver`,  preconfigurada con argumentos de seguridad y rendimiento. 

 - `NavigateTo`
```vbnet
driver As IWebDriver
url As String
```
Indica al `IWebDriver` que navegue a la URL especificada, manejando de forma segura timeouts y excepciones.
    
 - `WaitForPageReady`
```vbnet
driver As IWebDriver
Optional timeoutSeconds As Integer = 10
```
Espera hasta que la página cargue completamente (`document.readyState = "complete"`).

 - `WaitForElement`
```vbnet
driver As IWebDriver
by As By
Optional timeoutSeconds As Integer = 10
```
Espera a que un elemento coincidente con el selector `By` esté presente, visible e interactuable.
    
 - `ClickElementJs`
```vbnet
driver As IWebDriver
element As IWebElement
```
Realiza un clic sobre el elemento usando JavaScript, útil cuando el método de **Selenium** `element.Click()` falla por overlays u otros problemas.

 - `LogMessage`
```vbnet
plugin As DynamicPlugin
msg As String
```
Imprime un mensaje en el control `LogTextBox` asociado al plugin.
Ideal para mostrar mensajes de progreso, resultados o errores dentro de la propia interfaz de **MTAFRAT**.

 - `LogMessageFormat`
```vbnet
plugin As DynamicPlugin
msgFormat As String
ParamArray args As Object()
```
Funciona de manera similar a `LogMessage`, pero permite usar cadenas de formato para construir el mensaje dinámicamente, como `String.Format()`.

 - `PrintMessage`
```vbnet
plugin As DynamicPlugin
msg As String
```
Igual que la función `LogMessage`, pero imprime el mensaje tal cual, sin tiempo de marca.

 - `PrintMessageFormat`
```vbnet
plugin As DynamicPlugin
msgFormat As String
ParamArray args As Object()
```
Igual que la función `LogMessageFormat`, pero imprime el mensaje tal cual, sin tiempo de marca.

 - `NotifyMessage`
```vbnet
title As String
icon As MessageBoxIcon
msg As String
```
A diferencia de `LogMessage`, este método muestra un `MessageBox` emergente directamente al usuario. Si el Form está minimizado u oculto en el área de notificación, se muestra.
Se utiliza cuando se desea notificar un error importante o una acción que requiere atención inmediata, como por ejemplo para notificar la detección de registro abierto en un tracker.

 - `NotifyMessageFormat`
```vbnet
title As String
icon As MessageBoxIcon
msgFormat As String
ParamArray args As Object()
```
Funciona de manera similar a `NotifyMessage`, pero permite usar cadenas de formato para construir el mensaje dinámicamente, como `String.Format()`.

 - `ThrowIfStatusCode`
```vbnet
driver As IWebDriver
statusCode As Integer
afterDate As Date
```
Analiza las entradas del registro del navegador desde la fecha especificada para encontrar cualquier entrada que contenga el error de código de estado HTTP especificado. Si se encuentra, lanza una `Exception` con el mensaje correspondiente de la entrada del registro.

 - `ThrowIfAnyErrorStatusCode`
```vbnet
driver As IWebDriver
afterDate As Date
```
Analiza las entradas del registro del navegador desde la fecha especificada para encontrar cualquier entrada que contenga cualquier error de código de estado HTTP. Si se encuentra, lanza una `Exception` con el mensaje correspondiente de la entrada del registro.

También analiza el código fuente de la página actual, aplicando un tratamiento especial a las páginas protegidas por Cloudflare.

Este método ayuda a determinar si la página cargada actualmente ha devuelto un código de estado de error HTTP.

 - `DefaultRegistrationFormCheckProcedure` As `RegistrationFlags`
```vbnet
plugin As DynamicPlugin
driver As ChromeDriver
trigger As String
isOpenTrigger As Boolean
```
Una función de utilidad común utilizada por varios plugins, que encapsula los procedimientos predeterminados para navegar a una página de formulario de registro, verificar y devolver su estado actual, y manejar el registro de mensajes y las notificaciones en la interfaz de usuario.

 - `DefaultApplicationFormCheckProcedure` As `RegistrationFlags`
```vbnet
plugin As DynamicPlugin
driver As ChromeDriver
trigger As String
isOpenTrigger As Boolean
```
Una función de utilidad común utilizada por varios plugins, que encapsula los procedimientos predeterminados para navegar a una página de formulario de solicitud de membresía, verificar y devolver su estado actual, y manejar el registro de mensajes y las notificaciones en la interfaz de usuario.
