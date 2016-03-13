using System.IO;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestExtensions;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterfaceTests.Helpers;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class HandshakerTests : BaseTest
  {
    private readonly byte[] _positiveAckHello =
    {
      0x00, 0x00, 0x00, 0x00,
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

    private readonly Handshaker _handshaker;
    private readonly MemoryStream _rxStream;
    private readonly MemoryStream _txStream = new MemoryStream();

    public HandshakerTests()
    {
      _rxStream = new MemoryStream(_positiveAckHello);
      var binaryReader = new BinaryReader(_rxStream);

      var binaryWriter = new BinaryWriter(_txStream);

      _handshaker = new Handshaker(binaryReader, binaryWriter, ClientType.ControlDesk, "Fahrpult", "2.0");
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
      byte[] ackHelloThatRefusesConnection =
      {
        0x00, 0x00, 0x00, 0x00,
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
              0x13,
          0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF
      };

      _rxStream.ReinitialiseWith(ackHelloThatRefusesConnection);

      // When - Throws
      Assert.Throws<ConnectionRefusedException>(_handshaker.ShakeHands);
    }

    [TestMethod, Ignore]
    public void Connects_to_real_Zusi()
    {
      // Use this test while debugging to play around

      using (var tcpClient = new TcpClient("localhost", 1436))
      {
        //var networkStream = new EncapsulatedNetworkStream(tcpClient.GetStream());
        var binaryReader = new BinaryReader(tcpClient.GetStream());
        var binaryWriter = new BinaryWriter(tcpClient.GetStream());

        var handshaker = new Handshaker(binaryReader, binaryWriter, ClientType.ControlDesk, "Andi", "1.2.3");
        handshaker.ShakeHands();
      }
    }
  }
}
