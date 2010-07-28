using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Achievement
{
    public class AchievementCategoryEntry
    {
        public uint ID;                                           // 0
        public uint ParentCategory;                               // 1 -1 for main category
        //public string Name[16];                                       // 2-17
        //public uint NameFlags;                                    // 18
        //public uint SortOrder;                                  // 19
    }
}
