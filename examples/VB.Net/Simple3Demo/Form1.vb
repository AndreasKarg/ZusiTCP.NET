Imports Zusi_Datenausgabe
Public Class Form1
    ' We do want to have a ZusiTcpConn object, so here's the declaration
    ' With WithEvents we can handle events by marking the Subs with Handles MyTCPConnection.Exampleeventname
    Private WithEvents MyTCPConnection As ZusiTcp3TypeClient

    Public Sub New()
        InitializeComponent()

        ' When the application window is created, we create our new connection class as well.
        MyTCPConnection = New ZusiTcp3TypeClient("Zusi TCP Demo 1", ClientPriority.Low)

        ' We need to tell our connection object what measures to request from Zusi. 
        ' You may either use Zusi's native ID code or plain text as listed in the server's commandset.xml
        MyTCPConnection.RequestData("Geschwindigkeit") ' 0x1 (m/s)  . . . . . . . . . .  => FloatReceived
        MyTCPConnection.RequestData(&H15) ' "Fahrstufe" . . . . . . . . . . . . . . . .  => FloatReceived
        MyTCPConnection.RequestData(&H23) ' "Uhrzeit"   . . . . . . . . . . . . . . . .  => DateTimeReceived
        MyTCPConnection.RequestData(&H4B) ' "Datum"   . . . . . . . . . . . . . . . . .  => DateTimeReceived
        'MyTCPConnection.RequestData(2637) ' "LM Block, bis zu dem die Strecke frei ist" => StringReceived
        MyTCPConnection.RequestData(&H65) ' "Zugsicherung"  . . . . . . . . . . . . . .  => ZugsicherungReceived
        'MyTCPConnection.RequestData(2594) ' "LM LZB Ü-System"  . . . . . . . . . . . .  => BoolReceived
    End Sub

    Private Sub BtnConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnConnect.Click

        ' If we're currently disconnected...
        If (MyTCPConnection.ConnectionState = ConnectionState.Disconnected) Then
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
                MyTCPConnection.Disconnect()

                ' ... and then change the button label to "Connect".
                BtnConnect.Text = "Connect"
            End Try

            ' If we're currently connected or the connection fell into an error state...
        Else
            ' ... reset the connection by explicitly calling Disconnect()
            MyTCPConnection.Disconnect()

            ' ... and then change the button label to "Connect".
            BtnConnect.Text = "Connect"
        End If
    End Sub

    Private Sub TCPConnection_ErrorReceived(ByVal sender As Object, ByVal ex As ZusiTcpException) _
        Handles MyTCPConnection.ErrorReceived
        ' If something goes wrong...
        ' ... show the user what the connection object has to say.
        System.Windows.Forms.MessageBox.Show(String.Format("An error occured when receiving data: {0}", ex.Message))

        ' ... reset the connection by explicitly calling Disconnect()
        MyTCPConnection.Disconnect()

        ' ... and then change the button label to "Connect".
        BtnConnect.Text = "Connect"
    End Sub

    Private Sub TCPConnection_BoolReceived(ByVal sender As Object, ByVal data As DataSet(Of Boolean)) _
        Handles MyTCPConnection.BoolReceived
        Select Case data.Id
            Case Else
                'For unknown IDs...
        End Select
    End Sub

    Private Sub TCPConnection_BrakeConfigReceived(ByVal sender As Object, ByVal data As DataSet(Of BrakeConfiguration)) _
        Handles MyTCPConnection.BrakeConfigReceived
        Select Case data.Id
            Case Else
                'We didn't request a BrakeConfig-data
        End Select
    End Sub

    Private Sub TCPConnection_DateTimeReceived(ByVal sender As Object, ByVal data As DataSet(Of Date)) _
        Handles MyTCPConnection.DateTimeReceived
        Select Case data.Id
            Case &H23 ' "Uhrzeit" => DateTimeReceived
                lblUhrzeit.Text = "Uhrzeit: " & data.Value.ToLongTimeString
            Case Else
                'For unknown IDs...
        End Select
    End Sub

    Private Sub TCPConnection_DoorsReceived(ByVal sender As Object, ByVal data As DataSet(Of DoorState)) _
        Handles MyTCPConnection.DoorsReceived
        Select Case data.Id
            Case Else
                'We didn't request a Doors-data
        End Select
    End Sub

    Private Sub TCPConnection_FloatReceived(ByVal sender As Object, ByVal data As DataSet(Of Single)) _
        Handles MyTCPConnection.FloatReceived
        Select Case data.Id
            Case &H1 '2561 ' "Geschwindigkeit" => FloatReceived
                lblGeschw.Text = String.Format("Geschwindigkeit: {0} km/h ", (data.Value * 3.6).ToString("0.00")) 'two decimals
            Case &H15 '2576 ' "Fahrstufe" => FloatReceived
                lblFahrstufe.Text = "Fahrstufe: " & data.Value.ToString("0") 'no decimals
            Case Else
                'For unknown IDs...
        End Select
    End Sub

    Private Sub TCPConnection_IntReceived(ByVal sender As Object, ByVal data As DataSet(Of Integer)) _
        Handles MyTCPConnection.IntReceived
        Select Case data.Id
            Case Else
                'We didn't request a Integer-data
        End Select
    End Sub

    Private Sub TCPConnection_StringReceived(ByVal sender As Object, ByVal data As DataSet(Of String)) _
        Handles MyTCPConnection.StringReceived
        Select Case data.Id
            'Case 2637 ' "LM Block, bis zu dem die Strecke frei ist" => StringReceived
            '    lblFreiBis.Text = "Frei bis: " & data.Value
            Case Else
                'For unknown IDs...
        End Select
    End Sub

    Private Sub MyTCPConnection_ZugsicherungReceived(ByVal sender As Object, ByVal data As Zusi_Datenausgabe.DataSet(Of Zusi_Datenausgabe.Zugsicherung)) _
        Handles MyTCPConnection.ZugsicherungReceived
        Select Case data.Id
            Case &H65 ' "LM Block, bis zu dem die Strecke frei ist" => StringReceived
                Dim s32 As String = data.Value.ZugsicherungName
                s32 = s32.Replace("PZB", ":::PZB")
                s32 = s32.Replace("LZB", ":::LZB")
                Dim s33 As String() = s32.Split(New String() {":::"}, System.StringSplitOptions.RemoveEmptyEntries)
                Dim pzb As String = "PZB: Ohne"
                Dim lzb As String = "LZB: Ohne"
                For Each s1 As String In s33
                    If s1.Contains("PZB") Then
                        pzb = "PZB: " & s1
                    ElseIf s1.Contains("LZB") Then
                        lzb = "LZB: " & s1
                    Else
                        pzb &= " " & s1
                        lzb &= " " & s1
                        s1 = s1
                    End If
                Next
                lblPZB.Text = pzb
                lblLZB.Text = lzb
                lblPZB.Enabled = Not data.Value.StateIndusi.LM_Ue 'PZB off when LZB is on
                lblLZB.Enabled = data.Value.StateIndusi.LM_Ue 'enable LZB when it's on
            Case Else
                'For unknown IDs...
        End Select
    End Sub

    Private Sub BtnPause_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnPause.Click
        If MyTCPConnection Is Nothing Then Return
        MyTCPConnection.SendPause(Zusi_Datenausgabe.SwitchAction.Toogle)
    End Sub

    Private Sub BtnSpeed_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnSpeed.Click
        If MyTCPConnection Is Nothing Then Return
        MyTCPConnection.SendZeitraffer(Zusi_Datenausgabe.SwitchAction.Toogle)
    End Sub

    Private Sub BtnJump_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnJump.Click
        If MyTCPConnection Is Nothing Then Return
        MyTCPConnection.SendZeitsprung(Zusi_Datenausgabe.SwitchAction.Toogle)
    End Sub

    Private Sub BtnAP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnAP.Click
        If MyTCPConnection Is Nothing Then Return
        'MyTCPConnection.SetFog(600) 'FAIL
        'MyTCPConnection.SetFriction(600) 'FAIL
        'MyTCPConnection.SetLight(600) 'FAIL
        'MyTCPConnection.TimetabeRestart()'GEHT
        'MyTCPConnection.TrainStart("77")'GEHT
        'MyTCPConnection.
        'Dim k As New System.DateTime(
        MyTCPConnection.SendKIActive(Zusi_Datenausgabe.SwitchAction.Toogle) 'FAIL
    End Sub

    Private Sub BtnTexture_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnTexture.Click
        If MyTCPConnection Is Nothing Then Return
        Dim b32 As New System.Drawing.Bitmap(1000, 1000)
        Me.DrawToBitmap(b32, New System.Drawing.Rectangle(0, 0, 1000, 1000))
        Dim memstr As New System.IO.MemoryStream()
        b32.Save(memstr, System.Drawing.Imaging.ImageFormat.Bmp)
        MyTCPConnection.SendTexture(memstr.ToArray(), _
        0, "DAVID_rechts", 0, 0)
    End Sub
End Class
