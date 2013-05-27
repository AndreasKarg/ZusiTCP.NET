using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Zusi_Datenausgabe
{
	[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
	public class TCPServer
	{
		#region Fields

		private readonly List<TCPServerCleint> _clients = new List<TCPServerCleint>();
		private readonly List<Base_Connection> _clients_extern = new List<Base_Connection>();
		private readonly System.Collections.ObjectModel.ReadOnlyCollection<Base_Connection> _clients_extern_readonly ;

		private TcpListener _serverObj = null;
		private Thread _accepterThread = null;
		private TCPCommands _doc;

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Zusi_Datenausgabe.TCPServer"/> class.
		/// </summary>
		/// <param name="CommandsetDocument">The commandset document. Valid entrys for the types are 4ByteCommand, 8ByteCommand and LengthIn1ByteCommand.</param>
		public TCPServer (TCPCommands CommandsetDocument)
		{
			_clients_extern_readonly = new System.Collections.ObjectModel.ReadOnlyCollection<Base_Connection>(_clients_extern);
			_doc = CommandsetDocument;
		}
		/// <summary>
		/// Gets a list of all connected clients.
		/// </summary>
		/// <value>The clients.</value>
		public System.Collections.ObjectModel.ReadOnlyCollection<Base_Connection> Clients {get{return _clients_extern_readonly;}}
		private TCPServerCleint MasterL{get; set;}
		/// <summary>
		/// Gets the master.
		/// </summary>
		/// <value>The master.</value>
		public Base_Connection Master{get{return MasterL;}}
		private void ServerRelatedRequest(byte[] array, int id)
		{
			if ((id < 3840)||(id >= 4096))
				return;
			//ToDo: Code einfügen.
		}
		private void ConstByteCommandReceived(byte[] array, int id, TCPServerCleint sender)
		{
			ServerRelatedRequest(array,id);
			if (sender.ClientPriority == ClientPriority.Master)
			{
				foreach(TCPServerCleint cli in _clients)
				{
					if (cli != sender)
						cli.SendByteCommand(array,id);
				}
			}
		}
		private void LengthIn1ByteCommandReceived(byte[] array, int id, TCPServerCleint sender)
		{
			if (sender.ClientPriority == ClientPriority.Master)
			{
				foreach(TCPServerCleint cli in _clients)
				{
					if (cli != sender)
						cli.SendLengthIn1ByteCommand(array,id);
				}
			}
		}
		private void ConnectionConnectStatusChanged( TCPServerCleint sender)
		{
			if (sender.ConnectionState == ConnectionState.Connected)
			{
				if (sender.ClientPriority == ClientPriority.Master)
					this.MasterL = sender;
				if (!_clients.Contains(sender))
					_clients.Add (sender);
				if (!_clients_extern.Contains(sender))
					_clients_extern.Add (sender);
			}
			else
			{
				if (sender == this.MasterL)
					MasterL = null;
				_clients.Remove (sender);
				_clients_extern.Remove (sender);
				if (sender.ConnectionState == ConnectionState.Error)
				{
					sender.Disconnnect();
					sender.Dispose();
				}
			}
		}

		private IEnumerable<int> GetAbonentedIds()
		{
			List<int> lst = new List<int>();
			//Hier kann lst mit MUSS-ABONIEREN-Werten initialisiert werden, falls solcher gewünscht sind.
			foreach(TCPServerCleint cli in _clients)
			{
				foreach(int dat in cli.RequestedData)
				{
					if ((!lst.Contains(dat))&&((dat < 3840)||(dat >= 4096)))
						lst.Add (dat);
				}
			}
			return lst;
		}

		public bool IsStarted{get{return _accepterThread != null;}}
		/// <summary>
		/// Starts the server using the specified port.
		/// </summary>
		/// <param name="port">The port, the Server should use.</param>
		/// <exception cref="InvalidOperationException">Thrown, when the connection is already started.</exception>
		public void Start(int port)
		{
			if (IsStarted)
			{
				throw (new InvalidOperationException());
			}
			_accepterThread = new Thread(RunningLoop);
			try
			{
				_serverObj = new TcpListener(IPAddress.Any,port);
				_serverObj.Start();
				_accepterThread.Start();
			}
			catch
			{
				_accepterThread = null;
				if (_serverObj != null)
					_serverObj.Stop();
				_serverObj = null;
				throw;
			}

		}

		/// <summary>
		/// Stops the Server and closes all connected clients.
		/// </summary>
		public void Stop()
		{
			foreach(TCPServerCleint cli in _clients)
			{
				cli.Disconnnect();
			}
			_accepterThread.Abort();
		}
		private void RunningLoop()
		{
			TCPServerCleint cli = null;
			try
			{
				while (IsStarted)
				{
					TcpClient tc = _serverObj.AcceptTcpClient();
					cli = new TCPServerCleint(_doc,this);
					cli.TryBeginAcceptConnection(tc);
					cli = null;
				}
			}
			catch
			{
				if (cli != null)
					cli.Dispose();
				_serverObj.Stop();
				throw;
			}
		}

		private class TCPServerCleint : Base_Connection
		{
			public TCPServerCleint(TCPCommands commandsetDocument, TCPServer ServerBase)
				:base("Unknown",ClientPriority.Undefined,commandsetDocument,null)
			{
				this.ServerBase = ServerBase;
				this.ConnectionState_Changed += ConnectionConnectStatusChanged;
			}

			public TCPServer ServerBase {set; get;}


			protected int HandleDATA_4ByteCommand(BinaryReader input, int id)
			{
				ServerBase.ConstByteCommandReceived(input.ReadBytes(4),id, this);
				return 4;
			}
			protected int HandleDATA_8ByteCommand(BinaryReader input, int id)
			{
				ServerBase.ConstByteCommandReceived(input.ReadBytes(8),id, this);
				return 8;
			}
			protected int HandleDATA_LengthIn1ByteCommand(BinaryReader input, int id)
			{
				byte lng = input.ReadByte();
				ServerBase.LengthIn1ByteCommandReceived(input.ReadBytes(lng),id, this);
				return lng + 1;
			}

			public void SendByteCommand(byte[] array, int id)
			{
				if ((ConnectionState!=ConnectionState.Connected)||(!RequestedData.Contains(id)))
					return;
				List<byte> ida = new List<byte>( System.BitConverter.GetBytes(id));
				ida.RemoveAt(3);
				ida.Reverse();
				SendPacket(ida.ToArray(),array);
			}
			public void SendLengthIn1ByteCommand(byte[] array, int id)
			{
				if ((ConnectionState!=ConnectionState.Connected)||(!RequestedData.Contains(id)))
					return;
				List<byte> ida = new List<byte>( System.BitConverter.GetBytes(id));
				ida.RemoveAt(3);
				ida.Reverse();
				byte[] lng = {(byte) array.Length};
				SendPacket(ida.ToArray(),lng,array);
			}

			protected override void TryBeginAcceptConnection_IsMaster ()
			{
				this.RequestingData.Clear();
				this.RequestingData.AddRange(ServerBase.GetAbonentedIds());
				base.TryBeginAcceptConnection_IsMaster ();
			}

			private void ConnectionConnectStatusChanged(object sender, EventArgs e)
			{
				ServerBase.ConnectionConnectStatusChanged(this);
			}

			public void TryBeginAcceptConnection(TcpClient clientConnection)
			{
				if (ServerBase.Master == null)
					base.TryBeginAcceptConnection(clientConnection,null);
				else
					base.TryBeginAcceptConnection(clientConnection,ServerBase.Master.RequestedData);
			}
		}
	}
}

