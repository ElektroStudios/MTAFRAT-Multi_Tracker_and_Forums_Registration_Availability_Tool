Imports System
Imports System.Diagnostics
Imports System.Threading.Tasks
Imports System.Windows.Forms

Imports MTAFRAT
Imports MTAFRAT.PluginSupport

Imports OpenQA.Selenium
Imports OpenQA.Selenium.Chrome

<DebuggerStepThrough>
Class LstPlugin : Inherits DynamicPlugin

    ReadOnly headless As Boolean = True
    ReadOnly additionalArgs As String() = Array.Empty(Of String)()

    Overloads Async Function RunAsync() As Task(Of RegistrationFlags)
        Dim regFlags As RegistrationFlags = RegistrationFlags.Null

        Return Await Task.Run(
            Function()
                Using service As ChromeDriverService = Nothing,
                      driver As ChromeDriver = CreateChromeDriver(Me, service, headless, additionalArgs)

                    Const triggerRegistration As String = "Registration Is Disabled"
                    Const triggerApplication As String = "Applications Are Closed"
                    Try
                        regFlags = regFlags Or
                                   PluginSupport.DefaultRegistrationFormCheckProcedure(Me, driver, triggerRegistration,
                                                                                                   isOpenTrigger:=False)

                        regFlags = regFlags Or
                                   PluginSupport.DefaultApplicationFormCheckProcedure(Me, driver, triggerApplication,
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