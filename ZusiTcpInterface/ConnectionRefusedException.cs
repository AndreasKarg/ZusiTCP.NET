using System;
using System.Runtime.Serialization;

namespace ZusiTcpInterface
{
  [Serializable]
  public class ConnectionRefusedException : Exception
  {
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public ConnectionRefusedException()
    {
    }

    public ConnectionRefusedException(string message) : base(message)
    {
    }

    public ConnectionRefusedException(string message, Exception inner) : base(message, inner)
    {
    }

    protected ConnectionRefusedException(
      SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
  }
}