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

using System;
using System.Runtime.Serialization;

namespace Zusi_Datenausgabe
{
  /// <summary>
  /// Is thrown when an error occurs in the connection to the TCP server.
  /// </summary>
  [Serializable]
  public class ZusiTcpException : Exception
  {
    /// <summary>
    /// Create a new ZusiTcpException using msg as message.
    /// </summary>
    /// <param name="msg">The exception message.</param>
    public ZusiTcpException(string msg)
      : base(msg)
    {
    }

    /// <summary>
    /// Create a new ZusiTcpException.
    /// </summary>
    public ZusiTcpException()
    {
    }

    /// <summary>
    /// Create a new ZusiTcpException using msg as the message and e as the inner exception
    /// </summary>
    /// <param name="msg">The exception message.</param>
    /// <param name="e">The inner exception.</param>
    public ZusiTcpException(string msg, Exception e)
      : base(msg, e)
    {
    }

    /// <summary>
    /// Create a new ZusiTcpException with serialized data.
    /// </summary>
    /// <param name="serializationInfo">Serialization info</param>
    /// <param name="streamingContext">Streaming context</param>
    protected ZusiTcpException(SerializationInfo serializationInfo, StreamingContext streamingContext)
      : base(serializationInfo, streamingContext)
    {
    }
  }
}