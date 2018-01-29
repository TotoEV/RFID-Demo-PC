' Arduino and Visual Basic: Receiving Data From the Arduino
' A simple example of recing data from an Arduino
'

Imports System.IO
Imports System.IO.Ports
Imports System.Net
Imports System.Text

Public Class Form1

    Dim comPORT As String
    Dim receivedData As String = ""
    Dim Card1Charging As String = "Stopped"
    Dim Card2Charging As String = "Stopped"
    Dim CardIDNumber As Integer
    Dim AccessToken As String = "174303fb4870ed9400655d3f086ee990"



    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls Or SecurityProtocolType.Tls11 Or SecurityProtocolType.Tls12
        Timer1.Enabled = False
        comPORT = ""
        For Each sp As String In My.Computer.Ports.SerialPortNames
            comPort_ComboBox.Items.Add(sp)
        Next
    End Sub


    Private Sub comPort_ComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles comPort_ComboBox.SelectedIndexChanged
        If (comPort_ComboBox.SelectedItem <> "") Then
            comPORT = comPort_ComboBox.SelectedItem
        End If
    End Sub


    Private Sub connect_BTN_Click(sender As Object, e As EventArgs) Handles connect_BTN.Click
        If (connect_BTN.Text = "Connect") Then
            If (comPORT <> "") Then
                SerialPort1.Close()
                SerialPort1.PortName = comPORT
                SerialPort1.BaudRate = 9600
                SerialPort1.DataBits = 8
                SerialPort1.Parity = Parity.None
                SerialPort1.StopBits = StopBits.One
                SerialPort1.Handshake = Handshake.None
                SerialPort1.Encoding = System.Text.Encoding.Default 'very important!
                SerialPort1.ReadTimeout = 10000

                SerialPort1.Open()
                connect_BTN.Text = "Dis-connect"
                Timer1.Enabled = True
                Timer_LBL.Text = "Timer: ON"
            Else
                MsgBox("Select a COM port first")
            End If
        Else
            SerialPort1.Close()
            connect_BTN.Text = "Connect"
            Timer1.Enabled = False
            Timer_LBL.Text = "Timer: OFF"
        End If


    End Sub


    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        receivedData = ReceiveSerialData()
        RichTextBox1.Text &= receivedData

        If receivedData.Contains("Network: NRMA") And receivedData.Contains("Card ID: 8RPCI-4QKIL-J1DC") Then
            If Card1Charging = "Stopped" Then
                Card1Charging = "Started"
            ElseIf Card1Charging = "Started" Then
                Card1Charging = "Stopped"
            End If
            CardIDNumber = 3
            API(ChargingState:=Card1Charging, CardID:="8RPCI-4QKIL-J1DC", Network:="NRMA")
        ElseIf receivedData.Contains("Network: ActewAGL") And receivedData.Contains("Card ID: 6L69O-12D68-3N3L") Then
            If Card2Charging = "Stopped" Then
                Card2Charging = "Started"
            ElseIf Card2Charging = "Started" Then
                Card2Charging = "Stopped"
            End If
            CardIDNumber = 2
            API(ChargingState:=Card2Charging, CardID:="6L69O-12D68-3N3L", Network:="ActewAGL")
        End If



    End Sub


    Function ReceiveSerialData() As String
        Dim Incoming As String
        Try
            Incoming = SerialPort1.ReadExisting()
            If Incoming Is Nothing Then
                Return "nothing" & vbCrLf
            Else
                Return Incoming
            End If
        Catch ex As TimeoutException
            Return "Error: Serial Port read timed out."
        End Try

    End Function



    Private Sub clear_BTN_Click(sender As Object, e As EventArgs) Handles clear_BTN.Click
        RichTextBox1.Text = ""
    End Sub

    Public Function API(ChargingState As String, CardID As String, Network As String) As String
        Dim BodyText As String = Nothing
        Dim InUse As Integer = 0
        MsgBox(ChargingState)
        If ChargingState = "Stopped" Then
            InUse = 0
        ElseIf ChargingState = "Started" Then
            InUse = 1
        End If

        BodyText = "{""in_use"":" & InUse & ",""assigned_to"": 4}"
        MsgBox(BodyText)
        'convert text into ascii
        Dim encoding As New ASCIIEncoding()
        Dim Bytes As Byte() = encoding.GetBytes(BodyText)
        Dim request = TryCast(System.Net.WebRequest.Create("https://server.totoev.com.au/api/cards/" & CardIDNumber), System.Net.HttpWebRequest)

        request.Method = "PUT"
        request.Headers.Add("X-Access-Token", AccessToken)
        request.ContentType = "application/json"
        request.ContentLength = Bytes.Length

        'write the info into the json body
        Dim Stream = request.GetRequestStream()
        Stream.Write(Bytes, 0, Bytes.Length)
        Stream.Close()


        Dim responseContent As String
        Using response = TryCast(request.GetResponse(), System.Net.HttpWebResponse)
            Using reader = New System.IO.StreamReader(response.GetResponseStream())
                responseContent = reader.ReadToEnd()
                MessageBox.Show(responseContent)
            End Using
        End Using
        Return True
    End Function

End Class
