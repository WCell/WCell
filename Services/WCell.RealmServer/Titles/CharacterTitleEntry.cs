﻿using WCell.Constants.Misc;
using WCell.Core.DBC;

namespace WCell.RealmServer.Titles
{
    public class CharacterTitleEntry
    {
        public TitleId TitleId;         // 0
        // public uint Unk1;            // 1
        public string[] Names;           // 2-17
        // public string[] Names2;      // 19-34
        // public string Flags;         // 35 string unused.
        public TitleBitId BitIndex;     // 36

        public CharacterTitleEntry()
        {
            Names = new string[16];
        }
    }

    public class TitleConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            var characterTitleEntry = new CharacterTitleEntry();
            characterTitleEntry.TitleId = (TitleId)GetUInt32(rawData, 0);
            characterTitleEntry.Names = GetStrings(rawData, 2);
            characterTitleEntry.BitIndex = (TitleBitId)GetUInt32(rawData, 36);

            TitleMgr.CharacterTitleEntries[characterTitleEntry.TitleId] = characterTitleEntry;
        }
    }
}
