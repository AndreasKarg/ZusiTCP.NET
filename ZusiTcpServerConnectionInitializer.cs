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
  internal class ZusiTcpServerConnectionInitializer : ZusiTcpBaseConnection
  {
    private ZusiTcpServerMasterConnection _masterConnection;
    private ZusiTcpServerSlaveConnection _slaveConnection;
    private CommandSet _commands;

    #region Delegated Base Constructors

    public ZusiTcpServerConnectionInitializer(string clientId, ClientPriority priority, CommandSet commands, SynchronizationContext hostContext)
      : base(clientId, priority, hostContext)
    {
      _commands = commands;
    }

    public ZusiTcpServerConnectionInitializer(string clientId, ClientPriority priority, CommandSet commands)
      : base(clientId, priority)
    {
      _commands = commands;
    }

    #endregion

    public ZusiTcpServerSlaveConnection GetSlaveConnection(SynchronizationContext hostContext)
    {
      if (_slaveConnection == null)
      {
        InitializeSlaveConnection(hostContext);
      }
      return _slaveConnection;
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
      PacketSender.SendPacket(0, 2, 2);
      Dispose();
    }

    public ZusiTcpServerMasterConnection GetMasterConnection(SynchronizationContext hostContext, IEnumerable<int> requestedData)
    {
      if (_masterConnection == null)
      {
        InitializeMasterConnection(hostContext, requestedData);
      }
      return _masterConnection;
    }

    private void InitializeSlaveConnection(SynchronizationContext hostContext)
    {
      if (ClientPriority == ClientPriority.Undefined)
      {
        throw new NotSupportedException("Cannot create slave connection for unconnected client. Await handshake first.");
      }

      if (ClientPriority == ClientPriority.Master)
      {
        throw new NotSupportedException("Cannot create slave connection for master client.");
      }

      _slaveConnection = new ZusiTcpServerSlaveConnection(hostContext, ClientConnection, ClientId, ClientPriority);
    }

    private void InitializeMasterConnection(SynchronizationContext hostContext, IEnumerable<int> requestedData)
    {
      if (ClientPriority == ClientPriority.Undefined)
      {
        throw new NotSupportedException("Cannot create master connection for unconnected client. Await handshake first.");
      }

      if (ClientPriority != ClientPriority.Master)
      {
        throw new NotSupportedException("Cannot create master connection for slave client.");
      }

      _masterConnection = new ZusiTcpServerMasterConnection(hostContext, ClientConnection, ClientId, new List<int>(requestedData), _commands);
    }

    protected override void HandleHandshake()
    {
      try
      {
        ExpectResponse(ResponseType.Hello, 0);
      }
      catch
      {
        PacketSender.SendPacket(PacketSender.Pack(0, 2, 255));
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
