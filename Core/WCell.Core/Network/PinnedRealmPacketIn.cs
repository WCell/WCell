using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.RealmServer;
using System.Runtime.InteropServices;

namespace WCell.Core.Network
{
	/// <summary>
	/// A variation of RealmPacketIn that handles buffer allocation and pinning itself.
	/// </summary>
	public class PinnedRealmPacketIn : RealmPacketIn, IDisposable
	{
		GCHandle bufHandle;

		public PinnedRealmPacketIn(ArraySegment<byte> bytes)
			: base(bytes)
		{
			bufHandle = GCHandle.Alloc(bytes.Array, GCHandleType.Pinned);
		}

		public PinnedRealmPacketIn(byte[] bytes)
			: this(bytes, 0, bytes.Length)
		{
		}

		public PinnedRealmPacketIn(byte[] bytes, int offset, int length, ushort contentLength, PacketId packetId)
			: this(bytes, offset, length)
		{
			_payloadSize = contentLength;
			_packetID = packetId;
		}

		public PinnedRealmPacketIn(byte[] bytes, int offset, int length)
			: base(new ArraySegment<byte>(bytes, offset, length))
		{
			bufHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		}

		#region IDisposable Members

		/// <summary>
		/// Disposes the buffer handle
		/// </summary>
		public void Dispose()
		{
			bufHandle.Free();
			Cleanup();
		}

		#endregion

		public static PinnedRealmPacketIn CreateFromOutPacket(RealmPacketOut packet)
		{
			var oldBuf = packet.GetFinalizedPacket();
			return CreateFromOutPacket(oldBuf);
		}

		public static PinnedRealmPacketIn CreateFromOutPacket(byte[] outPacket)
		{
			return CreateFromOutPacket(outPacket, 0, outPacket.Length);
		}

		public static PinnedRealmPacketIn CreateFromOutPacket(byte[] outPacket, int offset, int length)
		{
			var buf = new byte[length + 2];

			Buffer.BlockCopy(outPacket, offset, buf, 0, RealmPacketOut.HEADER_SIZE);
			Buffer.BlockCopy(outPacket, RealmPacketOut.HEADER_SIZE, buf, HEADER_SIZE, length - RealmPacketOut.HEADER_SIZE);

			var inPacket = new PinnedRealmPacketIn(buf);
			inPacket.Initialize();

			return inPacket;
		}

		public static PinnedRealmPacketIn Create(byte[] inPacket)
		{
			return Create(inPacket, 0, inPacket.Length);
		}

		public static PinnedRealmPacketIn Create(byte[] inPacket, int offset, int length)
		{
			//var buf = new byte[length + 2];

			//Buffer.BlockCopy(outPacket, offset, buf, 0, RealmPacketOut.HEADER_SIZE);
			//Buffer.BlockCopy(outPacket, RealmPacketOut.HEADER_SIZE, buf, HEADER_SIZE, length - RealmPacketOut.HEADER_SIZE);

			//var inPacket = new PinnedRealmPacketIn(buf);
			var packet = new PinnedRealmPacketIn(inPacket, offset, length);
			packet.Initialize();

			return packet;
		}
	}
}
