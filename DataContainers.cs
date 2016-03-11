#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Zusi_Datenausgabe
{
  ///<summary>Represents data, that have been read form a Source.</summary>
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public struct ExtractedValue<T>
  {
    public ExtractedValue(int lng, T retVal) : this()
    {
      ExtractedLength = lng;
      ExtractedData = retVal;
    }

    ///<summary>The length, that was neccessary to extract the data.</summary>
    [Zusi_Datenausgabe.Compyling.ExtractedLengthAttribute()]
    public int ExtractedLength {private set; get;}

    ///<summary>The data, that was extracted.</summary>
    [Zusi_Datenausgabe.Compyling.ExtractedDataAttribute()]
    public T ExtractedData {private set; get;}
  }

  /// <summary>
  ///   Represents a dictionary class that translates names of Zusi measurements to their internal numbers.
  /// </summary>
  /// <typeparam name="TMeasure">Key parameter. (ID list: The measurement's name)</typeparam>
  /// <typeparam name="TValue">Value parameter. (ID list: The measurement's ID)</typeparam>
  [Serializable]
  public class ZusiData<TMeasure, TValue> : IEnumerable<KeyValuePair<TMeasure, TValue>>
  {
    private readonly Dictionary<TMeasure, TValue> _data = new Dictionary<TMeasure, TValue>();

    /// <summary>
    ///   Create a new ZusiData instance using source as the back-end storage object.
    ///   NOTE: The source object is used directly instead of duplicating its contents.
    /// </summary>
    /// <param name="source"></param>
    public ZusiData(Dictionary<TMeasure, TValue> source)
    {
      _data = source;
    }

    /// <summary>
    ///   Equivalent to this[] on a regular <see cref="Dictionary{TKey,TValue}" />
    /// </summary>
    /// <param name="id">ID of the measure.</param>
    /// <returns>The value of the measure.</returns>
    public TValue this[TMeasure id]
    {
      get { return _data[id]; }
      internal set { _data[id] = value; }
    }

    /// <summary>
    ///   Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    ///   A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    public IEnumerator<KeyValuePair<TMeasure, TValue>> GetEnumerator()
    {
      return _data.GetEnumerator();
    }

    /// <summary>
    ///   Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    ///   An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  /// <summary>
  ///   Represents the delegate type required for event handling. Used to transfer incoming data sets to the client application.
  /// </summary>
  /// <param name="data">Contains the new dataset.</param>
  /// <param name="sender">Contains the object triggering the event.</param>
  public delegate void ReceiveEvent<T>(object sender, DataSet<T> data);

  /// <summary>
  ///   Represents the delegate type required for handling a new client connected to the server.
  /// </summary>
  /// <param name="data">Contains the new dataset.</param>
  /// <param name="sender">Contains the new Client.</param>
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public delegate void ClientConnectedEvent(object sender, ZusiTcpBaseConnection data);

  /// <summary>
  ///   Represents the delegate type required for error event handling. Used to handle exceptions that occur in the reception thread.
  /// </summary>
  /// <param name="ex">Contains the exception that has occured.</param>
  /// <param name="sender">Contains the object triggering the event.</param>
  public delegate void ErrorEvent(object sender, ZusiTcpException ex);

  /// <summary>
  ///   Represents a structure containing the key and value of one dataset received via the TCP interface.
  /// </summary>
  /// <typeparam name="T">
  ///   Type of this data set. May be <see cref="float" />, <see cref="string" /> or <see cref="byte" />[]
  /// </typeparam>
  public struct DataSet<T>
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="DataSet{T}" /> structure and fills the Id and Value fields with the values
    ///   passed to the id and value parameters respectively.
    /// </summary>
    /// <param name="id">The id number of the measurement.</param>
    /// <param name="value">The value of the measurement.</param>
    public DataSet(int id, T value)
      : this()
    {
      Id = id;
      Value = value;
    }

    /// <summary>
    ///   Gets the Zusi ID number of this dataset.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    ///   Gets the new value of this dataset.
    /// </summary>
    public T Value { get; private set; }
  }

  /// <summary>
  ///   Mirrors the various door states modeled by Zusi.
  /// </summary>
  public enum DoorState
  {
    /// <summary>
    ///   Doors are released and can be opened when the train stops. (Not in Zusi 3, equal value to Opening)
    /// </summary>
    Released = 0,

    /// <summary>
    ///   Doors are open.
    /// </summary>
    Open = 1,

    /// <summary>
    ///   The conductor/PA system is announcing the imminent departure of the train. (Not in Zusi 3)
    /// </summary>
    Announcement = 2,

    /// <summary>
    ///   For door control systems of type "Selbstabfertigung" (= Handled by the train driver),
    ///   the doors can now be closed.
    /// </summary>
    ReadyToClose = 3,

    /// <summary>
    ///   The doors are in the process of closing.
    /// </summary>
    Closing = 4,

    /// <summary>
    ///   The doors are closed but the train is not yet allowed to depart. (In Zusi 3: The doors are closed only.)
    /// </summary>
    Closed = 5,

    /// <summary>
    ///   The train is allowed to depart. (Not in Zusi 3)
    /// </summary>
    Depart = 6,

    /// <summary>
    ///   The doors are closed and locked.
    /// </summary>
    Locked = 7,

    /// <summary>
    ///   Doors are in the process of opening. (Not in Zusi 2, equal value to Released)
    /// </summary>
    Opening = 0,

    /// <summary>
    ///   Door system has detected malfunction. (Not in Zusi 2)
    /// </summary>
    Error = -2,

  }

  /// <summary>
  ///   Mirrors the various PZB types modelled by Zusi.
  /// </summary>
  public enum PZBSystem
  {
    /// <summary>
    ///   The train does not have any PZB system.
    /// </summary>
    None = 0,

    /// <summary>
    ///   The train is equipped with Indusi H54.
    /// </summary>
    IndusiH54 = 1,

    /// <summary>
    ///   The train is equipped with Indusi I54.
    /// </summary>
    IndusiI54 = 2,

    /// <summary>
    ///   The train is equipped with Indusi I60.
    /// </summary>
    IndusiI60 = 3,

    /// <summary>
    ///   The train is equipped with Indusi I60R.
    /// </summary>
    IndusiI60R = 4,

    /// <summary>
    ///   The train is equipped with PZB 90 V1.5.
    /// </summary>
    PZB90V15 = 5,

    /// <summary>
    ///   The train is equipped with PZB 90 V1.6.
    /// </summary>
    PZB90V16 = 6,

    /// <summary>
    ///   The train is equipped with PZ80.
    /// </summary>
    PZ80 = 7,

    /// <summary>
    ///   The train is equipped with PZ80R.
    /// </summary>
    PZ80R = 8,

    /// <summary>
    ///   The train is equipped with LZB80/I80.
    /// </summary>
    LZB80I80 = 9,

    /// <summary>
    ///   The train is equipped with SBB Signum.
    /// </summary>
    SBBSignum = 10,
  }

  /// <summary>
  ///   Mirrors the brake pitches modeled by Zusi. For magnetic brake equipment <see cref="BrakeConfiguration" />.
  /// </summary>
  public enum BrakePitch
  {
    /// <summary>
    ///   The train is set to brake pitch (Bremsstellung) G
    /// </summary>
    G,

    /// <summary>
    ///   The train is set to brake pitch (Bremsstellung) P
    /// </summary>
    P,

    /// <summary>
    ///   The train is set to brake pitch (Bremsstellung) R
    /// </summary>
    R,
  }

  /// <summary>
  ///   Mirrors the brake settings modelled by Zusi.
  /// </summary>
  public struct BrakeConfiguration
  {
    /// <summary>
    ///   Indicates whether a magnetic brake is enabled (not active) on the train.
    /// </summary>
    public bool HasMgBrake { get; set; }

    /// <summary>
    ///   Contains the brake pitch used on the train.
    /// </summary>
    public BrakePitch Pitch { get; set; }


    /// <summary>
    ///   Checks if this instance is equal to another.
    /// </summary>
    public override bool Equals(object other)
    {
      if ((other == null) || (other.GetType() != typeof(BrakeConfiguration))) return false; //ToDo: Have a look at examples and may correct the second condition.
      BrakeConfiguration d = (BrakeConfiguration) other;
      return this == d;
    }

    /// <summary>
    ///   Gets a Hash-Code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return (HasMgBrake.GetHashCode() >> 4) ^ Pitch.GetHashCode();
    }

    /// <summary>
    ///   Checks if two configuarations are equal.
    /// </summary>
    public static bool operator == (BrakeConfiguration first, BrakeConfiguration second)
    {
      return first.HasMgBrake == second.HasMgBrake && first.Pitch == second.Pitch;
    }

    /// <summary>
    ///   Checks if two configuarations are inequal.
    /// </summary>
    public static bool operator != (BrakeConfiguration first, BrakeConfiguration second)
    {
      return first.HasMgBrake != second.HasMgBrake || first.Pitch != second.Pitch;
    }
  }

  /// <summary>
  ///   Represents the state of a TCP connection.
  /// </summary>
  public enum ConnectionState
  {
    /// <summary>
    ///   There is no connection to a server.
    /// </summary>
    Disconnected = 0,

    /// <summary>
    ///   A connection to a server has been established.
    /// </summary>
    Connected,

    /// <summary>
    ///   The Connection is trying to connect.
    /// </summary>
    Connecting,

    /// <summary>
    ///   An error has occured. Try disconnecting and then connecting again to solve the problem.
    /// </summary>
    Error,
  }

  /// <summary>
  ///   Represents the priority of the client in the Zusi TCP interface. Determines measurement update freqency.
  /// </summary>
  public enum ClientPriority
  {
    /// <summary>
    ///   Undefined priority.
    /// </summary>
    Undefined = 0,

    /// <summary>
    ///   Reserved for Zusi.
    /// </summary>
    Master = 01,

    /// <summary>
    ///   High priority for control desks and display applications.
    /// </summary>
    High = 02,

    /// <summary>
    ///   Medium priority.
    /// </summary>
    Medium = 03,

    /// <summary>
    ///   Low priority.
    /// </summary>
    Low = 04,

    /// <summary>
    ///   Maximum priority possible.
    /// </summary>
    RealTime = 05
  }

  /// <summary>
  ///   Represents a action performed to a swich.
  /// </summary>
  public enum SwitchAction
  {
    /// <summary>
    ///   Undefined priority.
    /// </summary>
    Off = 0,

    /// <summary>
    ///   Reserved for Zusi.
    /// </summary>
    On = 1,

    /// <summary>
    ///   High priority for control desks and display applications.
    /// </summary>
    Toogle = -1
  }
  
  public struct Notbremssystem
  {
    public string NotbremssystemName {get; set;} //01
    //Status (Complex)
    public bool Ready {get; set;} //03
    public bool Notbremsung {get; set;} //04
    public bool IsTesting {get; set;} //05
  }

  public struct Sifa
  {
    public enum SifaHornStatus
    {
      None = 0,
      Reminder = 1,
      Emergency = 2,
    }
    public string SifaName {get; set;} //01
    public bool OpticalReminderOn {get; set;} //02
    public SifaHornStatus HornStatus {get; set;} //03
    public bool HauptschalterSifa {get; set;} //04
    public bool StoerschalterSifa {get; set;} //05
    public bool AbsperrhahnOffen {get; set;} //06
  }

  public struct Zugsicherung
  {
    public struct IndusiConfig
    {
      public struct ConfigData
      {
        public System.Int16 BRH {get; set;}
        public System.Int16 BRA {get; set;}
        public System.Int16 ZL {get; set;}
        public System.Int16 VMZ {get; set;}
        //Zugart Undefined = 1, U, M, O //5
        //Mode Grunddaten=4 Ersatzdaten=5 Normal=6
      }
      //Zugart Undefined = 1, U, M, O //1
      public string DriverID {get; set;} //2
      public string TrainID {get; set;} //3
      public ConfigData Grunddaten {get; set;} //4
      public ConfigData Ersatzdaten {get; set;} //5
      public ConfigData CurrentData {get; set;} //6
      public bool Hauptschalter {get; set;} //7
      public bool StoerschalterPZB {get; set;} //8
      public bool StoerschalterLZB {get; set;} //9
      public bool AbsperrhahnOffen{get; set;} //A
    }
    public struct IndusiState
    {
      //An, ab, LowPressure, ZD-Eingabe, Normal, Pruef //2
      //ZB-Resond //3
      public string ZBResondAsString {get; set;} //4
      public bool LM_1000Hz {get; set;} //5
      public bool LM_U {get; set;} //6
      public bool LM_M {get; set;} //7
      public bool LM_O {get; set;} //8
      //Hupe-Status
      public bool LM_0500Hz {get; set;} //A
      public bool LM_2000Hz {get; set;} //B
      //MelderbildDecoded //C
      //Zustand LZB
      //LZB-Ende
      //E40
      //FfGT
      //V40
      //B40
      //Ü-Ausfall
      //Nothalt
      //Rechnerausfall
      //LZB-EL
      public bool LM_H {get; set;} //17
      public bool LM_E40 {get; set;} //18
      public bool LM_Ende {get; set;} //19
      public bool LM_B {get; set;} //1A
      public bool LM_Ue {get; set;} //1B
      public bool LM_G {get; set;} //1C
      public bool LM_EL {get; set;} //1D
      public bool LM_V40 {get; set;} //1E
      public bool LM_S {get; set;} //1F
      public bool LM_PruefStoer {get; set;} //20
      public float vSoll {get; set;} //21
      public float vZiel {get; set;} //22
      public float Zielweg {get; set;} //23
      //G-Blinkbar
      //PruefStoer-Blinkbar
      //CIR-ELKE
      //Anzeige-Spezial
      //Funktionspruefung
    }
    public string ZugsicherungName {get; set;}
    public IndusiConfig ConfigIndusi {get; set;}
    public IndusiState StateIndusi {get; set;}
  }

  public struct DoorSystem
  {
    public static byte DoorStateToByte(DoorState value)
    {
      switch (value)
      {
      case DoorState.Closed:
        return 0;
      case DoorState.Opening:
        return 1;
      case DoorState.Open:
        return 2;
      case DoorState.ReadyToClose:
        return 3;
      case DoorState.Closing:
        return 4;
      case DoorState.Error:
        return 5;
      case DoorState.Locked: //ToDo: Same meaning as Locked?
        return 6;
      default:
        throw new System.ArgumentOutOfRangeException();
      }
    }
    public static DoorState DoorStateFromByte(byte value)
    {
      switch (value)
      {
      case 0:
        return DoorState.Closed;
      case 1:
        return DoorState.Opening;
      case 2:
        return DoorState.Open;
      case 3:
        return DoorState.ReadyToClose;
      case 4:
        return DoorState.Closing;
      case 5:
        return DoorState.Error;
      case 6:
        return DoorState.Locked; //ToDo: Same meaning as Locked?
      default:
        throw new System.ArgumentOutOfRangeException();
      }
    }
    public string DoorSystemName {get; set;} //01
    public DoorState StatusLeft {get; set;} //02
    public DoorState StatusRight {get; set;} //03
    public bool MotorLocked {get; set;} //04
    public bool Schalter_UnlockDoorsLeft {get; set;} //05, 1,2
    public bool Schalter_UnlockDoorsRight {get; set;} //05, 2,3
    public bool LM_DoorsLeft {get; set;} //06
    public bool LM_DoorsRight {get; set;} //07
    //Status => Komplex
    public bool LM_DoorsForce {get; set;} //0B
    //Status => Komplex
    public bool LM_DoorsBoth {get; set;} //0C
    
    public DoorState Zusi2State
    {
      get
      {
        DoorState state1 = DoorStateFromByte(System.Math.Max(
            DoorStateToByte(StatusLeft), DoorStateToByte(StatusRight)));
        if (state1 == DoorState.Opening)
          state1 = DoorState.Open;
        if (state1 == DoorState.Error)
          state1 = DoorState.Open;
        if ((state1 == DoorState.Closed) && Schalter_UnlockDoorsLeft && Schalter_UnlockDoorsRight)
          state1 = DoorState.Released;
        return state1;
      }
    }
  }


}

