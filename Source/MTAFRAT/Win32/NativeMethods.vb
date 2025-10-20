﻿#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices
Imports System.Security

#End Region

Namespace Win32

    <SuppressUnmanagedCodeSecurity>
    Friend Module NativeMethods

#Region " kernel32 "

        ''' <summary>
        ''' Enables an application to inform the system that it is in use, 
        ''' thereby preventing the system from entering sleep or 
        ''' turning off the display while the application is running.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' For more information, see:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-setthreadexecutionstate">SetThreadExecutionState function (winbase.h)</see>.
        ''' </remarks>
        ''' 
        ''' <param name="flags">
        ''' The thread's execution requirements.
        ''' </param>
        ''' 
        ''' <returns>
        ''' If the function succeeds, the return value is the previous thread execution state.
        ''' <para></para>
        ''' If the function fails, the return value is <see cref="ExecutionStateFlags.Null"/>.
        ''' </returns>
        <DllImport("kernel32.dll", SetLastError:=True)>
        Friend Function SetThreadExecutionState(flags As ExecutionStateFlags) As ExecutionStateFlags
        End Function

#End Region

#Region " shell32 "

        ''' <summary>
        ''' Sends a message to the taskbar's status area icon (NotifyIcon).
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' For more information, see:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shell_notifyiconw">Shell_NotifyIconW function (shellapi.h)</see>.
        ''' </remarks>
        ''' 
        ''' <param name="message">
        ''' Specifies the action to be taken by this function.
        ''' </param>
        ''' 
        ''' <param name="refData">
        ''' A pointer to a <see cref="NotifyiconDataW"/> structure. 
        ''' <para></para>
        ''' The content of the structure depends on the value of <paramref name="message"/> parameter.
        ''' </param>
        ''' 
        ''' <returns>
        ''' Returns <see langword="True"/> if successful, or <see langword="False"/> otherwise. 
        ''' <para></para>
        ''' If <paramref name="message"/> parameter is set to <see cref="NotifyIconMessages.SetVersion"/>, 
        ''' the function returns <see langword="True"/> if the version was successfully changed, 
        ''' or <see langword="False"/> if the requested version is not supported.
        ''' </returns>
        <DllImport("shell32.dll", EntryPoint:="Shell_NotifyIconW", CharSet:=CharSet.Unicode)>
        Public Function Shell_NotifyIcon(message As NotifyIconMessages, ByRef refData As NotifyiconData) As Boolean
        End Function

#End Region

#Region " user32 "

        ''' <summary>
        ''' Flashes the specified window. It does not change the active state of the window.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' For more information, see:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-flashwindowex">FlashWindowEx function (winuser.h)</see>.
        ''' </remarks>
        ''' 
        ''' <param name="refFlashInfo">
        ''' A <see cref="FlashInfo"/> structure that defines the information required to flash the specified window.
        ''' </param>
        ''' 
        ''' <returns>
        ''' The return value specifies the window's state before the call to the <see cref="FlashWindowEx"/> function. 
        ''' <para></para>
        ''' If the window caption was drawn as active before the call, the return value is <see langword="True"/>. 
        ''' Otherwise, the return value is <see langword="False"/>.
        ''' </returns>
        <DllImport("user32.dll")>
        Friend Function FlashWindowEx(ByRef refFlashInfo As FlashInfo) As Boolean
        End Function

#End Region

    End Module

End Namespace
