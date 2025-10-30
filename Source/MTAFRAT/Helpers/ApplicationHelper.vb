#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Reflection

#End Region

''' <summary>
''' Provides helper methods for UI related operations.
''' </summary>
Public Module ApplicationHelper

#Region " Restricted Methods "

    ''' <summary>
    ''' Resets the remaining auto-plugin run interval to the default value (1 hour).
    ''' </summary>
    <DebuggerStepThrough>
    Friend Sub ResetRemainingAutoPluginRunInterval()

        AppGlobals.MainFormInstance.RemainingAutoPluginRunInterval = AppGlobals.AutomaticPluginRunInterval
    End Sub

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
    ''' Clears cached data for the entire application or for a specific plugin.
    ''' </summary>
    ''' 
    ''' <param name="plugin">
    ''' Optional. If provided, only the cache for the specified plugin will be cleared.
    ''' <para></para>
    ''' If not provided, the entire application cache, including Selenium and Chrome user caches, will be cleared.
    ''' </param>
    ''' 
    ''' <returns>
    ''' A <see cref="Task"/> representing the asynchronous operation.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Async Function ClearCache(Optional plugin As DynamicPlugin = Nothing) As Task

        Dim f As MainForm = AppGlobals.MainFormInstance
        f.UseWaitCursor = True
        f.TableLayoutPanel_Main.Enabled = False

        ' Performs a safety check to ensure that the directory we intend to delete is actually located within the application's folder.
        ' This prevents accidental deletion of directories outside the app if the associated code is modified by mistake.
        Dim isSubdirectoryOfMyApplication As Func(Of String, Boolean) =
            Function(directoryPath As String)
                Dim appBasePath As String = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                Dim parentFullPath As String = Path.GetFullPath(appBasePath).TrimEnd(Path.DirectorySeparatorChar) & Path.DirectorySeparatorChar
                Dim childFullPath As String = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar) & Path.DirectorySeparatorChar
                Return childFullPath.StartsWith(parentFullPath, StringComparison.OrdinalIgnoreCase)
            End Function

        Try
            If plugin Is Nothing Then ' Clear all cache.
                UIHelper.UpdateStatusLabelText(My.Resources.Strings.ClearingApplicationCacheMsg)
                Await Task.Run(
                    Sub()
                        SeleniumHelper.ClearCache()

                        If Directory.Exists(AppGlobals.SeleniumCachePath) AndAlso
                            isSubdirectoryOfMyApplication(AppGlobals.SeleniumCachePath) Then

                            Directory.Delete(AppGlobals.SeleniumCachePath, recursive:=True)
                        End If

                        If Directory.Exists(AppGlobals.ChromeUserCachePath) AndAlso
                           isSubdirectoryOfMyApplication(AppGlobals.ChromeUserCachePath) Then

                            Directory.Delete(AppGlobals.ChromeUserCachePath, recursive:=True)
                        End If
                    End Sub)
                MessageBox.Show(f, My.Resources.Strings.ApplicationCacheHasBeenCleaned, My.Application.Info.ProductName,
                                MessageBoxButtons.OK, MessageBoxIcon.Information)

            Else ' Clear only plugin's cache.
                UIHelper.UpdateStatusLabelText(My.Resources.Strings.ClearingPluginCacheMsg)
                Await Task.Run(
                    Sub()
                        If Directory.Exists(plugin.PluginCachePath) AndAlso
                           isSubdirectoryOfMyApplication(plugin.PluginCachePath) Then

                            Directory.Delete(plugin.PluginCachePath, recursive:=True)
                        End If
                    End Sub)
                MessageBox.Show(f, My.Resources.Strings.PluginCacheHasBeenCleaned, My.Application.Info.ProductName,
                                MessageBoxButtons.OK, MessageBoxIcon.Information)

            End If

        Catch ex As Exception
            MessageBox.Show(f, $"Error: {ex.Message}", My.Application.Info.ProductName,
                                MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            f.UseWaitCursor = False
            Cursor.Current = Cursors.Default
            f.TableLayoutPanel_Main.Enabled = True
            UIHelper.UpdateStatusLabelText("")
        End Try
    End Function

    ''' <summary>
    ''' Opens a plugin-related URL (Login, Registration, or Application) using the system's default web browser.
    ''' </summary>
    ''' 
    ''' <param name="plugin">
    ''' The <see cref="DynamicPlugin"/> instance that contains the URLs to open.
    ''' </param>
    ''' 
    ''' <param name="tagUrl">
    ''' The tag identifier specifying which URL to open.
    ''' <para></para>
    ''' Valid values are <see cref="AppGlobals.TagLoginUrl"/>, 
    ''' <see cref="AppGlobals.TagRegistrationUrl"/>, and 
    ''' <see cref="AppGlobals.TagApplicationUrl"/>.
    ''' </param>
    <DebuggerStepThrough>
    Friend Sub OpenPluginUrl(plugin As DynamicPlugin, tagUrl As String)

        Dim url As String = Nothing
        Select Case tagUrl

            Case AppGlobals.TagLoginUrl
                url = plugin.UrlLogin

            Case AppGlobals.TagRegistrationUrl
                url = plugin.UrlRegistration

            Case AppGlobals.TagApplicationUrl
                url = plugin.UrlApplication

        End Select

        Try
            Using p As New Process
                p.StartInfo.FileName = url
                p.StartInfo.UseShellExecute = True

                p.Start()
            End Using

        Catch ex As Exception
            Dim f As MainForm = AppGlobals.MainFormInstance
            MessageBox.Show(f, $"Error: {ex.Message}", My.Application.Info.ProductName,
                               MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

#End Region

End Module
