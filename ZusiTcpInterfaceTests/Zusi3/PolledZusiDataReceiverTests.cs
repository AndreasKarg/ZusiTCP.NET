using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSTestExtensions;
using System;
using System.Collections.Generic;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class PolledZusiDataReceiverTests : BaseTest
  {
    private readonly Queue<DataChunkBase> _cabDataChunks = new Queue<DataChunkBase>();
    private readonly PolledZusiDataReceiver _polledZusiDataReceiver;
    private readonly AddressBasedAttributeDescriptor _floatDescriptor = new AddressBasedAttributeDescriptor(new CabInfoAddress(1), "Float", "Float", "N/A", "N/A");
    private readonly AddressBasedAttributeDescriptor _boolDescriptor = new AddressBasedAttributeDescriptor(new CabInfoAddress(2), "Bool", "Bool", "N/A", "N/A");

    public PolledZusiDataReceiverTests()
    {
      var mockQueue = new Mock<IBlockingCollection<DataChunkBase>>();
      mockQueue.Setup(mock => mock.Take())
        .Returns(() => _cabDataChunks.Dequeue());

      mockQueue.Setup(mock => mock.Count)
        .Returns(() => _cabDataChunks.Count);

      _polledZusiDataReceiver = new PolledZusiDataReceiver(mockQueue.Object);
    }

    [TestMethod]
    public void Calls_correct_callbacks_when_supplied_with_data_chunks()
    {
      // Given
      float? lastReceivedFloat = null;
      Address lastReceivedFloatId = null;

      bool? lastReceivedBool = null;
      Address lastReceivedBoolId = null;

      var boolReceived = new Action<DataChunk<bool>>(args =>
      {
        lastReceivedBool = args.Payload;
        lastReceivedBoolId = args.Address;
      });

      var floatReceived = new Action<DataChunk<float>>(args =>
      {
        lastReceivedFloat = args.Payload;
        lastReceivedFloatId = args.Address;
      });

      const float expectedFloat = 3.0f;
      const bool expectedBool = true;
      var floatAddress = _floatDescriptor.Address;
      var boolAddress = _boolDescriptor.Address;

      _polledZusiDataReceiver.RegisterCallbackFor(floatAddress, floatReceived);
      _polledZusiDataReceiver.RegisterCallbackFor(boolAddress, boolReceived);

      _cabDataChunks.Enqueue(new DataChunk<float>(floatAddress, expectedFloat));
      _cabDataChunks.Enqueue(new DataChunk<bool>(boolAddress, expectedBool));

      // When
      _polledZusiDataReceiver.Service();

      // Then
      Assert.AreEqual(expectedFloat, lastReceivedFloat);
      Assert.AreEqual(floatAddress, lastReceivedFloatId);

      Assert.AreEqual(expectedBool, lastReceivedBool);
      Assert.AreEqual(boolAddress, lastReceivedBoolId);
    }

    [TestMethod]
    public void Throws_ArgumentException_when_another_callback_for_same_address_is_registered()
    {
      // Given
      var floatAddress = _floatDescriptor.Address;

      _polledZusiDataReceiver.RegisterCallbackFor<float>(floatAddress, chunk => { });

      // When - Throws
      Assert.Throws<ArgumentException>(() => _polledZusiDataReceiver.RegisterCallbackFor<float>(floatAddress, chunk => { }));
    }
  }
}