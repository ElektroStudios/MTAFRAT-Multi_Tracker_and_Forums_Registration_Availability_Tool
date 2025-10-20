#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

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

#End Region

End Module
