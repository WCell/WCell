/*************************************************************************
 *
 *   file		: AuthClient.cs
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
using Cell.Core;
using WCell.AuthServer.Accounts;
using WCell.Core;
using WCell.Core.Cryptography;

namespace WCell.AuthServer.Network
{
	/// <summary>
	/// Represents a client connected to the authentication server.
	/// </summary>
	public sealed class AuthClient : ClientBase, IAuthClient
	{
		/// <summary>
		/// The server this client is connected to.
		/// </summary>
		public new AuthenticationServer Server
		{
			get { return (AuthenticationServer)_server; }
		}

		/// <summary>
		/// The system information for this client.
		/// </summary>
		public ClientInformation Info
		{
			get;
			set;
		}

		/// <summary>
		/// The current user.
		/// </summary>
		public string AccountName
		{
			get;
			set;
		}

		public Account Account
		{
			get;
			set;
		}

		/// <summary>
		/// The authenticator instance for this client.
		/// </summary>
		public Authenticator Authenticator
		{
			get;
			set;
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="server">reference to the parent AuthServer</param>
		public AuthClient(AuthenticationServer server) : base(server) { }

		/// <summary>
		/// Tries to parse data that has been received.
		/// </summary>
		/// <param name="numBytes">the number of bytes waiting to be read</param>
		protected override bool OnReceive(BufferSegment segment)
		{
			var packet = new AuthPacketIn(segment, 0, _remainingLength);
			segment.IncrementUsage();

			Console.WriteLine("S <- C: " + packet.PacketId);
			AuthPacketManager.Instance.HandlePacket(this, packet);
			_remainingLength = 0;
			return true;
		}

		/// <summary>
		/// Sends a packet to the client.
		/// </summary>
		/// <param name="packet">the packet to send</param>
		public void Send(AuthPacketOut packet)
		{
			var pkt = packet.GetFinalizedPacket();
			Console.WriteLine("S -> C: " + packet.PacketId);
			//Console.WriteLine(WCellUtil.ToHex(packet.PacketId, pkt, 0, pkt.Length));
			Send(pkt);
		}
	}
}