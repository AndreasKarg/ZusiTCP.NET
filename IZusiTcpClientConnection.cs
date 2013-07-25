using System;
using System.Collections.Generic;
using System.Net;
using Zusi_Datenausgabe.EventManager;
using Zusi_Datenausgabe.ReadOnlyDictionary;

namespace Zusi_Datenausgabe
{
    public interface IZusiTcpClientConnection : ITypedEventSubscriber, IEventSubscriber<int>
    {
        /// <summary>
        /// Event called when an error has occured within the TCP interface.
        /// </summary>
        event EventHandler<ErrorEventArgs> ErrorReceived;

        /// <summary>
        /// Represents the name of the client.
        /// </summary>
        string ClientId { get; }

        /// <summary>
        /// Represents all measurements available in Zusi as a key-value list. Can be used to convert plain text names of
        /// measurements to their internal ID.
        /// </summary>
        /// <example>
        /// <code>
        /// ZusiTcpClientConnection myConn = [...]
        ///
        /// int SpeedID = myConn.IDs["Geschwindigkeit"]
        /// /* SpeedID now contains the value 01. */
        /// </code>
        /// </example>
        IReadOnlyDictionary<string, int> IDs { get; }

        /// <summary>
        /// Represents all measurements available in Zusi as a key-value list. Can be used to convert measurement IDs to their
        /// plain text name.
        /// </summary>
        /// <example>
        /// <code>
        /// ZusiTcpClientConnection myConn = [...]
        ///
        /// string SpeedName = myConn.ReverseIDs[1] /* ID 01 == current speed */
        /// /* SpeedName now contains the value "Geschwindigkeit". */
        /// </code>
        /// </example>
        IReadOnlyDictionary<int, string> ReverseIDs { get; }

        /// <summary>
        /// Represents the current connection state of the client.
        /// </summary>
        ConnectionState ConnectionState { get; }

        /// <summary>
        /// Represents the priority of the client. Cannot be changed after object creation.
        /// </summary>
        ClientPriority ClientPriority { get; }

        /// <summary>
        /// Represents a list of all measurements requested from Zusi. Add your required measurements
        /// here before connecting to the server.
        /// <seealso cref="IDs"/>
        /// </summary>
        List<int> RequestedData { get; }

        /// <summary>
        /// Returns the ID of the measurement specified in plain text.
        /// </summary>
        /// <param name="name">Name of the measurement.</param>
        /// <returns>Internal ID of the measurement.</returns>
        int this[string name] { get; }

        /// <summary>
        /// Returns the plain text name of the measurement specified by its ID.
        /// </summary>
        /// <param name="id">Internal ID of the measurement.</param>
        /// <returns>Name of the measurement.</returns>
        string this[int id] { get; }

        /// <summary>
        /// Establish a connection to the TCP server.
        /// </summary>
        /// <param name="hostName">The name or IP address of the host.</param>
        /// <param name="port">The port on the server to connect to (Default: 1435).</param>
        /// <exception cref="ArgumentException">This exception is thrown when the host address could
        /// not be resolved.</exception>
        void Connect(string hostName, int port);

        /// <summary>
        /// Establish a connection to the TCP server.
        /// </summary>
        /// <param name="endPoint">Specifies an IP end point to which the interface tries to connect.</param>
        /// <exception cref="ZusiTcpException">This exception is thrown when the connection could not be
        /// established.</exception>
        void Connect(IPEndPoint endPoint);

        /// <summary>
        /// Disconnect from the TCP server.
        /// </summary>
        void Disconnnect();

        /// <summary>
        /// Request the measurement passed as plain text in the parameter "name" from the server. Shorthand for
        /// <c>TCP.RequestedData.Add(TCP.IDs[name]);</c>.
        /// </summary>
        /// <param name="name">The name of the measurement.</param>
        void RequestData(string name);

        /// <summary>
        /// Request the measurement passed as ID in the parameter "id" from the server. Shorthand for
        /// <c>TCP.RequestedData.Add(id);</c>.
        /// </summary>
        /// <param name="id">The ID of the measurement.</param>
        void RequestData(int id);

        //TODO: Document
        void RequestData<T>(string name, EventHandler<DataReceivedEventArgs<T>> eventHandler);
        void RequestData<T>(int id, EventHandler<DataReceivedEventArgs<T>> eventHandler);
    }
}