namespace WCell.RealmServer.Misc
{
    public struct SimpleRange
    {
        public SimpleRange(float min, float max)
        {
            MinDist = min;
            MaxDist = max;
        }

        public float MinDist, MaxDist;

        public float Average
        {
            get { return MinDist + (MaxDist - MinDist) / 2f; }
        }
    }
}