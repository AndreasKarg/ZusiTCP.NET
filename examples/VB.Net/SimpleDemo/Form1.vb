Imports Zusi_Datenausgabe
Public Class Form1
    ' We do want to have a ZusiTcpConn object, so here's the declaration
    ' With WithEvents we can handle events by marking the Subs with Handles MyTCPConnection.Exampleeventname
    Private WithEvents MyTCPConnection As ZusiTcpClientConnection

    Public Sub New()
        InitializeComponent()
        RichTextBox1.Rtf = My.Resources.BlockFreiWarnung

        ' When the application window is created, we create our new connection class as well.
        MyTCPConnection = New ZusiTcpClientConnection("Zusi TCP Demo 1", ClientPriority.Low)

        ' We need to tell our connection object what measures to request from Zusi. 
        ' You may either use Zusi's native ID code or plain text as listed in the server's commandset.xml
        MyTCPConnection.RequestData("Geschwindigkeit") '2561 . . . . . . . . . . . . .  => FloatReceived
        MyTCPConnection.RequestData(2576) ' "Fahrstufe"  . . . . . . . . . . . . . . .  => FloatReceived
        MyTCPConnection.RequestData(2610) ' "Uhrzeit"  . . . . . . . . . . . . . . . .  => DateTimeReceived
        MyTCPConnection.RequestData(2637) ' "LM Block, bis zu dem die Strecke frei ist" => StringReceived
        MyTCPConnection.RequestData(2649) ' "PZB-System"   . . . . . . . . . . . . . .  => PZBReceived
        MyTCPConnection.RequestData(2594) ' "LM LZB Ü-System"  . . . . . . . . . . . .  => BoolReceived
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

    Private Sub TCPConnection_ErrorReceived(ByVal sender As Object, ByVal ex As ZusiTcpException) _
        Handles MyTCPConnection.ErrorReceived
        ' If something goes wrong...
        ' ... show the user what the connection object has to say.
        System.Windows.Forms.MessageBox.Show(String.Format("An error occured when receiving data: {0}", ex.Message))

        ' ... reset the connection by explicitly calling Disconnect()
        MyTCPConnection.Disconnnect()

        ' ... and then change the button label to "Connect".
        BtnConnect.Text = "Connect"
    End Sub

    Private Sub TCPConnection_BoolReceived(ByVal sender As Object, ByVal data As DataSet(Of Boolean)) _
        Handles MyTCPConnection.BoolReceived
        Select Case data.Id
            Case 2594 ' "LM LZB Ü-System" => BoolReceived
                lblPZB.Enabled = Not data.Value 'PZB off when LZB is on
                lblLZB.Enabled = data.Value 'enable LZB when it's on
            Case Else
                'For unknown IDs...
        End Select
    End Sub

    Private Sub TCPConnection_BrakeConfigReceived(ByVal sender As Object,
                                                  ByVal data As DataSet(Of BrakeConfiguration)) _
        Handles MyTCPConnection.BrakeConfigReceived
        Select Case data.Id
            Case Else
                'We didn't request a BrakeConfig-data
        End Select
    End Sub

    Private Sub TCPConnection_DateTimeReceived(ByVal sender As Object, ByVal data As DataSet(Of Date)) _
        Handles MyTCPConnection.DateTimeReceived
        Select Case data.Id
            Case 2610 ' "Uhrzeit" => DateTimeReceived
                lblUhrzeit.Text = "Uhrzeit: " & data.Value.ToLongTimeString
            Case Else
                'For unknown IDs...
        End Select
    End Sub

    Private Sub TCPConnection_DoorsReceived(ByVal sender As Object,
                                            ByVal data As DataSet(Of DoorState)) _
        Handles MyTCPConnection.DoorsReceived
        Select Case data.Id
            Case Else
                'We didn't request a Doors-data
        End Select
    End Sub

    Private Sub TCPConnection_FloatReceived(ByVal sender As Object, ByVal data As DataSet(Of Single)) _
        Handles MyTCPConnection.FloatReceived
        Select Case data.Id
            Case 2561 ' "Geschwindigkeit" => FloatReceived
                lblGeschw.Text = String.Format("Geschwindigkeit: {0} km/h ", data.Value.ToString("0.00")) 'two decimals
            Case 2576 ' "Fahrstufe" => FloatReceived
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

    Private Sub TCPConnection_PZBReceived(ByVal sender As Object,
                                          ByVal data As DataSet(Of PZBSystem)) _
        Handles MyTCPConnection.PZBReceived
        Select Case data.Id
            Case 2649 ' "PZB-System" => PZBReceived
                Select Case data.Value
                    Case Is = PZBSystem.None, PZBSystem.SBBSignum
                        lblPZB.Text = "PZB: keine"
                        lblLZB.Text = "LZB: keine"
                    Case PZBSystem.PZ80R, PZBSystem.PZB90V15, PZBSystem.PZB90V16
                        lblPZB.Text = "PZB: PZB 90"
                        lblLZB.Text = "LZB: keine"
                    Case PZBSystem.IndusiH54, PZBSystem.IndusiI54, PZBSystem.IndusiI60
                        lblPZB.Text = "PZB: Indusi"
                        lblLZB.Text = "LZB: keine"
                    Case PZBSystem.PZ80
                        lblPZB.Text = "PZB: PZ 80"
                        lblLZB.Text = "LZB: keine"
                    Case PZBSystem.IndusiI60R
                        lblPZB.Text = "PZB: Indusi I60R"
                        lblLZB.Text = "LZB: keine"
                    Case PZBSystem.LZB80I80
                        lblPZB.Text = "PZB: Indusi I60R"
                        lblLZB.Text = "LZB: LZB 80"
                    Case Else
                        lblPZB.Text = "PZB: Zustand nicht erkannt"
                        lblLZB.Text = "LZB: Zustand nicht erkannt"
                End Select
            Case Else
                'For unknown IDs...
        End Select
    End Sub

    Private Sub TCPConnection_StringReceived(ByVal sender As Object, ByVal data As DataSet(Of String)) _
        Handles MyTCPConnection.StringReceived
        Select Case data.Id
            Case 2637 ' "LM Block, bis zu dem die Strecke frei ist" => StringReceived
                lblFreiBis.Text = "Frei bis: " & data.Value
            Case Else
                'For unknown IDs...
        End Select
    End Sub
End Class
