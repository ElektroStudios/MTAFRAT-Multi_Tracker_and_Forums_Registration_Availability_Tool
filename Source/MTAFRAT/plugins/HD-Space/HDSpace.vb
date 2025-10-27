Imports System
Imports System.Diagnostics
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

Imports MTAFRAT
Imports MTAFRAT.PluginSupport

Imports OpenQA.Selenium
Imports OpenQA.Selenium.Chrome

<DebuggerStepThrough>
Class HDSpacePlugin : Inherits DynamicPlugin

    ' 📝 Notes
    ' ━━━━━━━━
    '
    ' VIP / Paid account registration URL: https://hd-space.org/index.php?page=donate

    ReadOnly headless As Boolean = False
    ReadOnly additionalArgs As String() = Array.Empty(Of String)()

    Overloads Async Function RunAsync() As Task(Of RegistrationFlags)
        Dim regFlags As RegistrationFlags = RegistrationFlags.Null

        Return Await Task.Run(
            Function()
                Using service As ChromeDriverService = Nothing,
                      driver As ChromeDriver = CreateChromeDriver(Me, service, headless, additionalArgs)

                    Try
                        regFlags = regFlags Or Me.CustomRegistrationCheck(driver)

                    Catch ex As Exception
                        PluginSupport.LogMessageFormat(Me, "StatusMsg_ExceptionFormat", ex.Message)
                        ' PluginSupport.NotifyMessageFormat("Error", MessageBoxIcon.Error, "StatusMsg_ExceptionFormat", ex.Message)

                    Finally
                        driver?.Quit()
                        PluginSupport.LogMessage(Me, "StatusMsg_OperationCompleted")
                        PluginSupport.PrintMessage(Me, String.Empty)
                    End Try
                End Using

                Return regFlags
            End Function)
    End Function

    Private Function CustomRegistrationCheck(driver As ChromeDriver) As RegistrationFlags

        Const triggerRegistration As String = "registrations are closed"

        PluginSupport.LogMessageFormat(Me, "StatusMsg_ConnectingFormat", Me.Name)
        PluginSupport.LogMessage(Me, $"➜ {Me.UrlRegistration}")
        PluginSupport.NavigateTo(driver, Me.UrlRegistration)

        PluginSupport.LogMessage(Me, "StatusMsg_CloudflareTrialWait")
        PluginSupport.WaitForPageReady(driver)
        Thread.Sleep(5000)
        Dim selector As By = By.CssSelector("input[type='image'][src='images/donatenew.png']")
        PluginSupport.WaitForElement(driver, selector)
        PluginSupport.LogMessage(Me, "StatusMsg_CloudflareTrialCompleted")
        PluginSupport.WaitForPageReady(driver)
        PluginSupport.LogMessage(Me, "StatusMsg_RegisterPageLoaded")

        Return PluginSupport.EvaluateRegistrationFormState(Me, driver, triggerRegistration, isOpenTrigger:=False)
    End Function

End Class