using System.IO;
using System.IO.Pipes;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly BlockingCollectionWrapper<IProtocolChunk> _blockingChunkQueue = new BlockingCollectionWrapper<IProtocolChunk>();

    public MessageReceiverTests()
    {
      var serverStream = new AnonymousPipeServerStream(PipeDirection.Out);
      var clientStream = new AnonymousPipeClientStream(PipeDirection.In, serverStream.ClientSafePipeHandle);
      var cancellableStream = new CancellableBlockingStream(clientStream, _cancellationTokenSource.Token);
      var binaryReader = new BinaryReader(cancellableStream);

      serverStream.Write(_samplePacket, 0, _samplePacket.Length);

      var handshakeConverter = new BranchingNodeConverter();
      handshakeConverter[0x02] = new AckHelloConverter();

      var rootNodeConverter = new TopLevelNodeConverter();
      rootNodeConverter[0x01] = handshakeConverter;

      _messageReceiver = new MessageReceiver(binaryReader, rootNodeConverter, _blockingChunkQueue);
    }

    [TestMethod]
    public void Reacts_to_cancellation_token_within_time_limit()
    {
      // Given
      var task = _messageReceiver.StartReceptionLoop();

      // When
      _cancellationTokenSource.Cancel(false);

      // Then
      Assert.IsTrue(task.Wait(500));
    }

    [TestMethod]
    public void Puts_deserialised_message_onto_queue()
    {
      // Given
      _messageReceiver.ProcessNextPacket();

      // When
      IProtocolChunk item;
      var itemTaken = _blockingChunkQueue.TryTake(out item, 500);

      // Then
      Assert.IsTrue(itemTaken);
      var ackHello = item as AckHelloPacket;
      Assert.IsNotNull(ackHello);

      Assert.AreEqual("3.0.1.0", ackHello.ZusiVersion);
      Assert.AreEqual("0", ackHello.ConnectionInfo);
      Assert.IsTrue(ackHello.ConnectionAccepted);
    }
  }
}