/*************************************************************************
 *
 *   file		: IRealmClient.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-30 02:51:11 +0100 (lø, 30 jan 2010) $

 *   revision		: $Rev: 1233 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Cell.Core;
using NLog;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Cryptography;
using WCell.PacketAnalysis;
using WCell.RealmServer.Debugging;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Stats;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.RealmServer.Network
{
	/// <summary>
	/// Represents a client connected to the realm server
	/// </summary>
	public sealed class RealmClient : ClientBase, IRealmClient
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private byte[] m_sessionKey;

		public static readonly List<IRealmClient> EmptyArray = new List<IRealmClient>();

		/// <summary>
		/// The server this client is connected to.
		/// </summary>
		public new Events.RealmServer Server
		{
			get { return (Events.RealmServer)_server; }
		}

		/// <summary>
		/// The <see cref="ClientInformation">system information</see> for this client.
		/// </summary>
		public ClientInformation Info { get; set; }

		/// <summary>
		/// The compressed addon data sent by the client.
		/// </summary>
		public byte[] Addons { get; set; }

		/// <summary>
		/// The account on this session.
		/// </summary>
		public RealmAccount Account { get; set; }

		/// <summary>
		/// The <see cref="Character" /> that the client is currently playing.
		/// </summary>
		public Character ActiveCharacter { get; set; }

		/// <summary>
		/// Whether or not this client is currently logging out.
		/// </summary>
		public bool IsOffline { get; set; }

		/// <summary>
		/// Whether or not communication with this client is encrypted.
		/// </summary>
		public bool IsEncrypted
		{
			get { return m_sessionKey != null; }
		}

		/// <summary>
		/// The local system uptime of the client.
		/// </summary>
		public uint ClientTime { get; set; }

		/// <summary>
		/// Connection latency between client and server.
		/// </summary>
		public int Latency { get; set; }

		/// <summary>
		/// The amount of time skipped by the client.
		/// </summary>
		/// <remarks>Deals with the the way we calculate movement delay.</remarks>
		public uint OutOfSyncDelay { get; set; }

		public uint LastClientMoveTime { get; set; }

		/// <summary>
		/// The client tick count.
		/// </summary>
		/// <remarks>It is set by opcodes 912/913, and seems to be a client ping sequence that is
		/// local to the map, and thus it resets to 0 on a map change.  Real usage isn't known.</remarks>
		public uint TickCount { get; set; }

		/// <summary>
		/// The client seed sent by the client during re-authentication.
		/// </summary>
		public uint ClientSeed { get; set; }

		/// <summary>
		/// The authentication message digest received from the client during re-authentication.
		/// </summary>
		public BigInteger ClientDigest { get; set; }

		/// <summary>
		/// Create an realm client for a given server.
		/// </summary>
		/// <param name="server">reference to the parent RealmServer</param>
		public RealmClient(Events.RealmServer server)
			: base(server)
		{
		}

		/// <summary>
		/// Pass recieved data into the packet buffer and try to parse.
		/// </summary>
		/// <param name="_remainingLength">number of bytes waiting to be read</param>
		/// <returns>false, if there is a part of a packet still remaining</returns>
		protected override bool OnReceive(BufferSegment segment)
		{
			var recvBuffer = segment.Buffer.Array;

			var i = 1;

			do
			{
				if (_remainingLength < RealmPacketIn.HEADER_SIZE)
				{
					return false;
				}

				RealmServerOpCode opcode;

				//var headerSize = GetContentInfo(recvBuffer, segment.Offset + _offset, out packetLength, out opcode);

				var offset = segment.Offset + _offset;
				int headerSize;
				bool isLargePacket;
				var packetLength = 0;
				if (IsEncrypted)
				{
					//headerSize = Decrypt(recvBuffer, offset, out packetLength, out opcode);
					var firstByte = GetDecryptedByte(recvBuffer, offset, 0);

					isLargePacket = (firstByte & 0x80) != 0;				// check for the big packet marker
					if (isLargePacket)
					{
						// packetLength has 23 bits
						if (_remainingLength < RealmPacketIn.LARGE_PACKET_HEADER_SIZE)
						{
							decryptUntil = 0;
							log.Warn("DecryptUntil: " + decryptUntil);
							return false;
						}

						packetLength = (firstByte & 0x7F) << 16;
						packetLength |= GetDecryptedByte(recvBuffer, offset, 1) << 8;
						packetLength |= GetDecryptedByte(recvBuffer, offset, 2);

						opcode = (RealmServerOpCode)GetDecryptedOpcode(recvBuffer, offset, 3);
						headerSize = RealmPacketIn.LARGE_PACKET_HEADER_SIZE;
					}
					else
					{
						// packetLength has 15 bits
						packetLength |= firstByte << 8;
						packetLength |= GetDecryptedByte(recvBuffer, offset, 1);

						opcode = (RealmServerOpCode)GetDecryptedOpcode(recvBuffer, offset, 2);
						headerSize = RealmPacketIn.HEADER_SIZE;
					}
				}
				else
				{
					packetLength = recvBuffer[offset] << 8 | recvBuffer[offset + 1];
					isLargePacket = false;

					// the opcode is actually 4 bytes, but can never go over 2, so we skip the last 2
					opcode = (RealmServerOpCode)(recvBuffer[offset + 2] | recvBuffer[offset + 3] << 8);
					headerSize = RealmPacketIn.HEADER_SIZE;
				}

				packetLength += (headerSize - 4);

				if (packetLength > BufferSize)
				{
					// packet is just too big
					var bytes = new byte[headerSize];
					Array.Copy(recvBuffer, offset, bytes, 0, headerSize);

					var str = Encoding.UTF8.GetString(bytes);
					if (str.Equals("GET HT", StringComparison.InvariantCultureIgnoreCase))
					{
						log.Warn("HTTP crawler bot connected from {0} - requesting: {1}", this, str);
					}
					else
					{
						LogUtil.ErrorException("Client {0} sent corrupted packet (ID: {1}) with size {2} bytes, which exceeds maximum: " +
						                       "{3} (packet #{4}, segment #{5}, LargePacket: {6}, Remaining: {7}, Header: {8} ({9}))",
						                       this, opcode, packetLength, BufferSize, i, segment.Number,
						                       isLargePacket,
						                       _remainingLength,
						                       bytes.ToString(" ", b => string.Format("{0:X2}", b)),
						                       str);
					}
					Disconnect();

					return false;
				}

				if (_remainingLength < packetLength)
				{
					// packet incomplete
					if (IsEncrypted)
					{
						decryptUntil = headerSize;
						log.Warn("DecryptUntil: {0}, HeaderSize: {1}, Packet: {2}", decryptUntil, headerSize, opcode);
					}
					return false;
				}

				var pkt = new RealmPacketIn(segment, _offset, packetLength, opcode, headerSize);
				segment.IncrementUsage();

				//.UpdatePacketCounters(pkt.PacketId, fullPacketSize);

				PerformanceCounters.PacketsReceivedPerSecond.Increment();
				PerformanceCounters.TotalBytesReceived.IncrementBy(packetLength);

				RealmPacketMgr.Instance.HandlePacket(this, pkt);

				_remainingLength -= packetLength;
				_offset += packetLength;
				decryptUntil = -1;
				i++;
			} while (_remainingLength > 0);

			return true;
		}

		/// <summary>
		/// Sends the given bytes representing a full packet, to the Client
		/// </summary>
		/// <param name="packet"></param>
		public override void Send(byte[] packet, int offset, int count)
		{
			if (IsOffline)
				return;

			PerformanceCounters.PacketsSentPerSecond.Increment();
			PerformanceCounters.TotalBytesSent.IncrementBy(count);

#if DEBUG
			DebugUtil.DumpPacketOut(Account, packet, offset, count, PacketSender.Server);
#endif

			if (IsEncrypted)
			{
				Encrypt(packet, offset);
			}

			base.Send(packet, offset, count);
		}

		public void Send(RealmPacketOut packet)
		{
			//_server.Debug(this, Resources.SendingPacket, packet, packet.Length);
			Send(packet.GetFinalizedPacket());
		}

		public override string ToString()
		{
			var infoStr = new StringBuilder();

			infoStr.Append(ClientAddress);
			infoStr.Append(":");
			infoStr.Append(Port);

			if (Account != null)
			{
				infoStr.Append(" - Account: ");
				infoStr.Append(Account.Name);
			}
			if (ActiveCharacter != null)
			{
				infoStr.Append(" - Char: ");
				infoStr.Append(ActiveCharacter.Name);
			}

			return infoStr.ToString();
		}

		public void Disconnect()
		{
			_server.DisconnectClient(this);
		}

		/// <summary>
		/// The session key for the latest session of this account.
		/// </summary>
		public byte[] SessionKey
		{
			get { return m_sessionKey; }
			set
			{
				m_sessionKey = value;
				m_packetCrypt = new PacketCrypt(value);
			}
		}

		#region Packet Encryption/Decryption

		private PacketCrypt m_packetCrypt;
		int encrypt;
		int decryptSeq, decryptUntil = -1;

		/// <summary>
		/// Encrypts the byte array
		/// </summary>
		/// <param name="data">The raw packet data to encrypt</param>
		private void Encrypt(byte[] data, int offset)
		{
			if (Interlocked.Exchange(ref encrypt, 1) == 1)
				log.Error("Encrypt Error");

			m_packetCrypt.Encrypt(data, offset, 4);
			Interlocked.Exchange(ref encrypt, 0);
		}

		//private int GetContentInfo(byte[] inputData, int dataStartOffset, out int packetLength, out RealmServerOpCode opcode)
		//{
		//}

		private byte GetDecryptedByte(byte[] inputData, int baseOffset, int offset)
		{
			if (Interlocked.Exchange(ref decryptSeq, 1) == 1)
				log.Error("Decrypt Error");

			var dataStartOffset = baseOffset + offset;
			if (decryptUntil < offset)
			{
				m_packetCrypt.Decrypt(inputData, dataStartOffset, 1);
			}

			Interlocked.Exchange(ref decryptSeq, 0);

			return inputData[dataStartOffset];
		}

		private int GetDecryptedOpcode(byte[] inputData, int baseOffset, int offset)
		{
			var dataStartOffset = baseOffset + offset;
			if (decryptUntil < offset + 4)
			{
				//if (decryptUntil > offset)		// must not happen
				//{
				//    m_packetCrypt.Decrypt(inputData, baseOffset + decryptUntil, (offset - decryptUntil + 4));
				//}
				m_packetCrypt.Decrypt(inputData, dataStartOffset, 4);
			}
			return BitConverter.ToInt32(inputData, dataStartOffset);
		}

		#endregion
	}
}