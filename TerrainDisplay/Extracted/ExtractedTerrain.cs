using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPQNav.MPQ;
using MPQNav.MPQ.ADT;
using MPQNav.MPQ.M2;
using MPQNav.MPQ.WDT;
using MPQNav.MPQ.WMO;

namespace TerrainDisplay.Extracted
{
    public class ExtractedTerrain : ITerrainManager
    {
        private int _mapId;
        private string _baseDirectory;
        private IADTManager _adtManager;
        private IWMOManager _wmoManager;
        private IM2Manager _m2Manager;

        private WDTFile _wdt;

        public IADTManager ADTManager
        {
            get { return _adtManager; }
        }

        public IWMOManager WMOManager
        {
            get { return _wmoManager; }
        }

        public IM2Manager M2Manager
        {
            get { return _m2Manager; }
        }

        public WDTFile WDT
        {
            get { return _wdt; }
        }

        public ExtractedTerrain(string dataPath, int mapId)
        {
            _mapId = mapId;
            _baseDirectory = dataPath;

            _adtManager = new ExtractedADTManager();
            _wmoManager = new ExtractedWMOManager();
            _m2Manager = new ExtractedM2Manager();
        }
    }
}
