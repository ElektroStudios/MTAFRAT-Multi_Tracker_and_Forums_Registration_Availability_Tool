#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Resources

Imports OpenQA.Selenium
Imports OpenQA.Selenium.Chrome
Imports OpenQA.Selenium.Support.UI

Imports LogEntry = OpenQA.Selenium.LogEntry ' Not OpenQA.Selenium.DevTools.LogEntry

#End Region

''' <summary>
''' Provides utility methods to support plugin operations.
''' </summary>
Public Module PluginSupport

#Region " Public Methods "

    ''' <summary>
    ''' Creates a new preconfigured <see cref="ChromeDriver"/> instance with optional headless mode.
    ''' </summary>
    ''' 
    ''' <param name="plugin">
    ''' The source <see cref="DynamicPlugin"/>.
    ''' </param>
    ''' 
    ''' <param name="headless">
    ''' If <see langword="True"/>, launches Chrome in headless mode.
    ''' </param>
    ''' 
    ''' <param name="arguments">
    ''' Optional. Additional arguments to add to the underlying <see cref="ChromeOptions"/> object.
    ''' </param>
    ''' 
    ''' <returns>
    ''' The resulting <see cref="ChromeDriver"/> instance.
    ''' </returns>
    <DebuggerStepThrough>
    Public Function CreateChromeDriver(plugin As DynamicPlugin, headless As Boolean, ParamArray arguments As String()) As ChromeDriver

        Dim options As New ChromeOptions() With {
            .AcceptInsecureCertificates = True,
            .EnableDownloads = False,
            .LeaveBrowserRunning = False,
            .UnhandledPromptBehavior = UnhandledPromptBehavior.Ignore,
            .UseStrictFileInteractability = True
        }

        options.AddAdditionalOption("useAutomationExtension", False)
        options.AddExcludedArgument("enable-automation")

        Dim defaultArgs As String() = {
            "--allow-insecure-localhost",
            "--disable-background-networking",
            "--disable-backgrounding-occluded-windows",
            "--disable-blink-features=AutomationControlled",
            "--disable-default-apps",
            "--disable-dev-shm-usage",
            "--disable-extensions",
            "--disable-features=Translate,TranslateUI",
            "--disable-gpu",
            "--disable-hang-monitor",
            "--disable-notifications",
            "--disable-popup-blocking",
            "--disable-prompt-on-repost",
            "--disable-sync",
            "--ignore-certificate-errors",
            "--ignore-ssl-errors",
            "--lang=en",
            "--no-first-run",
            "--no-sandbox",
            "--noerrdialogs",
            "--remote-debugging-pipe",
            "--test-type",
            $"--user-data-dir={plugin.PluginCachePath}",
            $"--profile-directory=_Profile.{plugin.UIMemberBaseName}"
        }

        Dim headlessArgs As String() = {
                "--headless=new",
                "--start-maximized",
                "--disable-site-isolation-trials",
                "--disable-web-security"
            }

        Dim nonHeadlessArgs As String() = {
                "--window-position=-32000,0",
                $"--window-size={Screen.PrimaryScreen.WorkingArea.Width},{Screen.PrimaryScreen.WorkingArea.Height}"
            }

        If headless Then
            options.AddArguments(headlessArgs)
        Else
            options.AddArguments(nonHeadlessArgs)
        End If

        options.AddArguments(defaultArgs)

        options.AddArguments(arguments)

        Dim service As ChromeDriverService = ChromeDriverService.CreateDefaultService()
        With service
            .DisableBuildCheck = False
            .EnableAppendLog = False
            .EnableVerboseLogging = False
            .HideCommandPromptWindow = True
            .LogLevel = Chromium.ChromiumDriverLogLevel.Info
            .LogPath = $"{AppGlobals.ChromeUserCachePath}\{plugin.UIMemberBaseName}\Session.log"
            .ReadableTimestamp = True
            .SuppressInitialDiagnosticInformation = False ' Note: If True, it hangs ChromeDriver initialization.
        End With
        ' Selenium driver sometimes causes an error if log file does not exists.
        If Not File.Exists(service.LogPath) Then
            Directory.CreateDirectory(Path.GetDirectoryName(service.LogPath))
            File.Create(service.LogPath, FileOptions.None).Dispose()
        End If

        Dim driver As New ChromeDriver(service, options)
        Return driver
    End Function

    ''' <summary>
    ''' Instructs the specified <see cref="IWebDriver"/> to navigate to the given URL.
    ''' </summary>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="IWebDriver"/> instance.
    ''' </param>
    ''' 
    ''' <param name="url">
    ''' The URL to navigate to.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub NavigateTo(driver As IWebDriver, url As String)

        Dim dateBeforeNav As Date = Date.UtcNow
        Dim previousTimeout As TimeSpan = driver.Manage().Timeouts().PageLoad
        Try
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30)
            driver.Navigate().GoToUrl(url)

        Catch ex As WebDriverTimeoutException
            Throw New Exception(String.Format(My.Resources.Strings.CantNavigateToUrlFormat, ex.Message))

        Finally
            driver.Manage().Timeouts().PageLoad = previousTimeout
            PluginSupport.ThrowIfAnyErrorStatusCode(driver, dateBeforeNav)

        End Try
    End Sub

    ''' <summary>
    ''' Waits until the web page loaded in the specified <see cref="IWebDriver"/> instance 
    ''' reports a ready state of <c>"complete"</c>.
    ''' </summary>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="IWebDriver"/> instance.
    ''' </param>
    ''' 
    ''' <param name="timeoutSeconds">
    ''' Optional. The maximum number of seconds to wait. Default is 30 seconds.
    ''' <para></para>
    ''' If the condition is not met within this time, a <see cref="WebDriverTimeoutException"/> is thrown.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub WaitForPageReady(drv As IWebDriver, Optional timeout As TimeSpan = Nothing)

        If timeout = Nothing Then
            timeout = New TimeSpan(hours:=0, minutes:=0, seconds:=30)
        End If
        Dim drvWait As New WebDriverWait(drv, timeout) With {
                .PollingInterval = TimeSpan.FromMilliseconds(250)
            }

        Dim js As IJavaScriptExecutor = TryCast(drv, IJavaScriptExecutor)
        If js Is Nothing Then
            Throw New ArgumentException("IWebDriver must support javascript execution", NameOf(drv))
        End If

        drvWait.Until(
                Function(x As IWebDriver)
                    Try
                        Dim readyState As String = js.ExecuteScript("if (document.readyState) return document.readyState;").ToString()
                        Return readyState.Equals("complete", StringComparison.InvariantCultureIgnoreCase)

                    Catch ex As InvalidOperationException
                        ' Window is no longer available
                        Return ex.Message.Contains("unable to get browser", StringComparison.InvariantCultureIgnoreCase)

                    Catch ex As WebDriverException
                        ' Browser is no longer available
                        Return ex.Message.Contains("unable to connect", StringComparison.InvariantCultureIgnoreCase)

                    Catch ex As Exception
                        Return False

                    End Try
                End Function)
    End Sub

    ''' <summary>
    ''' Waits until an element matching the specified <see cref="By"/> selector is present 
    ''' in the DOM of the specified <see cref="IWebDriver"/>.
    ''' </summary>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="IWebDriver"/> instance.
    ''' </param>
    ''' 
    ''' <param name="by">
    ''' The <see cref="By"/> selector used to locate the element.
    ''' </param>
    ''' 
    ''' <param name="timeoutSeconds">
    ''' Optional. The maximum number of seconds to wait. Default is 30 seconds.
    ''' <para></para>
    ''' If the condition is not met within this time, a <see cref="WebDriverTimeoutException"/> is thrown.
    ''' </param>
    ''' 
    ''' <returns>
    ''' If the function succeds, returns the found <see cref="IWebElement"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Public Function WaitForElement(driver As IWebDriver, by As By, Optional timeoutSeconds As Integer = 30) As IWebElement

        Dim wait As New WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds)) With {
            .PollingInterval = TimeSpan.FromMilliseconds(250)
        }

        Return wait.Until(Function(d As IWebDriver)
                              Try
                                  Dim element As IWebElement = d.FindElement(by)
                                  ' Check if element is displayed and enabled (interactable).
                                  If (element IsNot Nothing) AndAlso
                                      element.Displayed AndAlso
                                      element.Enabled Then

                                      Return element
                                  End If
                              Catch ex As NoSuchElementException
                                  ' Ignore.
                              End Try
                              ' Return Nothing to continue waiting.
                              Return Nothing
                          End Function)

        ' -------------------------------
        ' Previous logic of this function
        ' -------------------------------
        'Return wait.Until(
        '    Function(d As IWebDriver)
        '        Dim element As IWebElement = d.FindElement(by)
        '        Return If(element, Nothing)
        '    End Function)
    End Function

    ''' <summary>
    ''' Performs a click to the specified <see cref="IWebElement"/> using JavaScript execution.
    ''' <para></para>
    ''' This is useful when the standard <see cref="IWebElement.Click"/> method fails due to overlays, 
    ''' hidden elements, or other issues.
    ''' </summary>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="IWebDriver"/> instance.
    ''' </param>
    ''' 
    ''' <param name="element">
    ''' The <see cref="IWebElement"/> to be clicked.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub ClickElementJs(driver As IWebDriver, element As IWebElement)

        Dim js As IJavaScriptExecutor = TryCast(driver, IJavaScriptExecutor)
        js.ExecuteScript("try{Object.defineProperty(navigator, 'webdriver', {get: () => undefined});}catch(e){}", element)
    End Sub

    ''' <summary>
    ''' Logs a message to the plugin's associated status textbox.
    ''' </summary>
    ''' 
    ''' <param name="plugin">
    ''' The <see cref="DynamicPlugin"/> instance.
    ''' </param>
    ''' 
    ''' <param name="msg">
    ''' The message string to log. 
    ''' <para></para>
    ''' This can be either a plain string or a key from the application resource file (<c>MTAFRAT.Strings</c>) 
    ''' to automatically retrieve the localized text according to the current UI culture.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub LogMessage(plugin As DynamicPlugin, msg As String)

        Dim rm As ResourceManager = AppGlobals.StringsResourceManager
        Dim rmString As String = rm.GetString(msg, CultureInfo.CurrentUICulture)
        If Not String.IsNullOrEmpty(rmString) Then
            msg = rmString
        End If
        UIHelper.AppendLineWithTimestamp(plugin.StatusTextBox, msg, True)
    End Sub

    ''' <summary>
    ''' Logs a formatted message to the plugin's associated status textbox.
    ''' </summary>
    ''' 
    ''' <param name="plugin">
    ''' The <see cref="DynamicPlugin"/> instance.
    ''' </param>
    ''' 
    ''' <param name="msgFormat">
    ''' The message format string to log. 
    ''' <para></para>
    ''' This can be either a composite format string or a key from the application resource file (<c>MTAFRAT.Strings</c>) 
    ''' to automatically retrieve the localized text according to the current UI culture.
    ''' </param>
    ''' 
    ''' <param name="args">
    ''' An array of objects to format according to <paramref name="msgFormat"/>.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub LogMessageFormat(plugin As DynamicPlugin, msgFormat As String, ParamArray args As Object())

        Dim rm As ResourceManager = AppGlobals.StringsResourceManager
        Dim rmString As String = Nothing
        Try
            rmString = rm.GetString(msgFormat, CultureInfo.CurrentUICulture)
        Catch
        End Try
        If Not String.IsNullOrEmpty(rmString) Then
            msgFormat = rmString
        End If
        UIHelper.AppendLineWithTimestamp(plugin.StatusTextBox, String.Format(msgFormat, args), True)
    End Sub

    ''' <summary>
    ''' Displays a notification message to the user through a <see cref="MessageBox"/>.
    ''' </summary>
    ''' 
    ''' <param name="title">
    ''' The title of the message box.
    ''' </param>
    ''' 
    ''' <param name="icon">
    ''' The icon to display in the message box.
    ''' </param>
    ''' 
    ''' <param name="msg">
    ''' The message string to notify.
    ''' <para></para>
    ''' This can be either a plain string or a key from the application resource file (<c>MTAFRAT.Strings</c>) 
    ''' to automatically retrieve the localized text according to the current UI culture.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub NotifyMessage(title As String, icon As MessageBoxIcon, msg As String)

        Dim rm As ResourceManager = AppGlobals.StringsResourceManager
        Dim rmString As String = rm.GetString(msg, CultureInfo.CurrentUICulture)
        If Not String.IsNullOrEmpty(rmString) Then
            msg = rmString
        End If

        Dim f As MainForm = AppGlobals.MainFormInstance
        f.Invoke(Sub()
                     UIHelper.FlashTaskbar()
                     If Not f.Visible Then
                         UIHelper.ToggleMainFormVisibility()
                     End If
                     MessageBox.Show(f, msg, title, MessageBoxButtons.OK, icon)
                 End Sub)
    End Sub

    ''' <summary>
    ''' Displays a formatted notification message to the user through a <see cref="MessageBox"/>.
    ''' </summary>
    ''' 
    ''' <param name="title">
    ''' The title of the message box.
    ''' </param>
    ''' 
    ''' <param name="icon">
    ''' The icon to display in the message box.
    ''' </param>
    ''' 
    ''' <param name="msgFormat">
    ''' The message format string to notify.
    ''' <para></para>
    ''' This can be either a composite format string or a key from the application resource file (<c>MTAFRAT.Strings</c>) 
    ''' to automatically retrieve the localized text according to the current UI culture.
    ''' </param>
    ''' 
    ''' <param name="args">
    ''' An array of objects to format according to <paramref name="msgFormat"/>.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub NotifyMessageFormat(title As String, icon As MessageBoxIcon, msgFormat As String, ParamArray args As Object())

        Dim rm As ResourceManager = AppGlobals.StringsResourceManager
        Dim rmString As String = Nothing
        Try
            rmString = rm.GetString(msgFormat, CultureInfo.CurrentUICulture)
        Catch
        End Try
        If Not String.IsNullOrEmpty(rmString) Then
            msgFormat = rmString
        End If

        Dim f As MainForm = AppGlobals.MainFormInstance
        f.Invoke(Sub()
                     UIHelper.FlashTaskbar()
                     If Not f.Visible Then
                         UIHelper.ToggleMainFormVisibility()
                     End If
                     MessageBox.Show(f, String.Format(msgFormat, args), title, MessageBoxButtons.OK, icon)
                 End Sub)
    End Sub

    ''' <summary>
    ''' Analyzes the browser log entries since the specified date 
    ''' to find any entry containing an HTTP status code error, 
    ''' throwing an <see cref="Exception"/> with the corresponding log entry message if found.
    ''' <para></para>
    ''' It also analyzes the current page source, applying special handling for Cloudflare-protected pages.
    ''' <para></para>
    ''' This method helps determine whether the currently loaded page returned an HTTP error status code.
    ''' </summary>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="IWebDriver"/> instance pointing to the current page to be checked.
    ''' </param>
    ''' 
    ''' <param name="afterDate">
    ''' Only browser log entries with a <see cref="LogEntry.Timestamp"/> greater than 
    ''' or equal to this date are analyzed.
    ''' <para></para>
    ''' This allows filtering out logs from previous navigations or operations.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub ThrowIfAnyErrorStatusCode(driver As IWebDriver, afterDate As Date)

        Dim logs As ReadOnlyCollection(Of LogEntry) = driver.Manage().Logs.GetLog(LogType.Browser)
        For Each log As LogEntry In logs.Where(
                Function(x) x.Timestamp >= afterDate AndAlso
                            x.Message.Contains("status of", StringComparison.InvariantCultureIgnoreCase) AndAlso
                            Not PluginSupport.IsResourceUrlLogEntryMessage(x.Message) AndAlso
                            Not x.Message.Contains("/api/", StringComparison.InvariantCultureIgnoreCase))

            Select Case log.Level
                Case LogLevel.Severe
                    Dim msg As String = log.Message
                    If driver.PageSource.Contains("cloudflare", StringComparison.InvariantCultureIgnoreCase) Then
                        ' The first time navigating to a Cloudflare-protected page,
                        ' it may return a status code of 403 (Forbidden).
                        ' This happens before the Cloudflare trial is fully completed once.
                        ' Ignore it, as we have special handling for Cloudflare-protected pages below. 👇
                        Exit Select
                    End If

                    Throw New Exception(msg)
            End Select
        Next log

        If driver.PageSource.Contains("cloudflare", StringComparison.InvariantCultureIgnoreCase) AndAlso (
                driver.PageSource.Contains(".error-footer", StringComparison.InvariantCultureIgnoreCase) OrElse
                driver.PageSource.Contains("Web server is down", StringComparison.InvariantCultureIgnoreCase)
            ) Then

            Try
                Dim titleElement As IWebElement = driver.FindElement(By.TagName("title"))
                Dim titleText As String = titleElement.GetAttribute("textContent")
                Throw New Exception(titleText)

            Catch ex As Exception
                Throw
            End Try
        End If

    End Sub

    ''' <summary>
    ''' Analyzes the browser log entries since the specified date 
    ''' to find any entry containing the specified HTTP status code error, 
    ''' throwing an <see cref="Exception"/> with the corresponding log entry message if found.
    ''' </summary>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="IWebDriver"/> instance.
    ''' </param>
    ''' 
    ''' <param name="driver">
    ''' The status code to check for.
    ''' </param>
    ''' 
    ''' <param name="afterDate">
    ''' Only browser log entries with a <see cref="LogEntry.Timestamp"/> greater than 
    ''' or equal to this date are analyzed.
    ''' <para></para>
    ''' This allows filtering out logs from previous navigations or operations.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub ThrowIfStatusCode(driver As IWebDriver, statusCode As Integer, afterDate As Date)

        Dim logs As ReadOnlyCollection(Of LogEntry) = driver.Manage().Logs.GetLog(LogType.Browser)
        For Each log As LogEntry In logs.Where(
                Function(x) x.Timestamp >= afterDate AndAlso
                            x.Message.Contains($"status of {statusCode}", StringComparison.InvariantCultureIgnoreCase))

            Select Case log.Level
                Case LogLevel.Severe
                    Dim msg As String = log.Message
                    Throw New Exception(msg)
            End Select
        Next log

    End Sub

    ''' <summary>
    ''' If any <see cref="LogEntry"/> after the specified date
    ''' in the browser logs matches the specified HTTP error status code,
    ''' throws an <see cref="Exception"/> with the log entry message.
    ''' </summary>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="IWebDriver"/> instance.
    ''' </param>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="HttpStatusCode"/> to check for.
    ''' </param>
    ''' 
    ''' <param name="afterDate">
    ''' Only browser log entries with a <see cref="LogEntry.Timestamp"/> greater than 
    ''' or equal to this date are analyzed.
    ''' <para></para>
    ''' This allows filtering out logs from previous navigations or operations.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub ThrowIfStatusCode(driver As IWebDriver, statusCode As HttpStatusCode, afterDate As Date)

        ThrowIfStatusCode(driver, CInt(statusCode), afterDate)
    End Sub

