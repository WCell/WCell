﻿using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.RealmServer.Battlegrounds
{
    public class PvPDifficultyEntry
    {
        public int Id;
        public MapId mapId;
        public int bracketId;
        public int minLevel;
        public int maxLevel;
        public int difficulty;
    };

    public class PvPDifficultyConverter : AdvancedDBCRecordConverter<PvPDifficultyEntry>
    {
        public override PvPDifficultyEntry ConvertTo(byte[] rawData, ref int id)
        {
            var entry = new PvPDifficultyEntry
                            {
                                Id = (id = GetInt32(rawData, 0)),
                                mapId = (MapId)GetInt32(rawData, 1),
                                bracketId = GetInt32(rawData, 2),
                                minLevel = GetInt32(rawData, 3),
                                maxLevel = GetInt32(rawData, 4),
                            };
            return entry;
        }
    }
}