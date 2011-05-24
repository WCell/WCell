using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.NPCs;

namespace WCell.Addons.Default.MiscFixes
{
    /// <summary>
    /// Place to address database structure specific inconsistencies
    /// </summary>
    public class DBFixes
    {
        [Initialization(InitializationPass.Second)]
        public static void FixBadDBTables()
        {
            if (!ContentMgr.HasMapper(typeof(NPCBaseStats)))
                return;

            var mapper = ContentMgr.GetMapper(typeof (NPCBaseStats));
            using (mapper.Wrapper.Query("ALTER TABLE `creature_classlevelstats` CHANGE COLUMN `level` `level` TINYINT(3) NOT NULL FIRST, CHANGE COLUMN `class` `class` TINYINT(3) NOT NULL AFTER `level`, CHANGE COLUMN `basehp0` `basehp0` SMALLINT(5) NOT NULL AFTER `class`, CHANGE COLUMN `basehp1` `basehp1` SMALLINT(5) NOT NULL AFTER `basehp0`, CHANGE COLUMN `basehp2` `basehp2` SMALLINT(5) NOT NULL AFTER `basehp1`, CHANGE COLUMN `basehp3` `basehp3` SMALLINT(5) UNSIGNED NOT NULL DEFAULT '0' AFTER `basehp2`, CHANGE COLUMN `basemana` `basemana` SMALLINT(5) NOT NULL AFTER `basehp3`, CHANGE COLUMN `basearmor` `basearmor` SMALLINT(5) NOT NULL AFTER `basemana`;"))
            {
            }
        }
    }
}
