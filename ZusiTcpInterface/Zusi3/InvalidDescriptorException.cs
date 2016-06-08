using System;
using System.Runtime.Serialization;

namespace ZusiTcpInterface.Zusi3
{
  public class InvalidDescriptorException : Exception
  {
    public InvalidDescriptorException()
    {
    }

    public InvalidDescriptorException(string message) : base(message)
    {
    }

    public InvalidDescriptorException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected InvalidDescriptorException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}