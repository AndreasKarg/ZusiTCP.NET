Imports Zusi_Datenausgabe
'Hinweis: ich habe jetzt nicht viele Assembyls importiert. Wenn was nicht so gut geht > einfach mehr Assemblys importieren.
Public Class CMainWindow
    ' We do want to have a ZusiTcpConn object, so here's the declaration
    ' With WithEvents we can handle events by marking the Subs with Handles MyTCPConnection.Exampleeventname
    Private WithEvents MyTCPConnection As ZusiTcpClientConnection
    ' In this collection we'll save the states of th LMs.
    Private LmArray As New System.Collections.BitArray(8)
    ''' <summary>
    ''' Der Konstrukor. Er wird noch vor dem Start aufgerufen und sorgt dafür, dass unser Form so aussieht, wie wir es uns vorstellen.
    ''' </summary>
    Public Sub New()
        InitializeComponent()

        ' When the application window is created, we create our new connection class as well.
        MyTCPConnection = New ZusiTcpClientConnection("Zusi TCP Demo 1", ClientPriority.Low)

        ' We need to tell our connection object what measures to request from Zusi. 
        ' You may either use Zusi's native ID code or plain text as listed in the server's commandset.xml
        MyTCPConnection.RequestData(2561)   'Geschwindigkeit  . . .  => FloatReceived
        MyTCPConnection.RequestData(2563)   'Druck Bremszylinder  .  => FloatReceived
        MyTCPConnection.RequestData(2580)   'LM PZB 1000Hz  . . . .  => BoolReceived
        MyTCPConnection.RequestData(2581)   'LM PZB 500Hz . . . . .  => BoolReceived
        MyTCPConnection.RequestData(2582)   'LM PZB Befehl  . . . .  => BoolReceived
        MyTCPConnection.RequestData(2583)   'LM PZB Zugart U  . . .  => BoolReceived
        MyTCPConnection.RequestData(2584)   'LM PZB Zugart M  . . .  => BoolReceived
        MyTCPConnection.RequestData(2585)   'LM PZB Zugart O  . . .  => BoolReceived
        MyTCPConnection.RequestData(2596)   'LM Sifa  . . . . . . .  => BoolReceived

        'Die ComboBox mit Einträgen füllen
        AddCom2Combo(CbCom)
    End Sub
    ''' <summary>
    ''' Hier wird die Verbindung zum TCP-Server hergestellt.
    ''' </summary>
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

                ' ... and then change the button label to "Verbinden".
                BtnConnect.Text = "Verbinden"
            End Try

            ' If we're currently connected or the connection fell into an error state...
        Else
            ' ... reset the connection by explicitly calling Disconnect()
            MyTCPConnection.Disconnnect()

            ' ... and then change the button label to "Verbinden".
            BtnConnect.Text = "Verbinden"
        End If
    End Sub
    ''' <summary>
    ''' Wird ausgelöst, wenn in der TCPConnection ein Fehler aufgetreten ist.
    ''' </summary>
    Private Sub MyTCPConnection_ErrorReceived(ByVal sender As Object, ByVal ex As Zusi_Datenausgabe.ZusiTcpException) Handles MyTCPConnection.ErrorReceived
        ' If something goes wrong...
        ' ... show the user what the connection object has to say.
        System.Windows.Forms.MessageBox.Show(String.Format("An error occured when receiving data: {0}", ex.Message))

        ' ... reset the connection by explicitly calling Disconnect()
        MyTCPConnection.Disconnnect()

        ' ... and then change the button label to "Verbinden".
        BtnConnect.Text = "Verbinden"
    End Sub
    ''' <summary>
    ''' Hier wird eine Liste der vorhandennen COM-Ports erstellt
    ''' </summary>
    Private Sub AddCom2Combo(ByVal cbPort As System.Windows.Forms.ComboBox)
        ' Get a list of serial port names.
        Dim ports As String() = System.IO.Ports.SerialPort.GetPortNames()
        ' Show a label with Action information on it
        cbPort.Text = "The following serial ports were found:"
        ' Put each port name Into a comboBox control.
        Dim port As String
        For Each port In ports
            cbPort.Items.Add(port)
        Next port
        ' Select the first item in the combo control
        cbPort.SelectedIndex = 0
    End Sub
    ''' <summary>
    ''' Hier wird die Verbindung zu unserer Ausgabehardware hergestellt.
    ''' </summary>
    Private Sub ComCon_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComCon.Click
        If Not Serial.IsOpen Then
            Serial.PortName = CbCom.SelectedItem
            Serial.Open()
            ComCon.Text = "Trennen"
            Serial.WriteLine("ww")
            System.Threading.Thread.Sleep(500)
            If Serial.BytesToRead > 0 Then
                If Not Serial.ReadLine() = "I am a drivers desk" & System.Text.Encoding.UTF8.GetString(New Byte() {13}) Then
                    System.Windows.Forms.MessageBox.Show("An dem gewählten Port scheint das falsche Gerät angeschlossen zu sein!")
                    Serial.Close()
                    ComCon.Text = "Verbinden"
                End If
            Else
                System.Windows.Forms.MessageBox.Show("An dem gewählten Port scheint kein Gerät angeschlossen zu sein!")
                Serial.Close()
                ComCon.Text = "Verbinden"
            End If
        Else
            Serial.Close()
            ComCon.Text = "Verbinden"
        End If
    End Sub

    ''' <summary>
    ''' Wird ausgelöst, wenn ein Boolean-Wert empfangen wurde.
    ''' </summary>
    Private Sub MyTCPConnection_BoolReceived(ByVal sender As Object, ByVal data As Zusi_Datenausgabe.DataSet(Of Boolean)) Handles MyTCPConnection.BoolReceived
        Select Case data.Id
            Case 2584  'LM PZB Zugart M
                LmArray(0) = data.Value
            Case 2585  'M PZB Zugart O 
                LmArray(1) = data.Value
            Case 2582  'LM PZB Befehl 
                LmArray(2) = data.Value
            Case 2583  'LM PZB Zugart U
                LmArray(3) = data.Value
            Case 2581  'LM PZB 500Hz
                LmArray(4) = data.Value
            Case 2580  'LM PZB 1000Hz
                LmArray(5) = data.Value
            Case 2596   'Lm Sifa
                LmArray(6) = data.Value
        End Select
        If Serial.IsOpen Then 'Wenn wir mit unserer Hadware verbunden sind, können wir die Daten gleich senden.
            Dim OutChar(2) As Byte
            OutChar(0) = 80     'P
            LmArray.CopyTo(OutChar, 1) 'die Werte nach OutChar(1) kopieren.
            Serial.Write(OutChar, 0, 2)
        End If
    End Sub
    ''' <summary>
    ''' Wird ausgelöst, wenn ein Single-Wert empfangen wurde.
    ''' </summary>
    Private Sub MyTCPConnection_FloatReceived(ByVal sender As Object, ByVal data As Zusi_Datenausgabe.DataSet(Of Single)) Handles MyTCPConnection.FloatReceived
        If Serial.IsOpen Then
            Dim OutChar(2) As Byte
            Try
                Select Case data.Id
                    Case 2561   'Geschwindigkeit3
                        OutChar(0) = 86     'V
                        OutChar(1) = CByte(System.Math.Round(data.Value, 0))
                        Serial.Write(OutChar, 0, 2)
                    Case 2563   'Druck Bremszylinder 
                        OutChar(0) = 66     'B
                        OutChar(1) = CByte(System.Math.Round(15 * data.Value, 0))
                        Serial.Write(OutChar, 0, 2)
                End Select
            Catch ex As System.Exception
                ' If something goes wrong...
                ' ... show the user what the connection object has to say.
                System.Windows.Forms.MessageBox.Show(String.Format("An error occured when processing data: {0}", ex.Message))

                ' ... reset the connection by explicitly calling Disconnect()
                MyTCPConnection.Disconnnect()

                ' ... and then change the button label to "Verbinden".
                BtnConnect.Text = "Verbinden"
            End Try
        End If
    End Sub
End Class

