using System;
using WCell.MPQTool.StormLibWrapper;
using WCell.Terrain.Serialization;
using WCell.Util.NLog;
using WCell.Constants.World;

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


            HeightfieldExtractor.ExtractHeightfield(MapId.EasternKingdoms);
			HeightfieldExtractor.ExtractHeightfield(MapId.Kalimdor);


        	Console.WriteLine();
			Console.WriteLine("Press ANY key to exit...");
			Console.ReadKey();
			// TODO: Load tiles one by one and write them to disk
            //SimpleADTWriter.WriteAllWDTs();
			//Extractor.ExtractAllADTs();
        	//Extractor.CreateAndWriteAllMeshes();
        	//Extractor.ExtractAndWriteAll();
        }

        static Program()
        {
			var cfg = ExtractorConfiguration.Instance;
			WCellTerrainSettings.Config = cfg;
            new TileIdentifier();
        }
    }
}
