using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSTestExtensions;
using ZusiTcpInterface.Zusi3;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class HandshakerTests : BaseTest
  {
    private readonly AckHelloPacket _positiveAckHello = new AckHelloPacket("3.1.0.0.", "0", true);
    
    private readonly Handshaker _handshaker;
    private readonly Mock<IBlockingCollection<IProtocolChunk>> _rxQueue = new Mock<IBlockingCollection<IProtocolChunk>>();
    private readonly MemoryStream _txStream = new MemoryStream();

    public HandshakerTests()
    {
      var binaryWriter = new BinaryWriter(_txStream);

      _rxQueue.Setup(queue => queue.Take())
        .Returns(_positiveAckHello);

      _handshaker = new Handshaker(_rxQueue.Object, binaryWriter, ClientType.ControlDesk, "Fahrpult", "2.0");
    }

    [TestMethod]
    public void Sends_correct_hello_packet()
    {
      // Given
      byte[] expectedTxData
        = {0x00, 0x00, 0x00, 0x00,
           0x01, 0x00,
              0x00, 0x00, 0x00, 0x00,
              0x01, 0x00,
                0x04, 0x00, 0x00, 0x00,
                  0x01, 0x00,
                  0x02, 0x00,
                0x04, 0x00, 0x00, 0x00,
                  0x02, 0x00,
                  0x02, 0x00,
                0x0A, 0x00, 0x00, 0x00,
                  0x03, 0x00,
                  0x46, 0x61, 0x68, 0x72, 0x70, 0x75, 0x6C, 0x74,
                0x05, 0x00, 0x00, 0x00,
                  0x04, 0x00,
                  0x32, 0x2E, 0x30,
              0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF};

      // When
      _handshaker.ShakeHands();

      // Then
      CollectionAssert.AreEqual(expectedTxData, _txStream.ToArray());
    }

    [TestMethod]
    public void Throws_exception_when_confronted_with_refused_connection()
    {
      // Given
      var negativeAckHello = new AckHelloPacket("3.1.0.0.", "0", false);
      _rxQueue.Setup(queue => queue.Take())
        .Returns(negativeAckHello);

      // When - Throws
      Assert.Throws<ConnectionRefusedException>(_handshaker.ShakeHands);
    }
/*
    [TestMethod, Ignore]
    public void Connects_to_real_Zusi()
    {
      // Use this test while debugging to play around

      using (var tcpClient = new TcpClient("localhost", 1436))
      {
        var binaryWriter = new BinaryWriter(tcpClient.GetStream());

        var handshaker = new Handshaker(_rxQueue, binaryWriter, ClientType.ControlDesk, "Andi", "1.2.3");
        handshaker.ShakeHands();
      }
    }*/
  }
}
