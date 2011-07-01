using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Terrain.MPQ;
using WCell.MPQTool;
using WCell.Terrain.MPQ.ADTs;
using WCell.Terrain.MPQ.DBC;
using WCell.Util;

namespace WCell.Terrain.Serialization
{

	public class WDTReader
	{
		private const string baseDir = "WORLD\\MAPS\\";
		public const string Extension = ".wdt";

		public static IEnumerable<WDT> ReadAllWDTs()
		{
			return ReadAllWDTs(MPQFinder.GetDefaultFinder(WCellTerrainSettings.WoWPath));
		}

		public static IEnumerable<WDT> ReadAllWDTs(MPQFinder finder)
		{
			var entries = MapInfo.GetMapEntries();
			return entries.Select(mapEntry => ReadWDT(finder, mapEntry));
		}

		public static WDT ReadWDT(MapId mapId)
		{
			var map = MapInfo.GetMapInfo(mapId);
			if (map == null)
			{
				return null;
			}

			return ReadWDT(MPQFinder.GetDefaultFinder(WCellTerrainSettings.WoWPath), map);
		}

		public static WDT ReadWDT(MPQFinder finder, MapInfo entry)
		{
			var dir = entry.InternalName;
			var wdtDir = Path.Combine(baseDir, dir);
			var wdtName = dir;
			var wdtFilePath = Path.Combine(wdtDir, wdtName + Extension);

			if (!finder.FileExists(wdtFilePath)) return null;

			var wdt = new WDT(finder, entry.Id)
			{
				Entry = entry,
				Name = wdtName,
				Filename = wdtDir
			};

			using (var fileReader = new BinaryReader(finder.OpenFile(wdtFilePath)))
			{
				ReadMVER(fileReader, wdt);
				ReadMPHD(fileReader, wdt);
				ReadMAIN(fileReader, wdt);

				if (wdt.Header.IsWMOMap)
				{
					// No terrain, the map is a "global" wmo
					// MWMO and MODF chunks follow
					ReadMWMO(fileReader, wdt);
					ReadMODF(fileReader, wdt);
				}
			}

			return wdt;
		}


		static void ReadMWMO(BinaryReader fileReader, WDT wdt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			var endPos = fileReader.BaseStream.Position + size;
			while (fileReader.BaseStream.Position < endPos)
			{
				if (fileReader.PeekByte() == 0)
				{
					fileReader.BaseStream.Position++;
				}
				else
				{
					wdt.WmoFiles.Add(fileReader.ReadCString());
				}
			}
		}

		static void ReadMODF(BinaryReader fileReader, WDT wdt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			var endPos = fileReader.BaseStream.Position + size;
			while (fileReader.BaseStream.Position < endPos)
			{
				var objectDef = new MapObjectDefinition();
				var nameIndex = fileReader.ReadInt32(); // 4 bytes
				objectDef.FilePath = wdt.WmoFiles[nameIndex];
				objectDef.UniqueId = fileReader.ReadUInt32(); // 4 bytes
				objectDef.Position = fileReader.ReadVector3(); // 12 bytes
				objectDef.OrientationA = fileReader.ReadSingle(); // 4 Bytes
				objectDef.OrientationB = fileReader.ReadSingle(); // 4 Bytes
				objectDef.OrientationC = fileReader.ReadSingle(); // 4 Bytes
				objectDef.Extents = fileReader.ReadBoundingBox(); // 12*2 bytes
				objectDef.Flags = fileReader.ReadUInt16(); // 2 bytes
				objectDef.DoodadSetId = fileReader.ReadUInt16(); // 2 bytes
				objectDef.NameSet = fileReader.ReadUInt16(); // 2 bytes
				fileReader.ReadUInt16(); // padding

				wdt.WmoDefinitions.Add(objectDef);
			}
		}

		static void ReadMVER(BinaryReader fileReader, WDT wdt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();
			wdt.Version = fileReader.ReadInt32();
		}

		static void ReadMPHD(BinaryReader fileReader, WDT wdt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();
			wdt.Header.Header1 = (WDTFlags)fileReader.ReadInt32();
			wdt.Header.Header2 = fileReader.ReadInt32();
			wdt.Header.Header3 = fileReader.ReadInt32();
			wdt.Header.Header4 = fileReader.ReadInt32();
			wdt.Header.Header5 = fileReader.ReadInt32();
			wdt.Header.Header6 = fileReader.ReadInt32();
			wdt.Header.Header7 = fileReader.ReadInt32();
			wdt.Header.Header8 = fileReader.ReadInt32();
		}

		static void ReadMAIN(BinaryReader fileReader, WDT wdt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			// Rows are along the x-axis
			for (var x = 0; x < TerrainConstants.TilesPerMapSide; x++)
			{
				// Columns are along the y-axis
				for (var y = 0; y < TerrainConstants.TilesPerMapSide; y++)
				{
					//if (x == 48 && y == 30)
					//{
					//    wdt.TileProfile[y, x] = true;
					//}
					//else
					//{
					//    wdt.TileProfile[y, x] = false;
					//}
					// Stored as [col, row], that's weird.
					wdt.TileProfile[y, x] = (fileReader.ReadInt64() != 0);
				}
			}
		}

		static void PrintProfile(WDT wdt)
		{
			using (var file = new StreamWriter(wdt.Entry.InternalName + ".wdtprofile.txt"))
			{
				for (var x = 0; x < 64; x++)
				{
					for (var y = 0; y < 64; y++)
					{
						if (wdt.TileProfile[y, x])
						{
							file.Write("X");
						}
						else
						{
							file.Write(".");
						}
					}
					file.WriteLine();
				}
			}
		}
	}
}
