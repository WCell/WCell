using System.Collections.Generic;
using WCell.Constants.Misc;
using WCell.Core;
using WCell.Core.ClientDB;
using WCell.Core.Initialization;

namespace WCell.RealmServer.Titles
{
    public static class TitleMgr
    {
        public static readonly Dictionary<TitleId, CharacterTitleEntry> CharacterTitleEntries =
            new Dictionary<TitleId, CharacterTitleEntry>();

        [Initialization(InitializationPass.Fifth)]
        public static void InitTitles()
        {
            new DBCReader<TitleConverter>(RealmServerConfiguration.GetDBCFile(ClientDBConstants.DBC_CHARTITLES));
        }

        public static CharacterTitleEntry GetTitleEntry(TitleId titleId)
        {
            return CharacterTitleEntries[titleId];
        }

        public static CharacterTitleEntry GetTitleEntry(TitleBitId titleBitId)
        {
            foreach (var characterTitleEntry in CharacterTitleEntries.Values)
            {
                if(characterTitleEntry.BitIndex == titleBitId)
                    return characterTitleEntry;
            }

            return null;
        }
    }
}
