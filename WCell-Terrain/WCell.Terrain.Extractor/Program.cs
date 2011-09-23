using System;
using WCell.MPQTool.StormLibWrapper;
using WCell.Terrain.Serialization;
using WCell.Util.NLog;

namespace WCell.Terrain.Extractor
{
    public static class Program
    {
        public static void Main(string[] args)
        {
			// TODO: Re-use existing config

        	//throw new NotImplementedException("Terrain serialization is currently under re-construction.");
			
            LogUtil.SetupConsoleLogging();
            //WCellTerrainSettings.Initialize();

            NativeMethods.StormLibFolder = WCellTerrainSettings.LibDir;
            //NativeMethods.InitAPI();

			

			// TODO: Load tiles one by one and write them to disk
            //SimpleADTWriter.WriteAllWDTs();
			Extractizzle.DoShit();
        }

        static Program()
        {
			var cfg = ExtractorConfiguration.Instance;
			WCellTerrainSettings.Config = cfg;
            new TileIdentifier();
        }
    }
}
