#region header
// /*************************************************************************
//  * TCPServerConnectionInitializer.cs
//  * Contains main logic for the TCP interface.
//  * 
//  * (C) 2013-2013 Andreas Karg, <Clonkman@gmx.de>
//  * 
//  * This file is part of Zusi TCP Interface.NET.
//  *
//  * Zusi TCP Interface.NET is free software: you can redistribute it and/or
//  * modify it under the terms of the GNU General Public License as
//  * published by the Free Software Foundation, either version 3 of the
//  * License, or (at your option) any later version.
//  *
//  * Zusi TCP Interface.NET is distributed in the hope that it will be
//  * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  * GNU General Public License for more details.
//  *
//  * You should have received a copy of the GNU General Public License
//  * along with Zusi TCP Interface.NET. 
//  * If not, see <http://www.gnu.org/licenses/>.
//  * 
//  *************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Threading;

namespace Zusi_Datenausgabe
{
  internal class TCPServerConnectionInitializer : Base_Connection
  {
    private TCPServerSlaveConnection _slaveConnection;
    private TCPServerMasterConnection _masterConnection;

    #region Delegated Base Constructors
    public TCPServerConnectionInitializer(string clientId, ClientPriority priority, TCPCommands commandsetDocument, SynchronizationContext hostContext)
      : base(clientId, priority, commandsetDocument, hostContext)
    {
    }

    public TCPServerConnectionInitializer(string clientId, ClientPriority priority, TCPCommands commandsetDocument)
      : base(clientId, priority, commandsetDocument)
    {
    }

    public TCPServerConnectionInitializer(string clientId, ClientPriority priority, string commandsetPath, SynchronizationContext hostContext)
      : base(clientId, priority, commandsetPath, hostContext)
    {
    }

    public TCPServerConnectionInitializer(string clientId, ClientPriority priority, string commandsetPath)
      : base(clientId, priority, commandsetPath)
    {
    }

    public TCPServerConnectionInitializer(string clientId, ClientPriority priority, SynchronizationContext hostContext)
      : base(clientId, priority, hostContext)
    {
    }

    public TCPServerConnectionInitializer(string clientId, ClientPriority priority)
      : base(clientId, priority)
    {
    } 
    #endregion

    public event EventHandler SlaveConnectionInitialized;
    public event EventHandler MasterConnectionInitialized;

    private void OnMasterConnectionInitialized()
    {
      var handler = MasterConnectionInitialized;
      if (handler != null) handler(this, EventArgs.Empty);
    }

    private void OnSlaveConnectionInitialized()
    {
      var handler = SlaveConnectionInitialized;

      if (handler != null) handler(this, EventArgs.Empty);
    }

    public void RefuseConnectionAndTerminate()
    {
      SendPacket(0,2,2);
      Dispose();
    }

    public TCPServerSlaveConnection SlaveConnection
    {
      get
      {
        if (_slaveConnection == null)
          InitializeSlaveConnection();
        return _slaveConnection;
      }
    }

    public TCPServerMasterConnection GetMasterConnection(ICollection<int> requestedData)
    {
      if (_masterConnection == null)
      {
        InitializeMasterConnection(requestedData);
      }
      return _masterConnection;
    }

    private void InitializeSlaveConnection()
    {
      if(ClientPriority == ClientPriority.Undefined)
        throw new NotSupportedException("Cannot create slave connection for unconnected client. Await handshake first.");

      if(ClientPriority == ClientPriority.Master)
        throw new NotSupportedException("Cannot create slave connection for master client.");

      _slaveConnection = new TCPServerSlaveConnection(HostContext, ClientConnection, ClientId, ClientPriority);
    }

    private void InitializeMasterConnection(ICollection<int> requestedData)
    {
      if (ClientPriority == ClientPriority.Undefined)
        throw new NotSupportedException("Cannot create master connection for unconnected client. Await handshake first.");

      if (ClientPriority != ClientPriority.Master)
        throw new NotSupportedException("Cannot create master connection for slave client.");

      _masterConnection = new TCPServerMasterConnection(HostContext, ClientConnection, ClientId, requestedData);
    }

    protected override void HandleHandshake()
    {
      RequestedData.Clear();
      try
      {
        ExpectResponse(ResponseType.Hello, 0);
      }
      catch
      {
        SendPacket(Pack(0, 2, 255));
        throw;
      }

      switch (ClientPriority)
      {
        case ClientPriority.Undefined:
          throw new ZusiTcpException("Client has an undefined priority. This is not supported.");
        case ClientPriority.Master:
          OnMasterConnectionInitialized();
          break;
        default:
          OnSlaveConnectionInitialized();
          break;
      }
    }

    protected override void ReceiveLoop()
    {
      // Nothing to do.
    }
  }
}