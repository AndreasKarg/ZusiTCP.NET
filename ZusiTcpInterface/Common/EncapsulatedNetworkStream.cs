using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace ZusiTcpInterface.Common
{
  public class EncapsulatedNetworkStream : IReadableStream, IWritableStream
  {
    private readonly NetworkStream _networkStream;

    public EncapsulatedNetworkStream(NetworkStream networkStream)
    {
      _networkStream = networkStream;
    }

    public byte[] Read(int i)
    {
      var buffer = new byte[i];
      _networkStream.Read(buffer, 0, i);

      return buffer;
    }

    public void Write(IEnumerable<byte> data)
    {
      var buffer = data.ToArray();
      _networkStream.Write(buffer, 0, buffer.Length);
    }
  }
}