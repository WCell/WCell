using WCell.Constants.Items;

namespace WCell.RealmServer.Items
{
    public class ItemLevelInfo
    {
        public uint Level;

        public uint[] EpicPoints = new uint[ItemConstants.MaxRandPropPoints];

        public uint[] RarePoints = new uint[ItemConstants.MaxRandPropPoints];

        public uint[] UncommonPoints = new uint[ItemConstants.MaxRandPropPoints];
    }
}