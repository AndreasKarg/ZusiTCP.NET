using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ZusiTcpInterface;
using ZusiTcpInterface.Converters;
using ZusiTcpInterface.Packets;

namespace ZusiTcpInterfaceTests
{
  [TestClass]
  public class MessageReceiverTests
  {
    private readonly MessageReceiver _messageReceiver;

    private readonly byte[] _samplePacket =
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

    public MessageReceiverTests()
    {
      var serverStream = new MemoryStream(_samplePacket);
      var binaryReader = new BinaryReader(serverStream);

      var handshakeConverter = new NodeConverter();
      handshakeConverter.SubNodeConverters[0x02] = new AckHelloConverter();

      var rootNodeConverter = new RootNodeConverter();
      rootNodeConverter[0x01] = handshakeConverter;

      _messageReceiver = new MessageReceiver(binaryReader, rootNodeConverter);
    }

    [TestMethod]
    public void Returns_correct_next_chunk()
    {
      // Given
      // Message receiver as above

      // When
      var ackHello = (AckHelloPacket)_messageReceiver.GetNextChunk();

      // Then
      Assert.AreEqual("3.0.1.0", ackHello.ZusiVersion);
      Assert.AreEqual("0", ackHello.ConnectionInfo);
      Assert.IsTrue(ackHello.ConnectionAccepted);
    }
  }
}