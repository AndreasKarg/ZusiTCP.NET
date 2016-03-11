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
  public class ZusiTcp3TypeClientAbstract : ZusiTcp3ClientAbstract
  {
    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpTypeClientAbstract" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetDocument">The XML file containig the command set.</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    public ZusiTcp3TypeClientAbstract(string clientId,
                             string clientVersion,
                             SynchronizationContext hostContext,
                             CommandSet commandsetDocument)
      : base(clientId, clientVersion, hostContext, commandsetDocument)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpTypeClientAbstract" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetDocument">The XML file containig the command set.</param>
    /// <exception cref="ObjectUnsynchronisableException">Thrown, when SynchronizationContext.Current == null.</exception>
    public ZusiTcp3TypeClientAbstract(string clientId, string clientVersion, CommandSet commandsetDocument)
      : base(clientId, clientVersion, commandsetDocument)
    {
    }

    /// <summary>
    ///   Reads incoming data of type Single.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<Single> ReadSingle(ZusiTcp3AttributeAbstract input)
    {
      return new ExtractedValue<Single>(sizeof (Single), input.DataAsSingle);
    }

    /// <summary>
    ///   Reads incoming data of type Int.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<int> ReadInt(ZusiTcp3AttributeAbstract input)
    {
      throw new NotImplementedException();
//      return new ExtractedValue<int>(sizeof (Int32), input.ReadInt32());
    }

    /// <summary>
    ///   Reads incoming data of type String. This impentation forwards it to HandleDATA_ByteLengthString.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<string> ReadString(ZusiTcp3AttributeAbstract input)
    {
      return new ExtractedValue<string>(input.Data.Length, input.DataAsString);
    }

    /// <summary>
    ///   Reads incoming data of type DateTime.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<DateTime> ReadDateTime(ZusiTcp3AttributeAbstract input)
    {
      throw new NotImplementedException();
      // Delphi uses the double-based OLE Automation date for its date format.
//      double temp = input.ReadDouble();
//      DateTime time = DateTime.FromOADate(temp);

//      return new ExtractedValue<DateTime> (sizeof (Double), time);
    }

    private Single DateBuffer;
    /// <summary>
    ///   Reads incoming data of type DateTime that are sent as Single values by Zusi. In this case: The Date-Part.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<System.DateTime> ReadBufferedDateAsSingle(ZusiTcp3AttributeAbstract input)
    {
      ExtractedValue<Single> temp = ReadSingle(input);

      DateBuffer = temp.ExtractedData;

      return new ExtractedValue<System.DateTime> (temp.ExtractedLength,
                  new System.DateTime((System.Int64) ((System.Int64) 24 * 60 * 60 * 1000 * 1000 * 10 * ((double) DateBuffer + (double) TimeBuffer))));
    }

    private Single TimeBuffer;
    /// <summary>
    ///   Reads incoming data of type DateTime that are sent as Single values by Zusi. In this case: The Time-Part.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<System.DateTime> ReadBufferedTimeAsSingle(ZusiTcp3AttributeAbstract input)
    {
      ExtractedValue<Single> temp = ReadSingle(input);

      TimeBuffer = temp.ExtractedData;

      return new ExtractedValue<System.DateTime> (temp.ExtractedLength,
                  new System.DateTime((System.Int64) ((System.Int64) 24 * 60 * 60 * 1000 * 1000 * 10 * ((double) DateBuffer + (double) TimeBuffer))));
    }

    /// <summary>
    ///   Reads incoming data of type Bool that are sent as Single values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<bool> ReadBoolAsSingle(ZusiTcp3AttributeAbstract input)
    {
      /* Data is delivered as Single values that are only either 0.0 or 1.0.
       * For the sake of logic, convert these to actual booleans here.
       */
      ExtractedValue<Single> temp = ReadSingle(input);
      bool value = (temp.ExtractedData >= 0.5f);

      return new ExtractedValue<bool> (temp.ExtractedLength, value);
    }

    /// <summary>
    ///   Reads incoming data that is sent as Single values by Zusi and can
    ///   be a bool value as well as a single value.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected BoolAndSingleStruct ReadBoolAndSingle(ZusiTcp3AttributeAbstract input)
    {
      /* Data is delivered as Single values that are usually only either 0.0 or 1.0.
       * In some cases (PZ80!) the values are no Booleans at all, so we just post to both events.
       */
       ExtractedValue<Single> temp = ReadSingle(input);
      bool value = (temp.ExtractedData >= 0.5f);
      return new BoolAndSingleStruct(temp.ExtractedLength, value, temp.ExtractedData);
    }

    /// <summary>
    ///   Handle incoming data of type Int that are sent as Single values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<int> ReadIntAsSingle(ZusiTcp3AttributeAbstract input)
    {
      /* Data is delivered as Single values that are only either 0.0 or 1.0.
       * For the sake of logic, convert these to actual booleans here.
       */
       ExtractedValue<Single> temp = ReadSingle(input);
      int value = (int) Math.Round(temp.ExtractedData);

      return new ExtractedValue<int>(temp.ExtractedLength, value);
    }

    /// <summary>
    ///   Reads incoming data of type Bool that are sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<bool> ReadBoolAsInt(ZusiTcp3AttributeAbstract input)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
      ExtractedValue<Int32> temp = ReadInt(input);
      bool value = (temp.ExtractedData == 1);

      return new ExtractedValue<bool> (temp.ExtractedLength, value);
    }

    /// <summary>
    ///   Reads incoming brake information that is sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<BrakeConfiguration> ReadBrakesAsInt(ZusiTcp3AttributeAbstract input)
    {
      ExtractedValue<Int32> temp = ReadInt(input);

      BrakeConfiguration result;

      switch (temp.ExtractedData)
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

      return new ExtractedValue<BrakeConfiguration> (temp.ExtractedLength, result);
    }

    Notbremssystem bufferNotbremssystem;
    /// <summary>
    ///   Reads incoming Notbremssystem status information.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<Notbremssystem> ReadNotbremssystem(ZusiTcp3Node input)
    {
      ZusiTcp3AttributeAbstract attrNotbremssystemName = input.TryGetSubAttribute(0x1);
      //Status
      ZusiTcp3AttributeAbstract attrReady = input.TryGetSubAttribute(0x3);
      ZusiTcp3AttributeAbstract attrNotbremsung = input.TryGetSubAttribute(0x4);
      ZusiTcp3AttributeAbstract attrIsTesting = input.TryGetSubAttribute(0x5);

      if (attrNotbremssystemName != null)
        bufferNotbremssystem.NotbremssystemName = attrNotbremssystemName.DataAsString;
      if (attrReady != null)
        bufferNotbremssystem.Ready = (attrReady.DataAsByte > 1);
      if (attrNotbremsung != null)
        bufferNotbremssystem.Notbremsung = (attrNotbremsung.DataAsByte > 1);
      if (attrIsTesting != null)
        bufferNotbremssystem.IsTesting = (attrIsTesting.DataAsByte > 1);

      return new ExtractedValue<Notbremssystem> (0, bufferNotbremssystem);
    }

    Sifa bufferSifa;
    /// <summary>
    ///   Reads incoming Sifa status information.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<Sifa> ReadSifa(ZusiTcp3Node input)
    {
      ZusiTcp3AttributeAbstract attrSifaName = input.TryGetSubAttribute(0x1);
      ZusiTcp3AttributeAbstract attrOpticalReminderOn = input.TryGetSubAttribute(0x2);
      ZusiTcp3AttributeAbstract attrHornStatus = input.TryGetSubAttribute(0x3);
      ZusiTcp3AttributeAbstract attrHauptschalterSifa = input.TryGetSubAttribute(0x4);
      ZusiTcp3AttributeAbstract attrStoerschalterSifa = input.TryGetSubAttribute(0x5);
      ZusiTcp3AttributeAbstract attrAbsperrhahnOffen = input.TryGetSubAttribute(0x6);
      if (attrSifaName != null)
        bufferSifa.SifaName = attrSifaName.DataAsString;
      if (attrOpticalReminderOn != null)
        bufferSifa.OpticalReminderOn = (attrOpticalReminderOn.DataAsByte > 0);
      if (attrHornStatus != null)
        bufferSifa.HornStatus = (Sifa.SifaHornStatus) attrHornStatus.DataAsByte;
      if (attrStoerschalterSifa != null)
        bufferSifa.HauptschalterSifa = (attrHauptschalterSifa.DataAsByte > 1);
      if (attrStoerschalterSifa != null)
        bufferSifa.StoerschalterSifa = (attrStoerschalterSifa.DataAsByte > 1);
      if (attrAbsperrhahnOffen != null)
        bufferSifa.AbsperrhahnOffen = (attrAbsperrhahnOffen.DataAsByte > 1);

      return new ExtractedValue<Sifa> (0, bufferSifa);
    }


    Zugsicherung bufferZugsicherung;
    Zugsicherung.IndusiConfig bufferIndusiConfig;
    Zugsicherung.IndusiState bufferIndusiState;
    /// <summary>
    ///   Reads incoming PZB status information.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<Zugsicherung> ReadZugsicherung(ZusiTcp3Node input)
    {
      ZusiTcp3AttributeAbstract attrSicherungName = input.TryGetSubAttribute(0x1);
      ZusiTcp3Node nodeIndusiConfig = input.TryGetSubNode(0x2);
      ZusiTcp3Node nodeIndusiState = input.TryGetSubNode(0x3);
      if (attrSicherungName != null)
      {
        bufferZugsicherung.ZugsicherungName = attrSicherungName.DataAsString;
      }
      if (nodeIndusiConfig != null)
      {
        ZusiTcp3AttributeAbstract attrZugart = nodeIndusiConfig.TryGetSubAttribute(0x1);
        ZusiTcp3AttributeAbstract attrDriverID = nodeIndusiConfig.TryGetSubAttribute(0x2);
        ZusiTcp3AttributeAbstract attrTrainID = nodeIndusiConfig.TryGetSubAttribute(0x3);
        ZusiTcp3Node nodeGrunddaten = nodeIndusiConfig.TryGetSubNode(0x4);
        ZusiTcp3Node nodeErsatzdaten = nodeIndusiConfig.TryGetSubNode(0x5);
        ZusiTcp3Node nodeCurrentData = nodeIndusiConfig.TryGetSubNode(0x6);
        ZusiTcp3AttributeAbstract attrHauptschalter = nodeIndusiConfig.TryGetSubAttribute(0x7);
        ZusiTcp3AttributeAbstract attrStoerschalterPZB = nodeIndusiConfig.TryGetSubAttribute(0x8);
        ZusiTcp3AttributeAbstract attrStoerschalterLZB = nodeIndusiConfig.TryGetSubAttribute(0x9);
        ZusiTcp3AttributeAbstract attrAbsperrhahn = nodeIndusiConfig.TryGetSubAttribute(0xA);
//        if (attrZugart != null)
//          bufferIndusiConfig. = attrZugart.DataAsString;
        if (attrDriverID != null)
          bufferIndusiConfig.DriverID = attrDriverID.DataAsString;
        if (attrTrainID != null)
          bufferIndusiConfig.TrainID = attrTrainID.DataAsString;

        if (nodeGrunddaten != null)
          bufferIndusiConfig.Grunddaten = readConfig(nodeGrunddaten);
        if (nodeErsatzdaten != null)
          bufferIndusiConfig.Ersatzdaten = readConfig(nodeErsatzdaten);
        if (nodeCurrentData != null)
          bufferIndusiConfig.CurrentData = readConfig(nodeCurrentData);

        if (attrHauptschalter != null)
          bufferIndusiConfig.Hauptschalter = (attrHauptschalter.DataAsByte > 1);
        if (attrStoerschalterPZB != null)
          bufferIndusiConfig.StoerschalterPZB = (attrStoerschalterPZB.DataAsByte > 1);
        if (attrStoerschalterLZB != null)
          bufferIndusiConfig.StoerschalterLZB = (attrStoerschalterLZB.DataAsByte > 1);
        if (attrAbsperrhahn != null)
          bufferIndusiConfig.AbsperrhahnOffen = (attrAbsperrhahn.DataAsByte > 1);
      }
      if (nodeIndusiState != null)
      {
        ZusiTcp3AttributeAbstract attrZBResondAsString = nodeIndusiState.TryGetSubAttribute(0x4);
        ZusiTcp3AttributeAbstract attrLM_1000Hz = nodeIndusiState.TryGetSubAttribute(0x5);
        ZusiTcp3AttributeAbstract attrLM_U = nodeIndusiState.TryGetSubAttribute(0x6);
        ZusiTcp3AttributeAbstract attrLM_M = nodeIndusiState.TryGetSubAttribute(0x7);
        ZusiTcp3AttributeAbstract attrLM_O = nodeIndusiState.TryGetSubAttribute(0x8);

        ZusiTcp3AttributeAbstract attrLM_0500Hz = nodeIndusiState.TryGetSubAttribute(0xA);
        ZusiTcp3AttributeAbstract attrLM_2000Hz = nodeIndusiState.TryGetSubAttribute(0xB);

        ZusiTcp3AttributeAbstract attrLM_H = nodeIndusiState.TryGetSubAttribute(0x17);
        ZusiTcp3AttributeAbstract attrLM_E40 = nodeIndusiState.TryGetSubAttribute(0x18);
        ZusiTcp3AttributeAbstract attrLM_Ende = nodeIndusiState.TryGetSubAttribute(0x19);
        ZusiTcp3AttributeAbstract attrLM_B = nodeIndusiState.TryGetSubAttribute(0x1A);
        ZusiTcp3AttributeAbstract attrLM_Ue = nodeIndusiState.TryGetSubAttribute(0x1B);
        ZusiTcp3AttributeAbstract attrLM_G = nodeIndusiState.TryGetSubAttribute(0x1C);
        ZusiTcp3AttributeAbstract attrLM_EL = nodeIndusiState.TryGetSubAttribute(0x1D);
        ZusiTcp3AttributeAbstract attrLM_V40 = nodeIndusiState.TryGetSubAttribute(0x1E);
        ZusiTcp3AttributeAbstract attrLM_S = nodeIndusiState.TryGetSubAttribute(0x1F);
        ZusiTcp3AttributeAbstract attrLM_PruefStoer = nodeIndusiState.TryGetSubAttribute(0x20);
        ZusiTcp3AttributeAbstract attrVSoll = nodeIndusiState.TryGetSubAttribute(0x21);
        ZusiTcp3AttributeAbstract attrVZiel = nodeIndusiState.TryGetSubAttribute(0x22);
        ZusiTcp3AttributeAbstract attrZielweg = nodeIndusiState.TryGetSubAttribute(0x23);


        if (attrZBResondAsString != null)
          bufferIndusiState.ZBResondAsString  = attrZBResondAsString.DataAsString;
        if (attrLM_1000Hz != null)
          bufferIndusiState.LM_1000Hz = (attrLM_1000Hz.DataAsByte > 0);
        if (attrLM_U != null)
          bufferIndusiState.LM_U = (attrLM_U.DataAsByte > 0);
        if (attrLM_M != null)
          bufferIndusiState.LM_M = (attrLM_M.DataAsByte > 0);
        if (attrLM_O != null)
          bufferIndusiState.LM_O = (attrLM_O.DataAsByte > 0);

        if (attrLM_0500Hz != null)
          bufferIndusiState.LM_0500Hz = (attrLM_0500Hz.DataAsByte > 0);
        if (attrLM_2000Hz != null)
          bufferIndusiState.LM_2000Hz = (attrLM_2000Hz.DataAsByte > 0);

        if (attrLM_H != null)
          bufferIndusiState.LM_H = (attrLM_H.DataAsByte > 0);
        if (attrLM_E40 != null)
          bufferIndusiState.LM_E40 = (attrLM_E40.DataAsByte > 0);
        if (attrLM_Ende != null)
          bufferIndusiState.LM_Ende = (attrLM_Ende.DataAsByte > 0);
        if (attrLM_B != null)
          bufferIndusiState.LM_B = (attrLM_B.DataAsByte > 0);
        if (attrLM_Ue != null)
          bufferIndusiState.LM_Ue = (attrLM_Ue.DataAsByte > 0);
        if (attrLM_G != null)
          bufferIndusiState.LM_G = (attrLM_G.DataAsByte > 0);
        if (attrLM_EL != null)
          bufferIndusiState.LM_EL = (attrLM_EL.DataAsByte > 0);
        if (attrLM_V40 != null)
          bufferIndusiState.LM_V40 = (attrLM_V40.DataAsByte > 0);
        if (attrLM_S != null)
          bufferIndusiState.LM_S = (attrLM_S.DataAsByte > 0);
        if (attrLM_PruefStoer != null)
          bufferIndusiState.LM_PruefStoer = (attrLM_PruefStoer.DataAsByte > 0);
        if (attrVSoll != null)
          bufferIndusiState.vSoll = attrVSoll.DataAsSingle;
        if (attrVZiel != null)
          bufferIndusiState.vZiel = attrVZiel.DataAsSingle;
        if (attrZielweg != null)
          bufferIndusiState.Zielweg = attrZielweg.DataAsSingle;
      }

      bufferZugsicherung.ConfigIndusi = bufferIndusiConfig;
      bufferZugsicherung.StateIndusi = bufferIndusiState;

      return new ExtractedValue<Zugsicherung> (0, bufferZugsicherung);
    }
    private Zugsicherung.IndusiConfig.ConfigData readConfig(ZusiTcp3Node input)
    {
      var d = new Zugsicherung.IndusiConfig.ConfigData();
      ZusiTcp3AttributeAbstract attrBRH = input.TryGetSubAttribute(0x1);
      ZusiTcp3AttributeAbstract attrBRA = input.TryGetSubAttribute(0x2);
      ZusiTcp3AttributeAbstract attrZL = input.TryGetSubAttribute(0x3);
      ZusiTcp3AttributeAbstract attrVMZ = input.TryGetSubAttribute(0x4);
      ZusiTcp3AttributeAbstract attrZugart = input.TryGetSubAttribute(0x5);
      ZusiTcp3AttributeAbstract attrMode = input.TryGetSubAttribute(0x6);
      if (attrBRH != null)
        d.BRH = attrBRH.DataAsInt16;
      if (attrBRA != null)
        d.BRA = attrBRA.DataAsInt16;
      if (attrZL != null)
        d.ZL = attrZL.DataAsInt16;
      if (attrVMZ != null)
        d.VMZ = attrVMZ.DataAsInt16;
//      if (attrZugart != null)
//        bufferCommonData. = attrZugart.;
//      if (attrMode != null)
//        bufferCommonData. = attrMode.;
      return d;
    }

    DoorSystem bufferDoorSystem;
    /// <summary>
    ///   Reads incoming DoorSystem status information.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    protected ExtractedValue<DoorSystem> ReadDoorSystem(ZusiTcp3Node input)
    {
      ZusiTcp3AttributeAbstract attrDoorSystemName = input.TryGetSubAttribute(0x1);
      ZusiTcp3AttributeAbstract attrStatusLeft = input.TryGetSubAttribute(0x2);
      ZusiTcp3AttributeAbstract attrStatusRight = input.TryGetSubAttribute(0x3);
      ZusiTcp3AttributeAbstract attrMotorLocked = input.TryGetSubAttribute(0x4);
      ZusiTcp3AttributeAbstract attrSchalterUnlockDoors = input.TryGetSubAttribute(0x5);
      ZusiTcp3AttributeAbstract attrLMDoorsLeft = input.TryGetSubAttribute(0x6);
      ZusiTcp3AttributeAbstract attrLMDoorsRight = input.TryGetSubAttribute(0x7);
      ZusiTcp3AttributeAbstract attrLMDoorsForce = input.TryGetSubAttribute(0xB);
      ZusiTcp3AttributeAbstract attrLMDoorsBoth = input.TryGetSubAttribute(0xC);

      if (attrDoorSystemName != null)
        bufferDoorSystem.DoorSystemName = attrDoorSystemName.DataAsString;
      if (attrStatusLeft != null)
        bufferDoorSystem.StatusLeft = DoorSystem.DoorStateFromByte(attrStatusLeft.DataAsByte);
      if (attrStatusRight != null)
        bufferDoorSystem.StatusRight = DoorSystem.DoorStateFromByte(attrStatusRight.DataAsByte);
      if (attrMotorLocked != null)
        bufferDoorSystem.MotorLocked = (attrMotorLocked.DataAsByte > 0);
      if (attrSchalterUnlockDoors != null)
        bufferDoorSystem.Schalter_UnlockDoorsLeft = ((attrSchalterUnlockDoors.DataAsByte % 2) > 0);
      if (attrSchalterUnlockDoors != null)
        bufferDoorSystem.Schalter_UnlockDoorsRight = (((attrSchalterUnlockDoors.DataAsByte / 2) % 2) > 0);
      if (attrLMDoorsLeft != null)
        bufferDoorSystem.LM_DoorsLeft = (attrLMDoorsLeft.DataAsByte > 0);
      if (attrLMDoorsRight != null)
        bufferDoorSystem.LM_DoorsRight = (attrLMDoorsRight.DataAsByte > 0);
      if (attrLMDoorsForce != null)
        bufferDoorSystem.LM_DoorsForce = (attrLMDoorsForce.DataAsByte > 0);
      if (attrLMDoorsBoth != null)
        bufferDoorSystem.LM_DoorsBoth = (attrLMDoorsBoth.DataAsByte > 0);

      return new ExtractedValue<DoorSystem> (0, bufferDoorSystem);
    }

    /// <summary>
    ///   Sends a texture to the simulator.
    /// </summary>
    /// <param name="imageData">The bytes representing the image.</param>
    /// <param name="forView">Index of the view.</param>
    /// <param name="melderName">The name of the destination polygon.</param>
    /// <param name="subIndex">The index of the "Stellung".</param>
    /// <param name="textureIndex">The index of the texture.</param>
    public void SendTexture(byte[] imageData, byte forView, string melderName, System.Int16 subIndex, byte textureIndex /*= 0*/)
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node graphicNode = pultData.AddSubNode(0x10C);
      graphicNode.AddSubAttribute(0x1).DataAsByte = forView;
      graphicNode.AddSubAttribute(0x2).DataAsString = melderName;
      graphicNode.AddSubAttribute(0x3).DataAsInt16 = subIndex;
      graphicNode.AddSubAttribute(0x4).DataAsByte = textureIndex;
      graphicNode.AddSubAttribute(0x5).Data = imageData;
      socket.SendKnoten(pultData);
    }
    /// <summary>
    ///   Modifyes the pause key.
    /// </summary>
    /// <param name="action">The action doing the pause key.</param>
    public void SendPause(SwitchAction action)
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node controlNode = pultData.AddSubNode(0x10B);
      ZusiTcp3Node pauseNode = controlNode.AddSubNode(0x1);
      pauseNode.AddSubAttribute(0x1).DataAsInt16 = (System.Int16) action;
      socket.SendKnoten(pultData);
    }
    /// <summary>
    ///   Restarts the programe using the specifyed train.
    /// </summary>
    /// <param name="path">The path to the train.</param>
    public void RestartPrograme(string trainPath)
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node controlNode = pultData.AddSubNode(0x10B);
      ZusiTcp3Node restartNode = controlNode.AddSubNode(0x2);
      restartNode.AddSubAttribute(0x1).DataAsString = trainPath;
      socket.SendKnoten(pultData);
    }
    /// <summary>
    ///   Starts the following train when the programe is not already in an active simulation.
    /// </summary>
    /// <param name="path">The path to the train.</param>
    public void SimulationStart(string trainPath)
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node controlNode = pultData.AddSubNode(0x10B);
      ZusiTcp3Node startNode = controlNode.AddSubNode(0x3);
      startNode.AddSubAttribute(0x1).DataAsString = trainPath;
      socket.SendKnoten(pultData);
    }
    /// <summary>
    ///   Stops the simulation
    /// </summary>
    public void SimulationStop()
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node controlNode = pultData.AddSubNode(0x10B);
      ZusiTcp3Node stopNode = controlNode.AddSubNode(0x4);
      socket.SendKnoten(pultData);
    }
    /// <summary>
    ///   Reload current timetable
    /// </summary>
    public void TimetabeRestart()
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node controlNode = pultData.AddSubNode(0x10B);
      ZusiTcp3Node ttrNode = controlNode.AddSubNode(0x5);
      socket.SendKnoten(pultData);
    }
    /// <summary>
    ///   Starts the following train when the timetable is already loaded.
    /// </summary>
    /// <param name="trainNumber">The number of the train.</param>
    public void TrainStart(string trainNumber)
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node controlNode = pultData.AddSubNode(0x10B);
      ZusiTcp3Node startNode = controlNode.AddSubNode(0x6);
      startNode.AddSubAttribute(0x1).DataAsString = trainNumber;
      socket.SendKnoten(pultData);
    }
    /// <summary>
    ///   Modifyes the Zeitsprung key.
    /// </summary>
    /// <param name="action">The action doing the Zeitsprung key.</param>
    public void SendZeitsprung(SwitchAction action)
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node controlNode = pultData.AddSubNode(0x10B);
      ZusiTcp3Node zeitsprungNode = controlNode.AddSubNode(0x7);
      zeitsprungNode.AddSubAttribute(0x1).DataAsInt16 = (System.Int16) action;
      socket.SendKnoten(pultData);
    }
    /// <summary>
    ///   Modifyes the Zeitraffer key.
    /// </summary>
    /// <param name="action">The action doing the Zeitraffer key.</param>
    public void SendZeitraffer(SwitchAction action)
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node controlNode = pultData.AddSubNode(0x10B);
      ZusiTcp3Node zeitrafferNode = controlNode.AddSubNode(0x8);
      zeitrafferNode.AddSubAttribute(0x1).DataAsInt16 = (System.Int16) action;
      socket.SendKnoten(pultData);
    }
    /// <summary>
    ///   Modifyes the Helligkeit.
    /// </summary>
    /// <param name="value">0 = dark, 1 = sun</param>
    public void SetLight(float value)
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node controlNode = pultData.AddSubNode(0x10B);
      ZusiTcp3Node zeitrafferNode = controlNode.AddSubNode(0xA);
      zeitrafferNode.AddSubAttribute(0x1).DataAsSingle = value;
      socket.SendKnoten(pultData);
    }
    /// <summary>
    ///   Modifyes the Autopilot key.
    /// </summary>
    /// <param name="value">m able to look</param>
    public void SetFog(float value)
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node controlNode = pultData.AddSubNode(0x10B);
      ZusiTcp3Node zeitrafferNode = controlNode.AddSubNode(0x9);
      zeitrafferNode.AddSubAttribute(0x1).DataAsSingle = value;
      socket.SendKnoten(pultData);
    }
    /// <summary>
    ///   Modifyes the Autopilot key.
    /// </summary>
    /// <param name="value">typical values: 0.4</param>
    public void SetFriction(float value)
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node controlNode = pultData.AddSubNode(0x10B);
      ZusiTcp3Node zeitrafferNode = controlNode.AddSubNode(0xB);
      zeitrafferNode.AddSubAttribute(0x1).DataAsSingle = value;
      socket.SendKnoten(pultData);
    }
    /// <summary>
    ///   Modifyes the Autopilot key.
    /// </summary>
    /// <param name="action">The action doing the Autopilot key.</param>
    public void SendKIActive(SwitchAction action)
    {
      var pultData = new ZusiTcp3Node();
      pultData.ID = 2;
      ZusiTcp3Node controlNode = pultData.AddSubNode(0x10B);
      ZusiTcp3Node zeitrafferNode = controlNode.AddSubNode(0xC);
      zeitrafferNode.AddSubAttribute(0x1).DataAsInt16 = (System.Int16) action;
      socket.SendKnoten(pultData);
    }


  }
  /*[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public struct BoolAndSingleStruct
  {
    public BoolAndSingleStruct(int lng, bool retVal, float pz80Val) : this()
    {
      ExtractedLength = lng;
      ExtractedData = retVal;
      PZ80Data = pz80Val;
    }

    ///<summary>The length, that was neccessary to extract the data.</summary>
    [Zusi_Datenausgabe.Compyling.ExtractedLengthAttribute()]
    public int ExtractedLength {private set; get;}

    ///<summary>The data, that was extracted.</summary>
    [Zusi_Datenausgabe.Compyling.ExtractedDataAttribute()]
    public bool ExtractedData {private set; get;}

    ///<summary>The data, that was extracted.</summary>
    [Zusi_Datenausgabe.Compyling.ExtractedDataAttribute()]
    public float PZ80Data {private set; get;}
  }*/
}
