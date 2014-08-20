/*************************************************************************
 * Zusi-Datenausgabe.cs
 * Contains main logic for the TCP interface.
 *
 * (C) 2009-2011 Andreas Karg, <Clonkman@gmx.de>
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
using System.Text;
using System.Threading;

#endregion

[assembly: CLSCompliant(true)]

namespace Zusi_Datenausgabe
{
  /// <summary>
  ///   Represents the centerpiece of the Zusi TCP interface.
  ///   <para>
  ///     Usage:
  ///     <list type="number">
  ///       <item>
  ///         <description>
  ///           Implement event handlers.
  ///           These must conform to the <see cref="ReceiveEvent{T}" /> delegate.
  ///           All data sent by Zusi are converted to their appropriate types.
  ///         </description>
  ///       </item>
  ///       <item>
  ///         <description>
  ///           Create an instance of <see cref="ZusiTcpTypeClient" />, choosing a client priority. Recommended value for control desks is "High".
  ///           Add your event handlers to the appropriate events.
  ///         </description>
  ///       </item>
  ///       <item>
  ///         <description>
  ///           Add the required measurements using <see cref="ZusiTcpClientAbstract.RequestedData" />.
  ///           You can use either the correct ID numbers or the measurements' german names as specified in the TCP server's commandset.xml.
  ///         </description>
  ///       </item>
  ///       <item>
  ///         <description>
  ///           <see cref="ZusiTcpClientAbstract.Connect(string, int)" /> or <seealso cref="ZusiTcpClientAbstract.Connect(System.Net.IPEndPoint)" /> to the TCP server.
  ///         </description>
  ///       </item>
  ///       <item>
  ///         <description>
  ///           As soon as data is coming from the server, the respective events are called automatically, passing one new
  ///           dataset at a time.
  ///         </description>
  ///       </item>
  ///     </list>
  ///   </para>
  ///   Notice that ZusiTcpConn implements IDisposable, so remember to dispose of it properly when you are finished.
  /// </summary>
  public class ZusiTcpTypeClient : ZusiTcpTypeClientAbstract
  {
    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpTypeClient" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetDocument">The XML file containig the command set.</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    public ZusiTcpTypeClient(string clientId,
                             ClientPriority priority,
                             CommandSet commandsetDocument,
                             SynchronizationContext hostContext)
      : base(clientId, priority, hostContext, commandsetDocument)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpTypeClient" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetDocument">The XML file containig the command set.</param>
    /// <exception cref="ObjectUnsynchronisableException">Thrown, when SynchronizationContext.Current == null.</exception>
    public ZusiTcpTypeClient(string clientId, ClientPriority priority, CommandSet commandsetDocument)
      : base(clientId, priority, commandsetDocument)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpTypeClient" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetPath">Path to the XML file containing the command set.</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    public ZusiTcpTypeClient(string clientId,
                             ClientPriority priority,
                             string commandsetPath,
                             SynchronizationContext hostContext)
      : this(clientId, priority, CommandSet.LoadFromFile(commandsetPath), hostContext)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpTypeClient" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetPath">Path to the XML file containing the command set.</param>
    /// <exception cref="ObjectUnsynchronisableException">Thrown, when SynchronizationContext.Current == null.</exception>
    public ZusiTcpTypeClient(string clientId, ClientPriority priority, string commandsetPath)
      : this(clientId, priority, CommandSet.LoadFromFile(commandsetPath))
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpTypeClient" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    public ZusiTcpTypeClient(string clientId, ClientPriority priority, SynchronizationContext hostContext)
      : this(clientId, priority, "commandset.xml", hostContext)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpTypeClient" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <exception cref="ObjectUnsynchronisableException">Thrown, when SynchronizationContext.Current == null.</exception>
    public ZusiTcpTypeClient(string clientId, ClientPriority priority)
      : this(clientId, priority, "commandset.xml")
    {
    }

    #region Data reception handlers

    // ReSharper disable InconsistentNaming

    /// <summary>
    ///   Event used to handle incoming float data.
    /// </summary>
    public event ReceiveEvent<float> FloatReceived;

    /// <summary>
    ///   Event used to handle incoming string data.
    /// </summary>
    public event ReceiveEvent<string> StringReceived;

    /// <summary>
    ///   Event used to handle incoming integer data.
    /// </summary>
    public event ReceiveEvent<int> IntReceived;

    /// <summary>
    ///   Event used to handle incoming boolean data.
    /// </summary>
    public event ReceiveEvent<Boolean> BoolReceived;

    /// <summary>
    ///   Event used to handle incoming DateTime data.
    /// </summary>
    public event ReceiveEvent<DateTime> DateTimeReceived;

    /// <summary>
    ///   Event used to handle incoming door status data.
    /// </summary>
    public event ReceiveEvent<DoorState> DoorsReceived;

    /// <summary>
    ///   Event used to handle incoming PZB system type data.
    /// </summary>
    public event ReceiveEvent<PZBSystem> PZBReceived;

    /// <summary>
    ///   Event used to handle incoming brake configuration data.
    /// </summary>
    public event ReceiveEvent<BrakeConfiguration> BrakeConfigReceived;

    /// <summary>
    ///   Handle incoming data of type Single.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_Single(IBinaryReader input, int id)
    {
      var data = ReadSingle(input);
      PostToHost(FloatReceived, id, data.ReadedData);
      return data.ReadedLength;
    }

    /// <summary>
    ///   Handle incoming data of type Int.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_Int(IBinaryReader input, int id)
    {
      var data = ReadInt(input);
      PostToHost(IntReceived, id, data.ReadedData);
      return data.ReadedLength;
    }

    /// <summary>
    ///   Handle incoming data of type String. This impentation forwards it to HandleDATA_ByteLengthString.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_String(IBinaryReader input, int id)
    {
      var data = ReadString(input);
      PostToHost(StringReceived, id, data.ReadedData);
      return data.ReadedLength;
    }

    /// <summary>
    ///   Handle incoming data of Strings with given Length.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_ByteLengthString(IBinaryReader input, int id)
    {
      var data = ReadByteLengthString(input);
      PostToHost(StringReceived, id, data.ReadedData);
      return data.ReadedLength;
    }

    /// <summary>
    ///   Handle incoming data of Null-Terminated String.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_NullString(IBinaryReader input, int id)
    {
      var data = ReadNullString(input);
      PostToHost(StringReceived, id, data.ReadedData);
      return data.ReadedLength;
    }

    /// <summary>
    ///   Handle incoming data of type DateTime.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_DateTime(IBinaryReader input, int id)
    {
      var data = ReadDateTime(input);
      PostToHost(DateTimeReceived, id, data.ReadedData);
      return data.ReadedLength;
    }

    /// <summary>
    ///   Handle incoming data of type Bool that are sent as Single values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_BoolAsSingle(IBinaryReader input, int id)
    {
      var data = ReadBoolAsSingle(input);
      PostToHost(BoolReceived, id, data.ReadedData);
      return data.ReadedLength;
    }

    /// <summary>
    ///   Handle incoming data that is sent as Single values by Zusi and can
    ///   be a bool value as well as a single value.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_BoolAndSingle(IBinaryReader input, int id)
    {
      var data = ReadBoolAndSingle(input);
      PostToHost(BoolReceived, id, data.ReadedData);
      PostToHost(FloatReceived, id, data.PZ80Data);
      return data.ReadedLength;
    }

    /// <summary>
    ///   Handle incoming data of type Int that are sent as Single values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_IntAsSingle(IBinaryReader input, int id)
    {
      var data = ReadIntAsSingle(input);
      PostToHost(IntReceived, id, data.ReadedData);
      return data.ReadedLength;
    }

    /// <summary>
    ///   Handle incoming data of type Bool that are sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_BoolAsInt(IBinaryReader input, int id)
    {
      var data = ReadBoolAsInt(input);
      PostToHost(BoolReceived, id, data.ReadedData);
      return data.ReadedLength;
    }

    /// <summary>
    ///   Handle incoming door state data that is sent as an Int value by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_DoorsAsInt(IBinaryReader input, int id)
    {
      var data = ReadDoorsAsInt(input);
      PostToHost(DoorsReceived, id, data.ReadedData);
      return data.ReadedLength;
    }

    /// <summary>
    ///   Handle incoming PZB status information that is sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_PZBAsInt(IBinaryReader input, int id)
    {
      var data = ReadPZBAsInt(input);
      PostToHost(PZBReceived, id, data.ReadedData);
      return data.ReadedLength;
    }

    /// <summary>
    ///   Handle incoming brake information that is sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_BrakesAsInt(IBinaryReader input, int id)
    {
      var data = ReadBrakesAsInt(input);
      PostToHost(BrakeConfigReceived, id, data.ReadedData);
      return data.ReadedLength;
    }

    // ReSharper restore InconsistentNaming

    #endregion
  }
}
