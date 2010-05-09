/*************************************************************************
 *
 *   file		: ClientBase.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-01 23:18:34 +0100 (fr, 01 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1164 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;

namespace Cell.Core
{
	/// <summary>
	/// Base class for all clients.
	/// </summary>
	/// <seealso cref="ServerBase"/>
	public abstract class ClientBase : IClient
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		//public const int MinBufferSize = 1024;
		public const int BufferSize = CellDef.MAX_PBUF_SEGMENT_SIZE;

		private static readonly BufferManager Buffers = BufferManager.Default;


		/// <summary>
		/// Total number of bytes that have been received by all clients.
		/// </summary>
		private static long _totalBytesReceived;

		/// <summary>
		/// Total number of bytes that have been sent by all clients.
		/// </summary>
		private static long _totalBytesSent;

		/// <summary>
		/// Gets the total number of bytes sent to all clients.
		/// </summary>
		public static long TotalBytesSent
		{
			get { return _totalBytesSent; }
		}

		/// <summary>
		/// Gets the total number of bytes received by all clients.
		/// </summary>
		public static long TotalBytesReceived
		{
			get { return _totalBytesReceived; }
		}

		#region Private variables

		/// <summary>
		/// Number of bytes that have been received by this client.
		/// </summary>
		private uint _bytesReceived;

		/// <summary>
		/// Number of bytes that have been sent by this client.
		/// </summary>
		private uint _bytesSent;

		/// <summary>
		/// The socket containing the TCP connection this client is using.
		/// </summary>
		protected Socket _tcpSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		/// <summary>
		/// Pointer to the server this client is connected to.
		/// </summary>
		protected ServerBase _server;

		/// <summary>
		/// The port the client should receive UDP datagrams on.
		/// </summary>
		protected IPEndPoint _udpEndpoint;

		/// <summary>
		/// The buffer containing the data received.
		/// </summary>
		protected BufferSegment _bufferSegment;

		/// <summary>
		/// The offset in the buffer to write at.
		/// </summary>
		protected int _offset, _remainingLength;

		#endregion

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="server">The server this client is connected to.</param>
		protected ClientBase(ServerBase server)
		{
			_server = server;

			_bufferSegment = Buffers.CheckOut();
		}

		#region Public properties

		public ServerBase Server
		{
			get { return _server; }
		}
		/// <summary>
		/// Gets the IP address of the client.
		/// </summary>
		public IPAddress ClientAddress
		{
			get { return (_tcpSock != null && _tcpSock.RemoteEndPoint != null) ? 
				((IPEndPoint)_tcpSock.RemoteEndPoint).Address : null ; }
		}

		/// <summary>
		/// Gets the port the client is communicating on.
		/// </summary>
		public int Port
		{
			get { return (_tcpSock != null && _tcpSock.RemoteEndPoint != null) ? 
				((IPEndPoint)_tcpSock.RemoteEndPoint).Port : -1; }
		}

		/// <summary>
		/// Gets the port the client should receive UDP datagrams on.
		/// </summary>
		public IPEndPoint UdpEndpoint
		{
			get { return _udpEndpoint; }
			set { _udpEndpoint = value; }
		}

		/// <summary>
		/// Gets/Sets the socket this client is using for TCP communication.
		/// </summary>
		public Socket TcpSocket
		{
			get { return _tcpSock; }
			set
			{
				if (_tcpSock != null && _tcpSock.Connected)
				{
					_tcpSock.Shutdown(SocketShutdown.Both);
					_tcpSock.Close();
				}

				if (value != null)
				{
					_tcpSock = value;
				}
			}
		}

		public uint ReceivedBytes
		{
			get { return _bytesReceived; }
		}

		public uint SentBytes
		{
			get { return _bytesSent; }
		}

		public bool IsConnected
		{
			get { return _tcpSock != null && _tcpSock.Connected; }
		}
		#endregion

		/// <summary>
		/// Begins asynchronous TCP receiving for this client.
		/// </summary>
		public void BeginReceive()
		{
			ResumeReceive();
		}

		/// <summary>
		/// Resumes asynchronous TCP receiving for this client.
		/// </summary>
		private void ResumeReceive()
		{
			if (_tcpSock != null && _tcpSock.Connected)
			{
				var socketArgs = SocketHelpers.AcquireSocketArg();
				var offset = _offset + _remainingLength;

				socketArgs.SetBuffer(_bufferSegment.Buffer.Array, _bufferSegment.Offset + offset, BufferSize - offset);
				socketArgs.UserToken = this;
				socketArgs.Completed += ReceiveAsyncComplete;

				var willRaiseEvent = _tcpSock.ReceiveAsync(socketArgs);
				if (!willRaiseEvent)
				{
					ProcessRecieve(socketArgs);
				}
			}
		}

		private void ProcessRecieve(SocketAsyncEventArgs args)
		{
			try
			{
				var bytesReceived = args.BytesTransferred;

				if (bytesReceived == 0)
				//if (args.SocketError != SocketError.Success)
				{
					// no bytes means the client disconnected, so clean up!
					_server.DisconnectClient(this, true);
				}
				else
				{
					// increment our counters
					unchecked
					{
						_bytesReceived += (uint)bytesReceived;
					}

					Interlocked.Add(ref _totalBytesReceived, bytesReceived);

					_remainingLength += bytesReceived;

					if (OnReceive(_bufferSegment))
					{
						// packet processed entirely
						_offset = 0;
						_bufferSegment.DecrementUsage();
						_bufferSegment = Buffers.CheckOut();
					}
					else
					{
						EnsureBuffer();
					}

					ResumeReceive();
				}
			}
			catch (ObjectDisposedException)
			{
				_server.DisconnectClient(this, true);
			}
			catch (Exception e)
			{
				_server.Warning(this, e);
				_server.DisconnectClient(this, true);
			}
			finally
			{
				args.Completed -= ReceiveAsyncComplete;
				SocketHelpers.ReleaseSocketArg(args);
			}
		}

		private void ReceiveAsyncComplete(object sender, SocketAsyncEventArgs args)
		{
			ProcessRecieve(args);
		}

		/// <summary>
		/// Makes sure the underlying buffer is big enough (but will never exceed BufferSize)
		/// </summary>
		/// <param name="size"></param>
		protected void EnsureBuffer() //(int size)
		{
			//if (size > BufferSize - _offset)
			{
				// not enough space left in buffer: Copy to new buffer
				var newSegment = Buffers.CheckOut();
				Array.Copy(_bufferSegment.Buffer.Array,
					_bufferSegment.Offset + _offset,
					newSegment.Buffer.Array,
					newSegment.Offset,
					_remainingLength);
				_bufferSegment.DecrementUsage();
				_bufferSegment = newSegment;
				_offset = 0;
			}
		}

		/// <summary>
		/// Called when a packet has been received and needs to be processed.
		/// </summary>
		/// <param name="numBytes">The size of the packet in bytes.</param>
		protected abstract bool OnReceive(BufferSegment buffer);

		/// <summary>
		/// Asynchronously sends a packet of data to the client.
		/// </summary>
		/// <param name="packet">An array of bytes containing the packet to be sent.</param>
		public void Send(byte[] packet)
		{
			Send(packet, 0, packet.Length);
		}

		public void SendCopy(byte[] packet)
		{
			var copy = new byte[packet.Length];
			Array.Copy(packet, copy, packet.Length);
			Send(copy, 0, copy.Length);
		}

		public void Send(BufferSegment segment, int length)
		{
			Send(segment.Buffer.Array, segment.Offset, length);
		}

		/// <summary>
		/// Asynchronously sends a packet of data to the client.
		/// </summary>
		/// <param name="packet">An array of bytes containing the packet to be sent.</param>
		/// <param name="length">The number of bytes to send starting at offset.</param>
		/// <param name="offset">The offset into packet where the sending begins.</param>
		public virtual void Send(byte[] packet, int offset, int length)
		{
			if (_tcpSock != null && _tcpSock.Connected)
			{
				var args = SocketHelpers.AcquireSocketArg();
				if (args != null)
				{
					args.Completed += SendAsyncComplete;
					args.SetBuffer(packet, offset, length);
					args.UserToken = this;
					_tcpSock.SendAsync(args);

					unchecked
					{
						_bytesSent += (uint)length;
					}

					Interlocked.Add(ref _totalBytesSent, length);
				}
				else
				{
					log.Error("Client {0}'s SocketArgs are null", this);
				}
			}
		}

		private static void SendAsyncComplete(object sender, SocketAsyncEventArgs args)
		{
			args.Completed -= SendAsyncComplete;
			SocketHelpers.ReleaseSocketArg(args);
		}

		/// <summary>
		/// Connects the client to the server at the specified address and port.
		/// </summary>
		/// <remarks>This function uses IPv4.</remarks>
		/// <param name="host">The IP address of the server to connect to.</param>
		/// <param name="port">The port to use when connecting to the server.</param>
		public void Connect(string host, int port)
		{
			Connect(IPAddress.Parse(host), port);
		}

		/// <summary>
		/// Connects the client to the server at the specified address and port.
		/// </summary>
		/// <remarks>This function uses IPv4.</remarks>
		/// <param name="addr">The IP address of the server to connect to.</param>
		/// <param name="port">The port to use when connecting to the server.</param>
		public void Connect(IPAddress addr, int port)
		{
			if (_tcpSock != null)
			{
                if (_tcpSock.Connected)
                {
                    _tcpSock.Disconnect(true);
                }
			    _tcpSock.Connect(addr, port);

                BeginReceive();
			}
		}

		#region IDisposable

		~ClientBase()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_tcpSock != null && _tcpSock.Connected)
			{
				try
				{
					_bufferSegment.DecrementUsage();
					_tcpSock.Shutdown(SocketShutdown.Both);
					_tcpSock.Close();
					_tcpSock = null;
				}
				catch (SocketException/* exception*/)
				{
					// TODO: Check what exceptions we need to handle
				}
			}
		}

		#endregion

        public override string ToString()
        {
            return
                (TcpSocket == null || !TcpSocket.Connected
                     ? "<disconnected client>"
                     : (TcpSocket.RemoteEndPoint ?? (object) "<unknown client>")).ToString();
        }
	}
}
