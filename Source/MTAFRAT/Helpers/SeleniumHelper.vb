#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Management

#End Region

''' <summary>
''' Provides helper methods for selenium / selenium-manager.exe related operations.
''' </summary>
Friend Module SeleniumHelper

#Region " Restricted Methods "

    ''' <summary>
    ''' Initializes Selenium environment, and downloads Chrome if does not exists in cache path.
    ''' </summary>
    <DebuggerStepThrough>
    Friend Sub InitializeSelenium()
        Dim splashScreen As MainSplashScreen = AppGlobals.MainSplashScreenInstance
        If splashScreen.IsHandleCreated Then
            splashScreen.Invoke(Sub() splashScreen.Label_StatusLoad.Text = "Initializing Selenium (downloading Chrome)...")
        End If

        ' Set env var for this process (MUST be done before any Selenium usage).
        Environment.SetEnvironmentVariable("SE_CACHE_PATH", AppGlobals.SeleniumCachePath, EnvironmentVariableTarget.Process)

        ' Force Chrome download into Selenium cache path.
        Using p As New Process
            With p.StartInfo
                .FileName = AppGlobals.SeleniumManagerExecPath
                .ArgumentList.Add("--driver")
                .ArgumentList.Add("chromedriver")
                .ArgumentList.Add("--cache-path")
                .ArgumentList.Add($"""{AppGlobals.SeleniumCachePath}""")

                .WindowStyle = ProcessWindowStyle.Hidden
            End With

            p.Start()
            p.WaitForExit(TimeSpan.FromMinutes(5))
        End Using

    End Sub

    ''' <summary>
    ''' Clears the contents of the Selenium-manager cache.
    ''' </summary>
    <DebuggerStepThrough>
    Friend Sub ClearCache()
        ' Ensures cache path is correct if it gets modified for some unexpected reason.
        Environment.SetEnvironmentVariable("SE_CACHE_PATH", AppGlobals.SeleniumCachePath, EnvironmentVariableTarget.Process)

        Using p As New Process
            With p.StartInfo
                .FileName = AppGlobals.SeleniumManagerExecPath
                .ArgumentList.Add("--driver")
                .ArgumentList.Add("chromedriver")
                .ArgumentList.Add("--clear-cache")
                .ArgumentList.Add("--offline")

                .WindowStyle = ProcessWindowStyle.Hidden
            End With

            p.Start()
            p.WaitForExit(TimeSpan.FromMinutes(5))
        End Using

    End Sub

    ''' <summary>
    ''' Kills all <c>chromedriver.exe</c> processes that were launched by the current application process.
    ''' </summary>
    <DebuggerStepThrough>
    Friend Sub KillChildrenChromeDriverAndChromeProcesses()

        ' Retrieves the parent process ID of the specified process.
        Dim getParentProcessIdFunc As New Func(Of Integer, Integer)(
            Function(pid As Integer) As Integer
                Dim query As String = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {pid}"
                Dim options As New Management.EnumerationOptions() With {
                    .EnsureLocatable = False,
                    .ReturnImmediately = True,
                    .Rewindable = False,
                    .Timeout = TimeSpan.FromSeconds(5)
                }
                Using searcher As New ManagementObjectSearcher(query)
                    Dim obj As ManagementObject = searcher.Get().OfType(Of ManagementObject).SingleOrDefault()
                    If obj IsNot Nothing Then
                        Return Convert.ToInt32(obj("ParentProcessId"))
                    End If
                End Using

                Return 0
            End Function)

        Dim myProcessID As Integer = Environment.ProcessId
        For Each p As Process In Process.GetProcessesByName("chromedriver")
            Dim parentProcessId As Integer = getParentProcessIdFunc(p.Id)
            Dim isMyChild As Boolean = (parentProcessId = myProcessID)

            If isMyChild Then
                Try
                    p.Kill(entireProcessTree:=True)
                Catch
                End Try
            End If
        Next
    End Sub

#End Region

End Module
