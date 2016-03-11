/*************************************************************************
 * ZusiTCPException.cs
 * Contains the ZusiTcpException class.
 *
 * (C) 2009-2012 Andreas Karg, <Clonkman@gmx.de>
 *
 * This file is part of Zusi TCP Interface.NET.
 *
 * Zusi TCP Interface.NET is free software: you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * Zusi TCP Interface.NET is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with Zusi TCP Interface.NET.
 * If not, see <http://www.gnu.org/licenses/>.
 *
 *************************************************************************/

#region Using

using System;
using System.Runtime.Serialization;

#endregion

namespace Zusi_Datenausgabe
{
  /// <summary>
  ///   Is thrown when an error occurs in the connection to the TCP server.
  /// </summary>
  [Serializable]
  public class ZusiTcpException : Exception
  {
    /// <summary>
    ///   Create a new ZusiTcpException using msg as message.
    /// </summary>
    /// <param name="msg">The exception message.</param>
    public ZusiTcpException(string msg)
      : base(msg)
    {
    }

    /// <summary>
    ///   Create a new ZusiTcpException.
    /// </summary>
    public ZusiTcpException()
    {
    }

    /// <summary>
    ///   Create a new ZusiTcpException using msg as the message and e as the inner exception
    /// </summary>
    /// <param name="msg">The exception message.</param>
    /// <param name="e">The inner exception.</param>
    public ZusiTcpException(string msg, Exception e)
      : base(msg, e)
    {
    }

    /// <summary>
    ///   Create a new ZusiTcpException with serialized data.
    /// </summary>
    /// <param name="serializationInfo">Serialization info</param>
    /// <param name="streamingContext">Streaming context</param>
    protected ZusiTcpException(SerializationInfo serializationInfo, StreamingContext streamingContext)
      : base(serializationInfo, streamingContext)
    {
    }
  }

  /// <summary>
  ///   Is thrown when an object provides syncronized Events but no Syncronization Context was found.
  ///   This execption is thrown in constructors and is caused by the programmer. To avoid this exception
  ///   the constructor should be called later, when SynchronizationContext.Current is not null any more.
  /// </summary>
  [Serializable]
  public class ObjectUnsynchronisableException : Exception
  {
    /// <summary>
    ///   Create a new ObjectUnsynchronisableException using msg as message.
    /// </summary>
    /// <param name="msg">The exception message.</param>
    public ObjectUnsynchronisableException(string msg)
      : base(msg)
    {
    }

    /// <summary>
    ///   Create a new ObjectUnsynchronisableException.
    /// </summary>
    public ObjectUnsynchronisableException()
      : this("Cannot create TCP connection object: SynchronizationContext.Current is null. " +
             "This happens when the object is created before the context is initialized in " +
             "Application.Run() or equivalent. " +
             "Possible solution: Create object later, e.g. when the user clicks the \"Connect\" button. " +
             "If you do not need Syncrosisation use a constructor with explicit SynchronizationContext null.")
    {
    }

    /// <summary>
    ///   Create a new ObjectUnsynchronisableException using msg as the message and e as the inner exception
    /// </summary>
    /// <param name="msg">The exception message.</param>
    /// <param name="e">The inner exception.</param>
    public ObjectUnsynchronisableException(string msg, Exception e)
      : base(msg, e)
    {
    }

    /// <summary>
    ///   Create a new ObjectUnsynchronisableException with serialized data.
    /// </summary>
    /// <param name="serializationInfo">Serialization info</param>
    /// <param name="streamingContext">Streaming context</param>
    protected ObjectUnsynchronisableException(SerializationInfo serializationInfo, StreamingContext streamingContext)
      : base(serializationInfo, streamingContext)
    {
    }
  }
}