#End Region

#Region " Restricted Methods "

    ''' <summary>
    ''' Loads all dynamic plugins from the plugins directory 
    ''' and returns a list of <see cref="DynamicPlugin"/> objects.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' A list of <see cref="DynamicPlugin"/> objects representign each initialized plugin.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function LoadAllPluginsFromJson() As ReadOnlyCollection(Of DynamicPlugin)

        Dim plugins As New List(Of DynamicPlugin)

        If Not Directory.Exists(AppGlobals.PluginsDirectoryPath) Then
            Directory.CreateDirectory(AppGlobals.PluginsDirectoryPath)
        End If

        Dim options As New EnumerationOptions With {
            .AttributesToSkip = FileAttributes.None,
            .MatchCasing = MatchCasing.CaseInsensitive,
            .MatchType = MatchType.Simple,
            .RecurseSubdirectories = True
        }
        For Each file As String In Directory.GetFiles(AppGlobals.PluginsDirectoryPath, "*.json", options)
            Try
                plugins.Add(New DynamicPlugin(file))
            Catch ex As Exception
                Dim f As MainForm = AppGlobals.MainFormInstance
                If f.IsHandleCreated Then
                    f.Invoke(Sub() My.Forms.MainSplashScreen.Visible = False)
                End If
                MessageBox.Show(String.Format(My.Resources.Strings.ErrorLoadingPluginFormat, file, ex.Message),
                                My.Application.Info.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error)
                f.Close()
            End Try
        Next

        Return New ReadOnlyCollection(Of DynamicPlugin)(plugins)
    End Function

    ''' <summary>
    ''' Determines whether a given browser log entry message refers to a resource URL
    ''' that points to a file (e.g., CSS, JS, image file) rather than a page/document.
    ''' </summary>
    ''' 
    ''' <param name="msg">
    ''' The raw log entry message containing the URL.
    ''' </param>
    ''' 
    ''' <returns>
    ''' <see langword="True"/> if the URL in the message points to a file (has a filename with an extension); 
    ''' otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function IsResourceUrlLogEntryMessage(msg As String) As Boolean

        Try
            Dim url As String = msg.Substring(0, msg.IndexOf(" "c))
            If url.Contains("?"c) Then
                url = url.Substring(0, msg.IndexOf("?"c))
            End If

            Dim uri As New Uri(url)
            Dim filename As String = Path.GetFileName(uri.LocalPath)

            Return Not String.IsNullOrEmpty(filename) AndAlso
                   filename.Contains("."c) AndAlso
                   Not (filename.EndsWith(".htm", StringComparison.InvariantCultureIgnoreCase) OrElse
                        filename.EndsWith(".html", StringComparison.InvariantCultureIgnoreCase) OrElse
                        filename.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase)
                       )
        Catch
            Return False
        End Try
    End Function

#End Region

End Module