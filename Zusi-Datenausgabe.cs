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
using System.ComponentModel;
using System.Collections.ObjectModel;

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
        internal Dictionary<TMeasure, TValue> aZusiData = new Dictionary<TMeasure, TValue>();
        /// <summary>
        /// Enthält die generischen Werte.
        /// </summary>
        /// <param name="id">Die ID des Werts.</param>
        /// <returns>Einen generischen Wert.</returns>
        public TValue this[TMeasure id]
        {
            get { return aZusiData[id]; }
            internal set { aZusiData[id] = value; }
        }

        /// <summary>
        /// Gibt den Enumerator des zugrundeliegenden Dictionary zurück.
        /// </summary>
        /// <returns>Der Enumerator des zugrundeliegenden Dictionary.</returns>
        public Dictionary<TMeasure, TValue>.Enumerator GetEnumerator() { return aZusiData.GetEnumerator(); }
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
        /// Die ID-Nummer, die Zusi für die jeweilige Messgröße verwendet.
        /// </summary>
        public int Id { get; private set; }


        /// <summary>
        /// Der neue Wert der Messgröße.
        /// </summary>
        public T Value { get; private set; }

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
    /// Die Schnittstelle benutzt die Datentypen <see cref="Single"/>, <see cref="String"/> und <c>Byte[]</c>.</description></item>
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

        /// <summary>
        /// Der angegebene Name des Clients.
        /// </summary>
        public string ClientId { get; private set; }

        TcpClient ClientConnection = new TcpClient();

        ASCIIEncoding StringEncoder = new ASCIIEncoding();

        Thread StreamReaderThread;

        ReceiveEvent<float> FloatDel;
        ReceiveEvent<string> StrDel;
        ReceiveEvent<byte[]> ByteDel;

        SynchronizationContext HostContext;

        private void FloatMarshal(object o)
        {
            FloatDel.Invoke((DataSet<float>)o);
        }

        private void StringMarshal(object o)
        {
            StrDel.Invoke((DataSet<string>) o);
        }

        private void ByteMarshal(object o)
        {
            ByteDel.Invoke((DataSet<byte[]>) o);
        }

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

        private SortedList<int, int> aCommands = new SortedList<int, int>();

        private List<int> aRequestedData = new List<int>();

        /// <summary>
        /// Eine Liste aller von Zusi angeforderten Größen. Fügen Sie hier die ID-Nummern der Größen hinzu, 
        /// bevor Sie eine Verbindung zum TCP-Server herstellen. <seealso cref="Ids"/>
        /// </summary>
        public List<int> RequestedData
        {
            get
            {
                if (ConnectionState == ConnectionState.Disconnected)
                    return aRequestedData;
                else
                    return null;
            }
        }

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
            this.ClientPriority = priority;

            HostContext = SynchronizationContext.Current;

            MemoryStream DataIDs = null;

            try
            {
                DataIDs = new MemoryStream(DatenIDs.commands);
            }
            catch (Exception)
            {
                DataIDs.Dispose();
                throw;
            }
            BinaryFormatter BinIn = new BinaryFormatter();

            try
            {
                Ids = (ZusiData<string, int>)BinIn.Deserialize(DataIDs);

                ReverseIds = new ZusiData<int, string>();

                foreach (KeyValuePair<string, int> item in Ids)
                {
                    ReverseIds[item.Value] = item.Key;
                }

                aCommands = (SortedList<int, int>)BinIn.Deserialize(DataIDs);
            }
            finally
            {
                DataIDs.Dispose();
            }

            FloatDel = floatHandler;
            StrDel = stringHandler;
            ByteDel = byteHandler;
        }

        void SendPacket(params byte[] Message)
        {
            ClientConnection.Client.Send(BitConverter.GetBytes(Message.Length));
            ClientConnection.Client.Send(Message);
        }

        void SendPacket(params byte[][] Message)
        {
            int iTempLength = 0;
            foreach (byte[] item in Message)
            {
                iTempLength += item.Length;
            }

            ClientConnection.Client.Send(BitConverter.GetBytes(iTempLength));
            foreach (byte[] item in Message)
            {
                ClientConnection.Client.Send(item);
            }
        }

        static byte[] Pack(params byte[] Message) { return Message; }

        static int GetInstruction(int ByteA, int ByteB)
        {
            return ByteA * 256 + ByteB;
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
                if (ConnectionState == ConnectionState.Error) throw
                    (new ZusiTcpException("Network state is \"Error\". Disconnect first!"));

                if (ClientConnection == null)
                    ClientConnection = new TcpClient();

                ClientConnection.Connect(hostName, port);

                if (ClientConnection.Connected)
                {
                    StreamReaderThread = new Thread(ReceiveLoop);
                    StreamReaderThread.Name = "ZusiData Receiver";

                    SendPacket(Pack(0, 1, 2, (byte)ClientPriority,
                        Convert.ToByte(StringEncoder.GetByteCount(ClientId))), StringEncoder.GetBytes(ClientId));

                    ExpectResponse(ResponseType.AckHello, 0);

                    var aGetData = from iData in RequestedData
                                   group iData by (iData / 256);

                    List<byte[]> ReqDataBuffer = new List<byte[]>();

                    foreach (var aDataGroup in aGetData)
                    {
                        ReqDataBuffer.Clear();
                        ReqDataBuffer.Add(Pack(0, 3));

                        byte[] TempDataGroup = BitConverter.GetBytes(Convert.ToInt16(aDataGroup.Key));
                        ReqDataBuffer.Add(Pack(TempDataGroup[1], TempDataGroup[0]));

                        foreach (int iID in aDataGroup)
                        {
                            ReqDataBuffer.Add(Pack(Convert.ToByte(iID % 256)));
                        }

                        SendPacket(ReqDataBuffer.ToArray());

                        ExpectResponse(ResponseType.AckNeededData, aDataGroup.Key);
                    }

                    SendPacket(0, 3, 0, 0); ;
                    ExpectResponse(ResponseType.AckNeededData, 0);

                    ConnectionState = ConnectionState.Connected;

                    StreamReaderThread.Start();
                }

            }

            catch (Exception ex)
            {
                if (StreamReaderThread != null)
                {
                    StreamReaderThread.Abort();
                    StreamReaderThread = null;
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
            if (StreamReaderThread != null)
                StreamReaderThread.Abort();
            ConnectionState = ConnectionState.Disconnected;
            if (ClientConnection != null)
                ClientConnection.Close();
            ClientConnection = null;
        }

        private enum ResponseType
        {
            None = 0,
            AckHello = 2,
            AckNeededData = 4
        }

        private void ExpectResponse(ResponseType ExpResponse, int DataGroup)
        {
            byte[] ReadBuffer = new byte[7];

            ClientConnection.Client.Receive(ReadBuffer, 7, SocketFlags.None);
            MemoryStream BufStream = null;

            try
            {
                BufStream = new MemoryStream(ReadBuffer);
            }
            catch
            {
                BufStream.Dispose();
                BufStream = null;
                throw;
            }
            BinaryReader BufRead = new BinaryReader(BufStream);

            int iPacketLength = BufRead.ReadInt32();
            if (iPacketLength != 3)
                throw new ZusiTcpException("Invalid packet length: " + iPacketLength);

            int iReadInstr = GetInstruction(BufStream.ReadByte(), BufStream.ReadByte());
            if (iReadInstr != (int)ExpResponse)
                throw new ZusiTcpException("Invalid command from server: " + iReadInstr);

            try
            {
                int iResponse = BufStream.ReadByte();
                if (iResponse != 0)
                    switch (ExpResponse)
                    {
                        case ResponseType.AckHello:
                            throw new ZusiTcpException("HELLO not acknowledged.");
                        case ResponseType.AckNeededData:
                            switch (iResponse)
                            {
                                case 1:
                                    throw new ZusiTcpException("Unknown instruction set: " + DataGroup);
                                case 2:
                                    throw new ZusiTcpException("Client not connected");
                                default:
                                    throw new ZusiTcpException("NEEDED_DATA not acknowledged.");
                            }
                    }
            }
            catch
            {
                BufStream.Dispose();
                throw;
            }

            BufStream.Close();
            
        }

        /// <summary>
        /// Debug-Funktion, die aus der Rohdatei mit den Bezeichnungen für die einzelnen Messgrößen eine Ressource erzeugt.
        /// </summary>
        /// <param name="filename"></param>
        //[Conditional("DEBUG")]
        public static void CreateDataset(string filename)
        {
            Assembly INI = Assembly.LoadFrom("INI-Interface.dll");

            Type CfgFileType = INI.GetType("INI_Interface.CfgFile");
            MethodInfo GetCaption = CfgFileType.GetMethod("ge<tCaption");

            object INIDatei = CfgFileType.GetConstructor(new Type[] { typeof(string) }).Invoke(new object[] { filename });

            ZusiData<string, int> IDs = new ZusiData<string, int>();
            SortedList<int, int> Commands = new SortedList<int, int>();

            SortedList<string, string> CapContents =
                (SortedList<string, string>)GetCaption.Invoke(INIDatei, new string[] { "FriendlyNames" });

            foreach (string ID in CapContents.Keys)
            {
                IDs.aZusiData.Add(CapContents[ID], Convert.ToInt32(ID));
            }

            CapContents = (SortedList<string, string>)GetCaption.Invoke(INIDatei, new string[] { "Commands" });

            foreach (string Command in CapContents.Keys)
            {
                Commands.Add(Convert.ToInt32(Command), Convert.ToInt32(CapContents[Command]));
            }

            Stream Ausgabe = File.Create("commands.dat");
            BinaryFormatter BinOut = new BinaryFormatter();
            BinOut.Serialize(Ausgabe, IDs);
            BinOut.Serialize(Ausgabe, Commands);
            Ausgabe.Close();
        }

        internal void ReceiveLoop()
        {
            const int BufSize = 256;

            byte[] RecBuffer = new byte[BufSize];
            int iPacketLength;
            MemoryStream MemStream = new MemoryStream(RecBuffer);
            BinaryReader StreamReader = new BinaryReader(MemStream);
            Socket TCPSocket = ClientConnection.Client;

            while (true)
            {
                if (ConnectionState == ConnectionState.Connected)
                {
                    TCPSocket.Receive(RecBuffer, 4, SocketFlags.None);
                    MemStream.Seek(0, SeekOrigin.Begin);

                    iPacketLength = StreamReader.ReadInt32();

                    if (iPacketLength > BufSize)
                        throw new ZusiTcpException("Buffer overflow on data receive.");

                    TCPSocket.Receive(RecBuffer, iPacketLength, SocketFlags.None);
                    MemStream.Seek(0, SeekOrigin.Begin);

                    int iCurInstr = GetInstruction(MemStream.ReadByte(), MemStream.ReadByte());

                    if (iCurInstr < 10)
                        throw new ZusiTcpException("Non-DATA instruction received.");

                    while (MemStream.Position < iPacketLength)
                    {
                        //object[] CurDataset = new object[1];

                        int iCurID = MemStream.ReadByte() + 256 * iCurInstr;
                        int iCurDataLength;
                        if (aCommands.TryGetValue(iCurID, out iCurDataLength))
                        {
                            switch (iCurDataLength)
                            {
                                case 0:
                                    //CurDataset[0] = new DataSet<string>(iCurID, StreamReader.ReadString());
                                    //SyncObj.Invoke(StrDel, CurDataset);
                                    //StrDel.Invoke(new DataSet<string>(iCurID, StreamReader.ReadString()));
                                    HostContext.Post(new SendOrPostCallback(StringMarshal), new DataSet<string>(iCurID, StreamReader.ReadString()));
                                    break;
                                default:
                                    //CurDataset[0] = new DataSet<byte[]>(iCurID, StreamReader.ReadBytes(iCurDataLength));
                                    //ByteDel.Invoke(new DataSet<byte[]>(iCurID, StreamReader.ReadBytes(iCurDataLength)));
                                    HostContext.Post(new SendOrPostCallback(ByteMarshal), new DataSet<byte[]>(iCurID, StreamReader.ReadBytes(iCurDataLength)));
                                    break;
                            }
                        }
                        else
                        {
                            //CurDataset[0] = new DataSet<float>(iCurID, StreamReader.ReadSingle());
                            //FloatDel.Invoke(new DataSet<float>(iCurID, StreamReader.ReadSingle()));
                            HostContext.Post(new SendOrPostCallback(FloatMarshal), new DataSet<float>(iCurID, StreamReader.ReadSingle()));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gibt die ID-Nummer der im Parameter in Klartext angegebenen Messgröße zurück.
        /// </summary>
        /// <param name="name">Der Name der Messgröße</param>
        /// <returns>Die zugehörige ID-Nummer</returns>
        public int this[string name] { get { return Ids[name]; } }

        /// <summary>
        /// Gibt den Namen der im Parameter angegebenen ID-Nummer als Klartext zurück.
        /// </summary>
        /// <param name="id">Die ID-Nummer der Messgröße</param>
        /// <returns>Der Name der Messgröße</returns>
        public string this[int id] { get { return ReverseIds[id]; } }

        /// <summary>
        /// Fügt aRequestedData einen Eintrag mit dem angegebenen Messgrößennamen hinzu. Kurzform für TCP.RequestedData.Add(TCP.IDs[Name]);.
        /// </summary>
        /// <param name="name">Der Name der Messgröße</param>
        public void RequestData(string name) { aRequestedData.Add(Ids[name]); }

        /// <summary>
        /// Fügt aRequestedData einen Eintrag mit der angegebenen Messgrößen-ID hinzu. Kurzform für TCP.RequestedData.Add(ID);.
        /// </summary>
        /// <param name="id">Die ID-Nummer der Messgröße</param>
        public void RequestData(int id) { aRequestedData.Add(id); }

        /// <summary>
        /// Entsorgt die TCP-Verbindung
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ClientConnection != null)
                {
                    ClientConnection.Close();
                    ClientConnection = null;
                }

                if (StreamReaderThread != null)
                {
                    StreamReaderThread.Abort();
                    StreamReaderThread = null;
                }
            }
        }
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