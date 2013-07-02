#region header
// /*************************************************************************
//  * ZusiTcpClientNoWindsor.cs
//  * Contains logic for the TCP interface.
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
using System.Net;
using Castle.Windsor;

namespace Zusi_Datenausgabe
{
  public class ZusiTcpClientConnectionNoWindsor : IZusiTcpClientConnection, IDisposable
  {
    private IWindsorContainer _container;
    private readonly IZusiTcpClientConnection _clientConnection;

    private IZusiTcpConnectionFactory _connectionFactory;

    public ZusiTcpClientConnectionNoWindsor()
    {
      _container = new WindsorContainer()
        .Install(new WindsorInstaller()
        );

      _connectionFactory = _container.Resolve<IZusiTcpConnectionFactory>();
    }

    public ZusiTcpClientConnectionNoWindsor(string clientId, ClientPriority priority)
      : this()
    {
      _clientConnection = _connectionFactory.Create(clientId, priority);
    }

    public ZusiTcpClientConnectionNoWindsor(string clientId, ClientPriority priority, string commandsetPath) : this()
    {
      _clientConnection = _connectionFactory.Create(clientId, priority, commandsetPath);
    }


    public ZusiTcpClientConnectionNoWindsor(string clientId, ClientPriority priority, XmlTcpCommands commands)
      : this()
    {
      _clientConnection = _connectionFactory.Create(clientId, priority, commands);
    }

    #region Delegating members to _clientConnection

    public event EventHandler<DataReceivedEventArgs<float>> FloatReceived
    {
      add { _clientConnection.FloatReceived += value; }
      remove { _clientConnection.FloatReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<string>> StringReceived
    {
      add { _clientConnection.StringReceived += value; }
      remove { _clientConnection.StringReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<int>> IntReceived
    {
      add { _clientConnection.IntReceived += value; }
      remove { _clientConnection.IntReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<bool>> BoolReceived
    {
      add { _clientConnection.BoolReceived += value; }
      remove { _clientConnection.BoolReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<DateTime>> DateTimeReceived
    {
      add { _clientConnection.DateTimeReceived += value; }
      remove { _clientConnection.DateTimeReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<DoorState>> DoorsReceived
    {
      add { _clientConnection.DoorsReceived += value; }
      remove { _clientConnection.DoorsReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<PZBSystem>> PZBReceived
    {
      add { _clientConnection.PZBReceived += value; }
      remove { _clientConnection.PZBReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<BrakeConfiguration>> BrakeConfigReceived
    {
      add { _clientConnection.BrakeConfigReceived += value; }
      remove { _clientConnection.BrakeConfigReceived -= value; }
    }

    public event EventHandler<ErrorEventArgs> ErrorReceived
    {
      add { _clientConnection.ErrorReceived += value; }
      remove { _clientConnection.ErrorReceived -= value; }
    }

    public string ClientId
    {
      get { return _clientConnection.ClientId; }
    }

    public IReadOnlyDictionary<string, int> IDs
    {
      get { return _clientConnection.IDs; }
    }

    public IReadOnlyDictionary<int, string> ReverseIDs
    {
      get { return _clientConnection.ReverseIDs; }
    }

    public ConnectionState ConnectionState
    {
      get { return _clientConnection.ConnectionState; }
    }

    public ClientPriority ClientPriority
    {
      get { return _clientConnection.ClientPriority; }
    }

    public List<int> RequestedData
    {
      get { return _clientConnection.RequestedData; }
    }

    public int this[string name]
    {
      get { return _clientConnection[name]; }
    }

    public string this[int id]
    {
      get { return _clientConnection[id]; }
    }

    public void Connect(string hostName, int port)
    {
      _clientConnection.Connect(hostName, port);
    }

    public void Connect(IPEndPoint endPoint)
    {
      _clientConnection.Connect(endPoint);
    }

    public void Disconnnect()
    {
      _clientConnection.Disconnnect();
    }

    public void RequestData(string name)
    {
      _clientConnection.RequestData(name);
    }

    public void RequestData(int id)
    {
      _clientConnection.RequestData(id);
    }

    #endregion

    #region Implementation of IDisposable

    public void Dispose()
    {
      _connectionFactory.Dispose();
      _container.Release(_connectionFactory);
      _container.Dispose();
    }

    #endregion
  }
}