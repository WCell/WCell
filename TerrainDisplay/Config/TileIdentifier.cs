namespace TerrainDisplay
{
    public class TileIdentifier
    {
        public int MapId;
        public string MapName;
        public int TileX;
        public int TileY;
        
        public TileIdentifier(int mapId, string mapName, int tileX, int tileY)
        {
            MapId = mapId;
            MapName = mapName;
            TileX = tileX;
            TileY = tileY;
        }

        public static TileIdentifier Redridge
        {
            get
            {
                const int mapId = 0;
                const string mapName = "Azeroth";
                const int tileX = 49;
                const int tileY = 36;
                return new TileIdentifier(mapId, mapName, tileX, tileY);
            }
        }

        public static TileIdentifier CenterTile
        {
            get
            {
                const int mapId = 0;
                const string mapName = "Azeroth";
                const int tileX = 32;
                const int tileY = 32;
                return new TileIdentifier(mapId, mapName, tileX, tileY);
            }
        }

        public static TileIdentifier Stormwind
        {
            get
            {
                const int mapId = 0;
                const string mapName = "Azeroth";
                const int tileX = 48;
                const int tileY = 30;
                return new TileIdentifier(mapId, mapName, tileX, tileY);
            }
        }

        public static TileIdentifier BurningSteppes
        {
            get
            {
                const int mapId = 0;
                const string mapName = "Azeroth";
                const int tileX = 49;
                const int tileY = 33;
                return new TileIdentifier(mapId, mapName, tileX, tileY);
            }
        }
    }
}
