using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerrainDisplay;
using WCell.Terrain;
using WCell.Terrain.MPQ.WDT;
using WCell.MPQTool.StormLibWrapper;
using WCell.Terrain.Serialization;
using WCell.Util.NLog;

namespace TerrainExtractor
{
    public static class Program
    {
        public static void Main(string[] args)
        {
			// TODO: Re-use existing config
			
            LogUtil.SetupConsoleLogging();
            //WCellTerrainSettings.Initialize();

            NativeMethods.StormLibFolder = WCellTerrainSettings.LibDir;
            NativeMethods.InitAPI();

            WDTExtractor.ExportAll();
        }

        static Program()
        {
            new TileIdentifier();
        }
    }
}
