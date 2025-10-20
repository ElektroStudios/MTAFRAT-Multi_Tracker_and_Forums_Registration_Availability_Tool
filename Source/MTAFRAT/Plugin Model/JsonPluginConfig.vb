#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

''' <summary>
''' Represents the configuration of a plugin loaded from a JSON file.
''' </summary>
Public NotInheritable Class JsonPluginConfig

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the user-friendly name of this plugin.
    ''' </summary>
    Public Property Name As String

    ''' <summary>
    ''' Gets or sets the description of this plugin.
    ''' </summary>
    Public Property Description As String

    ''' <summary>
    ''' Gets or sets the URL related to this plugin (typically the login or register webpage).
    ''' </summary>
    Public Property Url As String

    ''' <summary>
    ''' Gets or sets the path to the icon or logo file representing this plugin.
    ''' </summary>
    Public Property IconPath As String

    ''' <summary>
    ''' Gets or sets the path to the VisualBasic.NET code file associated with this plugin.
    ''' </summary>
    Public Property VbCodeFile As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="JsonPluginConfig"/> class.
    ''' </summary>
    Public Sub New()
    End Sub

#End Region

End Class