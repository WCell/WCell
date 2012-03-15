namespace WCell.RealmServer.Misc
{
    public struct ActionButton
    {
        public const uint Size = 4;
        public const uint MaxAmount = 144;

        public static readonly byte[] EmptyButton = new byte[Size];

        public static byte[] CreateEmptyActionButtonBar()
        {
            return new byte[MaxAmount * Size];
        }

        public uint Index;
        public ushort Action;
        public byte Type;
        public byte Info;

        public void Set(byte[] actions)
        {
            var index = Index * 4;
            actions[index] = (byte)(Action & 0x00FF);
            actions[index + 1] = (byte)((Action & 0xFF00) >> 8);
            actions[index + 2] = Type;
            actions[index + 3] = Info;
        }

        public static void Set(byte[] actions, uint index, ushort action, byte type, byte info)
        {
            index = index * 4;
            actions[index] = (byte)(action & 0x00FF);
            actions[index + 1] = (byte)((action & 0xFF00) >> 8);
            actions[index + 2] = type;
            actions[index + 3] = info;
        }
    }
}
