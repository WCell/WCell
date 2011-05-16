using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.ClientDB;
using WCell.Util;

namespace WCell.RealmServer.Quests
{
    public class QuestHonorInfo
    {
        public int Level;
        public int RewHonor;
    }

    public class QuestHonorInfoConverter : ClientDBRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
			// TODO: Fix this (it's not per level)
            var honorInfo = new QuestHonorInfo
            {
                Level = GetInt32(rawData, 0) - 1,
                RewHonor = GetInt32(rawData, 1),
            };

            QuestMgr.QuestHonorInfos[(uint)honorInfo.Level] = honorInfo;
        }
    }
}
