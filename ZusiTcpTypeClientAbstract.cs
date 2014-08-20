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


namespace Zusi_Datenausgabe
{
  /// <summary>
  ///   Represents a ZusiTcpClientAbstract with predefined DataTypes Single, Int, String, ByteLengthString, 
  ///   NullString, DateTime, BoolAsSingle, BoolAndSingle, IntAsSingle, BoolAsInt, DoorsAsInt, PZBAsInt and BrakeAsInt.
  /// </summary>
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class ZusiTcpTypeClientAbstract : ZusiTcpClientAbstract
  {
    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpTypeClientAbstract" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetDocument">The XML file containig the command set.</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    public ZusiTcpTypeClientAbstract(string clientId,
                             ClientPriority priority,
                             SynchronizationContext hostContext,
                             CommandSet commandsetDocument)
      : base(clientId, priority, hostContext, commandsetDocument)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpTypeClientAbstract" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetDocument">The XML file containig the command set.</param>
    /// <exception cref="ObjectUnsynchronisableException">Thrown, when SynchronizationContext.Current == null.</exception>
    public ZusiTcpTypeClientAbstract(string clientId, ClientPriority priority, CommandSet commandsetDocument)
      : base(clientId, priority, commandsetDocument)
    {
    }

    /// <summary>
    ///   Reads incoming data of type Single.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ReadedValue<Single> ReadSingle(IBinaryReader input)
    {
      return new ReadedValue<Single>(sizeof (Single), input.ReadSingle());
    }

    /// <summary>
    ///   Reads incoming data of type Int.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ReadedValue<int> ReadInt(IBinaryReader input)
    {
      return new ReadedValue<int>(sizeof (Int32), input.ReadInt32());
    }

    /// <summary>
    ///   Reads incoming data of type String. This impentation forwards it to HandleDATA_ByteLengthString.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ReadedValue<string> ReadString(IBinaryReader input)
    {
      return ReadByteLengthString(input);
    }

    /// <summary>
    ///   Reads incoming data of Strings with given Length.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ReadedValue<string> ReadByteLengthString(IBinaryReader input)
    {
      string value = input.ReadString();
      return new ReadedValue<string>(value.Length + 1, value);
    }

    /// <summary>
    ///   Reads incoming data of Null-Terminated String.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ReadedValue<string> ReadNullString(IBinaryReader input)
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

      return new ReadedValue<string> (bytesRead, stringBuilder.ToString());
    }

    /// <summary>
    ///   Reads incoming data of type DateTime.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ReadedValue<DateTime> ReadDateTime(IBinaryReader input)
    {
      // Delphi uses the double-based OLE Automation date for its date format.
      double temp = input.ReadDouble();
      DateTime time = DateTime.FromOADate(temp);

      return new ReadedValue<DateTime> (sizeof (Double), time);
    }

    /// <summary>
    ///   Reads incoming data of type Bool that are sent as Single values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ReadedValue<bool> ReadBoolAsSingle(IBinaryReader input)
    {
      /* Data is delivered as Single values that are only either 0.0 or 1.0.
       * For the sake of logic, convert these to actual booleans here.
       */
      Single temp = input.ReadSingle();
      bool value = (temp >= 0.5f);

      return new ReadedValue<bool> (sizeof (Single), value);
    }

    /// <summary>
    ///   Reads incoming data that is sent as Single values by Zusi and can
    ///   be a bool value as well as a single value.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected BoolAndSingleStruct ReadBoolAndSingle(IBinaryReader input)
    {
      /* Data is delivered as Single values that are usually only either 0.0 or 1.0.
       * In some cases (PZ80!) the values are no Booleans at all, so we just post to both events.
       */
      Single temp = input.ReadSingle();
      bool value = (temp >= 0.5f);
      return new BoolAndSingleStruct(sizeof (Single), value, temp);
    }

    /// <summary>
    ///   Handle incoming data of type Int that are sent as Single values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ReadedValue<int> ReadIntAsSingle(IBinaryReader input)
    {
      /* Data is delivered as Single values that are only either 0.0 or 1.0.
       * For the sake of logic, convert these to actual booleans here.
       */
      Single temp = input.ReadSingle();
      int value = (int) Math.Round(temp);

      return new ReadedValue<int>(sizeof (Single), value);
    }

    /// <summary>
    ///   Reads incoming data of type Bool that are sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ReadedValue<bool> ReadBoolAsInt(IBinaryReader input)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
      Int32 temp = input.ReadInt32();
      bool value = (temp == 1);

      return new ReadedValue<bool> (sizeof (Int32), value);
    }

    /// <summary>
    ///   Reads incoming door state data that is sent as an Int value by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ReadedValue<DoorState> ReadDoorsAsInt(IBinaryReader input)
    {
      Int32 temp = input.ReadInt32();
      return new ReadedValue<DoorState> (sizeof (Int32), (DoorState) temp);
    }

    /// <summary>
    ///   Reads incoming PZB status information that is sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ReadedValue<PZBSystem> ReadPZBAsInt(IBinaryReader input)
    {
      Int32 temp = input.ReadInt32();
      return new ReadedValue<PZBSystem> (sizeof (Int32), (PZBSystem) temp);
    }

    /// <summary>
    ///   Reads incoming brake information that is sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ReadedValue<BrakeConfiguration> ReadBrakesAsInt(IBinaryReader input)
    {
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

      return new ReadedValue<BrakeConfiguration> (sizeof (Int32), result);
    }
  }

  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public struct BoolAndSingleStruct
  {
    public BoolAndSingleStruct(int lng, bool retVal, float pz80Val) : this()
    {
      ReadedLength = lng;
      ReadedData = retVal;
      PZ80Data = pz80Val;
    }

    ///<summary>The length, that was neccessary to extract the data.</summary>
    [Zusi_Datenausgabe.Compyling.ReadedLengthAttribute()]
    public int ReadedLength {private set; get;}

    ///<summary>The data, that was extracted.</summary>
    [Zusi_Datenausgabe.Compyling.ReadedDataAttribute()]
    public bool ReadedData {private set; get;}

    ///<summary>The data, that was extracted.</summary>
    [Zusi_Datenausgabe.Compyling.ReadedDataAttribute()]
    public float PZ80Data {private set; get;}
  }
}
