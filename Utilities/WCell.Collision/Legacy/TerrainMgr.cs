//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace WCell.Collision
//{
//    /// <summary>
//    /// Handles terrain info for a specified map
//    /// </summary>
//    public class TerrainMgr
//    {
//        const int MAP_RESOLUTION = 256;
//        internal const float TileSize = 1600f / 3f;
//        internal const float CHUNKSIZE = TileSize / 16.0f;
//        internal const float UNITSIZE = CHUNKSIZE / 8.0f;

//        const int TilesCount = 64;

//        const float MinX = -TilesCount * TileSize / 2;
//        const float MinY = -TilesCount * TileSize / 2;

//        const float MaxX = TilesCount * TileSize / 2;
//        const float MaxY = TilesCount * TileSize / 2;

//        const int CellsPerTile = 8;        
//        const float CellSize = TileSize / CellsPerTile;
//        const int SizeX = TilesCount * CellsPerTile;
//        const int SizeY = TilesCount * CellsPerTile;


//        CellTerrainInfo[,] CellInformation = new CellTerrainInfo[2, 2];
//        bool IsInstance;
//        int map;

//        public TerrainMgr(int mapId, bool instance)
//        {
//            map = mapId;
//            IsInstance = instance;
//        }


//        public bool LoadCellInformation(int tileX, int tileY)
//        {

//            if (IsCellLoaded(tileX,tileY))
//            {
//                return true;
//            }

//            return true;
//        }

//        public ushort GetAreaID(float x, float y)
//        {
//            if (!AreCoordinatesValid(x, y))
//                return 0;

//            // Convert the co-ordinates to cells.
//            int CellX = ConvertGlobalXCoordinate(x);
//            int CellY = ConvertGlobalYCoordinate(y);

//            if (!IsCellLoaded(CellX, CellY) && !LoadCellInformation(CellX, CellY))
//                return 0;

//            // Convert the co-ordinates to cell's internal
//            // system.
//            float IntX = ConvertInternalXCoordinate(x, CellX);
//            float IntY = ConvertInternalYCoordinate(y, CellY);

//            // Find the offset in the 2d array.
//            return CellInformation[CellX,CellY].AreaId[ConvertTo2dArray(IntX), ConvertTo2dArray(IntY)];
//        }

//        public float GetLandHeight(float x, float y)
//        {
//            if (!AreCoordinatesValid(x, y))
//                return 0.0f;

//            // Convert the co-ordinates to cells.
//            int cellX = ConvertGlobalXCoordinate(x);
//            int cellY = ConvertGlobalYCoordinate(y);

//            if (!IsCellLoaded(cellX, cellY) && !LoadCellInformation(cellX, cellY))
//                return 0.0f;

//            // Convert the co-ordinates to cell's internal
//            // system.
//            float internalX = ConvertInternalXCoordinate(x, cellX);
//            float IntY = ConvertInternalYCoordinate(y, cellY);

//            // Calculate x index.
//            float TempFloat = internalX * (MAP_RESOLUTION / CellsPerTile / CellSize);
//            int XOffset = (int)TempFloat;
//            if ((TempFloat - (XOffset * CellSize)) >= 0.5f)
//                ++XOffset;

//            // Calculate y index.
//            TempFloat = IntY * (MAP_RESOLUTION / CellsPerTile / CellSize);
//            int YOffset = (int)TempFloat;
//            if ((TempFloat - (YOffset * CellSize)) >= 0.5f)
//                ++YOffset;

//            // Return our cached information.
//            return CellInformation[cellX, cellY].HeightMap[XOffset, YOffset];
//        }

//        public float GetWaterHeight(float x, float y)
//        {
//            if (!AreCoordinatesValid(x, y))
//                return 0.0f;

//            // Convert the co-ordinates to cells.
//            int cellX = ConvertGlobalXCoordinate(x);
//            int cellY = ConvertGlobalYCoordinate(y);

//            if (!IsCellLoaded(cellX, cellY))
//                return 0.0f;

//            // Convert the co-ordinates to cell's internal
//            // system.
//            float internalX = ConvertInternalXCoordinate(x, cellX);
//            float internalY = ConvertInternalYCoordinate(y, cellY);

//            // Find the offset in the 2d array.
//            return CellInformation[cellX, cellY].LiquidLevel[ConvertTo2dArray(internalX), ConvertTo2dArray(internalY)];
//        }

//        public byte GetWaterType(float x, float y)
//        {
//            if (!AreCoordinatesValid(x, y))
//                return 0;

//            // Convert the co-ordinates to cells.
//            int CellX = ConvertGlobalXCoordinate(x);
//            int CellY = ConvertGlobalYCoordinate(y);

//            if (!IsCellLoaded(CellX, CellY))
//                return 0;

//            // Convert the co-ordinates to cell's internal
//            // system.
//            float IntX = ConvertInternalXCoordinate(x, CellX);
//            float IntY = ConvertInternalYCoordinate(y, CellY);

//            // Find the offset in the 2d array.
//            return CellInformation[CellX, CellY].LiquidType[ConvertTo2dArray(IntX), ConvertTo2dArray(IntY)];
//        }


//        private int ConvertGlobalXCoordinate(float x)
//        {
//            return (int)((MaxX - x) / CellSize);
//        }
//        private int ConvertGlobalYCoordinate(float y)
//        {
//            return (int)((MaxY - y) / CellSize);
//        }
//        private float ConvertInternalXCoordinate(float x, int cellX)
//        {
//            float X = (MaxX - x);
//            X -= (cellX * CellSize);
//            return X;
//        }
//        private float ConvertInternalYCoordinate(float y, int cellY)
//        {
//            float Y = (MaxY - y);
//            Y -= (cellY * CellSize);
//            return Y;
//        }

//        private int ConvertTo2dArray(float c)
//        {
//            return (int)(c * (16 / CellsPerTile / CellSize));
//        }

//        public CellTerrainInfo GetCellInformation(int x, int y)
//        {
//            return CellInformation[x,y];
//        }

//        public bool IsCellLoaded(int tileX, int tileY)
//        {
//            return CellInformation[tileX, tileY] != null;
//        }

//        public bool AreCoordinatesValid(float x, float y)
//        {
//            if (x > MaxX || x < MinX)
//                return false;
//            if (y > MaxY || y < MinY)
//                return false;
//            return true;
//        }
//    }

//    public class TerrainInfo
//    {

//    }


//    public class CellTerrainInfo
//    {
//        public ushort[,] AreaId = new ushort[2, 2];
//        public byte[,] LiquidType = new byte[2, 2];
//        public float[,] LiquidLevel = new float[2,2];
//        public float[,] HeightMap = new float[32, 32];
//    }
//}
