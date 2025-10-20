''' <summary>
''' Specifies the possible registration states detected during plugin execution.
''' </summary>
Public Enum RegistrationStatus

    ''' <summary>
    ''' The registration status could not be determined due to an error or unexpected condition.
    ''' </summary>
    Unknown

    ''' <summary>
    ''' The registration process is currently open.
    ''' </summary>
    Open

    ''' <summary>
    ''' The registration process is currently closed.
    ''' </summary>
    Closed

End Enum
