Imports System
Imports System.Diagnostics
Imports System.Threading.Tasks
Imports System.Windows.Forms

Imports MTAFRAT
Imports MTAFRAT.PluginSupport

Imports OpenQA.Selenium
Imports OpenQA.Selenium.Chrome

<DebuggerStepThrough>
Class HDOlimpoPlugin : Inherits DynamicPlugin

    Overloads Async Function RunAsync() As Task(Of RegistrationStatus)
        Return Await Task.Run(
            Function()
                Dim registerUrl As String = Me.Url
                Dim applicationUrl As String = "https://hd-olimpo.club/application"
                Dim regStatus As RegistrationStatus = RegistrationStatus.Unknown

                Using service As ChromeDriverService = Nothing,
                      driver As ChromeDriver = CreateChromeDriver(Me, service, headless:=True)
                    Try
                        ' Registration form check.
                        LogMessageFormat(Me, "StatusMsg_ConnectingFormat", Me.Name)
                        NavigateTo(driver, registerUrl)

                        WaitForPageReady(driver, TimeSpan.FromSeconds(10))
                        LogMessage(Me, "StatusMsg_RegisterPageLoaded")

                        Dim registerButton As IWebElement = WaitForElement(driver, By.CssSelector("a[href*='/register']"))
                        LogMessage(Me, "StatusMsg_RegisterButtonFound")

                        registerButton.Click()
                        LogMessage(Me, "StatusMsg_RegisterButtonClicked")
                        WaitForPageReady(driver, TimeSpan.FromSeconds(10))

                        Dim pageSource As String = driver.PageSource
                        LogMessage(Me, "StatusMsg_AnalyzingPageContent")
                        If pageSource.Contains("Registro libre cerrado", StringComparison.InvariantCultureIgnoreCase) Then
                            LogMessage(Me, "StatusMsg_DetectedRegClosed")
                            regStatus = RegistrationStatus.Closed
                        Else
                            LogMessage(Me, "StatusMsg_DetectedRegOpen")
                            NotifyMessageFormat("😄🎉🎉🎉", MessageBoxIcon.Information, "StatusMsg_MsgboxRegOpenFormat", Me.Name)
                            regStatus = RegistrationStatus.Open
                        End If

                        If regStatus = RegistrationStatus.Open Then
                            Exit Try
                        End If

                        ' Application form check.
                        LogMessageFormat(Me, "StatusMsg_ConnectingFormat", Me.Name)
                        NavigateTo(driver, applicationUrl)

                        WaitForPageReady(driver, TimeSpan.FromSeconds(10))
                        LogMessage(Me, "StatusMsg_ApplicationPageLoaded")

                        Dim applicationPageSource As String = driver.PageSource
                        LogMessage(Me, "StatusMsg_AnalyzingPageContent")
                        If applicationPageSource.Contains("Vuelve más tarde", StringComparison.OrdinalIgnoreCase) Then
                            LogMessage(Me, "StatusMsg_DetectedApplicationClosed")
                            regStatus = RegistrationStatus.Closed
                        Else
                            LogMessage(Me, "StatusMsg_DetectedApplicationOpen")
                            NotifyMessageFormat("😄🎉🎉🎉", MessageBoxIcon.Information, "StatusMsg_MsgboxApplicationOpenFormat", Me.Name)
                            regStatus = RegistrationStatus.Open
                        End If

                    Catch ex As Exception
                        LogMessageFormat(Me, "StatusMsg_ExceptionFormat", ex.Message)
                        ' NotifyMessageFormat("Error", MessageBoxIcon.Error, "StatusMsg_ExceptionFormat", ex.Message)

                    Finally
                        driver?.Quit()
                        LogMessage(Me, "StatusMsg_OperationCompleted")
                    End Try
                End Using

                Return regStatus
            End Function)
    End Function

End Class