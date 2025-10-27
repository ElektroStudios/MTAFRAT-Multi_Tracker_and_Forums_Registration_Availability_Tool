Imports System
Imports System.Diagnostics
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Windows.Forms.Design.AxImporter

Imports MTAFRAT
Imports MTAFRAT.PluginSupport

Imports OpenQA.Selenium
Imports OpenQA.Selenium.Chrome

<DebuggerStepThrough>
Class _3DTorrentsPlugin : Inherits DynamicPlugin

    ' 📝 Notes
    ' ━━━━━━━━
    '
    ' VIP / Paid account registration URL: http://www.3dtorrents.org/index.php?page=vip /
    '                                      http://www.3dtorrents.org/index.php?page=becomemember

    ReadOnly headless As Boolean = True
    ReadOnly additionalArgs As String() = {
        $"--unsafely-treat-insecure-origin-as-secure=http://www.3dtorrents.org/"
    } ' Required to avoid error 'net::ERR_SSL_PROTOCOL_ERROR'

    Overloads Async Function RunAsync() As Task(Of RegistrationFlags)
        Dim regFlags As RegistrationFlags = RegistrationFlags.Null

        Return Await Task.Run(
            Function()
                Using service As ChromeDriverService = Nothing,
                      driver As ChromeDriver = CreateChromeDriver(Me, service, headless, additionalArgs)

                    Const triggerRegistration As String = "registrations are closed"
                    Try
                        regFlags = regFlags Or
                                   PluginSupport.DefaultRegistrationFormCheckProcedure(Me, driver, triggerRegistration,
                                                                                                   isOpenTrigger:=False)

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

End Class