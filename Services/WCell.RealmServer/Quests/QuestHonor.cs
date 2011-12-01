using WCell.Core.ClientDB;

namespace WCell.RealmServer.Quests
{
    public class QuestHonorInfo
    {
        public int Level;
        public float RewHonor;
    }

    public class QuestHonorInfoConverter : ClientDBRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
			// TODO: Fix this (it's not per level)
            var honorInfo = new QuestHonorInfo
            {
                Level = GetInt32(rawData, 0) - 1,
                RewHonor = GetFloat(rawData, 1),
            };

            QuestMgr.QuestHonorInfos[(uint)honorInfo.Level] = honorInfo;
        }
    }
}
