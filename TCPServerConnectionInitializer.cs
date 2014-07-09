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

#region Using

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Zusi_Datenausgabe
{
  internal class TCPServerConnectionInitializer : Base_Connection
  {
    private TCPServerMasterConnection _masterConnection;
    private TCPServerSlaveConnection _slaveConnection;
    private TCPCommands _commands;

    #region Delegated Base Constructors

    public TCPServerConnectionInitializer(string clientId, ClientPriority priority, TCPCommands commands, SynchronizationContext hostContext)
      : base(clientId, priority, hostContext)
    {
      _commands = commands;
    }

    public TCPServerConnectionInitializer(string clientId, ClientPriority priority, TCPCommands commands)
      : base(clientId, priority)
    {
      _commands = commands;
    }

    #endregion

    public TCPServerSlaveConnection SlaveConnection
    {
      get
      {
        if (_slaveConnection == null)
        {
          InitializeSlaveConnection();
        }
        return _slaveConnection;
      }
    }

    public event EventHandler SlaveConnectionInitialized;
    public event EventHandler MasterConnectionInitialized;

    private void OnMasterConnectionInitialized()
    {
      EventHandler handler = MasterConnectionInitialized;
      if (handler != null)
      {
        handler(this, EventArgs.Empty);
      }
    }

    private void OnSlaveConnectionInitialized()
    {
      EventHandler handler = SlaveConnectionInitialized;

      if (handler != null)
      {
        handler(this, EventArgs.Empty);
      }
    }

    public void RefuseConnectionAndTerminate()
    {
      SendPacket(0, 2, 2);
      Dispose();
    }

    public TCPServerMasterConnection GetMasterConnection(IEnumerable<int> requestedData)
    {
      if (_masterConnection == null)
      {
        InitializeMasterConnection(requestedData);
      }
      return _masterConnection;
    }

    private void InitializeSlaveConnection()
    {
      if (ClientPriority == ClientPriority.Undefined)
      {
        throw new NotSupportedException("Cannot create slave connection for unconnected client. Await handshake first.");
      }

      if (ClientPriority == ClientPriority.Master)
      {
        throw new NotSupportedException("Cannot create slave connection for master client.");
      }

      _slaveConnection = new TCPServerSlaveConnection(HostContext, ClientConnection, ClientId, ClientPriority);
    }

    private void InitializeMasterConnection(IEnumerable<int> requestedData)
    {
      if (ClientPriority == ClientPriority.Undefined)
      {
        throw new NotSupportedException("Cannot create master connection for unconnected client. Await handshake first.");
      }

      if (ClientPriority != ClientPriority.Master)
      {
        throw new NotSupportedException("Cannot create master connection for slave client.");
      }

      _masterConnection = new TCPServerMasterConnection(HostContext, ClientConnection, ClientId, requestedData, _commands);
    }

    protected override void HandleHandshake()
    {
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
