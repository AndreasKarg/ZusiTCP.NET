using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Zusi_Datenausgabe
{
  public class NetworkIOHandler
  {
    private TcpClient _clientConnection;
    private NetworkStream _clientStream;
    private BinaryReader _clientReader;

    public NetworkIOHandler()
    {
    }

    public BinaryReader ClientReader
    {
      get { return _clientReader; }
    }

    private void SendToServer(byte[] message)
    {
      try
      {
        _clientStream.Write(message, 0, message.Length);
      }
      catch (IOException ex)
      {
        throw new ZusiTcpException("An error occured when trying to send data to the server.", ex);
      }
    }

    public void SendPacket(params byte[] message)
    {
      SendToServer(BitConverter.GetBytes(message.Length));
      SendToServer(message);
    }

    public void SendPacket(params byte[][] message)
    {
      int iTempLength = message.Sum(item => item.Length);

      SendToServer(BitConverter.GetBytes(iTempLength));

      foreach (var item in message)
      {
        SendToServer(item);
      }
    }

    public void EstablishConnection(IPEndPoint endPoint)
    {
      if (_clientConnection == null)
      {
        _clientConnection = new TcpClient(AddressFamily.InterNetwork);
      }

      try
      {
        _clientConnection.Connect(endPoint);
      }
      catch (SocketException ex)
      {
        throw new ZusiTcpException("Could not establish socket connection to TCP server. " +
                                   "Is the server running and enabled?",
          ex);
      }

      Debug.Assert(_clientConnection.Connected);

      _clientStream = _clientConnection.GetStream();
      _clientReader = new BinaryReader(_clientStream);
    }

    public void Disconnect()
    {
      if (_clientConnection != null)
      {
        _clientConnection.Close();
      }
      _clientConnection = null;
    }
  }
}