using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class DescriptorTests
  {
    [TestMethod]
    public void Equal_descriptors_are_considered_equal()
    {
      // Given
      var attributesA = new[]
      {
        new AttributeDescriptor(new CabInfoAddress(0x01), "Geschwindigkeit", "Geschwindigkeit", "m/s", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x02), "Druck Hauptluftleitung", "Druck Hauptluftleitung", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x03), "Test:Druck Bremszylinder", "Druck Bremszylinder", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x04), "Test:Druck Hauptluftbehälter", "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x05), "Test:Luftpresser läuft", "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x06), "Test2:Luftstrom Fbv", "Luftstrom Fbv", "-1...0...1", "Fail"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x07), "Test2:Luftstrom Zbv", "Luftstrom Zbv", "-1...0...1", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x08), "Test2:Lüfter an", "Lüfter an", "aus/an", "BoolAsSingle")
      };

      var attributesB = new[]
      {
        new AttributeDescriptor(new CabInfoAddress(0x01), "Geschwindigkeit", "Geschwindigkeit", "m/s", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x02), "Druck Hauptluftleitung", "Druck Hauptluftleitung", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x03), "Test:Druck Bremszylinder", "Druck Bremszylinder", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x04), "Test:Druck Hauptluftbehälter", "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x05), "Test:Luftpresser läuft", "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x06), "Test2:Luftstrom Fbv", "Luftstrom Fbv", "-1...0...1", "Fail"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x07), "Test2:Luftstrom Zbv", "Luftstrom Zbv", "-1...0...1", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x08), "Test2:Lüfter an", "Lüfter an", "aus/an", "BoolAsSingle")
      };

      var a = new DescriptorCollection(attributesA);
      var b = new DescriptorCollection(attributesB);

      // When
      var areEqual = a == b;
      var areInequal = a != b;
      var explicitEquals = a.Equals(b);
      var objectEquals = a.Equals((object)b);

      // Then
      Assert.IsTrue(areEqual);
      Assert.IsFalse(areInequal);
      Assert.IsTrue(explicitEquals);
      Assert.IsTrue(objectEquals);
    }

    [TestMethod]
    public void Top_level_inequality_treated_as_inequal()
    {
      // Given
      var attributesA = new[]
      {
        new AttributeDescriptor(new CabInfoAddress(0x01), "Geschwindigkeit", "Geschwindigkeit", "m/sx", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x02), "Druck Hauptluftleitung", "Druck Hauptluftleitung", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x03), "Test:Druck Bremszylinder", "Druck Bremszylinder", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x04), "Test:Druck Hauptluftbehälter", "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x05), "Test:Luftpresser läuft", "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x06), "Test2:Luftstrom Fbv", "Luftstrom Fbv", "-1...0...1", "Fail"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x07), "Test2:Luftstrom Zbv", "Luftstrom Zbv", "-1...0...1", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x08), "Test2:Lüfter an", "Lüfter an", "aus/an", "BoolAsSingle")
      };

      var attributesB = new[]
      {
        new AttributeDescriptor(new CabInfoAddress(0x01), "Geschwindigkeit", "Geschwindigkeit", "m/s", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x02), "Druck Hauptluftleitung", "Druck Hauptluftleitung", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x03), "Test:Druck Bremszylinder", "Druck Bremszylinder", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x04), "Test:Druck Hauptluftbehälter", "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x05), "Test:Luftpresser läuft", "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x06), "Test2:Luftstrom Fbv", "Luftstrom Fbv", "-1...0...1", "Fail"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x07), "Test2:Luftstrom Zbv", "Luftstrom Zbv", "-1...0...1", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x08), "Test2:Lüfter an", "Lüfter an", "aus/an", "BoolAsSingle")
      };

      var a = new DescriptorCollection(attributesA);
      var b = new DescriptorCollection(attributesB);

      // When
      var areEqual = a == b;
      var areInequal = a != b;
      var explicitEquals = a.Equals(b);
      var objectEquals = a.Equals((object)b);

      // Then
      Assert.IsFalse(areEqual);
      Assert.IsTrue(areInequal);
      Assert.IsFalse(explicitEquals);
      Assert.IsFalse(objectEquals);
    }

    [TestMethod]
    public void Different_number_of_attributes_is_inequal()
    {
      // Given
      var attributesA = new[]
      {
        new AttributeDescriptor(new CabInfoAddress(0x01), "Geschwindigkeit", "Geschwindigkeit", "m/s", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x02), "Druck Hauptluftleitung", "Druck Hauptluftleitung", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x03), "Test:Druck Bremszylinder", "Druck Bremszylinder", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x04), "Test:Druck Hauptluftbehälter", "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x05), "Test:Luftpresser läuft", "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x06), "Test2:Luftstrom Fbv", "Luftstrom Fbv", "-1...0...1", "Fail"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x07), "Test2:Luftstrom Zbv", "Luftstrom Zbv", "-1...0...1", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x08), "Test2:Lüfter an", "Lüfter an", "aus/an", "BoolAsSingle")
      };

      var attributesB = new[]
      {
        new AttributeDescriptor(new CabInfoAddress(0x01), "Geschwindigkeit", "Geschwindigkeit", "m/s", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x02), "Druck Hauptluftleitung", "Druck Hauptluftleitung", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x03), "Test:Druck Bremszylinder", "Druck Bremszylinder", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x05), "Test:Luftpresser läuft", "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x06), "Test2:Luftstrom Fbv", "Luftstrom Fbv", "-1...0...1", "Fail"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x07), "Test2:Luftstrom Zbv", "Luftstrom Zbv", "-1...0...1", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x08), "Test2:Lüfter an", "Lüfter an", "aus/an", "BoolAsSingle")
      };

      var a = new DescriptorCollection(attributesA);
      var b = new DescriptorCollection(attributesB);

      // When
      var areEqual = a == b;
      var areInequal = a != b;
      var explicitEquals = a.Equals(b);
      var objectEquals = a.Equals((object)b);

      // Then
      Assert.IsFalse(areEqual);
      Assert.IsTrue(areInequal);
      Assert.IsFalse(explicitEquals);
      Assert.IsFalse(objectEquals);
    }
  }
}