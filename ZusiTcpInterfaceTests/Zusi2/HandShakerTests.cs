using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ZusiTcpInterface.Zusi2;

namespace ZusiTcpInterfaceTests.Zusi2
{
  [TestClass]
  public class HandShakerTests
  {
    private readonly HandShaker _handShaker;

    private IEnumerable<byte> _writtenData = Enumerable.Empty<byte>();
    private readonly MockReadableStream _mockReadableStream = new MockReadableStream();

    public HandShakerTests()
    {
      var writableStream = SetupMockWritableStream();

      _handShaker = new HandShaker(_mockReadableStream.Stream, writableStream);
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
      Assert.Inconclusive("Lacking AckHello packet");

      // When
      

      // Then
    }
  }
}
