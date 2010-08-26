using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TerrainDisplay.MPQ;
using TerrainDisplay.MPQ.ADT;

namespace TerrainDisplay.Extracted
{
    public class ExtractedADTManager : IADTManager
    {
        private ExtractedTerrainManager _terrainManager;
        private int _mapId;
        private string _basePath;
        private IList<ADTBase> _mapTiles;

        public IList<ADTBase> MapTiles
        {
            get { return _mapTiles; }
        }

        public bool LoadTile(TileIdentifier tileId)
        {
            var tileX = tileId.TileX;
            var tileY = tileId.TileY;

            if (!Directory.Exists(_basePath))
            {
                throw new DirectoryNotFoundException(string.Format("Invalid base directory for extracted map data: {0}", _basePath));
            }

            var mapPath = Path.Combine(_basePath, _mapId.ToString());
            var fileName = string.Format("{0}_{1}{2}", tileY, tileX, ".map");
            var filePath = Path.Combine(mapPath, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Invalid path or file name: {0}", filePath);
            }

            var currentADT = ExtractedADTParser.Process(filePath, _mapId, tileX, tileY);
            
            foreach (var def in currentADT.M2Defs)
            {
                _terrainManager.M2Manager.Add(def);
            }

            foreach (var def in currentADT.WMODefs)
            {
                _terrainManager.WMOManager.AddWMO(def);
            }

            currentADT.GenerateHeightVertexAndIndices();
            currentADT.GenerateLiquidVertexAndIndices();

            _mapTiles.Add(currentADT);

            return true;
        }

        public ExtractedADTManager(ExtractedTerrainManager terrainManager, string basePath, int mapId)
        {
            _mapId = mapId;
            _terrainManager = terrainManager;
            _basePath = basePath;
            _mapTiles = new List<ADTBase>();
        }
    }
}
