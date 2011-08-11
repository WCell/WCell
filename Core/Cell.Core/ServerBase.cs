/*************************************************************************
 *
 *   file		: ServerBase.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-16 21:33:51 +0100 (lø, 16 jan 2010) $
 
 *   revision		: $Rev: 1197 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Cell.Core.Localization;
using NLog;
using System.Net.NetworkInformation;
using Cell.Core.Exceptions;

namespace Cell.Core
{
	/// <summary>
	/// Container class for the server object and the client IP.
	/// </summary>
	public class UDPSendToArgs
	{
		private ServerBase _server;
		private IPEndPoint _client;

		/// <summary>
		/// The server object receiving the UDP communications.
		/// </summary>
		public ServerBase Server
		{
			get { return _server; }
		}

		/// <summary>
		/// The IP address the data was received from.
		/// </summary>
		public IPEndPoint ClientIP
		{
			get { return _client; }
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="srvr">The server object receiving the UP communications.</param>
		/// <param name="client">The IP address the data was received from.</param>
		public UDPSendToArgs(ServerBase srvr, IPEndPoint client)
		{
			_server = srvr;
			_client = client;
		}
	}

	#region Event Delegates
	/// <summary>
	/// Handler used for the client connected event
	/// </summary>
	/// <param name="client">The client connection</param>
	public delegate void ClientConnectedHandler(IClient client);

	/// <summary>
	/// Handler used for client disconnected event
	/// </summary>
	/// <param name="client">The client connection</param>
	/// <param name="forced">Indicates if the client disconnection was forced</param>
	public delegate void ClientDisconnectedHandler(IClient client, bool forced);

	#endregion

	/// <summary>
	/// This is the base class for all server classes.
	/// <seealso cref="ClientBase"/>
	/// </summary>
	public abstract class ServerBase : IDisposable
	{
		#region Private variables

		protected static readonly Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// A hashtable containing all of the clients connected to the server.
		/// <seealso cref="ClientBase"/>
		/// </summary>
		protected HashSet<IClient> _clients = new HashSet<IClient>();

		/// <summary>
		/// The remote endpoint (IP address and port) of the adapter to use with TCP communiations.
		/// </summary>
		protected IPEndPoint _tcpEndpoint; // = new IPEndPoint(GetDefaultExternalIPAddress(), 0);

		/// <summary>
		/// The remote endpoint (IP address and port) of the adapter to use with UDP communiations.
		/// </summary>
		protected IPEndPoint _udpEndpoint; // = new IPEndPoint(GetDefaultExternalIPAddress(), 0);

		/// <summary>
		/// The socket the server listens on for incoming TCP connections.
		/// <seealso cref="ServerBase.Start"/>
		/// <seealso cref="ServerBase.TcpIP"/>
		/// <seealso cref="ServerBase.TcpPort"/>
		/// </summary>
		protected Socket _tcpListen;

		/// <summary>
		/// The socket the server listens on for incoming UDP packets.
		/// </summary>
		protected Socket _udpListen;

		/// <summary>
		/// The maximum number of pending connections.
		/// </summary>
		protected int _maxPendingCon = 100;

		/// <summary>
		/// True if the server is currently accepting connections.
		/// </summary>
		protected /*volatile*/ bool _running;

		/// <summary>
		/// True if TCP is enabled, default is true.
		/// </summary>
		protected bool TcpEnabledEnabled;

		/// <summary>
		/// True if UDP is enabled, default is false.
		/// </summary>
		protected bool UdpEnabledEnabled;

		/// <summary>
		/// The buffer for incoming UDP data.
		/// </summary>
		private byte[] _udpBuffer = new byte[1024];
		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the current status of the server.
		/// </summary>
		public virtual bool IsRunning
		{
			get { return _running; }
			set { _running = value; }
		}

		/// <summary>
		/// Gets/Sets the maximum number of pending connections.
		/// </summary>
		/// <value>The maximum number of pending connections.</value>
		public virtual int MaximumPendingConnections
		{
			get { return _maxPendingCon; }
			set
			{
				if (value > 0)
				{
					_maxPendingCon = value;
				}
			}
		}

		/// <summary>
		/// Gets/Sets the port the server will listen on for incoming TCP connections.
		/// <seealso cref="ServerBase.Start"/>
		/// <seealso cref="ServerBase.TcpIP"/>
		/// </summary>
		public virtual int TcpPort
		{
			get { return _tcpEndpoint.Port; }
			set { _tcpEndpoint.Port = value; }
		}

		/// <summary>
		/// Gets/Sets the port the server will listen on for incoming UDP connections.
		/// <seealso cref="ServerBase.Start"/>
		/// <seealso cref="ServerBase.UdpIP"/>
		/// </summary>
		public virtual int UdpPort
		{
			get { return _udpEndpoint.Port; }
			set { _udpEndpoint.Port = value; }
		}

		/// <summary>
		/// The IP address of the adapter the server will use for TCP communications.
		/// <seealso cref="ServerBase.Start"/>
		/// <seealso cref="ServerBase.TcpPort"/>
		/// </summary>
		public virtual IPAddress TcpIP
		{
			get { return _tcpEndpoint.Address; }
			set { _tcpEndpoint.Address = value; }
		}

		/// <summary>
		/// The IP address of the adapter the server will use for UDP communications.
		/// </summary>
		public virtual IPAddress UdpIP
		{
			get { return _udpEndpoint.Address; }
			set { _udpEndpoint.Address = value; }
		}

		/// <summary>
		/// The endpoint clients will connect to for TCP connections
		/// </summary>
		public virtual IPEndPoint TcpEndPoint
		{
			get { return _tcpEndpoint; }
			set { _tcpEndpoint = value; }
		}

		/// <summary>
		/// The endpoint clients will connect to for UDP connections
		/// </summary>
		public virtual IPEndPoint UdpEndPoint
		{
			get { return _udpEndpoint; }
			set { _udpEndpoint = value; }
		}

		/// <summary>
		/// Gets the number of clients currently connected to the server.
		/// </summary>
		public int ClientCount
		{
			get { return _clients.Count; }
		}

		/// <summary>
		/// The root path of this server assembly.
		/// </summary>
		public string RootPath
		{
			get { return Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName; }
		}

		/// <summary>
		/// Gets/Sets whether or not to use TCP communications.
		/// </summary>
		public bool TCPEnabled
		{
			get { return TcpEnabledEnabled; }
			set
			{
				if (_running && TcpEnabledEnabled != value)
				{
					if (value)
					{
						StartTCP();
					}
					else
					{
						StopTCP();
					}
				}
			}
		}

		/// <summary>
		/// Gets/Sets whether or not to use UDP communications.
		/// </summary>
		public bool UDPEnabled
		{
			get { return UdpEnabledEnabled; }
			set
			{
				if (UdpEnabledEnabled && !value && _running)
				{
					_udpListen.Close(60);
				}
				else if (!UdpEnabledEnabled && value && _running)
				{
					StartUDP();
				}
			}
		}

		/// <summary>
		/// Holds the sequence number for UDP packets
		/// </summary>
		public ushort UdpCounter { get; set; }

		#endregion

		#region Public Events

		public event ClientConnectedHandler ClientConnected;
		public event ClientDisconnectedHandler ClientDisconnected;

		#endregion

		#region State management
		/// <summary>
		/// Starts the server and begins accepting connections.
		/// <seealso cref="ServerBase.Stop"/>
		/// </summary>
		public virtual void Start(bool useTcp, bool useUdp)
		{
			try
			{
				if (!_running)
				{
					log.Info(Resources.BaseStart);
					IsRunning = true;

					if (useTcp)
					{
						//_tcpEndpoint = new IPEndPoint(GetDefaultExternalIPAddress(), 0);
						StartTCP();
					}
					if (useUdp)
					{
						//_udpEndpoint = new IPEndPoint(GetDefaultExternalIPAddress(), 0);
						StartUDP();
					}

					log.Info(Resources.ReadyForConnections, this);
				}
			}
			catch (InvalidEndpointException ex)
			{
				log.Fatal(Resources.InvalidEndpoint, ex.Endpoint);

				Stop();
			}
			catch (NoAvailableAdaptersException)
			{
				log.Fatal(Resources.NoNetworkAdapters);

				Stop();
			}
			//catch (SocketException e)
			//{
			//    // port is already occupied?
			//}
		}

		/// <summary>
		/// Stops the server and disconnects all clients.
		/// <seealso cref="ServerBase.Start"/>
		/// <seealso cref="ServerBase.RemoveAllClients"/>
		/// <seealso cref="ServerBase.DisconnectClient(Cell.Core.IClient)"/>
		/// </summary>
		public virtual void Stop()
		{
			log.Info(Resources.BaseStop);

			if (IsRunning)
			{
				IsRunning = false;

				RemoveAllClients();

				if (_tcpListen != null)
				{
					_tcpListen.Close(60);
				}

				if (_udpListen != null)
				{
					_udpListen.Close();
				}
			}
		}

		#endregion

		#region Client management

		/// <summary>
		/// Creates a new client object.
		/// <seealso cref="ServerBase.Start"/>
		/// </summary>
		/// <returns>A client object to wrap an incoming connection.</returns>
		protected abstract IClient CreateClient();

		/// <summary>
		/// Removes a client from the internal client list.
		/// <seealso cref="ServerBase.RemoveAllClients"/>
		/// </summary>
		/// <param name="client">The client to be removed</param>
		protected void RemoveClient(IClient client)
		{
			lock (_clients)
			{
				_clients.Remove(client);
			}
		}

		/// <summary>
		/// Disconnects and removes a client.
		/// <seealso cref="ServerBase.Stop"/>
		/// <seealso cref="ServerBase.RemoveAllClients"/>
		/// </summary>
		/// <param name="client">The client to be disconnected/removed</param>
		/// <param name="forced">Flag indicating if the client was disconnected already</param>
		public void DisconnectClient(IClient client, bool forced)
		{
			RemoveClient(client);

			try
			{
				OnClientDisconnected(client, forced);
				client.Dispose();
			}
			catch (ObjectDisposedException)
			{
				// Connection was already closed (probably by the remote side)
			}
			catch (Exception e)
			{
				LogManager.GetLogger(CellDef.CORE_LOG_FNAME).ErrorException("Could not disconnect client", e);
			}
		}

		/// <summary>
		/// Disconnects and removes a client.
		/// <seealso cref="ServerBase.Stop"/>
		/// <seealso cref="ServerBase.RemoveAllClients"/>
		/// </summary>
		/// <param name="client">The client to be disconnected/removed</param>
		public void DisconnectClient(IClient client)
		{
			DisconnectClient(client, true);
		}

		/// <summary>
		/// Disconnects all clients currently connected to the server.
		/// <seealso cref="ServerBase.Stop"/>
		/// <seealso cref="ServerBase.DisconnectClient(IClient)"/>
		/// </summary>
		public void RemoveAllClients()
		{
			lock (_clients)
			{
				foreach (var client in _clients)
				{
					try
					{
						OnClientDisconnected(client, true);

					}
					catch (ObjectDisposedException)
					{
					}
					catch (Exception e)
					{
						LogManager.GetLogger(CellDef.CORE_LOG_FNAME).Error(e.ToString());
					}
				}

				_clients.Clear();
			}
		}

		/// <summary>
		/// Called when a client has connected to the server.
		/// </summary>
		/// <param name="client">The client that has connected.</param>
		/// <returns>True if the connection is to be accepted.</returns>
		protected virtual bool OnClientConnected(IClient client)
		{
			Info(client, Resources.ClientConnected);

			ClientConnectedHandler handler = ClientConnected;
			if (handler != null)
			{
				handler(client);
			}

			return true;
		}

		/// <summary>
		/// Called when a client has been disconnected from the server.
		/// </summary>
		/// <param name="client">The client that has been disconnected.</param>
		/// <param name="forced">Indicates if the client disconnection was forced</param>
		protected virtual void OnClientDisconnected(IClient client, bool forced)
		{
			Info(client, Resources.ClientDisconnected);

			ClientDisconnectedHandler handler = ClientDisconnected;
			if (handler != null)
				handler(client, forced);

			client.Dispose();
		}

		#endregion

		#region Socket management

		/// <summary>
		/// Verifies that an endpoint exists as an address on the local network interfaces.
		/// </summary>
		/// <param name="endPoint">the endpoint to verify</param>
		public static void VerifyEndpointAddress(IPEndPoint endPoint)
		{
			if (!endPoint.Address.Equals(IPAddress.Any) &&
				!endPoint.Address.Equals(IPAddress.Loopback))
			{
				var interfaces = NetworkInterface.GetAllNetworkInterfaces();
				var endpointAddr = endPoint.Address;

				if (interfaces.Length > 0)
				{
					foreach (NetworkInterface iface in interfaces)
					{
						UnicastIPAddressInformationCollection uniAddresses = iface.GetIPProperties().UnicastAddresses;

						if (uniAddresses.Where(ipInfo => ipInfo.Address.Equals(endpointAddr)).Any())
						{
							return;
						}
					}

					throw new InvalidEndpointException(endPoint);
				}
				throw new NoAvailableAdaptersException();
			}
		}

		/// <summary>
		/// Get the default external IP address for the current machine. This is always the first
		/// IP listed in the host address list.
		/// </summary>
		/// <returns></returns>
		public static IPAddress GetDefaultExternalIPAddress()
		{
			return IPAddress.Loopback;
		}

		/// <summary>
		/// Begin listening for TCP connections. Should not be called directly - instead use <see cref="Start"/>
		/// <seealso cref="TCPEnabled"/>
		/// </summary>
		protected void StartTCP()
		{
			if (!TcpEnabledEnabled && _running)
			{
				VerifyEndpointAddress(TcpEndPoint);

				_tcpListen = new Socket(TcpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				try
				{
					_tcpListen.Bind(TcpEndPoint);
				}
				catch (Exception ex)
				{
					log.Error("Could not bind to Address {0}: {1}", TcpEndPoint, ex);
					return;
				}

				_tcpListen.Listen(MaximumPendingConnections);

				SocketHelpers.SetListenSocketOptions(_tcpListen);

				// We pass null the first time to create the arg
				StartAccept(null);

				TcpEnabledEnabled = true;
				Info(null, Resources.ListeningTCPSocket, TcpEndPoint);
			}
		}

		/// <summary>
		/// Begin listening for TCP connections. Should not be called directly - instead use <see cref="Start"/>
		/// <seealso cref="TCPEnabled"/>
		/// </summary>
		protected void StopTCP()
		{
			if (TcpEnabledEnabled)
			{
				try
				{
					_tcpListen.Close();
				}
				catch (Exception ex)
				{
					log.Warn("Exception occured while trying to close the TCP Connection", TcpEndPoint, ex);
				}

				_tcpListen = null;

				TcpEnabledEnabled = false;
				Info(null, Resources.ListeningTCPSocketStopped, TcpEndPoint);
			}
		}

		/// <summary>
		/// Begin listening for UDP connections. Should not be called directly - instead use <see cref="Start"/>
		/// <seealso cref="TCPEnabled"/>
		/// </summary>
		public void StartUDP()
		{
			if (!UdpEnabledEnabled && _running)
			{
				IPEndPoint udpEndpoint = new IPEndPoint(UdpIP, UdpPort);
				VerifyEndpointAddress(udpEndpoint);

				_udpListen = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				_udpListen.Bind(udpEndpoint);

				// We pass null the first time to create the arg
				StartReceivingUdp(null);

				UdpEnabledEnabled = true;
				Info(null, Resources.ListeningUDPSocket, UdpEndPoint);
			}
		}

		protected void StartAccept(SocketAsyncEventArgs acceptEventArg)
		{
			if (acceptEventArg == null)
			{
				acceptEventArg = new SocketAsyncEventArgs();
				acceptEventArg.Completed += AcceptEventCompleted;
			}
			else
			{
				// socket must be cleared since the context object is being reused
				acceptEventArg.AcceptSocket = null;
			}

			bool willRaiseEvent = _tcpListen.AcceptAsync(acceptEventArg);
			if (!willRaiseEvent)
			{
				ProcessAccept(acceptEventArg);
			}
		}

		private void AcceptEventCompleted(object sender, SocketAsyncEventArgs e)
		{
			ProcessAccept(e);
		}

		private void ProcessAccept(SocketAsyncEventArgs args)
		{
			try
			{
				if (!_running)
				{
					LogManager.GetLogger(CellDef.CORE_LOG_FNAME).Info(Resources.ServerNotRunning);
					return;
				}

				IClient client = CreateClient();
				client.TcpSocket = args.AcceptSocket;
				client.BeginReceive();

				StartAccept(args);

				if (OnClientConnected(client))
				{
					lock (_clients)
					{
						_clients.Add(client);
					}
				}
				else
				{
					client.TcpSocket.Shutdown(SocketShutdown.Both);
					client.TcpSocket.Close();
				}

			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException e)
			{
				// TODO: Add a proper exception handling for the different SocketExceptions Error Codes.
				LogManager.GetLogger(CellDef.CORE_LOG_FNAME).WarnException(Resources.SocketExceptionAsyncAccept, e);
			}
			catch (Exception e)
			{
				LogManager.GetLogger(CellDef.CORE_LOG_FNAME).FatalException(Resources.FatalAsyncAccept, e);
			}
		}

		#region UDP

		protected void StartReceivingUdp(SocketAsyncEventArgs args)
		{
			if (args == null)
			{
				args = new SocketAsyncEventArgs();
				args.Completed += UdpRecvEventCompleted;
			}

			EndPoint tempEP = new IPEndPoint(IPAddress.Any, 0);
			args.RemoteEndPoint = tempEP;

			args.SetBuffer(_udpBuffer, 0, _udpBuffer.Length);

			bool willRaiseEvent = _udpListen.ReceiveAsync(args);
			if (!willRaiseEvent)
			{
				ProcessUdpReceive(args);
			}
		}

		/// <summary>
		/// Handles an incoming UDP datagram.
		/// </summary>
		/// <param name="ar">The results of the asynchronous operation.</param>
		private void UdpRecvEventCompleted(object sender, SocketAsyncEventArgs e)
		{
			ProcessUdpReceive(e);
		}

		/// <summary>
		/// Handles an incoming UDP datagram.
		/// </summary>
		/// <param name="args">The results of the asynchronous operation.</param>
		private void ProcessUdpReceive(SocketAsyncEventArgs args)
		{
			try
			{
				int num_bytes = args.BytesTransferred;

				EndPoint ip = new IPEndPoint(IPAddress.Any, 0);
				ip = args.RemoteEndPoint;

				OnReceiveUDP(num_bytes, _udpBuffer, ip as IPEndPoint);

				StartReceivingUdp(args);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException e)
			{
				// TODO: Add a proper exception handling for the different SocketExceptions Error Codes.
				LogManager.GetLogger(CellDef.CORE_LOG_FNAME).WarnException(Resources.SocketExceptionAsyncAccept, e);
			}
			catch (Exception e)
			{
				LogManager.GetLogger(CellDef.CORE_LOG_FNAME).FatalException(Resources.FatalAsyncAccept, e);
			}
		}

		/// <summary>
		/// Handler for a UDP datagram.
		/// </summary>
		/// <param name="num_bytes">The number of bytes in the datagram.</param>
		/// <param name="buf">The buffer holding the datagram.</param>
		/// <param name="ip">The IP address of the sender.</param>
		protected abstract void OnReceiveUDP(int num_bytes, byte[] buf, IPEndPoint ip);

		/// <summary>
		/// Asynchronously sends a UDP datagram to the client.
		/// </summary>
		/// <param name="buf">An array of bytes containing the packet to be sent.</param>
		/// <param name="client">An IPEndPoint for the datagram to be sent to.</param>
		protected void SendUDP(byte[] buf, IPEndPoint client)
		{
			if (_udpListen != null)
			{
				_udpListen.BeginSendTo(buf, 0, buf.Length, SocketFlags.None, client, SendToCallback,
										new UDPSendToArgs(this, client));
			}
		}

		/// <summary>
		/// Called when a datagram has been sent.
		/// </summary>
		/// <param name="ar">The result of the asynchronous operation.</param>
		private static void SendToCallback(IAsyncResult ar)
		{
			UDPSendToArgs args = ar.AsyncState as UDPSendToArgs;
			try
			{
				if (args != null)
				{
					int num_bytes = args.Server._udpListen.EndSendTo(ar);
					args.Server.OnSendTo(args.ClientIP, num_bytes);
				}
			}
			catch (Exception e)
			{
				if (args != null) args.Server.Error(null, e);
			}
		}

		/// <summary>
		/// Called when a datagram has been sent.
		/// </summary>
		/// <param name="clientIP">The IP address of the recipient.</param>
		/// <param name="num_bytes">The number of bytes sent.</param>
		protected abstract void OnSendTo(IPEndPoint clientIP, int num_bytes);

		#endregion

		#endregion

		#region Logging

		/// <summary>
		/// Create a string for logging information about a given client given a formatted message and parameters
		/// </summary>
		/// <param name="client">Client which caused the event</param>
		/// <param name="msg">Message describing the event</param>
		/// <param name="parms">Parameters for formatting the message.</param>
		protected static string FormatLogString(IClient client, string msg, params object[] parms)
		{
			msg = (parms == null ? msg : string.Format(msg, parms));

			return client == null ? msg : string.Format("({0}) -> {1}", client, msg);
		}

		/// <summary>
		/// Generates a server error.
		/// </summary>
		/// <param name="e">An exception describing the error.</param>
		/// <param name="client">The client that generated the error.</param>
		public void Error(IClient client, Exception e)
		{
			Error(client, "Exception raised: " + e);
		}

		/// <summary>
		/// Generates a server error.
		/// </summary>
		/// <param name="parms">Parameters for formatting the message.</param>
		/// <param name="msg">The message describing the error.</param>
		/// <param name="client">The client that generated the error.</param>
		public virtual void Error(IClient client, string msg, params object[] parms)
		{
			log.Error(FormatLogString(client, msg, parms));
		}

		/// <summary>
		/// Generates a server warning.
		/// </summary>
		/// <param name="e">An exception describing the warning.</param>
		/// <param name="client">The client that generated the error.</param>
		public virtual void Warning(IClient client, Exception e)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn("{0} - {1}", client, e);
			}
		}

		/// <summary>
		/// Generates a server warning.
		/// </summary>
		/// <param name="parms">Parameters for formatting the message.</param>
		/// <param name="msg">The message describing the warning.</param>
		/// <param name="client">The client that generated the error.</param>
		public virtual void Warning(IClient client, string msg, params object[] parms)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(FormatLogString(client, msg, parms));
			}
		}

		/// <summary>
		/// Generates a server notification.
		/// </summary>
		/// <param name="msg">Text describing the notification.</param>
		/// <param name="parms">The parameters to pass to the function for formatting.</param>
		/// <param name="client">The client that generated the error.</param>
		public virtual void Info(IClient client, string msg, params object[] parms)
		{
			if (log.IsWarnEnabled)
			{
				log.Info(FormatLogString(client, msg, parms));
			}
		}

		/// <summary>
		/// Generates a server debug message.
		/// </summary>
		/// <param name="msg">Text describing the notification.</param>
		/// <param name="parms">The parameters to pass to the function for formatting.</param>
		/// <param name="client">The client that generated the error.</param>
		public virtual void Debug(IClient client, string msg, params object[] parms)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(FormatLogString(client, msg, parms));
			}
		}

		#endregion

		#region IDisposable

		~ServerBase()
		{
			Dispose(false);
		}

		/// <summary>
		/// Don't call this method outside of the Context that manages the server.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_running)
			{
				Stop();
			}
		}

		#endregion

		#region WMI
		//private bool IsNetworkConnected()
		//{
		//    bool connected = SystemInformation.Network;
		//    if (connected)
		//    {
		//        connected = false;
		//        System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher("SELECT NetConnectionStatus FROM Win32_NetworkAdapter");
		//        foreach (System.Management.ManagementObject networkAdapter in searcher.Get())
		//        {
		//            if (networkAdapter["NetConnectionStatus"] != null)
		//            {
		//                if (Convert.ToInt32(networkAdapter["NetConnectionStatus"]).Equals(2))
		//                {
		//                    connected = true;
		//                    break;
		//                }
		//            }
		//        }
		//        searcher.Dispose();
		//    }
		//    return connected;
		//}
		#endregion
	}
}