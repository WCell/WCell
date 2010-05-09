using System;
using System.IO;
using System.Text;
using NLog;
using WCell.MPQTool;
using WCell.Tools.Maps.Parsing;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps
{
	public static class WDTParser
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public static event Action<WDTFile> Parsed;
		public static MpqManager MpqManager;

		private const string baseDir = "WORLD\\maps\\";
		public const string Extension = ".wdt";

		internal static void EnsureEventEmpty()
		{
			if (Parsed != null)
			{
				throw new Exception("Parsed event is already in use.");
			}
		}

		public static void ExportAll()
		{
			log.Info("Extracting Terrain and World Objects - This will take a few minutes.....");
			var startTime = DateTime.Now;

			HeightMapExtractor.Prepare();
			WorldObjectExtractor.PrepareExtractor();

			ProcessAll();

			log.Info("Done ({0})", DateTime.Now - startTime);
		}

		/// <summary>
		/// Starts processing all WDT and ADT files
		/// </summary>
		public static void ProcessAll()
		{
			var wowRootDir = ToolConfig.Instance.GetWoWDir();
			MpqManager = new MpqManager(wowRootDir);
			var entryList = DBCMapReader.GetMapEntries();

			foreach (var mapEntry in entryList)
			{
				//if (mapEntry.Id != (int)MapId.EasternKingdoms) continue;
				Process(mapEntry);
			}
		}

		public static void Process(MpqManager manager, DBCMapEntry entry)
		{
			MpqManager = manager;
			Process(entry);
		}

		static void Process(DBCMapEntry entry)
		{
			if (Parsed == null)
			{
				throw new Exception("WDTParser.Parsed must be set before calling WDTParser.Process");
			}

			var dir = entry.MapDirName;
			var wdtDir = Path.Combine(baseDir, dir);
			var wdtName = dir;

			var wdtFilePath = Path.Combine(wdtDir, wdtName + Extension);
			if (!MpqManager.FileExists(wdtFilePath)) return;

			var wdt = new WDTFile
			{
				Manager = MpqManager,
				Entry = entry,
				Name = wdtName,
				Path = wdtDir
			};

			using (var fileReader = new BinaryReader(MpqManager.OpenFile(wdtFilePath)))
			{
				ReadMVER(fileReader, wdt);
				ReadMPHD(fileReader, wdt);
				ReadMAIN(fileReader, wdt);

				if ((wdt.Header.Header1 & WDTFlags.GlobalWMO) != 0)
				{
					// No terrain, the map is a "global" wmo
					// MWMO and MODF chunks follow
					ReadMWMO(fileReader, wdt);
					ReadMODF(fileReader, wdt);
				}
			}

			Parsed(wdt);
		}

		static void ReadMVER(BinaryReader fileReader, WDTFile wdt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();
			wdt.Version = fileReader.ReadInt32();
		}

		static void ReadMPHD(BinaryReader fileReader, WDTFile wdt)
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

		static void ReadMAIN(BinaryReader fileReader, WDTFile wdt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			for (var y = 0; y < 64; y++)
			{
				for (var x = 0; x < 64; x++)
				{
					wdt.TileProfile[x, y] = (fileReader.ReadInt64() != 0);
				}
			}
		}

		static void ReadMWMO(BinaryReader fileReader, WDTFile wdt)
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
					wdt.ObjectFiles.Add(fileReader.ReadCString());
				}
			}
		}

		static void ReadMODF(BinaryReader fileReader, WDTFile wdt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			var endPos = fileReader.BaseStream.Position + size;
			while (fileReader.BaseStream.Position < endPos)
			{
				var objectDef = new MapObjectDefinition();
				var nameIndex = fileReader.ReadInt32(); // 4 bytes
				objectDef.FileName = wdt.ObjectFiles[nameIndex];
				objectDef.UniqueId = fileReader.ReadUInt32(); // 4 bytes
				objectDef.Position = fileReader.ReadVector3(); // 12 bytes
				objectDef.OrientationA = fileReader.ReadSingle(); // 4 Bytes
				objectDef.OrientationB = fileReader.ReadSingle(); // 4 Bytes
				objectDef.OrientationC = fileReader.ReadSingle(); // 4 Bytes
				objectDef.Extents = fileReader.ReadBoundingBox(); // 12*2 bytes
				objectDef.Flags = fileReader.ReadUInt16(); // 2 bytes
				objectDef.DoodadSet = fileReader.ReadUInt16(); // 2 bytes
				objectDef.NameSet = fileReader.ReadUInt16(); // 2 bytes
				fileReader.ReadUInt16(); // padding

				wdt.ObjectDefinitions.Add(objectDef);
			}
		}

		public static byte PeekByte(this BinaryReader binReader)
		{
			byte b = binReader.ReadByte();
			binReader.BaseStream.Position -= 1;
			return b;
		}

		public static bool HasData(this BinaryReader br)
		{
			return br.BaseStream.Position < br.BaseStream.Length;
		}

		public static string ReadCString(this BinaryReader binReader)
		{
			StringBuilder sb = new StringBuilder();
			byte c;

			while ((c = binReader.ReadByte()) != 0)
			{
				sb.Append((char)c);
			}

			return sb.ToString();
		}

		public static BoundingBox ReadBoundingBox(this BinaryReader br)
		{
			return new BoundingBox(br.ReadVector3(), br.ReadVector3());
		}

		public static Vector3 ReadVector3(this BinaryReader br)
		{
			return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
		}

		public static Plane ReadPlane(this BinaryReader br)
		{
			return new Plane(br.ReadVector3(), br.ReadSingle());
		}

		public static string ReadFixedString(this BinaryReader br, int size)
		{
			var bytes = br.ReadBytes(size);

			for (int i = 0; i < size; i++)
			{
				if (bytes[i] == 0)
				{
					return Encoding.ASCII.GetString(bytes, 0, i);
				}
			}

			return Encoding.ASCII.GetString(bytes);
		}

		public static Quaternion ReadQuaternion(this BinaryReader br)
		{
			return new Quaternion(br.ReadVector3(), br.ReadSingle());
		}
	}
}
