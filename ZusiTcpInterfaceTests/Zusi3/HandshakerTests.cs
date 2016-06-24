using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSTestExtensions;
using System.Collections.Generic;
using System.IO;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.DOM;
using ZusiTcpInterface.Zusi3.Packets;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class HandshakerTests : BaseTest
  {
    private readonly AckHelloPacket _positiveAckHello = new AckHelloPacket("3.1.0.0.", "0", true);
    private readonly AckNeededDataPacket _positiveAckNeededDataPacket = new AckNeededDataPacket(true);

    private readonly List<CabInfoAddress> _neededData = new List<CabInfoAddress>
      {
        new CabInfoAddress(0x01),
        new CabInfoAddress(0x1B, 0x1C)
      };

    private readonly Handshaker _handshaker;
    private readonly Mock<IBlockingCollection<IProtocolChunk>> _rxQueue = new Mock<IBlockingCollection<IProtocolChunk>>();
    private readonly MemoryStream _txStream = new MemoryStream();
    private readonly BinaryWriter _binaryWriter;

    public HandshakerTests()
    {
      var binaryWriter = new BinaryWriter(_txStream);

      var rxPackets = new Queue<IProtocolChunk>();
      rxPackets.Enqueue(_positiveAckHello);
      rxPackets.Enqueue(_positiveAckNeededDataPacket);

      _rxQueue.Setup(queue => queue.Take())
        .Returns(rxPackets.Dequeue);

      _binaryWriter = binaryWriter;
      _handshaker = new Handshaker(_rxQueue.Object, _binaryWriter, ClientType.ControlDesk, "Fahrpult", "2.0", _neededData);
    }

    [TestMethod]
    public void Performs_valid_handshake()
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
              0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF,
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00,
              0x00, 0x00, 0x00, 0x00,
              0x03, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x0A, 0x00,
                  0x04, 0x00, 0x00, 0x00,
                    0x01, 0x00,
                    0x01, 0x00,
                  0x04, 0x00, 0x00, 0x00,
                    0x01, 0x00,
                    0x1B, 0x00,
                0xFF, 0xFF, 0xFF, 0xFF,
              0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF};

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

    [TestMethod]
    public void Throws_exception_when_simulator_does_not_acknowledge_needed_data()
    {
      // Given
      var receivedPackets = new Queue<IProtocolChunk>();
      receivedPackets.Enqueue(_positiveAckHello);
      receivedPackets.Enqueue(new AckNeededDataPacket(false));

      _rxQueue.Setup(queue => queue.Take())
        .Returns(receivedPackets.Dequeue);

      // When - Throws
      Assert.Throws<ConnectionRefusedException>(_handshaker.ShakeHands);
    }

    [TestMethod]
    public void Ignores_duplicate_ids_in_needed_data()
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
              0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF,
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00,
              0x00, 0x00, 0x00, 0x00,
              0x03, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x0A, 0x00,
                  0x04, 0x00, 0x00, 0x00,
                    0x01, 0x00,
                    0x01, 0x00,
                  0x04, 0x00, 0x00, 0x00,
                    0x01, 0x00,
                    0x1B, 0x00,
                0xFF, 0xFF, 0xFF, 0xFF,
              0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF};

      List<CabInfoAddress> neededData = new List<CabInfoAddress>
      {
        new CabInfoAddress(0x01),
        new CabInfoAddress(0x1B, 0x01),
        new CabInfoAddress(0x1B, 0x02),
        new CabInfoAddress(0x01)
      };

      var handshaker = new Handshaker(_rxQueue.Object, _binaryWriter, ClientType.ControlDesk, "Fahrpult", "2.0", neededData);

      // When
      handshaker.ShakeHands();

      // Then
      CollectionAssert.AreEqual(expectedTxData, _txStream.ToArray());
    }
  }
}