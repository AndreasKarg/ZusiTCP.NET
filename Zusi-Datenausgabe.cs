/*************************************************************************
 * 
 * Zusi TCP interface for .NET
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

[assembly: CLSCompliant(true)]

namespace Zusi_Datenausgabe
{
    /// <summary>
    /// Die Wörterbuch-Klasse, in der beispielsweise die ID-Liste umgesetzt ist.
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

#if DEBUG
        public Dictionary<TMeasure, TValue> Data
        {
            [DebuggerStepThrough]
            get { return _data; }
        }
#endif

        /// <summary>
        /// Gibt den Enumerator des zugrundeliegenden Dictionary zurück.
        /// </summary>
        /// <returns>Der Enumerator des zugrundeliegenden Dictionary.</returns>
        public Dictionary<TMeasure, TValue>.Enumerator GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }

    /// <summary>
    /// Der Delegat-Typ, der für die Ereignisbehandlung nötig ist.
    /// </summary>
    /// <param name="data"></param>
    public delegate void ReceiveEvent<T>(DataSet<T> data);

    /// <summary>
    /// Struktur, über die Datensätze an das Anwenderprogramm übergeben werden.
    /// </summary>
    /// <typeparam name="T">Der Datentyp für den Zusi-Datensatz. Single, String oder Byte[]</typeparam>
    public struct DataSet<T>
    {
        /// <summary>
        /// Ein ganz normaler Konstruktor.
        /// </summary>
        /// <param name="id">Die ID-Nummer der Messgröße</param>
        /// <param name="value">Der neue Wert der Messgröße</param>
        public DataSet(int id, T value)
            : this()
        {
            Id = id;
            Value = value;
        }

        /// <summary>
        /// Die ID-Nummer, die Zusi für die jeweilige Messgröße verwendet.
        /// </summary>
        public int Id { get; private set; }


        /// <summary>
        /// Der neue Wert der Messgröße.
        /// </summary>
        public T Value { get; private set; }
    }

    /// <summary>
    /// Das Kernstück der Zusi-TCP-Schnittstelle.
    /// <para>Schritte bei der Verwendung:
    /// <list type="bullet">
    /// <item><description>Man benötigt wahlweise eine Windows Forms-Anwendung oder ein anderes Objekt, das die
    /// <see cref="System.ComponentModel.ISynchronizeInvoke" />-Schnittstelle implementiert.</description></item>
    /// 
    /// <item><description>Ereignismethoden innerhalb der Klasse des Windows-Fensters oder des Schnittstellenobjekts implementieren.
    /// Diese müssen dem Delegaten Zusi_Datenausgabe.Dataset.ReceiveEvent entsprechen.
    /// Die Schnittstelle benutzt die Datentypen <see cref="float"/>, <see cref="string"/> und <c>Byte[]</c>.</description></item>
    /// 
    /// <item><description>Instanz der Klasse erzeugen (<see cref="ZusiTcpConn"/>). Für Fahrpulte wird 
    /// die Priorität "High" empfohlen. Als Parameter für SynchronizesObject und die Handler-Delegaten das Windows-Fenster
    /// bzw. die Ereignismethoden angeben.</description></item>
    /// <item><description>Der Eigenschaft RequestedData (<see cref="ZusiTcpConn.RequestedData"/>) die benötigten Größen als IDs hinzufügen.
    /// Bei Bedarf können die ID-Nummern aus dem deutschen Klartextnamen wie in der Zusi-Dokumentation(z.B. "Geschwindigkeit") direkt aus der Eigenschaft
    /// IDs (<see cref="Ids"/>) entnommen werden.</description></item>
    /// <item><description>Per Connect-Methode (<see cref="Connect"/>) die Verbindung zum TCP-Server herstellen.</description></item>
    /// <item><description>Sobald von Zusi Daten kommen, werden diese automatisch aufbereitet. Für jeden empfangenen Wert
    /// wird die zum jeweiligen Typ gehörige Ereignismethode aufgerufen. Dort kann mit den Daten beliebig weiter verfahren werden.</description></item>
    /// </list></para>
    /// </summary>
    public class ZusiTcpConn : IDisposable
    {
        private readonly ReceiveEvent<byte[]> _byteDel;
        private readonly ReceiveEvent<float> _floatDel;

        private readonly SynchronizationContext _hostContext;
        private readonly ReceiveEvent<string> _strDel;
        private readonly ASCIIEncoding _stringEncoder = new ASCIIEncoding();
        private readonly SortedList<int, int> _commands = new SortedList<int, int>();

        private readonly List<int> _requestedData = new List<int>();
        private TcpClient _clientConnection = new TcpClient();
        private Thread _streamReaderThread;

        /// <summary>
        /// Der Konstruktor für die ZusiTCPConn-Klasse. 
        /// </summary>
        /// <param name="clientId">Der Name des Clients. Bspw. "Am Hansi sei Broggramm"</param>
        /// <param name="priority">Die Priorität des Clients. Beeinflusst die 
        /// Aktualisierungsraten der Datensätze. Für Fahrpultanwendungen wird "High" empfohlen.</param>
        /// <param name="floatHandler">Ein Ereignishandler, der Zusi_Datenausgabe.Dataset.ReceiveEvent entspricht und Daten mit Fließkommawerten annimmt.</param>
        /// <param name="stringHandler">Ein Ereignishandler, der Zusi_Datenausgabe.Dataset.ReceiveEvent entspricht und Daten mit Strings annimmt.</param>
        /// <param name="byteHandler">Ein Ereignishandler, der Zusi_Datenausgabe.Dataset.ReceiveEvent entspricht und Daten mit Byte-Arrays annimmt</param>
        public ZusiTcpConn(string clientId, ClientPriority priority,
                           ReceiveEvent<float> floatHandler,
                           ReceiveEvent<string> stringHandler,
                           ReceiveEvent<byte[]> byteHandler)
        {
            ClientId = clientId;
            ClientPriority = priority;

            _hostContext = SynchronizationContext.Current;

            MemoryStream dataIDs = null;

            try
            {
                dataIDs = new MemoryStream(DatenIDs.commands);
            }
            catch (Exception)
            {
                if (dataIDs != null) 
                    dataIDs.Dispose();
                throw;
            }


            var binIn = new BinaryFormatter();

            try
            {
                Ids = (ZusiData<string, int>) binIn.Deserialize(dataIDs);

                ReverseIds = new ZusiData<int, string>();

                foreach (var item in Ids)
                {
                    ReverseIds[item.Value] = item.Key;
                }

                _commands = (SortedList<int, int>) binIn.Deserialize(dataIDs);
            }
            finally
            {
                dataIDs.Dispose();
            }

            _floatDel = floatHandler;
            _strDel = stringHandler;
            _byteDel = byteHandler;
        }

        /// <summary>
        /// Der angegebene Name des Clients.
        /// </summary>
        public string ClientId { get; private set; }

        /// <summary>
        /// Enthält eine vollständige Liste aller Größen, die von Zusi ausgegeben werden, als Schlüssel-Wert-Paare. Schlüssel ist der deutsche Klartextname der Größe. <c>IDs["Geschwindigkeit"]</c> gibt also den Wert <c>01</c> zurück.
        /// </summary>
        public ZusiData<string, int> Ids { get; private set; }

        /// <summary>
        /// Die Umkehrung der Eigenschaft IDs: Zu den einzelnen ID-Nummern kann der jeweilige Name abgerufen werden.
        /// </summary>
        public ZusiData<int, string> ReverseIds { get; private set; }

        /// <summary>
        /// Enthält den aktuellen Verbindungszustand der Schnittstelle.
        /// </summary>
        public ConnectionState ConnectionState { get; private set; }

        /// <summary>
        /// Enthält die Priorität des Clients.
        /// </summary>
        public ClientPriority ClientPriority { get; private set; }

        /// <summary>
        /// Eine Liste aller von Zusi angeforderten Größen. Fügen Sie hier die ID-Nummern der Größen hinzu, 
        /// bevor Sie eine Verbindung zum TCP-Server herstellen. <seealso cref="Ids"/>
        /// </summary>
        public List<int> RequestedData
        {
            get {
                return ConnectionState == ConnectionState.Disconnected ? _requestedData : null;
            }
        }

        /// <summary>
        /// Gibt die ID-Nummer der im Parameter in Klartext angegebenen Messgröße zurück.
        /// </summary>
        /// <param name="name">Der Name der Messgröße</param>
        /// <returns>Die zugehörige ID-Nummer</returns>
        public int this[string name]
        {
            get { return Ids[name]; }
        }

        /// <summary>
        /// Gibt den Namen der im Parameter angegebenen ID-Nummer als Klartext zurück.
        /// </summary>
        /// <param name="id">Die ID-Nummer der Messgröße</param>
        /// <returns>Der Name der Messgröße</returns>
        public string this[int id]
        {
            get { return ReverseIds[id]; }
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

        private void FloatMarshal(object o)
        {
            _floatDel.Invoke((DataSet<float>) o);
        }

        private void StringMarshal(object o)
        {
            _strDel.Invoke((DataSet<string>) o);
        }

        private void ByteMarshal(object o)
        {
            _byteDel.Invoke((DataSet<byte[]>) o);
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
        /// Stellt eine Verbindung zum TCP-Server her.
        /// </summary>
        /// <param name="hostName">Enthält den Hostnamen oder die IP-Adresse, unter der der TCP-Server erreichbar ist.</param>
        /// <param name="port">Der Port, unter dem der TCP-Server erreichbar ist.</param>
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
        /// Trennt die Verbindung zum TCP-Server.
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
                            throw new ZusiTcpException("HELLO not acknowledged.");
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

#if DEBUG
        /// <summary>
        /// Debug-Funktion, die aus der Rohdatei mit den Bezeichnungen für die einzelnen Messgrößen eine Ressource erzeugt.
        /// </summary>
        /// <param name="filename"></param>
        public static void CreateDataset(string filename)
        {
            Assembly ini = Assembly.LoadFrom("INI-Interface.dll");

            Type cfgFileType = ini.GetType("INI_Interface.CfgFile");
            MethodInfo getCaption = cfgFileType.GetMethod("ge<tCaption");

            var constructorInfo = cfgFileType.GetConstructor(new[] {typeof (string)});
            if (constructorInfo == null) return;

            object iniDatei = constructorInfo.Invoke(new object[] {filename});

            var IDs = new ZusiData<string, int>();
            var commands = new SortedList<int, int>();

            var capContents =
                (SortedList<string, string>) getCaption.Invoke(iniDatei, new[] {"FriendlyNames"});

            foreach (string ID in capContents.Keys)
            {
                IDs.Data.Add(capContents[ID], Convert.ToInt32(ID));
            }

            capContents = (SortedList<string, string>) getCaption.Invoke(iniDatei, new[] {"Commands"});

            foreach (string command in capContents.Keys)
            {
                commands.Add(Convert.ToInt32(command), Convert.ToInt32(capContents[command]));
            }

            Stream outputStream = File.Create("commands.dat");
            var binOut = new BinaryFormatter();
            binOut.Serialize(outputStream, IDs);
            binOut.Serialize(outputStream, commands);
            outputStream.Close();
        }
#endif

        internal void ReceiveLoop()
        {
            const int bufSize = 256;

            var recBuffer = new byte[bufSize];
            var memStream = new MemoryStream(recBuffer);
            var streamReader = new BinaryReader(memStream);
            Socket tcpSocket = _clientConnection.Client;

            while (true)
            {
                if (ConnectionState != ConnectionState.Connected) continue;

                tcpSocket.Receive(recBuffer, 4, SocketFlags.None);
                memStream.Seek(0, SeekOrigin.Begin);

                int iPacketLength = streamReader.ReadInt32();

                if (iPacketLength > bufSize)
                    throw new ZusiTcpException("Buffer overflow on data receive.");

                tcpSocket.Receive(recBuffer, iPacketLength, SocketFlags.None);
                memStream.Seek(0, SeekOrigin.Begin);

                int iCurInstr = GetInstruction(memStream.ReadByte(), memStream.ReadByte());

                if (iCurInstr < 10)
                    throw new ZusiTcpException("Non-DATA instruction received.");

                while (memStream.Position < iPacketLength)
                {
                    //object[] CurDataset = new object[1];

                    int iCurID = memStream.ReadByte() + 256*iCurInstr;
                    int iCurDataLength;
                    if (_commands.TryGetValue(iCurID, out iCurDataLength))
                    {
                        switch (iCurDataLength)
                        {
                            case 0:
                                //CurDataset[0] = new DataSet<string>(iCurID, StreamReader.ReadString());
                                //SyncObj.Invoke(StrDel, CurDataset);
                                //StrDel.Invoke(new DataSet<string>(iCurID, StreamReader.ReadString()));
                                _hostContext.Post(StringMarshal,
                                                  new DataSet<string>(iCurID, streamReader.ReadString()));
                                break;
                            default:
                                //CurDataset[0] = new DataSet<byte[]>(iCurID, StreamReader.ReadBytes(iCurDataLength));
                                //ByteDel.Invoke(new DataSet<byte[]>(iCurID, StreamReader.ReadBytes(iCurDataLength)));
                                _hostContext.Post(ByteMarshal,
                                                  new DataSet<byte[]>(iCurID, streamReader.ReadBytes(iCurDataLength)));
                                break;
                        }
                    }
                    else
                    {
                        //CurDataset[0] = new DataSet<float>(iCurID, StreamReader.ReadSingle());
                        //FloatDel.Invoke(new DataSet<float>(iCurID, StreamReader.ReadSingle()));
                        _hostContext.Post(FloatMarshal, new DataSet<float>(iCurID, streamReader.ReadSingle()));
                    }
                }
            }
        }

        /// <summary>
        /// Fügt _requestedData einen Eintrag mit dem angegebenen Messgrößennamen hinzu. Kurzform für TCP.RequestedData.Add(TCP.IDs[Name]);.
        /// </summary>
        /// <param name="name">Der Name der Messgröße</param>
        public void RequestData(string name)
        {
            _requestedData.Add(Ids[name]);
        }

        /// <summary>
        /// Fügt _requestedData einen Eintrag mit der angegebenen Messgrößen-ID hinzu. Kurzform für TCP.RequestedData.Add(ID);.
        /// </summary>
        /// <param name="id">Die ID-Nummer der Messgröße</param>
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
    }

    /// <summary>
    /// Der Zustand, in dem sich die Verbindung befindet.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// Es besteht keine Verbindung zum TCP-Server.
        /// </summary>
        Disconnected = 0,

        /// <summary>
        /// Eine Verbindung zum TCP-Server wurde hergestellt.
        /// </summary>
        Connected,

        /// <summary>
        /// Ein Fehler ist aufgetreten. Verbindung trennen und wieder herstellen, um das Problem zu lösen.
        /// </summary>
        Error,
    }

    /// <summary>
    /// Stellt die Priorität des Clients bei der Datenausgabe dar. Beeinflusst die Frequenz, mit der aktualisierte Daten verschickt werden.
    /// </summary>
    public enum ClientPriority
    {
        /// <summary>
        /// Undefinierte Priorität.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Reserviert für Zusi.
        /// </summary>
        Master = 01,

        /// <summary>
        /// Hohe Priorität für Fahrpulte und Anzeigeprogramme.
        /// </summary>
        High = 02,

        /// <summary>
        /// Mittlere Priorität
        /// </summary>
        Medium = 03,

        /// <summary>
        /// Niedrige Priorität
        /// </summary>
        Low = 04,

        /// <summary>
        /// Maximalstesteste Priorität überhauptst.
        /// </summary>
        RealTime = 05
    }
}