using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Achievement
{
    // AchievementCollection class which each Character has an instance of
    public class AchievementCollection : IEnumerable<AchievementRecord>
    {
        internal Dictionary<uint, AchievementRecord> ById = new Dictionary<uint, AchievementRecord>();
        
        public IEnumerator<AchievementRecord> GetEnumerator()
        {
            return ById.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
