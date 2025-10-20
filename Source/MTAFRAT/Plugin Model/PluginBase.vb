#Region " Option Statements "

Option Explicit On
Option Strict On
Option Infer Off

#End Region

''' <summary>
''' Base class for all plugins. This class must be inherited.
''' </summary>
Public MustInherit Class PluginBase : Implements IEquatable(Of PluginBase)

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the name of this plugin.
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
    ''' Gets or sets the image associated with this plugin.
    ''' </summary>
    Public Property Image As Image

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="PluginBase"/> class.
    ''' </summary>
    Public Sub New()
    End Sub

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Asynchronously executes the plugin's main logic.
    ''' </summary>
    ''' 
    ''' <param name="statusTextBox">
    ''' A <see cref="TextBox"/> control where status messages can be logged during execution.
    ''' </param>
    ''' 
    ''' <returns>
    ''' A <see cref="Task(Of RegistrationStatus)"/> representing the asynchronous operation.
    ''' </returns>
    Public MustOverride Async Function RunAsync(statusTextBox As TextBox) As Task(Of RegistrationStatus)

    ''' <summary>
    ''' Returns a <see cref="String" /> that represents this instance.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' A <see cref="String" /> that represents this instance.
    ''' </returns>
    Public Overrides Function ToString() As String

        Return Me.Name
    End Function

    ''' <summary>
    ''' Returns a hash code for this instance.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    ''' </returns>
    Public Overrides Function GetHashCode() As Integer

        Return If(Me.ToString?.ToLowerInvariant().GetHashCode(), 0)
    End Function

    ''' <summary>
    ''' Determines whether the specified <see cref="PluginBase"/> is equal to this instance.
    ''' </summary>
    ''' 
    ''' <param name="other">
    ''' The <see cref="PluginBase"/> to compare with this instance.
    ''' </param>
    ''' 
    ''' <returns>
    ''' <see langword="True"/> if the specified <see cref="PluginBase"/> is equal to this instance; 
    ''' otherwise, <see langword="False"/>.
    ''' </returns>
    Public Overloads Function Equals(other As PluginBase) As Boolean Implements IEquatable(Of PluginBase).Equals

        Return (other IsNot Nothing) AndAlso String.Equals(Me.ToString(), other.ToString(), StringComparison.InvariantCultureIgnoreCase)
    End Function

    ''' <summary>
    ''' Determines whether the specified <see cref="Object"/> is equal to this instance.
    ''' </summary>
    ''' 
    ''' <param name="obj">
    ''' The <see cref="Object"/> to compare with this instance.
    ''' </param>
    ''' 
    ''' <returns>
    ''' <see langword="True"/> if the specified <see cref="Object"/> is equal to this instance; 
    ''' otherwise, <see langword="False"/>.
    ''' </returns>
    Public Overrides Function Equals(obj As Object) As Boolean

        Return Me.Equals(TryCast(obj, PluginBase))
    End Function

#End Region

End Class