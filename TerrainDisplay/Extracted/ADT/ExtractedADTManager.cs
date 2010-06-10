using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MPQNav.MPQ;
using MPQNav.MPQ.ADT;

namespace TerrainDisplay.Extracted
{
    public class ExtractedADTManager : IADTManager
    {
        private ExtractedTerrain _terrain;
        private int _mapId;
        private string _basePath;
        private IList<ADTBase> _mapTiles;

        public IList<ADTBase> MapTiles
        {
            get { return _mapTiles; }
        }

        public bool LoadTile(int tileX, int tileY)
        {
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
                _terrain.M2Manager.Add(def);
            }

            foreach (var def in currentADT.WMODefs)
            {
                _terrain.WMOManager.AddWMO(def);
            }

            currentADT.GenerateHeightVertexAndIndices();
            currentADT.GenerateLiquidVertexAndIndices();

            _mapTiles.Add(currentADT);

            return true;
        }

        public ExtractedADTManager(ExtractedTerrain terrain, string basePath, int mapId)
        {
            _mapId = mapId;
            _terrain = terrain;
            _basePath = basePath;
            _mapTiles = new List<ADTBase>();
        }
    }
}
