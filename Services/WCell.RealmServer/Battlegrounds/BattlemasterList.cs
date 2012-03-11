using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.RealmServer.Battlegrounds
{
    public class BattlemasterList
    {
        public BattlegroundId BGId;
        public MapId MapId;
    }

    public class BattlemasterConverter : AdvancedDBCRecordConverter<BattlemasterList>
    {
        public override BattlemasterList ConvertTo(byte[] rawData, ref int bgId)
        {
            bgId = GetInt32(rawData, 0);

            var list = new BattlemasterList
                        {
                            BGId = ((BattlegroundId)bgId),
                            MapId = ((MapId)GetUInt32(rawData, 1))
                        };
            return list;
        }
    }
}