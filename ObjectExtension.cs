#region Using

using System.Diagnostics;

#endregion

namespace Zusi_Datenausgabe
{
  internal static class ObjectExtension
  {
    public static T AssertedCast<T>(this object o) where T : class
    {
      Debug.Assert(o is T);
      return o as T;
    }
  }
}
