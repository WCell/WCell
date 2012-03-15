namespace WCell.PacketAnalysis
{
    public struct DirectedPacketId
    {
        public uint OpCode;
        public PacketSender Sender;

        public uint Order
        {
            get
            {
                return (uint)Sender;
            }
        }
    }

    public enum PacketSender
    {
        Any = 0,
        Server = 1,
        Client = 2
    }
}