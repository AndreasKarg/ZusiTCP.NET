#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

#endregion

namespace Zusi_Datenausgabe
{
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public abstract class ZusiTcp3ClientAbstract
  {
    private readonly CommandSet _commands;
    private readonly SwitchableReadOnlyList<int> _requestedData = new SwitchableReadOnlyList<int>();
    public string ClientId {get; private set;}
    public string ClientVersion {get; private set;}
    public string ServerVersion {get; private set;}
    public string ServerVerbindungsinfo {get; private set;}

    protected readonly SynchronizationContext HostContext;

    protected ZusiTcp3ClientAbstract(string clientId,
                              string clientVersion,
                              SynchronizationContext hostContext,
                              CommandSet commands)
    {
      if (commands == null)
      {
        throw new ArgumentNullException("commands");
      }
      _commands = commands;
      ClientId = clientId;
      ClientVersion = clientVersion;
      HostContext = hostContext;
    }

    protected ZusiTcp3ClientAbstract(string clientId, string clientVersion, CommandSet commands)
    {
      if (commands == null)
      {
        throw new ArgumentNullException("commands");
      }
      _commands = commands;
      ClientId = clientId;
      ClientVersion = clientVersion;

      if (SynchronizationContext.Current == null)
      {
        throw new ObjectUnsynchronisableException();
      }

      HostContext = SynchronizationContext.Current;
    }

    /// <summary>
    ///   Represents a list of all measurements which will be requested from Zusi on connecting. Add your required measurements
    ///   here before connecting to the server. List is read-only while connected.
    ///   <seealso cref="IDs" />
    /// </summary>
    public virtual ICollection<int> RequestedData
    {
      get { return _requestedData; }
    }


    /// <summary>
    ///   Represents all measurements available in Zusi as a key-value list. Can be used to convert plain text names of
    ///   measurements to their internal ID.
    /// </summary>
    /// <example>
    ///   <code>
    ///  ZusiTcpConn myConn = [...]
    /// 
    ///  int SpeedID = myConn.IDs["Geschwindigkeit"]
    ///  /* SpeedID now contains the value 01. */
    ///  </code>
    /// </example>
    public ZusiData<string, int> IDs
    {
      get { return _commands.IDByName; }
    }

    /// <summary>
    ///   Represents all measurements available in Zusi as a key-value list. Can be used to convert measurement IDs to their
    ///   plain text name.
    /// </summary>
    /// <example>
    ///   <code>
    ///  ZusiTcpConn myConn = [...]
    /// 
    ///  string SpeedName = myConn.ReverseIDs[1] /* ID 01 == current speed */
    ///  /* SpeedName now contains the value "Geschwindigkeit". */
    ///  </code>
    /// </example>
    public ZusiData<int, string> ReverseIDs
    {
      get { return _commands.NameByID; }
    }

    /// <summary>
    ///   Returns the plain text name of the measurement specified by its ID.
    /// </summary>
    /// <param name="id">Internal ID of the measurement.</param>
    /// <returns>Name of the measurement.</returns>
    public string this[int id]
    {
      get { return ReverseIDs[id]; }
    }

    /// <summary>
    ///   Returns the ID of the measurement specified in plain text.
    /// </summary>
    /// <param name="name">Name of the measurement.</param>
    /// <returns>Internal ID of the measurement.</returns>
    public int this[string name]
    {
      get { return IDs[name]; }
    }

    /// <summary>
    ///   Establish a connection to the TCP server.
    /// </summary>
    /// <param name="hostName">The name or IP address of the host.</param>
    /// <param name="port">The port on the server to connect to (Default: 1435).</param>
    /// <exception cref="ArgumentException">
    ///   This exception is thrown when the host address could
    ///   not be resolved.
    /// </exception>
    public void Connect(string hostName, int port)
    {
      var hostAddresses = Dns.GetHostAddresses(hostName);
      IPAddress myAddress;

      /* The TCP server supports only IPv4, so we have to filter out v6. */
      try
      {
        myAddress = hostAddresses.First(x => x.AddressFamily == AddressFamily.InterNetwork);
      }
      catch (Exception ex)
      {
        throw new ArgumentException("Host name could not be resolved.", hostName, ex);
      }

      Connect(new IPEndPoint(myAddress, port));
    }

    /// <summary>
    ///   Establish a connection to the TCP server.
    /// </summary>
    /// <param name="endPoint">Specifies an IP end point to which the interface tries to connect.</param>
    /// <exception cref="ZusiTcpException">
    ///   This exception is thrown when the connection could not be
    ///   established.
    /// </exception>
    protected void Connect(IPEndPoint endPoint)
    {
      TcpClient clientConnection = new TcpClient(AddressFamily.InterNetwork);

      ValidateConnectionState();
      ConnectionState = ConnectionState.Connecting;
      if (RequestedData.Count == 0)
      {
        throw (new ZusiTcpException("No Data marked to be requested. Request Data first."));
      }

      try
      {
        clientConnection.Connect(endPoint);
      }
      catch (SocketException ex)
      {
        throw new ZusiTcpException("Could not establish socket connection to TCP server. " +
                                   "Is the server running and enabled?",
                                   ex);
      }

      InitializeClient(clientConnection);
    }

    /// <summary>
    ///   Request the measurement passed as plain text in the parameter "name" from the server. Shorthand for
    ///   <c>TCP.RequestedData.Add(TCP.IDs[name]);</c>. Must be called before connecting.
    /// </summary>
    /// <param name="name">The name of the measurement.</param>
    /// <exception cref="ZusiTcpException">Thrown, when communciation is not disconnected.</exception>
    public void RequestData(string name)
    {
      if (ConnectionState != ConnectionState.Disconnected)
      {
        throw (new ZusiTcpException("Network state must be \"Disconnect\". Disconnect first!"));
      }
      RequestedData.Add(IDs[name]);
    }

    /// <summary>
    ///   Request the measurement passed as ID in the parameter "id" from the server. Shorthand for
    ///   <c>TCP.RequestedData.Add(id);</c>. Must be called before connecting.
    /// </summary>
    /// <param name="id">The ID of the measurement.</param>
    /// <exception cref="ZusiTcpException">Thrown, when communciation is not disconnected.</exception>
    public void RequestData(int id)
    {
      if (ConnectionState != ConnectionState.Disconnected)
      {
        throw (new ZusiTcpException("Network state must be \"Disconnect\". Disconnect first!"));
      }
      RequestedData.Add(id);
    }

    private TcpClient tcpConnection;
    protected ZusiTcp3Socket socket {get; private set;}
    Dictionary<string, MethodInfo> dataHandlers = null;

    protected void InitializeClient(TcpClient client)
    {
      tcpConnection = client;
      dataHandlers = new Dictionary<string, MethodInfo>();
      socket = new ZusiTcp3Socket();
      socket.ReadException += PostExToHost;
      socket.ProcessKnoten += NodeReceived;
      socket.Stream = client.GetStream();

      var communicationNode = new ZusiTcp3Node();
      communicationNode.ID = 1;
      ZusiTcp3Node helloNode = communicationNode.AddSubNode(1);
      helloNode.AddSubAttribute(1).DataAsInt16 = 2; //Protocol-Version: 2
      helloNode.AddSubAttribute(2).DataAsInt16 = 2; //Client-Type: Fahrpult
      helloNode.AddSubAttribute(3).DataAsString = ClientId;
      helloNode.AddSubAttribute(4).DataAsString = ClientVersion;
      socket.SendKnoten(communicationNode);
    }
    /// <summary>
    ///   Disconnect from the TCP server.
    /// </summary>
    public void Disconnect()
    {
      Disconnect(ConnectionState.Disconnected);
      ServerVersion = "";
      ServerVerbindungsinfo = "";
    }
    private void Disconnect(ConnectionState reason)
    {
      if (socket != null)
      {
        socket.Stream = null;
      }
      socket = null;

      ConnectionState = reason;
      if (tcpConnection != null)
      {
        tcpConnection.Close();
      }
      tcpConnection = null;
      dataHandlers = null;
    }
    protected void ValidateConnectionState()
    {
      if (ConnectionState == ConnectionState.Error)
      {
        throw (new ZusiTcpException("Network state is \"Error\". Disconnect first!"));
      }
    }
    private ConnectionState _connectionState = ConnectionState.Disconnected;
    /// <summary>
    ///   Represents the current connection state of the client.
    /// </summary>
    public virtual ConnectionState ConnectionState
    {
      get { return _connectionState; }
      protected set
      {
        if (_connectionState == value) return; //Connection State did not change.
        _connectionState = value;
        if (HostContext != null)
        {
          HostContext.Post(ConnectMarshal, new EventArgs());
        }
        else
        {
          ConnectMarshal(new EventArgs());
        }
        _requestedData.IsReadOnly = (value == ConnectionState.Connected);
      }
    }


    /// <summary>
    ///   Event called when an error has occured within the TCP interface.
    /// </summary>
    public event ErrorEvent ErrorReceived;

    /// <summary>
    ///   Event called the ConnectionState changed.
    /// </summary>
    public event EventHandler ConnectionState_Changed;

    private void ErrorMarshal(object o)
    {
      if (ErrorReceived == null)
      {
        return;
      }

      ErrorReceived.Invoke(this, (ZusiTcpException) o);
    }

    private void ConnectMarshal(object o)
    {
      if (ConnectionState_Changed == null)
      {
        return;
      }

      ConnectionState_Changed.Invoke(this, (EventArgs) o);
    }

    /// <summary>
    ///   Raises the ErrorReceived-event.
    /// </summary>
    /// <param name="ex">The exception.</param>
    private void PostExToHost(object sender, System.Exception ex0)
    {
      ZusiTcpException ex = new ZusiTcpException(ex0.Message, ex0);
      if (HostContext != null)
      {
        HostContext.Post(ErrorMarshal, ex);
      }
      else
      {
        ErrorMarshal(ex);
      }
    }

    /// <summary>
    ///   When you have received a data packet from the server and are done
    ///   processing it in a HandleDATA-routine, call PostToHost() to trigger
    ///   an event for this type.
    /// </summary>
    /// <typeparam name="T">
    ///   Contains the data type for which the event is thrown.
    ///   This can be safely ommitted.
    /// </typeparam>
    /// <param name="Event">Contains the event that is to be thrown.</param>
    /// <param name="id">Contains the Zusi command ID.</param>
    /// <param name="value">Contains the new value of the measure.</param>
    protected void PostToHost<T>(ReceiveEvent<T> Event, int id, T value)
    {
      if (Event == null)
      {
        return;
      }

      if (HostContext != null)
      {
        HostContext.Post(EventMarshal<T>, new MarshalArgs<T>(Event, id, value));
      }
      else
      {
        EventMarshal<T>(new MarshalArgs<T>(Event, id, value));
      }
    }

    private void EventMarshal<T>(object o)
    {
      var margs = (MarshalArgs<T>) o;
      margs.Event.Invoke(this, margs.Data);
    }
    private struct MarshalArgs<T>
    {
      public MarshalArgs(ReceiveEvent<T> recveiveEvent, int id, T data)
        : this()
      {
        Event = recveiveEvent;
        Data = new DataSet<T>(id, data);
      }

      public ReceiveEvent<T> Event { get; private set; }

      public DataSet<T> Data { get; private set; }
    }


    virtual protected void BeginHANDLE_Datas()
    {
    }
    virtual protected void EndHANDLE_Datas()
    {
    }


    private void NodeReceived(object sender, ZusiTcp3Node data)
    {
        switch (data.ID)
        {
            case 1:
            {
                ZusiTcp3Node nodeAck = data.TryGetSubNode(0x2);
                if (nodeAck == null)
                {
                    break;
                }

                ZusiTcp3AttributeAbstract attrServerVersion = nodeAck.TryGetSubAttribute(0x1);
                ZusiTcp3AttributeAbstract attrServerInfo = nodeAck.TryGetSubAttribute(0x2);
                ZusiTcp3AttributeAbstract attrServerAccepted = nodeAck.TryGetSubAttribute(0x3);

                if (attrServerVersion != null)
                    ServerVersion = attrServerVersion.DataAsString;
                if (attrServerInfo != null)
                    ServerVerbindungsinfo = attrServerInfo.DataAsString;
                if (attrServerAccepted == null)
                {
                    break;
                }
                byte accept = attrServerAccepted.DataAsByte;

                if (accept != 0)
                {
                    PostExToHost(this,
                        new ZusiTcpException(string.Format("Connection not Accepted, error code {0}.", accept)));
                    break;
                }
                
                //If Accepted send the requested data:
                var pultData = new ZusiTcp3Node();
                pultData.ID = 2;
                ZusiTcp3Node needData = pultData.AddSubNode(0x3);
                ZusiTcp3Node fstData = needData.AddSubNode(0xA); //At the moment only 0xA supported.
                foreach (int dataID in _requestedData)
                {
                    fstData.AddSubAttribute(0x1).DataAsInt16 = (System.Int16) dataID;
                }
                socket.SendKnoten(pultData);
            }
                break;
            case 2:
            {
                ZusiTcp3Node nodeAck = data.TryGetSubNode(0x4);
                ZusiTcp3Node nodeFst = data.TryGetSubNode(0xA);
                if (nodeAck != null)
                {
                    ZusiTcp3AttributeAbstract attrDataAccepted = nodeAck.TryGetSubAttribute(0x1);
                    if (attrDataAccepted != null)
                    {
                        byte accept = attrDataAccepted.DataAsByte;
                        if (accept == 0)
                            ConnectionState = ConnectionState.Connected;
                        else
                        {
                            PostExToHost(this, new ZusiTcpException(string.Format("Data not Accepted, error code {0}.", accept)));
                        }
                    }
                }

                if (nodeFst != null)
                {
                    BeginHANDLE_Datas();
                    foreach(ZusiTcp3AttributeAbstract cur in nodeFst.Attributes)
                    {
                        CommandEntry curCommand = _commands[cur.ID];

                        MethodInfo handlerMethod;

                        if (!dataHandlers.TryGetValue(curCommand.Type, out handlerMethod))
                        {
                            handlerMethod = GetType().GetMethod(
                                String.Format("HandleDATA_{0}", curCommand.Type),
                                BindingFlags.Instance | BindingFlags.NonPublic,
                                null,
                                new[] {typeof (ZusiTcp3AttributeAbstract), typeof (int)},
                                null);

                            dataHandlers.Add(curCommand.Type, handlerMethod);
                        }
                        try
                        {
                            handlerMethod.Invoke(this, new object[] {cur, cur.ID});
                        }
                        catch
                        {
                            //Even a failed processing data is no longer a problem.
                        }
                    }
                    foreach(ZusiTcp3Node cur in nodeFst.Nodes)
                    {
                        CommandEntry curCommand = _commands[cur.ID];

                        MethodInfo handlerMethod;

                        if (!dataHandlers.TryGetValue(curCommand.Type, out handlerMethod))
                        {
                            handlerMethod = GetType().GetMethod(
                                String.Format("HandleDATA_{0}", curCommand.Type),
                                BindingFlags.Instance | BindingFlags.NonPublic,
                                null,
                                new[] {typeof (ZusiTcp3Node), typeof (int)},
                                null);

                            dataHandlers.Add(curCommand.Type, handlerMethod);
                        }
                        try
                        {
                            handlerMethod.Invoke(this, new object[] {cur, cur.ID});
                        }
                        catch
                        {
                            //Even a failed processing data is no longer a problem.
                        }
                    }
                    EndHANDLE_Datas();
                }


            }
                break;
        }
    }
  }
}
