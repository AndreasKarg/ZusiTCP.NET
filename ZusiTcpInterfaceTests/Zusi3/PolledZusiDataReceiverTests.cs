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
    private readonly AttributeDescriptor _floatDescriptor = new AttributeDescriptor(new CabInfoAddress(1), "Float", "Float", "N/A", "N/A");
    private readonly AttributeDescriptor _boolDescriptor = new AttributeDescriptor(new CabInfoAddress(2), "Bool", "Bool", "N/A", "N/A");
    private readonly AttributeDescriptor _stringDescriptor = new AttributeDescriptor(new CabInfoAddress(3), "String", "String", "N/A", "N/A");

    public PolledZusiDataReceiverTests()
    {
      var mockQueue = new Mock<IBlockingCollection<DataChunkBase>>();
      mockQueue.Setup(mock => mock.Take())
        .Returns(() => _cabDataChunks.Dequeue());

      mockQueue.Setup(mock => mock.Count)
        .Returns(() => _cabDataChunks.Count);

      var descriptors = new DescriptorCollection(new []{_floatDescriptor, _boolDescriptor, _stringDescriptor});

      _polledZusiDataReceiver = new PolledZusiDataReceiver(descriptors, mockQueue.Object);
    }

    [TestMethod]
    public void Calls_correct_callbacks_when_supplied_with_data_chunks()
    {
      // Given
      float? lastReceivedFloat = null;
      Address lastReceivedFloatId = null;

      bool? lastReceivedBool = null;
      Address lastReceivedBoolId = null;

      string lastReceivedString = null;
      Address lastReceivedStringId = null;

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

      var stringReceived = new Action<DataChunk<string>>(args =>
      {
        lastReceivedString = args.Payload;
        lastReceivedStringId = args.Address;
      });

      const float expectedFloat = 3.0f;
      const bool expectedBool = true;
      string expectedString = "Test";

      var floatAddress = _floatDescriptor.Address;
      var boolAddress = _boolDescriptor.Address;
      var stringAddress = _stringDescriptor.Address;

      _polledZusiDataReceiver.RegisterCallbackFor(_floatDescriptor, floatReceived);
      _polledZusiDataReceiver.RegisterCallbackFor(boolAddress, boolReceived);
      _polledZusiDataReceiver.RegisterCallbackFor("String", stringReceived);

      _cabDataChunks.Enqueue(new DataChunk<float>(floatAddress, expectedFloat));
      _cabDataChunks.Enqueue(new DataChunk<bool>(boolAddress, expectedBool));
      _cabDataChunks.Enqueue(new DataChunk<string>(stringAddress, expectedString));

      // When
      _polledZusiDataReceiver.Service();

      // Then
      Assert.AreEqual(expectedFloat, lastReceivedFloat);
      Assert.AreEqual(floatAddress, lastReceivedFloatId);

      Assert.AreEqual(expectedBool, lastReceivedBool);
      Assert.AreEqual(boolAddress, lastReceivedBoolId);

      Assert.AreEqual(expectedString, lastReceivedString);
      Assert.AreEqual(stringAddress, lastReceivedStringId);
    }

    [TestMethod]
    public void Throws_ArgumentException_when_another_callback_for_same_address_is_registered()
    {
      // Given
      var floatAddress = _floatDescriptor.Address;

      _polledZusiDataReceiver.RegisterCallbackFor<float>(floatAddress, chunk => { });

      // When - Throws
      Assert.Throws<ArgumentException>(() => _polledZusiDataReceiver.RegisterCallbackFor<float>(floatAddress, chunk => { }));
      Assert.Throws<ArgumentException>(() => _polledZusiDataReceiver.RegisterCallbackFor<float>(_floatDescriptor, chunk => { }));
      Assert.Throws<ArgumentException>(() => _polledZusiDataReceiver.RegisterCallbackFor<float>("Float", chunk => { }));
    }
  }
}