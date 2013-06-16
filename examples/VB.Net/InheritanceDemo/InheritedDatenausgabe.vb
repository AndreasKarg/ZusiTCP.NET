'If we want to add a new type, we have to inherit the ZusiTcpCon.
Public Class InheritedDatenausgabe
    Inherits Zusi_Datenausgabe.ZusiTcpClientConnection
    'Declare a construcor - the old needs attributes
    Public Sub New(ByVal clientID As String, ByVal priority As Zusi_Datenausgabe.ClientPriority)
        'Call - explicit - the old construcor
        MyBase.New(clientID, priority)
    End Sub
    'But WE need the constructor with the alternative commandset path. So create this construtor, too.
    Public Sub New(ByVal clientId As String, ByVal priority As Zusi_Datenausgabe.ClientPriority, ByVal commandsetPath As String)
        MyBase.New(clientId, priority, commandsetPath)
    End Sub

    'When data arrives, we'll raise an Event. So we have to declare it.
    'Note: With this XML-Comment users have a documentation right in the IntelliSense.
    ''' <summary>
    ''' Event used to handle incoming Exampletype-data.
    ''' </summary>
    Public Event ExampletypeReceived As Zusi_Datenausgabe.ReceiveEvent(Of Exampletype)

    'This Sub will raise the event and inform the clients, that new datas just arrived 
    ''' <summary>
    ''' Raises the ExampletypeReceived-Event
    ''' </summary>
    Protected Overridable Sub OnExampletypeReceived(ByVal sender As Object, ByVal data As Zusi_Datenausgabe.DataSet(Of Exampletype))
        RaiseEvent ExampletypeReceived(sender, data)
    End Sub



    'During the Zusi_Datenausgabe-assembly is using reflection, we have to use exactly 
    'this name and exactly this attributes for handling our new type. It will be called by the baseclass automaticly.
    Protected Sub HandleDATA_Exampletype(ByVal input As System.IO.BinaryReader, ByVal id As Integer)
        'When a new Exempletype arrived ...
        Dim ExampleInst As New Exampletype '... declare a instance to store the datas

        ExampleInst.Byte1 = input.ReadByte '... and configurate our type by setting the bytes.
        ExampleInst.Byte2 = input.ReadByte '    To set the bytes, we have to read byte by byte out of the stream.
        ExampleInst.Byte3 = input.ReadByte
        ExampleInst.Byte4 = input.ReadByte

        'Now pass the data back to the main thread. 
        'In the main thread OnExampletypeReceived should be called to raise the event and inform the clients.
        Me.PostToHost(Of Exampletype)(CType(AddressOf OnExampletypeReceived, Zusi_Datenausgabe.ReceiveEvent(Of Exampletype)), id, ExampleInst)
    End Sub


End Class
