/*************************************************************************
 * Zusi-Datenausgabe.cs
 * Contains main logic for the TCP interface.
 * 
 * (C) 2009-2011 Andreas Karg, <Clonkman@gmx.de>
 * 
 * This file is part of Zusi TCP Interface.NET.
 *
 * Zusi TCP Interface.NET is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * Zusi TCP Interface.NET is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Zusi TCP Interface.NET. 
 * If not, see <http://www.gnu.org/licenses/>.
 * 
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using TCPCommandset;

[assembly: CLSCompliant(true)]

namespace Zusi_Datenausgabe
{
    /// <summary>
    /// Represents a dictionary class that translates names of Zusi measurements to their internal numbers.
    /// </summary>
    /// <typeparam name="TMeasure">Der Schlüssel-Parameter. (ID-Liste: Name der Größe)</typeparam>
    /// <typeparam name="TValue">Der Wert-Parameter. (ID-Liste: Die ID-Nummer der Größe)</typeparam>
    [Serializable]
    public class ZusiData<TMeasure, TValue>
    {
        private readonly Dictionary<TMeasure, TValue> _data = new Dictionary<TMeasure, TValue>();

        /// <summary>
        /// Enthält die generischen Werte.
        /// </summary>
        /// <param name="id">Die ID des Werts.</param>
        /// <returns>Einen generischen Wert.</returns>
        public TValue this[TMeasure id]
        {
            get { return _data[id]; }
            internal set { _data[id] = value; }
        }

        /// <summary>
        /// Gibt den Enumerator des zugrundeliegenden Dictionary zurück.
        /// </summary>
        /// <returns>Der Enumerator des zugrundeliegenden Dictionary.</returns>
        public Dictionary<TMeasure, TValue>.Enumerator GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public ZusiData(Dictionary<TMeasure, TValue> source)
        {
            _data = source;
        }
    }

    /// <summary>
    /// Represents the delegate type required for event handling. Used to transfer incoming data sets to the client application.
    /// </summary>
    /// <param name="data">Contains the new dataset.</param>
    /// <param name="sender">Contains the object triggering the event.</param>
    public delegate void ReceiveEvent<T>(object sender, DataSet<T> data);

    /// <summary>
    /// Represents the delegate type required for error event handling. Used to handle exceptions that occur in the reception thread.
    /// </summary>
    /// <param name="ex">Contains the exception that has occured.</param>
    /// <param name="sender">Contains the object triggering the event.</param>
    public delegate void ErrorEvent(object sender, ZusiTcpException ex);

    /// <summary>    
    /// Represents a structure containing the key and value of one dataset received via the TCP interface.
    /// </summary>
    /// <typeparam name="T">Type of this data set. May be <see cref="float"/>, <see cref="string"/> or <see cref="byte"/>[]</typeparam>
    public struct DataSet<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSet{T}"/> structure and fills the Id and Value fields with the values
        /// passed to the id and value parameters respectively.
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
        /// Gets the Zusi ID number of this dataset.
        /// </summary>
        public int Id { get; private set; }


        /// <summary>
        /// Gets the new value of this dataset.
        /// </summary>
        public T Value { get; private set; }
    }

    /// <summary>
    /// Represents the centerpiece of the Zusi TCP interface.
    /// <para>Requirement:
    /// <list type="bullet">
    /// <item><description>
    /// An object implementing the <see cref="ISynchronizeInvoke"/> interface (e.g. a Windows Forms application)
    /// </description></item>
    /// </list></para>
    /// 
    /// <para>Usage:
    /// <list type="number">
    /// <item><description>
    /// Implement event handlers within an object that implements the  <see cref="ISynchronizeInvoke"/> interface (e.g. a Windows Forms application).
    /// These must conform to the <see cref="ReceiveEvent{T}"/> delegate.
    /// The interface uses the types <see cref="float"/>, <see cref="string"/> and <see cref="byte"/>[].
    /// </description></item>
    /// 
    /// <item><description>
    /// Create an instance of <see cref="ZusiTcpConn"/>, choosing a client priority. Recommended value for control desks is "High".
    /// Pass your handler degates to the constructor.
    /// </description></item>
    /// <item><description>
    /// Add the required measurements using <see cref="RequestedData"/>.  
    /// Der Eigenschaft RequestedData (<see cref="ZusiTcpConn.RequestedData"/>) die benötigten Größen als IDs hinzufügen.
    /// Bei Bedarf können die ID-Nummern aus dem deutschen Klartextnamen wie in der Zusi-Dokumentation(z.B. "Geschwindigkeit") direkt aus der Eigenschaft
    /// IDs (<see cref="IDs"/>) entnommen werden.</description></item>
    /// <item><description>
    /// <see cref="Connect"/> to the TCP server.</description></item>
    /// <item><description>As soon as data is coming from the server, the respective events are called automatically, passing one new
    /// dataset at a time.
    /// ToDo: Maybe enable events on a per-measurement basis.</description></item>
    /// </list></para>
    /// </summary>
    public class ZusiTcpConn : IDisposable
    {
        #region Fields
        private readonly SynchronizationContext _hostContext;
        private readonly ASCIIEncoding _stringEncoder = new ASCIIEncoding();

        private readonly List<int> _requestedData = new List<int>();
        private TcpClient _clientConnection = new TcpClient();
        private Thread _streamReaderThread;
        private readonly TCPCommands _commands;
        #endregion

        /// <summary>
        /// Event used to handle incoming string data.
        /// </summary>
        public event ErrorEvent ErrorReceived;

        /// <summary>
        /// Initializes a new <see cref="ZusiTcpConn"/> object that uses the specified event handlers to pass datasets to the client application.
        /// </summary>
        /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
        /// <param name="priority"> Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
        /// <param name="commandsetPath">Path to the XML file containing the command set.</param>
        public ZusiTcpConn(string clientId, ClientPriority priority, String commandsetPath = "commandset.xml")
        {
            ClientId = clientId;
            ClientPriority = priority;

            _hostContext = SynchronizationContext.Current;

            _commands = TCPCommands.LoadFromFile(commandsetPath);
        }

        /// <summary>
        /// Represents the name of the client.
        /// </summary>
        public string ClientId { get; private set; }

        /// <summary>
        /// Represents all measurements available in Zusi as a key-value list. Can be used to convert plain text names of
        /// measurements to their internal ID.
        /// </summary>
        /// <example>
        /// <code>
        /// ZusiTcpConn myConn = [...]
        /// 
        /// int SpeedID = myConn.IDs["Geschwindigkeit"]
        /// /* SpeedID now contains the value 01. */
        /// </code>
        /// </example>
        public ZusiData<string, int> IDs { get { return _commands.IDByName; } }

        /// <summary>
        /// Represents all measurements available in Zusi as a key-value list. Can be used to convert measurement IDs to their
        /// plain text name.
        /// </summary>
        /// <example>
        /// <code>
        /// ZusiTcpConn myConn = [...]
        /// 
        /// string SpeedName = myConn.ReverseIDs[1] /* ID 01 == current speed */
        /// /* SpeedName now contains the value "Geschwindigkeit". */
        /// </code>
        /// </example>
        public ZusiData<int, string> ReverseIDs { get { return _commands.NameByID; } }

        /// <summary>
        /// Represents the current connection state of the client.
        /// </summary>
        public ConnectionState ConnectionState { get; private set; }

        /// <summary>
        /// Represents the priority of the client. Cannot be changed after object creation.
        /// </summary>
        public ClientPriority ClientPriority { get; private set; }

        /// <summary>
        /// Represents a list of all measurements requested from Zusi. Add your required measurements
        /// here before connecting to the server.
        /// <seealso cref="IDs"/>
        /// </summary>
        public List<int> RequestedData
        {
            get {
                return ConnectionState == ConnectionState.Disconnected ? _requestedData : null;
            }
        }

        /// <summary>
        /// Returns the ID of the measurement specified in plain text.
        /// </summary>
        /// <param name="name">Name of the measurement.</param>
        /// <returns>Internal ID of the measurement.</returns>
        public int this[string name]
        {
            get { return IDs[name]; }
        }

        /// <summary>
        /// Returns the plain text name of the measurement specified by its ID.
        /// </summary>
        /// <param name="id">Internal ID of the measurement.</param>
        /// <returns>Name of the measurement.</returns>
        public string this[int id]
        {
            get { return ReverseIDs[id]; }
        }

        #region IDisposable Members

        /// <summary>
        /// Entsorgt die TCP-Verbindung
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void ErrorMarshal(object o)
        {
            ErrorReceived.Invoke(this, (ZusiTcpException)o);
        }

        private void SendPacket(params byte[] message)
        {
            _clientConnection.Client.Send(BitConverter.GetBytes(message.Length));
            _clientConnection.Client.Send(message);
        }

        private void SendPacket(params byte[][] message)
        {
            int iTempLength = message.Sum(item => item.Length);

            _clientConnection.Client.Send(BitConverter.GetBytes(iTempLength));
            
            foreach (var item in message)
            {
                _clientConnection.Client.Send(item);
            }
        }

        private static byte[] Pack(params byte[] message)
        {
            return message;
        }

        private static int GetInstruction(int byteA, int byteB)
        {
            return byteA*256 + byteB;
        }

        /// <summary>
        /// Establishes a connection to the TCP server.
        /// </summary>
        /// <param name="hostName">The name or IP address of the host.</param>
        /// <param name="port">The port on the server to connect to (Default: 1435).</param>
        public void Connect(string hostName, int port)
        {
            try
            {
                if (ConnectionState == ConnectionState.Error)
                    throw
                        (new ZusiTcpException("Network state is \"Error\". Disconnect first!"));

                if (_clientConnection == null)
                    _clientConnection = new TcpClient();

                _clientConnection.Connect(hostName, port);

                if (_clientConnection.Connected)
                {
                    _streamReaderThread = new Thread(ReceiveLoop) {Name = "ZusiData Receiver"};

                    SendPacket(Pack(0, 1, 2, (byte) ClientPriority,
                                    Convert.ToByte(_stringEncoder.GetByteCount(ClientId))),
                               _stringEncoder.GetBytes(ClientId));

                    ExpectResponse(ResponseType.AckHello, 0);

                    var aGetData = from iData in RequestedData
                                   group iData by (iData / 256);

                    var reqDataBuffer = new List<byte[]>();

                    foreach (var aDataGroup in aGetData)
                    {
                        reqDataBuffer.Clear();
                        reqDataBuffer.Add(Pack(0, 3));

                        byte[] tempDataGroup = BitConverter.GetBytes(Convert.ToInt16(aDataGroup.Key));
                        reqDataBuffer.Add(Pack(tempDataGroup[1], tempDataGroup[0]));

                        reqDataBuffer.AddRange(aDataGroup.Select(iID => Pack(Convert.ToByte(iID%256))));

                        SendPacket(reqDataBuffer.ToArray());

                        ExpectResponse(ResponseType.AckNeededData, aDataGroup.Key);
                    }

                    SendPacket(0, 3, 0, 0);

                    ExpectResponse(ResponseType.AckNeededData, 0);

                    ConnectionState = ConnectionState.Connected;

                    _streamReaderThread.Start();
                }
            }

            catch (Exception ex)
            {
                if (_streamReaderThread != null)
                {
                    _streamReaderThread.Abort();
                    _streamReaderThread = null;
                }
                ConnectionState = ConnectionState.Error;
                throw new ZusiTcpException("Error connecting to server.", ex);
            }
        }

        /// <summary>
        /// Disconnect from the TCP server.
        /// </summary>
        public void Disconnnect()
        {
            if (_streamReaderThread != null)
                _streamReaderThread.Abort();
            ConnectionState = ConnectionState.Disconnected;
            if (_clientConnection != null)
                _clientConnection.Close();
            _clientConnection = null;
        }

        private void ExpectResponse(ResponseType expResponse, int dataGroup)
        {
            var readBuffer = new byte[7];

            _clientConnection.Client.Receive(readBuffer, 7, SocketFlags.None);
            MemoryStream bufStream = null;

            try
            {
                bufStream = new MemoryStream(readBuffer);
            }
            catch
            {
                if (bufStream != null) bufStream.Dispose();
                throw;
            }
            var bufRead = new BinaryReader(bufStream);

            int iPacketLength = bufRead.ReadInt32();
            if (iPacketLength != 3)
                throw new ZusiTcpException("Invalid packet length: " + iPacketLength);

            int iReadInstr = GetInstruction(bufStream.ReadByte(), bufStream.ReadByte());
            if (iReadInstr != (int) expResponse)
                throw new ZusiTcpException("Invalid command from server: " + iReadInstr);

            try
            {
                int iResponse = bufStream.ReadByte();
                if (iResponse != 0)
                    switch (expResponse)
                    {
                        case ResponseType.AckHello:
                            switch (iResponse)
                            {
                                case 1:
                                    throw new ZusiTcpException("Too many connections.");
                                case 2:
                                    throw new ZusiTcpException("Zusi is already connected. No more connections allowed.");
                                default:
                                    throw new ZusiTcpException("HELLO not acknowledged.");
                            }

                        case ResponseType.AckNeededData:
                            switch (iResponse)
                            {
                                case 1:
                                    throw new ZusiTcpException("Unknown instruction set: " + dataGroup);
                                case 2:
                                    throw new ZusiTcpException("Client not connected");
                                default:
                                    throw new ZusiTcpException("NEEDED_DATA not acknowledged.");
                            }
                    }
            }
            catch
            {
                bufStream.Dispose();
                throw;
            }

            bufStream.Close();
        }

        internal void ReceiveLoop()
        {
            const int bufSize = 256;

            var recBuffer = new byte[bufSize];
            var memStream = new MemoryStream(recBuffer);
            var streamReader = new BinaryReader(memStream);
            Socket tcpSocket = _clientConnection.Client;
            var dataHandlers = new Dictionary<string, MethodInfo>();

            try
            {
                while (ConnectionState == ConnectionState.Connected)
                {
                    TCPReceive(4, recBuffer);
                    memStream.Seek(0, SeekOrigin.Begin);

                    int packetLength = streamReader.ReadInt32();

                    if (packetLength > bufSize)
                        throw new ZusiTcpException("Buffer overflow on data receive.");

                    TCPReceive(packetLength, recBuffer);
                    memStream.Seek(0, SeekOrigin.Begin);

                    int curInstr = GetInstruction(memStream.ReadByte(), memStream.ReadByte());

                    if (curInstr < 10)
                        throw new ZusiTcpException("Unexpected Non-DATA instruction received.");

                    while (memStream.Position < packetLength)
                    {
                        int curID = memStream.ReadByte() + 256*curInstr;

                        CommandEntry curCommand = _commands[curID];

                        MethodInfo handlerMethod;

                        if (!dataHandlers.TryGetValue(curCommand.Type, out handlerMethod))
                        {
                            handlerMethod = GetType().GetMethod(String.Format("HandleDATA_{0}", curCommand.Type),
                                                                              BindingFlags.Instance |
                                                                              BindingFlags.NonPublic,
                                                                              null, new[] {typeof (BinaryReader),
                                                                              typeof(int)}, null);

                            if (handlerMethod == null)
                            {
                                throw new ZusiTcpException(String.Format("Unknown type {0} for DATA ID {1} (\"{2}\") occured.",
                                    curCommand.Type, curID, curCommand.Name));
                            }

                            dataHandlers.Add(curCommand.Type, handlerMethod);
                        }

                        handlerMethod.Invoke(this, new object[] {streamReader, curID});
                    }
                }
            }
            catch (ZusiTcpException e)
            {
                Disconnnect();
                ConnectionState = ConnectionState.Error;
                _hostContext.Post(ErrorMarshal, e);
            }
        }

        private void TCPReceive(int singleLength, byte[] recBuffer)
        {
            try
            {
                _clientConnection.Client.Receive(recBuffer, singleLength, SocketFlags.None);
            }
            catch (IndexOutOfRangeException e)
            {
                /* This happens when the server is shut down and the TCP connection is "forcefully" closed. */
                throw new ZusiTcpException("Error in connection to TCP server.", e);
            }
        }

        /// <summary>
        /// Request the measurement passed as plain text in the parameter "name" from the server. Shorthand for
        /// <c>TCP.RequestedData.Add(TCP.IDs[name]);</c>.
        /// </summary>
        /// <param name="name">The name of the measurement.</param>
        public void RequestData(string name)
        {
            _requestedData.Add(IDs[name]);
        }

        /// <summary>
        /// Request the measurement passed as ID in the parameter "id" from the server. Shorthand for
        /// <c>TCP.RequestedData.Add(id);</c>.
        /// </summary>
        /// <param name="id">The ID of the measurement.</param>
        public void RequestData(int id)
        {
            _requestedData.Add(id);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (_clientConnection != null)
            {
                _clientConnection.Close();
                _clientConnection = null;
            }

            if (_streamReaderThread == null) return;

            _streamReaderThread.Abort();
            _streamReaderThread = null;
        }

        #region Nested type: ResponseType

        private enum ResponseType
        {
            None = 0,
            AckHello = 2,
            AckNeededData = 4
        }

        #endregion

        #region Data reception handlers
        /// <summary>
        /// Event used to handle incoming float data.
        /// </summary>
        public event ReceiveEvent<float> FloatReceived;

        /// <summary>
        /// Event used to handle incoming string data.
        /// </summary>
        public event ReceiveEvent<string> StringReceived;

        /// <summary>
        /// Event used to handle incoming integer data.
        /// </summary>
        public event ReceiveEvent<int> IntReceived;

        /// <summary>
        /// Event used to handle incoming boolean data.
        /// </summary>
        public event ReceiveEvent<Boolean> BoolReceived;

        /// <summary>
        /// Event used to handle incoming DateTime data.
        /// </summary>
        public event ReceiveEvent<DateTime> DateTimeReceived;

        /// <summary>
        /// Event used to handle incoming boolean data.
        /// </summary>
        public event ReceiveEvent<DoorState> DoorsReceived;

        /// <summary>
        /// Event used to handle incoming PZB system type data.
        /// </summary>
        public event ReceiveEvent<PZBSystem> PZBReceived;

        /// <summary>
        /// Event used to handle incoming brake configuration data.
        /// </summary>
        public event ReceiveEvent<BrakeConfiguration> BrakeConfigReceived;

        private struct MarshalArgs<T>
        {
            public ReceiveEvent<T> Event { get; private set; }
            public DataSet<T> Data { get; private set; }

            public MarshalArgs(ReceiveEvent<T> recveiveEvent, int id, T data) : this()
            {
                Event = recveiveEvent;
                Data = new DataSet<T>(id, data);
            }
        }

        private void EventMarshal<T>(object o)
        {
            var margs = (MarshalArgs<T>) o;
            margs.Event.Invoke(this, margs.Data);
        }

        protected void PostToHost<T>(ReceiveEvent<T> Event, int id, T value)
        {
            _hostContext.Post(EventMarshal<T>, new MarshalArgs<T>(Event, id, value));
        }

        protected void HandleDATA_Single(BinaryReader input, int id)
        {
            PostToHost(FloatReceived, id, input.ReadSingle());
        }

        protected void HandleDATA_Int(BinaryReader input, int id)
        {
            PostToHost(IntReceived, id, input.ReadInt32());
        }

        protected void HandleDATA_String(BinaryReader input, int id)
        {
            PostToHost(StringReceived, id, input.ReadString());
        }

        protected void HandleDATA_DateTime(BinaryReader input, int id)
        {
            // Delphi uses the double-based OLE Automation date for its date format.
            double temp = input.ReadDouble();
            DateTime time = DateTime.FromOADate(temp);

            PostToHost(DateTimeReceived, id, time);
        }

        protected void HandleDATA_BoolAsSingle(BinaryReader input, int id)
        {
            /* Data is delivered as Single values that are only either 0.0 or 1.0.
             * For the sake of logic, convert these to actual booleans here.
             */
            Single temp = input.ReadSingle();
            bool value = (temp >= 0.5f);
            PostToHost(BoolReceived, id, value);
        }

        protected void HandleDATA_IntAsSingle(BinaryReader input, int id)
        {
            /* Data is delivered as Single values that are only either 0.0 or 1.0.
             * For the sake of logic, convert these to actual booleans here.
             */
            Single temp = input.ReadSingle();
            int value = (int)Math.Round(temp);
            PostToHost(IntReceived, id, value);
        }

        protected void HandleDATA_BoolAsInt(BinaryReader input, int id)
        {
            /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
            Int32 temp = input.ReadInt32();
            bool value = (temp == 1);
            PostToHost(BoolReceived, id, value);
        }

        protected void HandleDATA_DoorsAsInt(BinaryReader input, int id)
        {
            /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
            Int32 temp = input.ReadInt32();
            PostToHost(DoorsReceived, id, (DoorState)temp);
        }

        protected void HandleDATA_PZBAsInt(BinaryReader input, int id)
        {
            /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
            Int32 temp = input.ReadInt32();
            PostToHost(PZBReceived, id, (PZBSystem)temp);
        }

        protected void HandleDATA_BrakesAsInt(BinaryReader input, int id)
        {
            /* Data is delivered as Int values that are only either 0 or 1.
             * For the sake of logic, convert these to actual booleans here.
             */
            Int32 temp = input.ReadInt32();

            BrakeConfiguration result;

            switch(temp)
            {
                case 0:
                    result = new BrakeConfiguration() {HasMgBrake = false, Setting = BrakeSetting.G};
                    break;

                case 1:
                    result = new BrakeConfiguration() {HasMgBrake = false, Setting = BrakeSetting.P};
                    break;

                case 2:
                    result = new BrakeConfiguration() {HasMgBrake = false, Setting = BrakeSetting.R};
                    break;

                case 3:
                    result = new BrakeConfiguration() {HasMgBrake = true, Setting = BrakeSetting.P};
                    break;

                case 4:
                    result = new BrakeConfiguration() {HasMgBrake = true, Setting = BrakeSetting.R};
                    break;

                default:
                    throw new ZusiTcpException("Invalid value received for brake configuration.");
            }

            PostToHost(BrakeConfigReceived, id, result);
        }
        #endregion
    }

    public enum DoorState
    {
        Released = 0,
        Open = 1,
        Announcement = 2,
        ReadyToClose = 3,
        Closing = 4,
        Closed = 5,
        Depart = 6,
        Locked = 7,
    }

    public enum PZBSystem
    {
        None = 0,
        IndusiH54 = 1,
        IndusiI54 = 2,
        IndusiI60 = 3,
        IndusiI60R = 4,
        PZB90V15 = 5,
        PZB90V16 = 6,
        PZ80 = 7,
        PZ80R = 8,
        LZB80I80 = 9,
        SBBSignum = 10,
    }

    public enum BrakeSetting
    {
        G, P, R,
    }

    public struct BrakeConfiguration
    {
        public bool HasMgBrake { get; set; }
        public BrakeSetting Setting { get; set; }
    }

    /// <summary>
    /// Represents the state of a TCP connection.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// There is no connection to a server.
        /// </summary>
        Disconnected = 0,

        /// <summary>
        /// A connection to a server has been established.
        /// </summary>
        Connected,

        /// <summary>
        /// An error has occured. Try disconnecting and then connecting again to solve the problem.
        /// </summary>
        Error,
    }

    /// <summary>
    /// Represents the priority of the client in the Zusi TCP interface. Determines measurement update freqency.
    /// </summary>
    public enum ClientPriority
    {
        /// <summary>
        /// Undefined priority.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Reserved for Zusi.
        /// </summary>
        Master = 01,

        /// <summary>
        /// High priority for control desks and display applications.
        /// </summary>
        High = 02,

        /// <summary>
        /// Medium priority.
        /// </summary>
        Medium = 03,

        /// <summary>
        /// Low priority.
        /// </summary>
        Low = 04,

        /// <summary>
        /// Maximum priority possible.
        /// </summary>
        RealTime = 05
    }
}