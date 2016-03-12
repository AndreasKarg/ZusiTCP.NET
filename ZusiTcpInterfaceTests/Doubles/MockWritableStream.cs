using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using ZusiTcpInterface.Common;

namespace ZusiTcpInterfaceTests.Doubles
{
  internal class MockWritableStream
  {
    private readonly MemoryStream _writtenData = new MemoryStream();
    private readonly IWritableStream _stream;

    public MockWritableStream()
    {
      var mockWritableStream = new Mock<IWritableStream>();

      mockWritableStream.Setup(
        stream =>
          stream.Write(It.IsNotNull<IEnumerable<byte>>()))
          .Callback<IEnumerable<byte>>(Write);

      _stream = mockWritableStream.Object;
    }

    void Write(IEnumerable<byte> data)
    {
      var dataAsArray = data.ToArray();
      _writtenData.Write(dataAsArray, 0, dataAsArray.Length);
    }

    public IWritableStream Stream
    {
      get { return _stream; }
    }

    public byte[] WrittenData
    {
      get { return _writtenData.ToArray(); }
    }
  }
}