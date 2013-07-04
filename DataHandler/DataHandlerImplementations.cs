using System;

namespace Zusi_Datenausgabe.DataHandler
{
  public class DataHandlerForSingle : DataHandlerBase<float>
  {
    #region Overrides of DataHandlerBase<TOutput>

    public override float HandleData(IBinaryReader reader, out int bytesRead)
    {
      bytesRead = sizeof (float);
      return reader.ReadSingle();
    }

    public override string InputType
    {
      get
      {
        return "Single";
      }
    }

    #endregion
  }

  public class DataHandlerForInt : DataHandlerBase<int>
  {
    #region Overrides of DataHandlerBase<TOutput>

    public override int HandleData(IBinaryReader reader, out int bytesRead)
    {
      bytesRead = sizeof(Int32);
      return reader.ReadInt32();
    }

    public override string InputType
    {
      get
      {
        return "Int";
      }
    }

    #endregion
  }

  public class DataHandlerForString : DataHandlerBase<string>
  {
    #region Overrides of DataHandlerBase<TOutput>

    public override string HandleData(IBinaryReader reader, out int bytesRead)
    {
      const int lengthPrefixSize = 1;

      string result = reader.ReadString();

      bytesRead = result.Length + lengthPrefixSize;

      return result;
    }

    public override string InputType
    {
      get
      {
        return "String";
      }
    }

    #endregion
  }

  public class DataHandlerForDateTime : DataHandlerBase<DateTime>
  {
    #region Overrides of DataHandlerBase<TOutput>

    public override DateTime HandleData(IBinaryReader reader, out int bytesRead)
    {
      // Delphi uses the double-based OLE Automation date for its date format.
      bytesRead = sizeof(Double);
      double temp = reader.ReadDouble();

      return DateTime.FromOADate(temp);
    }

    public override string InputType
    {
      get
      {
        return "DateTime";
      }
    }

    #endregion
  }

  public class DataHandlerForBoolAsSingle : DataHandlerBase<bool>
  {
    #region Overrides of DataHandlerBase<TOutput>

    public override bool HandleData(IBinaryReader reader, out int bytesRead)
    {
      /* Data is delivered as Single values that are only either 0.0 or 1.0.
       * For the sake of logic, convert these to actual booleans here.
       */
      bytesRead = sizeof(float);

      float temp = reader.ReadSingle();

      return (temp >= 0.5f);
    }

    public override string InputType
    {
      get
      {
        return "BoolAsSingle";
      }
    }

    #endregion
  }

  public class DataHandlerForBoolAndSingle : DataHandlerBase<Tuple<bool, float>>
  {
    #region Overrides of DataHandlerBase<TOutput>

    public override Tuple<bool, float> HandleData(IBinaryReader reader, out int bytesRead)
    {
      /* Data is delivered as Single values that are usually only either 0.0 or 1.0.
       * In some cases (PZ80!) the values are no Booleans at all, so we just post to both events.
       */
      bytesRead = sizeof(Single);

      float floatVal = reader.ReadSingle();
      bool boolVal = (floatVal >= 0.5f);

      return new Tuple<bool, float>(boolVal, floatVal);
    }

    public override string InputType
    {
      get
      {
        return "BoolAndSingle";
      }
    }

    #endregion
  }

  public class DataHandlerForIntAsSingle : DataHandlerBase<int>
  {
    #region Overrides of DataHandlerBase<TOutput>

    public override int HandleData(IBinaryReader reader, out int bytesRead)
    {
      /* Data is delivered as Single values that are only either 0.0 or 1.0.
       * For the sake of logic, convert these to actual booleans here.
       */
      bytesRead = sizeof(Single);

      float temp = reader.ReadSingle();
      int value = (int)Math.Round(temp);
      return value;
    }

    public override string InputType
    {
      get
      {
        return "IntAsSingle";
      }
    }

    #endregion
  }

  public class DataHandlerForBoolAsInt : DataHandlerBase<bool>
  {
    #region Overrides of DataHandlerBase<TOutput>

    public override bool HandleData(IBinaryReader reader, out int bytesRead)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
       * For the sake of logic, convert these to actual booleans here.
       */
      bytesRead = sizeof(Int32);
      Int32 temp = reader.ReadInt32();
      return (temp == 1);
    }

    public override string InputType
    {
      get
      {
        return "BoolAsInt";
      }
    }

    #endregion
  }

  public class DataHandlerForDoorsAsInt : DataHandlerBase<DoorState>
  {
    #region Overrides of DataHandlerBase<TOutput>

    public override DoorState HandleData(IBinaryReader reader, out int bytesRead)
    {
      /* Data is delivered as Int values comprising the door state.
       * For the sake of logic, convert these to actual booleans here.
       */
      bytesRead = sizeof(Int32);
      Int32 temp = reader.ReadInt32();

      return (DoorState) temp;
    }

    public override string InputType
    {
      get
      {
        return "DoorsAsInt";
      }
    }

    #endregion
  }

  public class DataHandlerForBrakesAsInt : DataHandlerBase<BrakeConfiguration>
  {
    #region Overrides of DataHandlerBase<TOutput>

    public override BrakeConfiguration HandleData(IBinaryReader reader, out int bytesRead)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
       * For the sake of logic, convert these to actual booleans here.
       */
      bytesRead = sizeof(Int32);
      Int32 temp = reader.ReadInt32();

      BrakeConfiguration result;

      switch (temp)
      {
        case 0:
          result = new BrakeConfiguration() { HasMgBrake = false, Pitch = BrakePitch.G };
          break;

        case 1:
          result = new BrakeConfiguration() { HasMgBrake = false, Pitch = BrakePitch.P };
          break;

        case 2:
          result = new BrakeConfiguration() { HasMgBrake = false, Pitch = BrakePitch.R };
          break;

        case 3:
          result = new BrakeConfiguration() { HasMgBrake = true, Pitch = BrakePitch.P };
          break;

        case 4:
          result = new BrakeConfiguration() { HasMgBrake = true, Pitch = BrakePitch.R };
          break;

        default:
          throw new ZusiTcpException("Invalid value received for brake configuration.");
      }

      return result;
    }

    public override string InputType
    {
      get
      {
        return "BrakesAsInt";
      }
    }

    #endregion
  }

  public class DataHandlerForPzbAsInt : DataHandlerBase<PZBSystem>
  {
    #region Overrides of DataHandlerBase<TOutput>

    public override PZBSystem HandleData(IBinaryReader reader, out int bytesRead)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
       * For the sake of logic, convert these to actual booleans here.
       */
      bytesRead = sizeof(Int32);
      Int32 temp = reader.ReadInt32();

      return (PZBSystem) temp;
    }

    public override string InputType
    {
      get
      {
        return "PZBAsInt";
      }
    }

    #endregion
  }
}