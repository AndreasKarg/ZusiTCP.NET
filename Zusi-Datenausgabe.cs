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
  ///           Create an instance of <see cref="ZusiTcpConn" />, choosing a client priority. Recommended value for control desks is "High".
  ///           Add your event handlers to the appropriate events.
  ///         </description>
  ///       </item>
  ///       <item>
  ///         <description>
  ///           Add the required measurements using <see cref="ZusiTcpReceiver.RequestedData" />.
  ///           You can use either the correct ID numbers or the measurements' german names as specified in the TCP server's commandset.xml.
  ///         </description>
  ///       </item>
  ///       <item>
  ///         <description>
  ///           <see cref="ZusiTcpReceiver.Connect" /> or <seealso cref="ZusiTcpReceiver.Connect(System.Net.IPEndPoint)" /> to the TCP server.
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
  public class ZusiTcpConn : ZusiTcpReceiver
  {
    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpConn" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetDocument">The XML file containig the command set.</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    public ZusiTcpConn(string clientId,
                       ClientPriority priority,
                       TCPCommands commandsetDocument,
                       SynchronizationContext hostContext)
      : base(clientId, priority, hostContext, commandsetDocument)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpConn" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetDocument">The XML file containig the command set.</param>
    public ZusiTcpConn(string clientId, ClientPriority priority, TCPCommands commandsetDocument)
      : base(clientId, priority, commandsetDocument)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpConn" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetPath">Path to the XML file containing the command set.</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    public ZusiTcpConn(string clientId,
                       ClientPriority priority,
                       string commandsetPath,
                       SynchronizationContext hostContext)
      : this(clientId, priority, TCPCommands.LoadFromFile(commandsetPath), hostContext)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpConn" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetPath">Path to the XML file containing the command set.</param>
    public ZusiTcpConn(string clientId, ClientPriority priority, string commandsetPath)
      : this(clientId, priority, TCPCommands.LoadFromFile(commandsetPath))
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpConn" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    public ZusiTcpConn(string clientId, ClientPriority priority, SynchronizationContext hostContext)
      : this(clientId, priority, "commandset.xml", hostContext)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpConn" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    public ZusiTcpConn(string clientId, ClientPriority priority)
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
      PostToHost(FloatReceived, id, input.ReadSingle());

      return sizeof (Single);
    }

    /// <summary>
    ///   Handle incoming data of type Int.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_Int(IBinaryReader input, int id)
    {
      PostToHost(IntReceived, id, input.ReadInt32());

      return sizeof (Int32);
    }

    /// <summary>
    ///   Handle incoming data of type String. This impentation forwards it to HandleDATA_ByteLengthString.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_String(IBinaryReader input, int id)
    {
      return HandleDATA_ByteLengthString(input, id);
    }

    /// <summary>
    ///   Handle incoming data of Strings with given Length.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_ByteLengthString(IBinaryReader input, int id)
    {
      string value = input.ReadString();
      PostToHost(StringReceived, id, value);

      return value.Length + 1;
    }

    /// <summary>
    ///   Handle incoming data of Null-Terminated String.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_NullString(IBinaryReader input, int id)
    {
      StringBuilder stringBuilder = new StringBuilder();
      int bytesRead = 0;
      byte curByte;

      do
      {
        curByte = input.ReadByte();
        stringBuilder.Append(curByte);
        bytesRead++;
      } while (curByte != 0);

      PostToHost(StringReceived, id, stringBuilder.ToString());

      return bytesRead;
    }

    /// <summary>
    ///   Handle incoming data of type DateTime.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_DateTime(IBinaryReader input, int id)
    {
      // Delphi uses the double-based OLE Automation date for its date format.
      double temp = input.ReadDouble();
      DateTime time = DateTime.FromOADate(temp);

      PostToHost(DateTimeReceived, id, time);

      return sizeof (Double);
    }

    /// <summary>
    ///   Handle incoming data of type Bool that are sent as Single values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_BoolAsSingle(IBinaryReader input, int id)
    {
      /* Data is delivered as Single values that are only either 0.0 or 1.0.
       * For the sake of logic, convert these to actual booleans here.
       */
      Single temp = input.ReadSingle();
      bool value = (temp >= 0.5f);
      PostToHost(BoolReceived, id, value);

      return sizeof (Single);
    }

    /// <summary>
    ///   Handle incoming data that is sent as Single values by Zusi and can
    ///   be a bool value as well as a single value.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_BoolAndSingle(IBinaryReader input, int id)
    {
      /* Data is delivered as Single values that are usually only either 0.0 or 1.0.
       * In some cases (PZ80!) the values are no Booleans at all, so we just post to both events.
       */
      Single temp = input.ReadSingle();
      bool value = (temp >= 0.5f);
      PostToHost(BoolReceived, id, value);
      PostToHost(FloatReceived, id, temp);

      return sizeof (Single);
    }

    /// <summary>
    ///   Handle incoming data of type Int that are sent as Single values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_IntAsSingle(IBinaryReader input, int id)
    {
      /* Data is delivered as Single values that are only either 0.0 or 1.0.
       * For the sake of logic, convert these to actual booleans here.
       */
      Single temp = input.ReadSingle();
      int value = (int) Math.Round(temp);
      PostToHost(IntReceived, id, value);

      return sizeof (Single);
    }

    /// <summary>
    ///   Handle incoming data of type Bool that are sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_BoolAsInt(IBinaryReader input, int id)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
      Int32 temp = input.ReadInt32();
      bool value = (temp == 1);
      PostToHost(BoolReceived, id, value);

      return sizeof (Int32);
    }

    /// <summary>
    ///   Handle incoming door state data that is sent as an Int value by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_DoorsAsInt(IBinaryReader input, int id)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
      Int32 temp = input.ReadInt32();
      PostToHost(DoorsReceived, id, (DoorState) temp);

      return sizeof (Int32);
    }

    /// <summary>
    ///   Handle incoming PZB status information that is sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_PZBAsInt(IBinaryReader input, int id)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
      Int32 temp = input.ReadInt32();
      PostToHost(PZBReceived, id, (PZBSystem) temp);

      return sizeof (Int32);
    }

    /// <summary>
    ///   Handle incoming brake information that is sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_BrakesAsInt(IBinaryReader input, int id)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
      Int32 temp = input.ReadInt32();

      BrakeConfiguration result;

      switch (temp)
      {
        case 0:
          result = new BrakeConfiguration {HasMgBrake = false, Pitch = BrakePitch.G};
          break;

        case 1:
          result = new BrakeConfiguration {HasMgBrake = false, Pitch = BrakePitch.P};
          break;

        case 2:
          result = new BrakeConfiguration {HasMgBrake = false, Pitch = BrakePitch.R};
          break;

        case 3:
          result = new BrakeConfiguration {HasMgBrake = true, Pitch = BrakePitch.P};
          break;

        case 4:
          result = new BrakeConfiguration {HasMgBrake = true, Pitch = BrakePitch.R};
          break;

        default:
          throw new ZusiTcpException("Invalid value received for brake configuration.");
      }

      PostToHost(BrakeConfigReceived, id, result);

      return sizeof (Int32);
    }

    // ReSharper restore InconsistentNaming

    #endregion
  }
}
