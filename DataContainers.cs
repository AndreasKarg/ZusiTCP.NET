#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Zusi_Datenausgabe
{
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
    ///   Doors are released and can be opened when the train stops.
    /// </summary>
    Released = 0,

    /// <summary>
    ///   Doors are open.
    /// </summary>
    Open = 1,

    /// <summary>
    ///   The conductor/PA system is announcing the imminent departure of the train.
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
    ///   The doors are closed but the train is not yet allowed to depart.
    /// </summary>
    Closed = 5,

    /// <summary>
    ///   The train is allowed to depart.
    /// </summary>
    Depart = 6,

    /// <summary>
    ///   The doors are closed and locked.
    /// </summary>
    Locked = 7,
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
}
