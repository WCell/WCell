using WCell.Constants.Misc;
using WCell.Core.DBC;

namespace WCell.RealmServer.Titles
{
    public class CharacterTitleEntry
    {
        public TitleId TitleId;         // 0
        // public uint Unk1;            // 1
        public string Names;           // 2
        // public string[] Names2;      // 3
        // public string Flags;         // 4 string unused.
        public TitleBitId BitIndex;     // 5
    }

    public class TitleConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            var characterTitleEntry = new CharacterTitleEntry();
            characterTitleEntry.TitleId = (TitleId) GetUInt32(rawData, 0);
			characterTitleEntry.Names = GetString(rawData, 2);
            characterTitleEntry.BitIndex = (TitleBitId)GetUInt32(rawData, 5);


            TitleMgr.CharacterTitleEntries[characterTitleEntry.TitleId] = characterTitleEntry;
        }
    }
}
