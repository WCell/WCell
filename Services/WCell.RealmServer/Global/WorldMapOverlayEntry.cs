using WCell.Constants.World;

namespace WCell.RealmServer.Global
{
    public class WorldMapOverlayEntry
    {
        public WorldMapOverlayId WorldMapOverlayId;     // 0
        // public MapId MapId;                          // 1
        public ZoneId[] ZoneTemplateId;                      // 2-5
                                                        // 6-7 always 0, possible part of areatableID[]
        //char* internal_name                           // 8
                                                        // 9-16 some ints

        public WorldMapOverlayEntry()
        {
            ZoneTemplateId = new ZoneId[4];
        }
    }
}
