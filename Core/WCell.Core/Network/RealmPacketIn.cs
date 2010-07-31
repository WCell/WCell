/*************************************************************************
 *
 *   file		: RealmPacketIn.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-28 12:55:13 +0800 (Mon, 28 Apr 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 301 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using Cell.Core;
using WCell.Constants;
using WCell.Core.Network;
using System.Diagnostics;

namespace WCell.RealmServer
{
	/// <summary>
	/// Reads data from incoming packet stream, targetted specifically for the realm server
	/// </summary>
	public class RealmPacketIn : PacketIn
	{
		#region Fields

		public const int HEADER_SIZE = 6;
		public const int LARGE_PACKET_HEADER_SIZE = 7;		
		public const int LARGE_PACKET_THRESHOLD = 32767;

		protected bool _oversizedPacket;
		protected int _headerSize;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs a RealmPacketIn object given the buffer to read, the offset to read from, and the number of bytes to read.
		/// Do not use this, unless you have a BufferManager that ensures the BufferWrapper's content to be pinned. 
		/// For self-managed RealmPackets, use <c>DisposableRealmPacketIn</c>.
		/// </summary>
		/// <param name="segment">buffer container to read from</param>
		/// <param name="offset">offset to read from relative to the segment offset</param>
		/// <param name="length">number of bytes to read</param>
		/// <param name="opcode">the opcode of this packet</param>
		public RealmPacketIn(BufferSegment segment, int offset, int length, RealmServerOpCode opcode, int headerSize)
			: base(segment, offset, length)
		{
			_packetID = opcode;
			Position = headerSize;
			_headerSize = headerSize;
			_oversizedPacket = (_headerSize == LARGE_PACKET_HEADER_SIZE);
		}

		#endregion

		#region Properties

		public override int HeaderSize
		{
			get { return _headerSize; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return _packetID + " (Length: " + Length + ")";
		}

		#endregion

		/// <summary>
		/// Make sure to Dispose the copied packet!
		/// </summary>
		/// <returns></returns>
        public DisposableRealmPacketIn Copy()
		{
			var segment = BufferManager.GetSegment(Length);

			if (ContentLength > Length || segment.Buffer.Array.Length <= segment.Offset + Length)
			{
				throw new Exception(
					string.Format("Cannot copy Packet \"" + this + "\" because of invalid Boundaries - " +
					"ArrayLength: {0} PacketLength: {1}, Offset: {2}",
					segment.Buffer.Array.Length, Length, segment.Offset));
			}

		    Buffer.BlockCopy(_segment.Buffer.Array, _segment.Offset + _offset, segment.Buffer.Array, segment.Offset, Length);

		    return new DisposableRealmPacketIn(segment, 0, Length, ContentLength,
		                                       (RealmServerOpCode) _packetID.RawId);
		}
	}
}