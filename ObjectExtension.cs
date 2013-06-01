using System.Diagnostics;

namespace Zusi_Datenausgabe
{
  static internal class ObjectExtension
  {
    public static T AssertedCast<T>(this object o) where T : class
    {
      Debug.Assert(o is T);
      return o as T;
    }
  }
}