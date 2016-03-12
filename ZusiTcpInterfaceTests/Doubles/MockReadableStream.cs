using System.IO;
using Moq;
using ZusiTcpInterface.Common;

namespace ZusiTcpInterfaceTests.Doubles
{
  internal class MockReadableStream
  {
    private MemoryStream _dataToRead = new MemoryStream();
    private readonly IReadableStream _stream;

    public MockReadableStream()
    {
      var mockReadableStream = new Mock<IReadableStream>();

      mockReadableStream.Setup(
        stream =>
          stream.Read(It.IsAny<int>()))
        .Returns((int length) => Read(length));

      _stream = mockReadableStream.Object;
    }

    private byte[] Read(int length)
    {
      byte[] data = new byte[length];
      _dataToRead.Read(data, 0, length);

      return data;
    }

    public IReadableStream Stream
    {
      get { return _stream; }
    }

    public void SetDataToRead(byte[] serialisedAckHello)
    {
      _dataToRead = new MemoryStream(serialisedAckHello);
    }
  }
}