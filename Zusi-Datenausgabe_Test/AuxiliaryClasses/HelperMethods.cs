using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Zusi_Datenausgabe_Test.AuxiliaryClasses
{
  public static class HelperMethods
  {
    public static T ExpectException<T>(Action action) where T: Exception
    {
      try
      {
        action();
      }
      catch (Exception ex)
      {
        Assert.IsInstanceOfType(ex, typeof(T));
        return ex as T;
      }

      Assert.Fail("No Exception thrown.");
      return null;
    }
  }
}