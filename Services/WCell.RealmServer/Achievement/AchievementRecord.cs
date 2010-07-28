using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;

namespace WCell.RealmServer.Achievement
{
    public class AchievementRecord
    {
        [Property]
        public uint CharacterGuid { set; get; }

        [Property]
        public AchievementEntry AchievementId { get; set; }

        [Property]
        public DateTime Date { get; set; }
    }
}
