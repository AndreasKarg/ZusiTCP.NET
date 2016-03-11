using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface;

namespace ZusiTcpInterfaceTests
{
  [TestClass]
  public class HelloPacketTests
  {
    [TestMethod]
    public void Serialised_Hello_matches_specification()
    {
      // Given
      byte[] expected = {0x0d, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x02, 0x08, 0x46, 0x61, 0x68, 0x72, 0x70, 0x75, 0x6C, 0x74};

      var helloPacket = new HelloPacket(ClientType.ControlDesk, "Fahrpult");

      // When
      var serialised = helloPacket.Serialise().ToArray();

      CollectionAssert.AreEqual(expected, serialised);
    }
  }
}
