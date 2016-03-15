using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ZusiTcpInterface.Zusi3;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class MessageReceiverTests
  {
    private readonly MessageReceiver _messageReceiver;

    readonly byte[] _samplePacket =
         { 0x00, 0x00, 0x00, 0x00,
           0x01, 0x00,
             0x00, 0x00, 0x00, 0x00,
               0x02, 0x00,
               0x09, 0x00, 0x00, 0x00,
                 0x01, 0x00,
                 0x33, 0x2E, 0x30, 0x2E, 0x31, 0x2E, 0x30,
               0x03, 0x00, 0x00, 0x00,
                 0x02, 0x00,
                 0x30,
               0x03, 0x00, 0x00, 0x00,
                 0x03, 0x00,
                 0x00,
             0xFF, 0xFF, 0xFF, 0xFF,
           0xFF, 0xFF, 0xFF, 0xFF
         };

    private readonly List<IProtocolChunk> _protocolChunks = new List<IProtocolChunk>();

    public MessageReceiverTests()
    {
      var serverStream = new MemoryStream(_samplePacket);
      var binaryReader = new BinaryReader(serverStream);

      var handshakeConverter = new BranchingNodeConverter();
      handshakeConverter[0x02] = new AckHelloConverter();

      var rootNodeConverter = new TopLevelNodeConverter();
      rootNodeConverter[0x01] = handshakeConverter;

      var mockQueue = new Mock<IBlockingCollection<IProtocolChunk>>();
      mockQueue.Setup(queue => queue.Add(It.IsNotNull<IProtocolChunk>()))
        .Callback<IProtocolChunk>(chunk => _protocolChunks.Add(chunk));

      _messageReceiver = new MessageReceiver(binaryReader, rootNodeConverter, mockQueue.Object);
    }

    [TestMethod]
    public void Puts_deserialised_message_onto_queue()
    {
      // Given
      // Message receiver as above

      // When
      _messageReceiver.ProcessNextPacket();

      // Then
      var ackHello = (AckHelloPacket)_protocolChunks.Single();

      Assert.AreEqual("3.0.1.0", ackHello.ZusiVersion);
      Assert.AreEqual("0", ackHello.ConnectionInfo);
      Assert.IsTrue(ackHello.ConnectionAccepted);
    }
  }
}