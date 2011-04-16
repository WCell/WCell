using System;
using WCell.Constants;

namespace WCell.RealmServer.Stats
{
    public struct ExtendedPacketInfo
    {
        public RealmServerOpCode OpCode;
        public int AveragePacketSize;
		public int PacketCount;
		public int StandardDeviation;
		public long TotalPacketSize;
		public DateTime StartTime;
		public DateTime EndTime;
    }
}