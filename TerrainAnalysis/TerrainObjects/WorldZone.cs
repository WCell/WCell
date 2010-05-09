using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WCell.Collision;
using System.Collections;
using TerrainAnalysis.TerrainObjects;
using Microsoft.DirectX;

namespace TerrainAnalysis
{
    public class WorldZone
    {
    	readonly MapTile[,] zoneGrid = new MapTile[TerrainConstants.RegionBounds.TilesPerColumn, TerrainConstants.RegionBounds.TilesPerRow];
        private bool topLeftAssigned=false;
        Microsoft.DirectX.Vector2 topLeft, bottomRight;
        private readonly string zoneName;
        private readonly bool highRes;

        public WorldZone(string ZoneName, bool highResChunks)
        {
            this.zoneName = ZoneName;
            this.highRes = highResChunks;
        }

        public void ProcessZone()
        {
            var wdtI = new WDTImporter("World\\Maps\\" + zoneName + "\\" + zoneName + ".wdt");
            var mapLoader = new Thread(new ParameterizedThreadStart(MapLoaderThread));
            mapLoader.Start(wdtI);
        }

        private void MapLoaderThread(object wdtImporter)
        {
            WDTImporter wdtI = (WDTImporter)wdtImporter;
            wdtI.Process();
            WDT m_wdt = wdtI.WDTData;

            if (m_wdt.HasTerrain)
            {
                FireZoneType(ZoneType.HasTerrain);
                for (int i = 0; i < TerrainConstants.RegionBounds.TilesPerColumn; i++)
                {
                    for (int j = 0; j < TerrainConstants.RegionBounds.TilesPerRow; j++)
                    {
                        if (m_wdt.TileProfile[i, j] == 1)
                        {
                            string fileName = this.zoneName + "_" + j.ToString() + "_" + i.ToString() + ".adt";
                            ADTImporter adtI = new ADTImporter("World\\Maps\\" + this.zoneName + "\\" + fileName, this.highRes);
                            adtI.Process();
                            ADT m_adt = adtI.ADT;
                            this.zoneGrid[i, j] = new MapTile(m_adt,i,j);

                            if (topLeftAssigned == false)
                                topLeft = new Vector2(j*TerrainConstants.Tile.TileSize, i*TerrainConstants.Tile.TileSize);
                            bottomRight = new Vector2(j * TerrainConstants.Tile.TileSize, i * TerrainConstants.Tile.TileSize);
                            topLeftAssigned = true;

                            FireZoneLoad(LoadEventType.TileLoaded, fileName);
                        }
                        else
                            this.zoneGrid[i, j] = null;
                    }
                }

                this.AssociateTiles();
                FireZoneLoad(LoadEventType.ChunksLinked, this);
            }
            else
            {
                FireZoneType(ZoneType.NoTerrain);
            }

            Thread.CurrentThread.Abort();
        }

        public enum ZoneType
        {
            HasTerrain,
            NoTerrain
        };

        public enum LoadEventType
        {
            TileLoaded,
            ChunksLinked,
            ZoneLoaded
        };

        public delegate void ZoneTypeEventhandler(ZoneType zoneType);
        public event ZoneTypeEventhandler OnZoneTypeEvent;

        public delegate void ZoneLoadEventHandler(LoadEventType loadEvent, object eventObject);
        public event ZoneLoadEventHandler OnLoadEvent;

        protected virtual void FireZoneType(ZoneType zoneType)
        {
            if (this.OnZoneTypeEvent != null)
                OnZoneTypeEvent(zoneType);
        }

        protected virtual void FireZoneLoad(LoadEventType loadType, object eventObject)
        {
            if (this.OnLoadEvent != null)
                this.OnLoadEvent(loadType, eventObject);            
        }

