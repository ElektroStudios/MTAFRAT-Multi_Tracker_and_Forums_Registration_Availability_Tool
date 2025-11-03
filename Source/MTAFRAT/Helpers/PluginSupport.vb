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
Imports System.Net.Http
Imports System.Resources
Imports System.Threading

Imports OpenQA.Selenium
Imports OpenQA.Selenium.Chrome
Imports OpenQA.Selenium.Support.UI

Imports SeleniumCookie = OpenQA.Selenium.Cookie     ' instead of System.Net.Cookie
Imports SeleniumLogEntry = OpenQA.Selenium.LogEntry ' instead of OpenQA.Selenium.DevTools.LogEntry

#End Region

''' <summary>
''' Provides helper members related to plugin operations.
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
    ''' <param name="refService">
    ''' Receives the <see cref="ChromeDriverService"/> instance created by this function.
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
    Public Function CreateChromeDriver(plugin As DynamicPlugin, ByRef refService As ChromeDriverService, headless As Boolean, ParamArray arguments As String()) As ChromeDriver

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

        refService = ChromeDriverService.CreateDefaultService()
        With refService
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
        If Not File.Exists(refService.LogPath) Then
            Directory.CreateDirectory(Path.GetDirectoryName(refService.LogPath))
            File.Create(refService.LogPath, FileOptions.None).Dispose()
        End If

        Dim driver As New ChromeDriver(refService, options)
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

        If Not NetworkHelper.IsNetworkAvailable Then
            Throw New Exception(My.Resources.Strings.StatusMsg_NetworkIsNotAvailable)
        End If

        If Not NetworkHelper.UrlExists(New Uri(url)) Then
            Throw New Exception(My.Resources.Strings.StatusMsg_UrlNotfound404)
        End If

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
    Public Sub WaitForPageReady(driver As IWebDriver, Optional timeout As TimeSpan = Nothing)

        If timeout = Nothing Then
            timeout = New TimeSpan(hours:=0, minutes:=0, seconds:=30)
        End If

        Dim drvWait As New WebDriverWait(driver, timeout) With {
            .PollingInterval = TimeSpan.FromMilliseconds(500)
        }

        Dim js As IJavaScriptExecutor = TryCast(driver, IJavaScriptExecutor)
        If js Is Nothing Then
            Throw New ArgumentException(My.Resources.Strings.IWebDriverMustSupportJavascriptExecution, NameOf(driver))
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
            .PollingInterval = TimeSpan.FromMilliseconds(500)
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

        UIHelper.AppendLineWithTimestamp(plugin.LogTextBox, ResolveLocalizedString(msg), addNewLine:=True)
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

        UIHelper.AppendLineWithTimestamp(plugin.LogTextBox, ResolveLocalizedString(msgFormat, args), addNewLine:=True)
    End Sub

    ''' <summary>
    ''' Prints a message to the plugin's associated status textbox.
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
    Public Sub PrintMessage(plugin As DynamicPlugin, msg As String)

        UIHelper.AppendLine(plugin.LogTextBox, ResolveLocalizedString(msg), addNewLine:=True)
    End Sub

    ''' <summary>
    ''' Prints a formatted message to the plugin's associated status textbox.
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
    Public Sub PrintMessageFormat(plugin As DynamicPlugin, msgFormat As String, ParamArray args As Object())

        UIHelper.AppendLine(plugin.LogTextBox, ResolveLocalizedString(msgFormat, args), addNewLine:=True)
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
    ''' A common utility function used by multiple plugins that encapsulates the default steps to navigate 
    ''' to a registration form page, check and return its current state, and handle message logging and UI notifications.
    ''' </summary>
    ''' 
    ''' <param name="plugin">
    ''' The <see cref="DynamicPlugin"/> instance calling this method.
    ''' </param>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="ChromeDriver"/> instance associated to the plugin.
    ''' </param>
    ''' 
    ''' <param name="trigger">
    ''' The trigger phrase that indicates the registration form is open or closed.
    ''' </param>
    ''' 
    ''' <param name="isOpenTrigger">
    ''' If <see langword="True"/>, the <paramref name="trigger"/> phrase indicates that the form is open;
    ''' if <see langword="False"/>, the <paramref name="trigger"/> phrase indicates that the form is closed.
    ''' </param>
    ''' 
    ''' <returns>
    ''' A <see cref="RegistrationFlags"/> value indicating whether the registration form is open or closed.
    ''' </returns>
    <DebuggerStepThrough>
    Public Function DefaultRegistrationFormCheckProcedure(plugin As DynamicPlugin, driver As ChromeDriver,
                                                          trigger As String, isOpenTrigger As Boolean) As RegistrationFlags

        Dim pluginUrl As String = plugin.UrlRegistration

        PluginSupport.LogMessageFormat(plugin, "StatusMsg_ConnectingFormat", plugin.Name)
        PluginSupport.LogMessage(plugin, $"➜ {pluginUrl}")
        PluginSupport.NavigateTo(driver, pluginUrl)

        If PluginSupport.IsCloudflareChallengeRequired(pluginUrl) Then
            PluginSupport.WaitToCompleteCloudflareChallenge(plugin, driver, timeout:=30000)
        End If

        PluginSupport.WaitForPageReady(driver)
        If Not driver.Url.Equals(pluginUrl, StringComparison.InvariantCultureIgnoreCase) Then
            Throw New Exception(My.Resources.Strings.CurrentBrowserUrlDiffersFromPluginUrl & $" ({pluginUrl} ➜ {driver.Url})")
        End If
        PluginSupport.LogMessage(plugin, "StatusMsg_RegisterPageLoaded")

        Return PluginSupport.EvaluateRegistrationFormState(plugin, driver, trigger, isOpenTrigger)
    End Function

    ''' <summary>
    ''' A common utility function used by multiple plugins that encapsulates the default steps to navigate 
    ''' to an application form page, check and return its current state, and handle message logging and UI notifications.
    ''' </summary>
    ''' 
    ''' <param name="plugin">
    ''' The <see cref="DynamicPlugin"/> instance calling this method.
    ''' </param>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="ChromeDriver"/> instance associated to the plugin.
    ''' </param>
    ''' 
    ''' <param name="trigger">
    ''' A trigger phrase that indicates the application form is open or closed.
    ''' </param>
    ''' 
    ''' <param name="isOpenTrigger">
    ''' If <see langword="True"/>, the <paramref name="trigger"/> phrase indicates that the application form is open;
    ''' if <see langword="False"/>, the <paramref name="trigger"/> phrase indicates that the application form is closed.
    ''' </param>
    ''' 
    ''' <returns>
    ''' A <see cref="RegistrationFlags"/> value indicating whether the application form is open or closed.
    ''' </returns>
    <DebuggerStepThrough>
    Public Function DefaultApplicationFormCheckProcedure(plugin As DynamicPlugin, driver As ChromeDriver,
                                                         trigger As String, isOpenTrigger As Boolean) As RegistrationFlags

        Dim pluginUrl As String = plugin.UrlApplication

        PluginSupport.LogMessageFormat(plugin, "StatusMsg_ConnectingFormat", plugin.Name)
        PluginSupport.LogMessage(plugin, $"➜ {pluginUrl}")
        PluginSupport.NavigateTo(driver, pluginUrl)

        If PluginSupport.IsCloudflareChallengeRequired(pluginUrl) Then
            PluginSupport.WaitToCompleteCloudflareChallenge(plugin, driver, timeout:=30000)
        End If

        PluginSupport.WaitForPageReady(driver)
        If Not driver.Url.Equals(pluginUrl, StringComparison.InvariantCultureIgnoreCase) Then
            Throw New Exception(My.Resources.Strings.CurrentBrowserUrlDiffersFromPluginUrl & $" ({pluginUrl} ➜ {driver.Url})")
        End If
        PluginSupport.LogMessage(plugin, "StatusMsg_ApplicationPageLoaded")

        Return PluginSupport.EvaluateApplicationFormState(plugin, driver, trigger, isOpenTrigger)
    End Function

    ''' <summary>
    ''' Evaluates a registration form state based on source page content.
    ''' <para></para>
    ''' It logs the result message and optionally displays a notification if registration form is open.
    ''' </summary>
    ''' 
    ''' <param name="plugin">
    ''' The <see cref="DynamicPlugin"/> instance calling this method.
    ''' </param>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="ChromeDriver"/> instance containing the page source.
    ''' </param>
    ''' 
    ''' <param name="trigger">
    ''' The trigger phrase that indicates the registration form is open or closed.
    ''' </param>
    ''' 
    ''' <param name="isOpenTrigger">
    ''' If <see langword="True"/>, the <paramref name="trigger"/> phrase indicates that the form is open;
    ''' if <see langword="False"/>, the <paramref name="trigger"/> phrase indicates that the form is closed.
    ''' </param>
    ''' 
    ''' <returns>
    ''' A <see cref="RegistrationFlags"/> value indicating whether the registration form is open or closed.
    ''' </returns>
    <DebuggerStepThrough>
    Public Function EvaluateRegistrationFormState(plugin As DynamicPlugin, driver As ChromeDriver,
                                                  trigger As String, isOpenTrigger As Boolean) As RegistrationFlags

        PluginSupport.LogMessage(plugin, "StatusMsg_AnalyzingPageContent")

        If String.IsNullOrWhiteSpace(trigger) Then
            PluginSupport.LogMessage(plugin, "StatusMsg_TriggerRegEmpty")
            Return RegistrationFlags.RegistrationUnknown
        End If

        Dim pageSource As String = driver.PageSource
        If String.IsNullOrWhiteSpace(pageSource) OrElse
            pageSource.Equals("<html><head></head><body></body></html>", StringComparison.InvariantCultureIgnoreCase) Then

            PluginSupport.LogMessageFormat(plugin, "StatusMsg_PageSourceEmptyFormat", {pageSource})
            Return RegistrationFlags.RegistrationUnknown
        End If

        Dim triggerFound As Boolean = pageSource.Contains(trigger, StringComparison.InvariantCultureIgnoreCase)

        Dim result As RegistrationFlags =
            If(isOpenTrigger,
               If(triggerFound, RegistrationFlags.RegistrationOpen, RegistrationFlags.RegistrationClosed),
               If(triggerFound, RegistrationFlags.RegistrationClosed, RegistrationFlags.RegistrationOpen))

        Select Case result
            Case RegistrationFlags.RegistrationOpen
                PluginSupport.LogMessage(plugin, "StatusMsg_DetectedRegOpen")
                PluginSupport.NotifyMessageFormat("😄🎉🎉🎉", MessageBoxIcon.Information, "StatusMsg_MsgboxRegOpenFormat", plugin.Name)
            Case RegistrationFlags.RegistrationClosed
                PluginSupport.LogMessage(plugin, "StatusMsg_DetectedRegClosed")
        End Select

        Return result
    End Function

    ''' <summary>
    ''' Evaluates an application form state based on source page content.
    ''' <para></para>
    ''' It logs the result message and optionally displays a notification if application form is open.
    ''' </summary>
    ''' 
    ''' <param name="plugin">
    ''' The <see cref="DynamicPlugin"/> instance calling this method.
    ''' </param>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="ChromeDriver"/> instance containing the page source.
    ''' </param>
    ''' 
    ''' <param name="trigger">
    ''' A trigger phrase that indicates the application form is open or closed.
    ''' </param>
    ''' 
    ''' <param name="isOpenTrigger">
    ''' If <see langword="True"/>, the <paramref name="trigger"/> phrase indicates that the application form is open;
    ''' if <see langword="False"/>, the <paramref name="trigger"/> phrase indicates that the application form is closed.
    ''' </param>
    ''' 
    ''' <returns>
    ''' A <see cref="RegistrationFlags"/> value indicating whether the application form is open or closed.
    ''' </returns>
    <DebuggerStepThrough>
    Public Function EvaluateApplicationFormState(plugin As DynamicPlugin, driver As ChromeDriver,
                                                 trigger As String, isOpenTrigger As Boolean) As RegistrationFlags

        PluginSupport.LogMessage(plugin, "StatusMsg_AnalyzingPageContent")

        If String.IsNullOrWhiteSpace(trigger) Then
            PluginSupport.LogMessage(plugin, "StatusMsg_TriggerAppEmpty")
            Return RegistrationFlags.ApplicationUnknown
        End If

        Dim pageSource As String = driver.PageSource
        If String.IsNullOrWhiteSpace(pageSource) OrElse
            pageSource.Equals("<html><head></head><body></body></html>", StringComparison.InvariantCultureIgnoreCase) Then

            PluginSupport.LogMessageFormat(plugin, "StatusMsg_PageSourceEmptyFormat", {pageSource})
            Return RegistrationFlags.ApplicationUnknown
        End If

        Dim triggerFound As Boolean = pageSource.Contains(trigger, StringComparison.InvariantCultureIgnoreCase)

        Dim result As RegistrationFlags =
            If(isOpenTrigger,
               If(triggerFound, RegistrationFlags.ApplicationOpen, RegistrationFlags.ApplicationClosed),
               If(triggerFound, RegistrationFlags.ApplicationClosed, RegistrationFlags.ApplicationOpen))

        Select Case result
            Case RegistrationFlags.ApplicationOpen
                PluginSupport.LogMessage(plugin, "StatusMsg_DetectedApplicationOpen")

                Dim f As MainForm = AppGlobals.MainFormInstance
                If f.DarkCheckBox_AllowPluginApplicationFormCheck.Checked Then
                    PluginSupport.NotifyMessageFormat("😄🎉🎉🎉", MessageBoxIcon.Information, "StatusMsg_MsgboxApplicationOpenFormat", plugin.Name)
                End If

            Case RegistrationFlags.ApplicationClosed
                PluginSupport.LogMessage(plugin, "StatusMsg_DetectedApplicationClosed")
        End Select

        Return result
    End Function

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
    ''' Only browser log entries with a <see cref="SeleniumLogEntry.Timestamp"/> greater than 
    ''' or equal to this date are analyzed.
    ''' <para></para>
    ''' This allows filtering out logs from previous navigations or operations.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub ThrowIfAnyErrorStatusCode(driver As IWebDriver, afterDate As Date)

        Dim logs As ReadOnlyCollection(Of SeleniumLogEntry) = driver.Manage().Logs.GetLog(LogType.Browser)
        For Each log As SeleniumLogEntry In logs.Where(
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
                        ' This happens before the Cloudflare challenge is fully completed once.
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
    ''' Only browser log entries with a <see cref="SeleniumLogEntry.Timestamp"/> greater than 
    ''' or equal to this date are analyzed.
    ''' <para></para>
    ''' This allows filtering out logs from previous navigations or operations.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub ThrowIfStatusCode(driver As IWebDriver, statusCode As Integer, afterDate As Date)

        Dim logs As ReadOnlyCollection(Of SeleniumLogEntry) = driver.Manage().Logs.GetLog(LogType.Browser)
        For Each log As SeleniumLogEntry In logs.Where(
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
    ''' If any <see cref="SeleniumLogEntry"/> after the specified date
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
    ''' Only browser log entries with a <see cref="SeleniumLogEntry.Timestamp"/> greater than 
    ''' or equal to this date are analyzed.
    ''' <para></para>
    ''' This allows filtering out logs from previous navigations or operations.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub ThrowIfStatusCode(driver As IWebDriver, statusCode As HttpStatusCode, afterDate As Date)

        PluginSupport.ThrowIfStatusCode(driver, CInt(statusCode), afterDate)
    End Sub

    ''' <summary>
    ''' Determines whether the specified web page requires to complete a Cloudflare challenge to proceed.
    ''' </summary>
    ''' 
    ''' <param name="url">
    ''' The target URL to check.
    ''' </param>
    ''' 
    ''' <returns>
    ''' <see langword="True"/> if the page requires a Cloudflare challenge to proceed; 
    ''' otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Public Function IsCloudflareChallengeRequired(url As String) As Boolean

        Dim challengeIndicators As String() = {
            "challenge-error-text",
            "/cdn-cgi/challenge-platform",
            "<title>Just a moment...</title>",
            "window._cf_chl_opt"
        }

        Using handler As New HttpClientHandler() With {
                .AllowAutoRedirect = True,
                .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
            }

            Using client As New HttpClient(handler)
                Dim resp As HttpResponseMessage = client.GetAsync(url).ConfigureAwait(False).GetAwaiter().GetResult()
                Dim body As String = resp.Content.ReadAsStringAsync().ConfigureAwait(False).GetAwaiter().GetResult()

                Return resp.StatusCode <> HttpStatusCode.OK AndAlso
                       Not String.IsNullOrWhiteSpace(body) AndAlso
                       challengeIndicators.Any(Function(indicator As String) body.Contains(indicator, StringComparison.InvariantCultureIgnoreCase))

            End Using
        End Using
    End Function

    ''' <summary>
    ''' Waits for the Cloudflare challenge to complete by detecting and validating the 
    ''' <c>cf_clearance</c> cookie within the specified timeout period.
    ''' </summary>
    ''' 
    ''' <param name="plugin">
    ''' The <see cref="DynamicPlugin"/> instance.
    ''' </param>
    ''' 
    ''' <param name="driver">
    ''' The <see cref="ChromeDriver"/> instance.
    ''' </param>
    ''' 
    ''' <param name="timeout">
    ''' Optional. The maximum time to wait, in milliseconds, for the challenge to complete. 
    ''' <para></para>
    ''' Default value is 30000 (30 seconds).
    ''' </param>
    <DebuggerStepThrough>
    Public Sub WaitToCompleteCloudflareChallenge(plugin As DynamicPlugin, driver As ChromeDriver,
                                                 Optional timeout As Integer = 30000)

        PluginSupport.LogMessage(plugin, "StatusMsg_CloudflareChallengeWait")

        Dim result As String = driver.ExecuteScript("return navigator.userAgent;").ToString()
        Dim isHeadlessChrome As Boolean = result.Contains("HeadlessChrome", StringComparison.InvariantCultureIgnoreCase)
        If isHeadlessChrome Then
            Throw New InvalidOperationException(My.Resources.Strings.UsingHeadlessChromeForCloudflareChallengeAttempt)
        End If

        ' cf_clearance:
        '   Clearance Cookie stores the proof of challenge passed.
        '   It is used to no longer issue a challenge if present. It is required to reach an origin server.
        ' https://developers.cloudflare.com/fundamentals/reference/policies-compliances/cloudflare-cookies/#additional-cookies-used-by-the-challenge-platform

        Dim conditionFunction As Func(Of Boolean) =
            Function()
                Dim cfCookie As SeleniumCookie = driver.Manage().Cookies.GetCookieNamed("cf_clearance")
                Return cfCookie IsNot Nothing AndAlso
                       Not String.IsNullOrWhiteSpace(cfCookie.Value) AndAlso
                       (cfCookie.Expiry.HasValue AndAlso Date.Now.AddMilliseconds(timeout) < cfCookie.Expiry.Value) AndAlso
                       Not driver.PageSource.Contains("challenges.cloudflare")
            End Function

        Dim startTime As Date = Date.Now
        While Not conditionFunction.Invoke()
            If (Date.Now - startTime).TotalMilliseconds >= timeout Then
                Throw New TimeoutException(My.Resources.Strings.StatusMsg_CloudflareChallengeTimedOut)
            End If
            Thread.Sleep(3000)
        End While

        PluginSupport.LogMessage(plugin, "StatusMsg_CloudflareChallengeCompleted")
    End Sub

#End Region

#Region " Restricted Methods "

    ''' <summary>
    ''' Resolves the specified string as either a direct text value 
    ''' or a key from the application's localized string resources (<see cref="AppGlobals.StringsResourceManager"/>),
    ''' automatically applying localization based on the current UI culture. 
    ''' <para></para>
    ''' If formatting arguments are provided, the resulting text will be formatted accordingly.
    ''' </summary>
    ''' 
    ''' <param name="keyOrText">
    ''' A plain string or a resource key to look up in the application's localized string resources.
    ''' <para></para>
    ''' If the provided string does not exist as a resource key, it will be used as-is.
    ''' </param>
    ''' 
    ''' <param name="args">
    ''' Optional formatting arguments to be applied if <paramref name="keyOrText"/> (or its localized value)
    ''' contains composite format placeholders.
    ''' </param>
    ''' 
    ''' <returns>
    ''' A localized and optionally formatted string suitable for display to the user.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function ResolveLocalizedString(keyOrText As String, ParamArray args As Object()) As String

        Dim rm As ResourceManager = AppGlobals.StringsResourceManager
        Dim localized As String = rm.GetString(keyOrText, CultureInfo.CurrentUICulture)

        ' If the string exists in the resource file, use the localized version.
        If Not String.IsNullOrEmpty(localized) Then
            keyOrText = localized
        End If

        ' If formatting arguments are provided, apply them.
        If args IsNot Nothing AndAlso args.Length > 0 Then
            keyOrText = String.Format(keyOrText, args)
        End If

        Return keyOrText
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