using System;
using System.Text;

namespace Zusi_Datenausgabe
{
  public static class PacketDeserialiser
  {

    /// <summary>
    ///   Reads incoming data of type Single.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    public static ExtractedValue<float> ReadSingle(IBinaryReader input)
    {
      return new ExtractedValue<float>(sizeof (Single), input.ReadSingle());
    }

    /// <summary>
    ///   Reads incoming data of type Int.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    public static ExtractedValue<int> ReadInt(IBinaryReader input)
    {
      return new ExtractedValue<int>(sizeof (Int32), input.ReadInt32());
    }

    /// <summary>
    ///   Reads incoming data of type String. This impentation forwards it to HandleDATA_ByteLengthString.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    public static ExtractedValue<string> ReadString(IBinaryReader input)
    {
      return ReadByteLengthString(input);
    }

    /// <summary>
    ///   Reads incoming data of Strings with given Length.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    public static ExtractedValue<string> ReadByteLengthString(IBinaryReader input)
    {
      string value = input.ReadString();
      return new ExtractedValue<string>(value.Length + 1, value);
    }

    /// <summary>
    ///   Reads incoming data of Null-Terminated String.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    public static ExtractedValue<string> ReadNullString(IBinaryReader input)
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

      return new ExtractedValue<string> (bytesRead, stringBuilder.ToString());
    }

    /// <summary>
    ///   Reads incoming data of type DateTime.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    public static ExtractedValue<DateTime> ReadDateTime(IBinaryReader input)
    {
      // Delphi uses the double-based OLE Automation date for its date format.
      double temp = input.ReadDouble();
      DateTime time = DateTime.FromOADate(temp);

      return new ExtractedValue<DateTime> (sizeof (Double), time);
    }

    /// <summary>
    ///   Reads incoming data of type Bool that are sent as Single values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    public static ExtractedValue<bool> ReadBoolAsSingle(IBinaryReader input)
    {
      /* Data is delivered as Single values that are only either 0.0 or 1.0.
       * For the sake of logic, convert these to actual booleans here.
       */
      Single temp = input.ReadSingle();
      bool value = (temp >= 0.5f);

      return new ExtractedValue<bool> (sizeof (Single), value);
    }

    /// <summary>
    ///   Reads incoming data that is sent as Single values by Zusi and can
    ///   be a bool value as well as a single value.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    public static BoolAndSingleStruct ReadBoolAndSingle(IBinaryReader input)
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
    public static ExtractedValue<int> ReadIntAsSingle(IBinaryReader input)
    {
      /* Data is delivered as Single values that are only either 0.0 or 1.0.
       * For the sake of logic, convert these to actual booleans here.
       */
      Single temp = input.ReadSingle();
      int value = (int) Math.Round(temp);

      return new ExtractedValue<int>(sizeof (Single), value);
    }

    /// <summary>
    ///   Reads incoming data of type Bool that are sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    public static ExtractedValue<bool> ReadBoolAsInt(IBinaryReader input)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
      Int32 temp = input.ReadInt32();
      bool value = (temp == 1);

      return new ExtractedValue<bool> (sizeof (Int32), value);
    }

    /// <summary>
    ///   Reads incoming door state data that is sent as an Int value by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    public static ExtractedValue<DoorState> ReadDoorsAsInt(IBinaryReader input)
    {
      Int32 temp = input.ReadInt32();
      return new ExtractedValue<DoorState> (sizeof (Int32), (DoorState) temp);
    }

    /// <summary>
    ///   Reads incoming PZB status information that is sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    public static ExtractedValue<PZBSystem> ReadPZBAsInt(IBinaryReader input)
    {
      Int32 temp = input.ReadInt32();
      return new ExtractedValue<PZBSystem> (sizeof (Int32), (PZBSystem) temp);
    }

    /// <summary>
    ///   Reads incoming brake information that is sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    public static ExtractedValue<BrakeConfiguration> ReadBrakesAsInt(IBinaryReader input)
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

      return new ExtractedValue<BrakeConfiguration> (sizeof (Int32), result);
    }
  }
}