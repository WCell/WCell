using WCell.Constants.World;

namespace WCell.RealmServer.Calendars
{
    internal class CalendarStaticRaidReset
    {
        private MapId m_mapId;
        private uint m_resetInseconds;
        private uint m_negativeOffset;

        public CalendarStaticRaidReset(MapId mapId, uint seconds, uint offset)
        {
            m_mapId = mapId;
            m_resetInseconds = seconds;
            m_negativeOffset = offset;
        }

        private static readonly CalendarStaticRaidReset[] raidResets = new CalendarStaticRaidReset[]
        {
            new CalendarStaticRaidReset(MapId.TempestKeep,604800,0),
            new CalendarStaticRaidReset(MapId.BlackTemple,604800,0),
            new CalendarStaticRaidReset(MapId.RuinsOfAhnQiraj,259200,68400),
            new CalendarStaticRaidReset(MapId.AhnQirajTemple,604800,68400),
            new CalendarStaticRaidReset(MapId.MagtheridonsLair,604800,0),
            new CalendarStaticRaidReset(MapId.ZulGurub,259200,68400),
            new CalendarStaticRaidReset(MapId.Karazhan,604800,0),
            new CalendarStaticRaidReset(MapId.TheObsidianSanctum, 604800,0),
            new CalendarStaticRaidReset(MapId.CoilfangSerpentshrineCavern,604800,0),
            //new CalendarStaticRaidReset(MapId.WintergraspRaid,604800,0),
            new CalendarStaticRaidReset(MapId.Naxxramas,604800,0),
            new CalendarStaticRaidReset(MapId.TheEyeOfEternity, 604800,0),
            new CalendarStaticRaidReset(MapId.TheBattleForMountHyjal,604800,0),
            new CalendarStaticRaidReset(MapId.GruulsLair,604800,0),
            new CalendarStaticRaidReset(MapId.ZulAman,259200,0),
            new CalendarStaticRaidReset(MapId.OnyxiasLair,432000,0),
            new CalendarStaticRaidReset(MapId.MoltenCore,604800,0),
            new CalendarStaticRaidReset(MapId.BlackwingLair,604800,0),
            new CalendarStaticRaidReset(MapId.Ulduar,604800,0),
            new CalendarStaticRaidReset(MapId.TheSunwell,604800,0)
        };
    }
}