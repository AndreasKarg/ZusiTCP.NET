using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ZusiTcpInterface;

namespace ZusiTcpInterfaceTests
{
  [TestClass]
  public class HandShakerTests
  {
    private readonly HandShaker _handShaker;

    private IEnumerable<byte> _writtenData = Enumerable.Empty<byte>();

    public HandShakerTests()
    {
      var readableStream = SetupMockReadableStream();
      var writableStream = SetupMockWritableStream();

      _handShaker = new HandShaker(readableStream, writableStream);
    }

    private static IReadableStream SetupMockReadableStream()
    {
      var mockReadableStream = new Mock<IReadableStream>();

      return mockReadableStream.Object;
    }

    private IWritableStream SetupMockWritableStream()
    {
      var mockWritableStream = new Mock<IWritableStream>();

      mockWritableStream.Setup(stream => stream.Write(It.IsNotNull<IEnumerable<byte>>()))
        .Callback<IEnumerable<byte>>(data => _writtenData = _writtenData.Concat(data));

      return mockWritableStream.Object;
    }

    [TestMethod]
    public void Sends_Hello()
    {
      // Given
      var clientType = ClientType.PASystem;
      var clientName = "Handschäke!";

      var expectedPacket = new HelloPacket(clientType, clientName);
      var serialisedExpectedPacket = expectedPacket.Serialise().ToArray();

      // When
      _handShaker.ShakeHands(clientType, clientName);

      // Then
      CollectionAssert.AreEqual(serialisedExpectedPacket, _writtenData.ToArray());
    }

    [TestMethod]
    public void Throws_exception_when_connection_is_refused()
    {
      // Given
       

      // When
      

      // Then
    }
  }
}
