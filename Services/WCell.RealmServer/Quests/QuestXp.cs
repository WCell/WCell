using WCell.Core.DBC;

namespace WCell.RealmServer.Quests
{
    public class QuestXPInfo
    {
        public int Level;
        public int[] RewXP;
    }

    public class QuestXpConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            var xpInfo = new QuestXPInfo
            {
                Level = GetInt32(rawData, 0),
                RewXP = new[]
				{
					GetInt32(rawData, 2),
					GetInt32(rawData, 3),
					GetInt32(rawData, 4),
					GetInt32(rawData, 5),
					GetInt32(rawData, 6),
					GetInt32(rawData, 7),
					GetInt32(rawData, 8),
					GetInt32(rawData, 9)
				}
            };
            QuestMgr.QuestXpInfos[(uint)xpInfo.Level] = xpInfo;
        }
    }
}