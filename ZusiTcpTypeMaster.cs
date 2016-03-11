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
  ///   Represents a Master in the ZusiTcp-Protocol. Can be used to create a "virtual Zusi".
  /// </summary>
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class ZusiTcpTypeMaster : ZusiTcpMasterAbstract
  {
    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpTypeMaster" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    public ZusiTcpTypeMaster(string clientId, SynchronizationContext hostContext)
      : base(clientId, hostContext)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpTypeMaster" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <exception cref="ObjectUnsynchronisableException">Thrown, when SynchronizationContext.Current == null.</exception>
    public ZusiTcpTypeMaster(string clientId)
      : base(clientId)
    {
    }

    #region Data reception handlers

    /// <summary>
    ///   Send data data of type Single.
    /// </summary>
    /// <param name="value">The value to send.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    public void SendSingle(Single value, int id)
    {
      SendByteCommand(System.BitConverter.GetBytes(value), id);
    }

    /// <summary>
    ///   Send data data of type Int.
    /// </summary>
    /// <param name="value">The value to send.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    public void SendInt(int value, int id)
    {
      SendByteCommand(System.BitConverter.GetBytes(value), id);
    }

    /// <summary>
    ///   Send data of type String. This impentation forwards it to SendByteLengthString.
    /// </summary>
    /// <param name="value">The value to send.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    public void SendString(string value, int id)
    {
      SendByteLengthString(value, id);
    }

    /// <summary>
    ///   Send data of Strings with given Length.
    /// </summary>
    /// <param name="value">The value to send.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    public void SendByteLengthString(string value, int id)
    {
      SendByteCommand(PacketSender.PackArrays(new byte[][] {new byte[] {(byte) value.Length}, 
        System.Text.Encoding.Default.GetBytes(value)}), 
                      id);
    }

    /// <summary>
    ///   Send data of Null-Terminated String.
    /// </summary>
    /// <param name="value">The value to send.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    public void SendNullString(string value, int id)
    {
      SendByteCommand(PacketSender.PackArrays(new byte[][] {System.Text.Encoding.Default.GetBytes(value), 
        new byte[] {0}}), 
                      id);
    }

    /// <summary>
    ///   Send data of type DateTime.
    /// </summary>
    /// <param name="value">The value to send.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    public void SendDateTime(DateTime value, int id)
    {
      // Delphi uses the double-based OLE Automation date for its date format.
      double temp = value.ToOADate();
      SendByteCommand(System.BitConverter.GetBytes(temp), id);
    }

    /// <summary>
    ///   Send data of type Bool that are sent as Single values by Zusi.
    /// </summary>
    /// <param name="value">The value to send.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    public void SendBoolAsSingle(bool value, int id)
    {
      Single temp = (value) ? 1 : 0;
      SendByteCommand(System.BitConverter.GetBytes(temp), id);
    }

    /// <summary>
    ///   Send data of type Int that are sent as Single values by Zusi.
    /// </summary>
    /// <param name="value">The value to send.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    public void SendIntAsSingle(int value, int id)
    {
      Single temp = value;
      SendByteCommand(System.BitConverter.GetBytes(temp), id);
    }

    /// <summary>
    ///   Send data of type Bool that are sent as Int values by Zusi.
    /// </summary>
    /// <param name="value">The value to send.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    public void SendBoolAsInt(bool value, int id)
    {
      Int32 temp = (value) ? 1 : 0;
      SendByteCommand(System.BitConverter.GetBytes(temp), id);
    }

    /// <summary>
    ///   Send door state data that is sent as an Int value by Zusi.
    /// </summary>
    /// <param name="value">The value to send.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    public void SendDoorsAsInt(DoorState value, int id)
    {
      Int32 temp = (int) value;
      SendByteCommand(System.BitConverter.GetBytes(temp), id);
    }

    /// <summary>
    ///   Send PZB status information that is sent as Int values by Zusi.
    /// </summary>
    /// <param name="value">The value to send.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    public void SendPZBAsInt(PZBSystem value, int id)
    {
      Int32 temp = (int) value;
      SendByteCommand(System.BitConverter.GetBytes(temp), id);
    }

    /// <summary>
    ///   Send brake information that is sent as Int values by Zusi.
    /// </summary>
    /// <param name="value">The value to send.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    public void SendBrakesAsInt(BrakeConfiguration value, int id)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
      Int32 temp = 0;

      switch (value.Pitch)
      {
        case BrakePitch.G:
          temp = 0;
          break;

        case BrakePitch.P:
          temp = 1;
          break;

        case BrakePitch.R:
          temp = 2;
          break;

        default:
          throw new ZusiTcpException("Tryed to send Invalid value for brake configuration.");
      }
      if (value.HasMgBrake) temp += 2;

      if (value.HasMgBrake && value.Pitch == BrakePitch.G)
          throw new ZusiTcpException("Tryed to send Invalid value for brake configuration.");
 
      SendByteCommand(System.BitConverter.GetBytes(temp), id);
    }

    #endregion
  }
}