        /// <summary>
        /// Expands the Oct-Tree from within a single tile to adjacent tiles. 
        /// The end result being that chunk relationships will be independent of the tiles
        /// they exist in. This eliminates the complexity of movement and ray-casting from
        /// one tile to another. 
        /// </summary>
        private void AssociateTiles()
        {
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    if (this.zoneGrid[i, j] != null)
                    {
                        MapTile currentTile = this.zoneGrid[i, j];
                        if (j > 0)
                        {
                            if (zoneGrid[i,j - 1] != null)                            
                                BridgeLeftRight(zoneGrid[i,j-1].TileEdge(TileEdgeComponent.Right),currentTile.TileEdge(TileEdgeComponent.Left));

                            if (i > 0)
                            {
                                if (zoneGrid[i - 1, j - 1] != null)
                                {
                                    MapTileChunk tempChunk = zoneGrid[i - 1, j - 1].TileEdge(TileEdgeComponent.BottomRight)[0];
                                    MapTileChunk thisChunk = zoneGrid[i, j].TileEdge(TileEdgeComponent.TopLeft)[0];

                                    tempChunk.treePointers[(int)ChunkTree.BottomRight] = thisChunk;
                                    thisChunk.treePointers[(int)ChunkTree.TopLeft] = tempChunk;
                                }
                            }
                        }
                        if (i > 0)
                        {
                            if (zoneGrid[i - 1, j] != null)
                                BridgeTopBottom(zoneGrid[i - 1, j].TileEdge(TileEdgeComponent.Bottom), currentTile.TileEdge(TileEdgeComponent.Top));

                            if (j < 15)
                            {
                                if (zoneGrid[i - 1, j + 1] != null)
                                {
                                    MapTileChunk tempChunk = zoneGrid[i - 1, j + 1].TileEdge(TileEdgeComponent.BottomLeft)[0];
                                    MapTileChunk thisChunk = zoneGrid[i, j].TileEdge(TileEdgeComponent.TopRight)[0];

                                    tempChunk.treePointers[(int)ChunkTree.BottomLeft] = thisChunk;
                                    thisChunk.treePointers[(int)ChunkTree.TopRight] = tempChunk;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void BridgeLeftRight(MapTileChunk[] left, MapTileChunk[] right)
        {
            for (int i = 0; i < left.Count(); i++)
            {
                left[i].treePointers[(int)ChunkTree.Right] = right[i];
                right[i].treePointers[(int)ChunkTree.Left] = left[i];
                if (i > 0)
                {
                    left[i].treePointers[(int)ChunkTree.TopRight] = right[i - 1];
                    right[i].treePointers[(int)ChunkTree.TopLeft] = left[i - 1];

                    left[i - 1].treePointers[(int)ChunkTree.BottomRight] = right[i];
                    right[i - 1].treePointers[(int)ChunkTree.BottomLeft] = left[i];
                }
            }
        }

        private void BridgeTopBottom(MapTileChunk[] top, MapTileChunk[] bottom)
        {
            for (int i = 0; i < top.Count(); i++)
            {
                top[i].treePointers[(int)ChunkTree.Bottom] = bottom[i];
                bottom[i].treePointers[(int)ChunkTree.Top] = top[i];
                if (i > 0)
                {
                    top[i].treePointers[(int)ChunkTree.BottomLeft] = bottom[i - 1];
                    bottom[i].treePointers[(int)ChunkTree.TopLeft] = top[i - 1];

                    top[i - 1].treePointers[(int)ChunkTree.BottomRight] = bottom[i];
                    bottom[i - 1].treePointers[(int)ChunkTree.TopRight] = top[i];
                }
            }
        }

        public ArrayList GetChunksInRadius(Microsoft.DirectX.Vector2 position, float radius)
        {
            int radiusInChunks = (int)(radius / TerrainConstants.HighResChunk.ChunkSize);

            if (radiusInChunks == 0) radiusInChunks = 1;

            int tileRow = (int)(position.Y / TerrainConstants.Tile.TileSize);
            int tileColumn = (int)(position.X / TerrainConstants.Tile.TileSize);

            MapTile currentTile = this.zoneGrid[tileRow, tileColumn];
            if (currentTile != null)
            {
                position.X -= tileColumn * TerrainConstants.Tile.TileSize;
                position.Y -= tileRow * TerrainConstants.Tile.TileSize;

                MapTileChunk baseChunk = currentTile.GetChunk((int)(position.Y / TerrainConstants.HighResChunk.ChunkSize), (int)(position.X / TerrainConstants.HighResChunk.ChunkSize));

                int countUp = 0, countLeft = 0, countRight = 0;
                int rowIndex = 0;

                MapTileChunk pointerChunk = baseChunk;

                while (pointerChunk.treePointers[(int)ChunkTree.Top] != null && countUp < radiusInChunks)
                {
                    countUp++;
                    pointerChunk = pointerChunk.treePointers[(int)ChunkTree.Top];
                }

                ArrayList chunkArray = new ArrayList();
                MapTileChunk rowCenterPointer = pointerChunk;
                rowIndex = countUp;

                while (pointerChunk != null && rowIndex > -radiusInChunks)
                {
                    rowCenterPointer = pointerChunk;
                    countLeft = 0;
                    countRight = 0;
                    while (pointerChunk.treePointers[(int)ChunkTree.Left] != null && ((rowIndex * rowIndex) + (countLeft * countLeft)) <= radiusInChunks*radiusInChunks)
                    {
                        pointerChunk = pointerChunk.treePointers[(int)ChunkTree.Left];
                        countLeft++;
                    }

                    while (pointerChunk.treePointers[(int)ChunkTree.Right] != null && ((rowIndex * rowIndex) + (countRight * countRight)) <= radiusInChunks*radiusInChunks)
                    {
                        chunkArray.Add(pointerChunk);
                        pointerChunk = pointerChunk.treePointers[(int)ChunkTree.Right];
                        if (countLeft > 0)
                            countLeft--;
                        else
                            countRight++;
                    }

                    pointerChunk = rowCenterPointer;
                    pointerChunk = pointerChunk.treePointers[(int)ChunkTree.Bottom];
                    rowIndex--;
                }
                return chunkArray;
            }
            else
                return null;
        }

        public Microsoft.DirectX.Vector2 ZoneCenter
        {
            get
            {
                return (topLeft + bottomRight) * 0.5f;
            }
        }

        public MapTile[,] WorldGrid
        {
            get
            {
                return this.zoneGrid;
            }
        }

        public TerrainCorrection NearestNormal(Microsoft.DirectX.Vector3 position, Microsoft.DirectX.Vector3 direction)
        {
            int tileX, tileZ, chunkX, chunkZ;
            
            tileX = (int)(position.X / TerrainConstants.Tile.TileSize);
            tileZ = (int)(position.Z / TerrainConstants.Tile.TileSize);

            position.X -= (tileX * TerrainConstants.Tile.TileSize);
            position.Z -= (tileZ * TerrainConstants.Tile.TileSize);
            chunkX = (int)(position.X/TerrainConstants.HighResChunk.ChunkSize);
            chunkZ = (int)(position.Z/TerrainConstants.HighResChunk.ChunkSize);

            position.X -= (chunkX * TerrainConstants.HighResChunk.ChunkSize);
            position.Z -= (chunkZ * TerrainConstants.HighResChunk.ChunkSize);

            MapTileChunk MTC = this.zoneGrid[tileZ, tileX].GetChunk(chunkZ,chunkX);
            return MTC.ClosestNormal(position, direction);
        }
    }
}
