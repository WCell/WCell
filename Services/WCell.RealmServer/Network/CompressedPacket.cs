using System.IO;
using Cell.Core;
using WCell.Constants;
using WCell.Core;

namespace WCell.RealmServer.Network
{
    public class CompressedPacket : RealmPacketOut
    {
        BinaryWriter backingStream = new BinaryWriter(new MemoryStream(2000));

        public CompressedPacket()
            : base(RealmServerOpCode.SMSG_COMPRESSED_MOVES)
        {
        }

        public void AddPacket(RealmPacketOut packet)
        {
            if (packet.ContentLength > 255)
                throw new InvalidDataException("Packets added to a compressed stream must have length less than 255");

            backingStream.Write((byte)packet.ContentLength);
            backingStream.Write((ushort)packet.OpCode);
            backingStream.Write(packet.GetFinalizedPacket(), packet.HeaderSize, packet.ContentLength);
        }

        protected override void FinalizeWrite()
        {
            // Set us at the payload start
            Position = HeaderSize;

            int deflatedLength;

            var segment = BufferManager.GetSegment(ContentLength);

            var backingBuf = (backingStream.BaseStream as MemoryStream).GetBuffer(); // backingStream can only be a MemoryStream
            Compression.CompressZLib(backingBuf, segment.Buffer.Array, 7, out deflatedLength);
            Write(deflatedLength);
            Write(segment.Buffer.Array);

            segment.DecrementUsage();// should check in the buffer

            base.FinalizeWrite();
        }
    }
}