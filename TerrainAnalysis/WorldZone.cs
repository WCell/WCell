using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WCell.Collision;
using Microsoft.Xna.Framework;
using System.Collections;

namespace TerrainAnalysis
{
    public class WorldZone
    {
        MapTile[,] zoneGrid = new MapTile[64, 64];
        private bool topLeftAssigned=false;
        Microsoft.Xna.Framework.Vector2 topLeft, bottomRight;
        private readonly string zoneName;

        public WorldZone(string ZoneName)
        {
            this.zoneName = ZoneName;
        }

        public void ProcessZone()
        {
            WDTImporter wdtI = new WDTImporter("World\\Maps\\" + zoneName + "\\" + zoneName + ".wdt");
            Thread mapLoader = new Thread(new ParameterizedThreadStart(MapLoaderThread));
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
                for (int i = 0; i < 64; i++)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        if (m_wdt.TileProfile[i, j] == 1)
                        {
                            string fileName = this.zoneName + "_" + j.ToString() + "_" + i.ToString() + ".adt";
                            ADTImporter adtI = new ADTImporter("World\\Maps\\" + this.zoneName + "\\" + fileName, false);
                            adtI.Process();
                            ADT m_adt = adtI.ADT;
                            this.zoneGrid[i, j] = new MapTile(m_adt,i,j);

                            if (topLeftAssigned == false)
                                topLeft = new Vector2(j*TILELENGTH, i*TILELENGTH);
                            bottomRight = new Vector2(j * TILELENGTH, i * TILELENGTH);
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

        private static float TILELENGTH = 533.3333f; //scaler for tile width in the rendering system
        private static float CHUNKLENGTH = TILELENGTH / 16.0f; //16x16 chunks per tile

        public ArrayList GetChunksInRadius(Microsoft.Xna.Framework.Vector2 position, float radius)
        {
            int radiusInChunks = (int)(radius / CHUNKLENGTH);

            if (radiusInChunks == 0) radiusInChunks = 1;

            int tileRow = (int)(position.Y / TILELENGTH);
            int tileColumn = (int)(position.X / TILELENGTH);

            MapTile currentTile = this.zoneGrid[tileRow, tileColumn];
            if (currentTile != null)
            {
                position.X -= tileColumn * TILELENGTH;
                position.Y -= tileRow * TILELENGTH;

                MapTileChunk baseChunk = currentTile.GetChunk((int)(position.Y / CHUNKLENGTH), (int)(position.X / CHUNKLENGTH));

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

        public Microsoft.Xna.Framework.Vector2 ZoneCenter
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

        public Microsoft.DirectX.Vector3 NearestNormal(Microsoft.DirectX.Vector3 position, Microsoft.DirectX.Vector2 direction)
        {
            int tileX, tileZ, chunkX, chunkZ;
            
            tileX = (int)(position.X / TILELENGTH);
            tileZ = (int)(position.Z / TILELENGTH);

            position.X -= (tileX * TILELENGTH);
            position.Z -= (tileZ * TILELENGTH);
            chunkX = (int)(position.X/CHUNKLENGTH);
            chunkZ = (int)(position.Z/CHUNKLENGTH);

            position.X -= (chunkX * CHUNKLENGTH);
            position.Z -= (chunkZ * CHUNKLENGTH);

            MapTileChunk MTC = this.zoneGrid[tileZ, tileX].GetChunk(chunkZ,chunkX);
            return MTC.ClosestNormal(position, direction);
        }
    }
}
