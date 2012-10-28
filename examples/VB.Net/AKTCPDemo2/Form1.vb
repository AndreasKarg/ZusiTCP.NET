Imports Zusi_Datenausgabe
Public Class Form1
    ' We need the new Type here
    Private WithEvents MyTCPConnection As InheritedDatenausgabe

    Public Sub New()
        InitializeComponent()

        ' We've changed the name of the commandset.xml, so we have to hand the new name. 
        MyTCPConnection = New InheritedDatenausgabe("Zusi TCP Demo 2", ClientPriority.Low, "commandset_New.xml")

        ' We request from Zusi the following data:
        MyTCPConnection.RequestData("Examplename") '2645  => ExampletypeReceived
    End Sub

    Private Sub BtnConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnConnect.Click

        ' If we're currently disconnected...
        If (MyTCPConnection.ConnectionState = Zusi_Datenausgabe.ConnectionState.Disconnected) Then
            ' ... try to ... 
            Try
                ' ... establish a connection using the hostname and port number from the main window.
                MyTCPConnection.Connect(TbServer.Text, CType(TbPort.Text, Integer))

                ' When successful, change the button label to "Disconnect".
                BtnConnect.Text = "Disconnect"

                ' If something goes wrong...
            Catch ex As ZusiTcpException
                ' ... show the user what the connection object has to say.
                System.Windows.Forms.MessageBox.Show(String.Format("An error occured when trying to connect: {0}", ex.Message))

                ' ... reset the connection by explicitly calling Disconnect()
                MyTCPConnection.Disconnnect()

                ' ... and then change the button label to "Connect".
                BtnConnect.Text = "Connect"
            End Try

            ' If we're currently connected or the connection fell into an error state...
        Else
            ' ... reset the connection by explicitly calling Disconnect()
            MyTCPConnection.Disconnnect()

            ' ... and then change the button label to "Connect".
            BtnConnect.Text = "Connect"
        End If
    End Sub

    Private Sub MyTCPConnection_ExampletypeReceived(ByVal sender As Object, ByVal data As Zusi_Datenausgabe.DataSet(Of Exampletype)) Handles MyTCPConnection.ExampletypeReceived
        ' So here we can handle our new data.

        Dim currentText As New System.Collections.Generic.List(Of String)(lblOut.Text.Split(New String() _
        {System.Environment.NewLine}, System.StringSplitOptions.RemoveEmptyEntries))
        ' Create a list of each line in the label "lblOut"
        currentText.Insert(0, Date.Now.ToString("HH:mm:ss") & "." & Date.Now.Millisecond.ToString("000") & "  " & data.Value.Byte1.ToString("X2") & "  " & _
        data.Value.Byte2.ToString("X2") & "  " & data.Value.Byte3.ToString("X2") & "  " & data.Value.Byte4.ToString("X2"))
        ' Insert a line at position 0 with the current time and each byte hexadicimal.

        'If the text has more than 15 lines, remove the last ones
        While currentText.Count > 15
            currentText.RemoveAt(15)
        End While

        'Join the text to one string again and set it to the label.
        lblOut.Text = String.Join(System.Environment.NewLine, currentText.ToArray)

    End Sub
End Class
