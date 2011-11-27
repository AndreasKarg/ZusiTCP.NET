/*************************************************************************
 * 
 * Zusi TCP interface for .NET
 * (C) 2009-2011 Andreas Karg, <Clonkman@gmx.de>
 * 
 * This file is part of Zusi TCP Interface.NET.
 *
 * Zusi TCP Interface.NET is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * Zusi TCP Interface.NET is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Zusi TCP Interface.NET. 
 * If not, see <http://www.gnu.org/licenses/>.
 * 
 *************************************************************************/

using System;
using System.Runtime.Serialization;

namespace Zusi_Datenausgabe
{
    /// <summary>
    /// Wird geschmissen, wenn bei der Verbindung zum TCP-Server ein Fehler auftritt.
    /// </summary>
    [Serializable]
    public class ZusiTcpException : Exception
    {
        /// <summary>
        /// Standardkonstruktor
        /// </summary>
        /// <param name="msg">Nachricht</param>
        public ZusiTcpException(string msg) : base(msg)
        {
        }

        /// <summary>
        /// Noch ein Standardkonstruktor
        /// </summary>
        public ZusiTcpException()
        {
        }

        /// <summary>
        /// Konstruktor, der eine Nachricht und eine Ausnahme annimmt.
        /// </summary>
        /// <param name="msg">Nachricht</param>
        /// <param name="e">Ausnahme</param>
        public ZusiTcpException(string msg, Exception e) : base(msg, e)
        {
        }

        /// <summary>
        /// Serialisierungskonstruktor
        /// </summary>
        /// <param name="serializationInfo">Serialisierungsinfo</param>
        /// <param name="streamingContext">Streaming context</param>
        protected ZusiTcpException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}