/*************************************************************************
 *
 *   file		: RealmPacketOut.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-05-04 00:27:17 +0800 (Sun, 04 May 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 314 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Core.Network;

namespace WCell.RealmServer
{
	/// <summary>
	/// Packet sent to the realm server
	/// </summary>
	public class RealmPacketOut : PacketOut
	{
		/// <summary>
		/// Constant indicating this <c>RealmPacketOut</c> header size.
		/// </summary>
		public const int HEADER_SIZE = 4;

		public const int FullUpdatePacketHeaderSize = HEADER_SIZE + 4;

		public const int MaxPacketSize = ushort.MaxValue;

		//public static void WriteHeader(byte[] buffer, int offset, int length, PacketId opcode)
		//{

		//}

		public RealmPacketOut(PacketId packetId)
			: this(packetId, 128)
		{
		}

		public RealmPacketOut(PacketId packetId, int maxContentLength)
			: base(packetId, maxContentLength + HEADER_SIZE)
		{
			Position += 2;						// length
			Write((ushort)packetId.RawId);
		}

		/// <summary>
		/// The <c>RealmPacketOut</c> header size.
		/// </summary>
		public override int HeaderSize
		{
			get { return HEADER_SIZE; }
		}

		/// <summary>
		/// The opcode of the packet
		/// </summary>
		public RealmServerOpCode OpCode
		{
			get { return (RealmServerOpCode)PacketId.RawId; }
			set
			{
				if (OpCode != value)
				{
					var pos = Position;
					Position = 2;
					WriteUShort((ushort)value);
					Position = pos;
				}
			}
		}

		/// <summary>
		/// Finalize packet data
		/// </summary>
		protected override void FinalizeWrite()
		{
			base.FinalizeWrite();

			Position = 0;
			WriteUShortBE((ushort)(BaseStream.Length - 2));
		}
	}
}