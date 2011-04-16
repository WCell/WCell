namespace WCell.Tools.Maps.Structures
{
    public struct OffsetLocation
    {
        public int Count;
        public int Offset;

        public override string ToString()
        {
            return string.Format("{{Count = {0}, Offset = {1}}}", Count, Offset);
        }
    }
}