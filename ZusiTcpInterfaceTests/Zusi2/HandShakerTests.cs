using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface.Zusi2;

namespace ZusiTcpInterfaceTests.Zusi2
{
  [TestClass]
  public class HandShakerTests
  {
    private readonly HandShaker _handShaker;

    private readonly MemoryStream _rxStream = new MemoryStream();
    private readonly MemoryStream _txStream = new MemoryStream();

    public HandShakerTests()
    {
      var binaryReader = new BinaryReader(_rxStream);
      var binaryWriter = new BinaryWriter(_txStream);

      _handShaker = new HandShaker(binaryReader, binaryWriter);
    }

    [TestMethod]
    public void Sends_Hello()
    {
      // Given
      var clientType = ClientType.PASystem;
      var clientName = "Handschäke!";

      var expectedPacket = new HelloPacket(clientType, clientName);

      var serialisedExpectedPacket = new MemoryStream();       
      expectedPacket.Serialise(new BinaryWriter(serialisedExpectedPacket));

      // When
      _handShaker.ShakeHands(clientType, clientName);

      // Then
      CollectionAssert.AreEqual(serialisedExpectedPacket.ToArray(), _txStream.ToArray());
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
