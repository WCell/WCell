using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.DBC;

namespace WCell.RealmServer.Quests
{
    public struct QuestRewRepInfo
    {
        public int Id;
        public int[] RewRep;
    }

    public class QuestRewRepConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            var questRewRepInfo = new QuestRewRepInfo
            {
                Id = GetInt32(rawData, 0),
                RewRep = new[]
				{
					GetInt32(rawData, 2),
					GetInt32(rawData, 3),
					GetInt32(rawData, 4),
					GetInt32(rawData, 5),
					GetInt32(rawData, 6),
					GetInt32(rawData, 7),
					GetInt32(rawData, 8),
					GetInt32(rawData, 9),
                    GetInt32(rawData, 10)
				}
            };
            QuestMgr.QuestRewRepInfos[(uint)questRewRepInfo.Id - 1] = questRewRepInfo;
        }
    }
}
