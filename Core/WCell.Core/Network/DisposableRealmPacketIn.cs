using System;
using Cell.Core;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.RealmServer;

namespace WCell.Core.Network
{
	/// <summary>
	/// This kind of RealmPacketIn frees the used BufferSegment no disposal
	/// </summary>
	public class DisposableRealmPacketIn : RealmPacketIn
	{
		protected static Logger log = LogManager.GetCurrentClassLogger();

		public DisposableRealmPacketIn(BufferSegment segment, int offset, int length, int contentLength, RealmServerOpCode packetId)
			: base(segment, offset, length, packetId, (length - contentLength))
		{
		}

		/// <summary>
		/// Exposed BufferSegment for customizations etc.
		/// </summary>
		public BufferSegment Segment
		{
			get { return _segment; }
		}

		//public override void Dispose()
		//{
		//    base.Dispose();
		//    _segment.Free();
		//}

		public static DisposableRealmPacketIn CreateFromOutPacket(RealmPacketOut packet)
		{
			var oldBuf = packet.GetFinalizedPacket();
			return CreateFromOutPacket(oldBuf, 0, oldBuf.Length);
		}

		public static DisposableRealmPacketIn CreateFromOutPacket(BufferSegment segment, RealmPacketOut packet)
		{
			var bytes = packet.GetFinalizedPacket();
			return Create(packet.PacketId, bytes, packet.HeaderSize, bytes.Length - packet.HeaderSize, segment);
		}

		public static DisposableRealmPacketIn CreateFromOutPacket(byte[] outPacket, int offset, int totalLength)
		{
			var index = offset;
			var opcode = (RealmServerOpCode)(outPacket[index + 2] | (outPacket[index + 3] << 8));

			var segment = BufferManager.GetSegment(totalLength + 2);

			return Create(opcode, outPacket,
				offset + RealmPacketOut.HEADER_SIZE, totalLength - RealmPacketOut.HEADER_SIZE, segment);
		}

		public static DisposableRealmPacketIn CreateFromOutPacket(BufferSegment oldSegment, BufferSegment newSegment, int totalLength)
		{
			return CreateFromOutPacket(oldSegment, newSegment, 0, totalLength);
		}

		public static DisposableRealmPacketIn CreateFromOutPacket(BufferSegment oldSegment, BufferSegment newSegment, int offset, int totalLength)
		{
			var index = oldSegment.Offset + offset;
			var opcode = (RealmServerOpCode)(oldSegment.Buffer.Array[index + 2] | (oldSegment.Buffer.Array[index + 3] << 8));

			return Create(opcode, oldSegment.Buffer.Array,
				oldSegment.Offset + offset + RealmPacketOut.HEADER_SIZE, totalLength - RealmPacketOut.HEADER_SIZE, newSegment);
		}

		public static DisposableRealmPacketIn Create(PacketId opCode, byte[] outPacketContent)
		{
			var segment = BufferManager.GetSegment(outPacketContent.Length + HEADER_SIZE);
			return Create(opCode, outPacketContent, 0, outPacketContent.Length, segment);
		}

		public static DisposableRealmPacketIn Create(PacketId opCode, byte[] outPacketContent, int contentOffset, int contentLength,
			BufferSegment segment)
		{
#if DEBUG
			if (!Enum.IsDefined(typeof(RealmServerOpCode), opCode.RawId))
			{
				throw new Exception("Packet had undefined Opcode: " + opCode);
			}
#endif

			int headerSize = (contentLength > WCellConstants.HEADER_CHANGE_THRESHOLD ? LARGE_PACKET_HEADER_SIZE : HEADER_SIZE);

			var totalLength = contentLength + headerSize;

			var packetSize = totalLength - (headerSize - 4);

			if (headerSize == LARGE_PACKET_HEADER_SIZE)
			{
				segment.Buffer.Array[segment.Offset] = (byte)((packetSize >> 16) | 0x80);
				segment.Buffer.Array[segment.Offset + 1] = (byte)(packetSize >> 8);
				segment.Buffer.Array[segment.Offset + 2] = (byte)packetSize;

				segment.Buffer.Array[segment.Offset + 3] = (byte)((int)opCode.RawId);
				segment.Buffer.Array[segment.Offset + 4] = (byte)(((int)opCode.RawId) >> 8);
			}
			else
			{
				segment.Buffer.Array[segment.Offset] = (byte)(packetSize >> 8);
				segment.Buffer.Array[segment.Offset + 1] = (byte)packetSize;

				segment.Buffer.Array[segment.Offset + 2] = (byte)((int)opCode.RawId);
				segment.Buffer.Array[segment.Offset + 3] = (byte)(((int)opCode.RawId) >> 8);
			}

			Array.Copy(outPacketContent, contentOffset, segment.Buffer.Array, segment.Offset + headerSize, contentLength);

			return new DisposableRealmPacketIn(segment, 0, totalLength,
																contentLength, (RealmServerOpCode)opCode.RawId);
		}
	}
}