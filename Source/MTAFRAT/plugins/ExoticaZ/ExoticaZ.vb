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
Class ExoticaZPlugin : Inherits DynamicPlugin

    Overloads Async Function RunAsync() As Task(Of RegistrationStatus)
        Return Await Task.Run(
            Function()
                Using service As ChromeDriverService = Nothing,
                      driver As ChromeDriver = CreateChromeDriver(Me, service, headless:=False)
                    Try
                        LogMessageFormat(Me, "StatusMsg_ConnectingFormat", Me.Name)
                        NavigateTo(driver, Me.Url)

                        LogMessage(Me, "StatusMsg_CloudflareTrialWait")
                        WaitForPageReady(driver, TimeSpan.FromSeconds(10))
                        Thread.Sleep(5000)
                        Dim selector As By = By.CssSelector("#content-area")
                        WaitForElement(driver, selector, 30)
                        LogMessage(Me, "StatusMsg_CloudflareTrialCompleted")

                        Dim pageSource As String = driver.PageSource
                        LogMessage(Me, "StatusMsg_AnalyzingPageContent")
                        If pageSource.Contains("We sometimes open registration", StringComparison.InvariantCultureIgnoreCase) Then
                            LogMessage(Me, "StatusMsg_DetectedRegClosed")
                            Return RegistrationStatus.Closed
                        Else
                            LogMessage(Me, "StatusMsg_DetectedRegOpen")
                            NotifyMessageFormat("😄🎉🎉🎉", MessageBoxIcon.Information, "StatusMsg_MsgboxRegOpenFormat", Me.Name)
                            Return RegistrationStatus.Open
                        End If

                    Catch ex As Exception
                        LogMessageFormat(Me, "StatusMsg_ExceptionFormat", ex.Message)
                        ' NotifyMessageFormat("Error", MessageBoxIcon.Error, "StatusMsg_ExceptionFormat", ex.Message)

                    Finally
                        driver?.Quit()
                        LogMessage(Me, "StatusMsg_OperationCompleted")
                    End Try
                End Using

                Return RegistrationStatus.Unknown
            End Function)
    End Function

End Class