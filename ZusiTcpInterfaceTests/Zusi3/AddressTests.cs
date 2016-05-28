using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface.Zusi3;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class AddressTests
  {
    [TestMethod]
    public void Identical_addresses_are_treated_as_equal()
    {
      // Given
      var address1 = new Address(1, 2, 3);
      var address2 = new Address(1, 2, 3);

      // When
      var areEqual = address1 == address2;
      var areInequal = address1 != address2;

      // Then
      Assert.IsTrue(areEqual);
      Assert.IsFalse(areInequal);
      Assert.AreEqual(address1, address2);
      Assert.IsTrue(address1.Equals(address2));
    }
  }
}