using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Zusi_Datenausgabe.EventManager;

namespace Zusi_Datenausgabe
{
  // TODO: DIfy
  public interface IDataReceptionHandler
  {
    IBinaryReader ClientReader
    {
      [DebuggerStepThrough]
      get;

      [DebuggerStepThrough]
      set;
    }

    int HandleData(ICommandEntry curCommand, int curID);
    MethodInfo GetHandlerMethod(ICommandEntry curCommand, int curID);
  }

  public class DataReceptionHandler : IDataReceptionHandler
  {
    private readonly SynchronizationContext _hostContext;
    private readonly IDictionary<string,MethodInfo> _dataHandlers;
    private IBinaryReader _clientReader;
    private IEventInvoker<int> _eventInvoker;

    public IBinaryReader ClientReader
    {
      [DebuggerStepThrough]
      get { return _clientReader; }

      [DebuggerStepThrough]
      set { _clientReader = value; }
    }

    public DataReceptionHandler(SynchronizationContext hostContext, IEventInvoker<int> eventInvoker,
      IDictionary<string, MethodInfo> dataHandlers)
    {
      _hostContext = hostContext;
      _dataHandlers = dataHandlers;
      _eventInvoker = eventInvoker;
    }

    public int HandleData(ICommandEntry curCommand, int curID)
    {
      ValidateClientReader();

      MethodInfo handlerMethod = GetHandlerMethod(curCommand, curID);

      int bytesRead = (int)handlerMethod.Invoke(this, new object[] { _clientReader, curID });
      return bytesRead;
    }

    private void ValidateClientReader()
    {
      Debug.Assert(_clientReader != null);
    }

    public MethodInfo GetHandlerMethod(ICommandEntry curCommand, int curID)
    {
      MethodInfo handlerMethod;

      if (_dataHandlers.TryGetValue(curCommand.Type, out handlerMethod))
        return handlerMethod;

      handlerMethod = ReflectHandlerMethod(curCommand, curID);

      _dataHandlers.Add(curCommand.Type, handlerMethod);
      return handlerMethod;
    }

    private MethodInfo ReflectHandlerMethod(ICommandEntry curCommand, int curID)
    {
      MethodInfo handlerMethod = GetType().GetMethod(
        String.Format("HandleDATA_{0}", curCommand.Type),
        BindingFlags.Instance | BindingFlags.NonPublic,
        null,
        new[] { typeof(IBinaryReader), typeof(int) },
        null);

      if (handlerMethod == null)
      {
        throw new ZusiTcpException(
          String.Format(
            "Unknown type {0} for DATA ID {1} (\"{2}\") occured.", curCommand.Type, curID, curCommand.Name));
      }

      /* Make sure the handler method returns an int. */
      Debug.Assert(handlerMethod.ReturnType == typeof(int));
      return handlerMethod;
    }

    #region DATA handlers
    /// <summary>
    /// When you have received a data packet from the server and are done
    /// processing it in a HandleDATA-routine, call PostToHost() to trigger
    /// an event for this type.
    /// </summary>
    /// <typeparam name="T">Contains the data type for which the event is thrown.
    /// This can be safely ommitted.</typeparam>
    /// <param name="id">Contains the Zusi command ID.</param>
    /// <param name="value">Contains the new value of the measure.</param>
    public void PostToHost<T>(int id, T value)
    {
      _hostContext.Post(EventMarshal<T>, new MarshalArgs<T>(id, value));
    }

    /// <summary>
    /// Handle incoming data of type Single.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_Single(IBinaryReader input, int id)
    {
      PostToHost(id, input.ReadSingle());

      return sizeof(Single);
    }

    /// <summary>
    /// Handle incoming data of type Int.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_Int(IBinaryReader input, int id)
    {
      PostToHost(id, input.ReadInt32());

      return sizeof(Int32);
    }

    /// <summary>
    /// Handle incoming data of type String.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_String(IBinaryReader input, int id)
    {
      const int lengthPrefixSize = 1;

      string result = input.ReadString();
      PostToHost(id, result);

      return result.Length + lengthPrefixSize;
    }

    /// <summary>
    /// Handle incoming data of type DateTime.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_DateTime(IBinaryReader input, int id)
    {
      // Delphi uses the double-based OLE Automation date for its date format.
      double temp = input.ReadDouble();
      DateTime time = DateTime.FromOADate(temp);

      PostToHost(id, time);

      return sizeof(Double);
    }

    /// <summary>
    /// Handle incoming data of type Bool that are sent as Single values by Zusi.
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
      PostToHost(id, value);

      return sizeof(Single);
    }

    /// <summary>
    /// Handle incoming data that is sent as Single values by Zusi and can
    /// be a bool value as well as a single value.
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
      PostToHost(id, value);
      PostToHost(id, temp);

      return sizeof(Single);
    }

    /// <summary>
    /// Handle incoming data of type Int that are sent as Single values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_IntAsSingle(IBinaryReader input, int id)
    {
      /* Data is delivered as Single values that are only either 0.0 or 1.0.
       * For the sake of logic, convert these to actual booleans here.
       */
      Single temp = input.ReadSingle();
      int value = (int)Math.Round(temp);
      PostToHost(id, value);

      return sizeof(Single);
    }

    /// <summary>
    /// Handle incoming data of type Bool that are sent as Int values by Zusi.
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
      PostToHost(id, value);

      return sizeof(Int32);
    }

    /// <summary>
    /// Handle incoming door state data that is sent as an Int value by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_DoorsAsInt(IBinaryReader input, int id)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
      Int32 temp = input.ReadInt32();
      PostToHost(id, (DoorState)temp);

      return sizeof(Int32);
    }

    /// <summary>
    /// Handle incoming PZB status information that is sent as Int values by Zusi.
    /// </summary>
    /// <param name="input">The binary reader comprising the input data stream.</param>
    /// <param name="id">Contains the Zusi command id for this packet.</param>
    protected int HandleDATA_PZBAsInt(IBinaryReader input, int id)
    {
      /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
      Int32 temp = input.ReadInt32();
      PostToHost(id, (PZBSystem)temp);

      return sizeof(Int32);
    }

    /// <summary>
    /// Handle incoming brake information that is sent as Int values by Zusi.
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

      PostToHost(id, result);

      return sizeof(Int32);
    }
    #endregion

    private struct MarshalArgs<T>
    {
      public DataReceivedEventArgs<T> Data { get; private set; }

      public MarshalArgs(int id, T data)
        : this()
      {
        Data = new DataReceivedEventArgs<T>(id, data);
      }
    }

    private void EventMarshal<T>(object o)
    {
      var margs = (MarshalArgs<T>)o;
      _eventInvoker.Invoke(margs.Data.Id, this, margs.Data);
    }
  }

  public interface IDataReceptionHandlerFactory
  {
    void Release(IDataReceptionHandler handler);

    IDataReceptionHandler Create(SynchronizationContext hostContext, IEventInvoker<int> eventInvoker);
  }
}