using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using WCell.Constants;
using WCell.Constants.World;
using WCell.MPQTool;
using WCell.Terrain.MPQ;
using WCell.Terrain.MPQ.ADTs;
using WCell.Terrain.MPQ.WMOs;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.Serialization
{
	/// <summary>
	/// Parses ADTs from their original format
	/// </summary>
	public static class ADTReader
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		private const string baseDir = "WORLD\\MAPS\\";
		public const string Extension = ".adt";


		public static ADT ReadADT(MPQFinder mpqFinder, WDT terrain, Point2D coords)
		{
			var map = terrain.MapId;
			var name = TileIdentifier.GetName(map);

			if (name == null)
			{
				throw new ArgumentException("Map does not exist: " + map);
			}
			var fileName = string.Format("{0}\\{0}_{1}_{2}{3}", name, coords.Y, coords.X, Extension);
			var filePath = Path.Combine(baseDir, fileName);
			var adt = new ADT(coords, terrain);

			if (!mpqFinder.FileExists(filePath))
			{
				log.Error("ADT file does not exist: ", filePath);
			}

			using (var stream = mpqFinder.OpenFile(filePath))
			using (var fileReader = new BinaryReader(stream))
			{
				ReadMVER(fileReader, adt);

				ReadMHDR(fileReader, adt);

				if (adt.Header.offsInfo != 0)
				{
					fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsInfo;
					ReadMCIN(fileReader, adt);
				}
				//if (adt.Header.offsTex != 0)
				//{
				//    fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsTex;
				//    ReadMTEX(fileReader, adt);
				//}
				if (adt.Header.offsModels != 0)
				{
					fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsModels;
					ReadMMDX(fileReader, adt);
				}
				if (adt.Header.offsModelIds != 0)
				{
					fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsModelIds;
					ReadMMID(fileReader, adt);
				}
				if (adt.Header.offsMapObjects != 0)
				{
					fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsMapObjects;
					ReadMWMO(fileReader, adt);
				}
				if (adt.Header.offsMapObjectIds != 0)
				{
					fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsMapObjectIds;
					ReadMWID(fileReader, adt);
				}
				if (adt.Header.offsDoodadDefinitions != 0)
				{
					fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsDoodadDefinitions;
					ReadMDDF(fileReader, adt);
				}
				if (adt.Header.offsObjectDefinitions != 0)
				{
					fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsObjectDefinitions;
					ReadMODF(fileReader, adt);
				}
				//if (adt.Header.offsFlightBoundary != 0)
				//{
				//    fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsFlightBoundary;
				//    ReadMFBO(fileReader, adt);
				//}
				if (adt.Header.offsMH2O != 0)
				{
					fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsMH2O;
					ReadMH2O(fileReader, adt);
				}

				ReadMCNK(fileReader, adt);
			}

			// add WMOs & M2s
			adt.WMOs = new WMORoot[adt.ObjectDefinitions.Count];
			for (var i = 0; i < adt.ObjectDefinitions.Count; i++)
			{
				var def = adt.ObjectDefinitions[i];
				var wmo = terrain.GetOrReadWMO(def);
				adt.WMOs[i] = wmo;
			}

			adt.M2s = new M2[adt.DoodadDefinitions.Count];
			for (var i = 0; i < adt.DoodadDefinitions.Count; i++)
			{
				var def = adt.DoodadDefinitions[i];
				var m2 = terrain.GetOrReadM2(def);
				adt.M2s[i] = m2;
			}


			return adt;
		}

		static void ReadMVER(BinaryReader fileReader, ADT adt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			adt.Version = fileReader.ReadInt32();
		}

		static void ReadMHDR(BinaryReader fileReader, ADT adt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			adt.Header.Base = (uint)fileReader.BaseStream.Position;

			var pad = fileReader.ReadUInt32();

			adt.Header.offsInfo = fileReader.ReadUInt32();
			adt.Header.offsTex = fileReader.ReadUInt32();
			adt.Header.offsModels = fileReader.ReadUInt32();
			adt.Header.offsModelIds = fileReader.ReadUInt32();
			adt.Header.offsMapObjects = fileReader.ReadUInt32();
			adt.Header.offsMapObjectIds = fileReader.ReadUInt32();
			adt.Header.offsDoodadDefinitions = fileReader.ReadUInt32();
			adt.Header.offsObjectDefinitions = fileReader.ReadUInt32();
			adt.Header.offsFlightBoundary = fileReader.ReadUInt32();
			adt.Header.offsMH2O = fileReader.ReadUInt32();

			var pad3 = fileReader.ReadUInt32();
			var pad4 = fileReader.ReadUInt32();
			var pad5 = fileReader.ReadUInt32();
			var pad6 = fileReader.ReadUInt32();
			var pad7 = fileReader.ReadUInt32();
		}

		static void ReadMCIN(BinaryReader fileReader, ADT adt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			adt.MapChunkInfo = new MapChunkInfo[256];
			for (var i = 0; i < 256; i++)
			{
				var mcin = new MapChunkInfo
							   {
								   Offset = fileReader.ReadUInt32(),
								   Size = fileReader.ReadUInt32(),
								   Flags = fileReader.ReadUInt32(),
								   AsyncId = fileReader.ReadUInt32()
							   };

				adt.MapChunkInfo[i] = mcin;
			}
		}

		static void ReadMTEX(BinaryReader br, ADT adt)
		{

		}

		static void ReadMMDX(BinaryReader fileReader, ADT adt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			long endPos = fileReader.BaseStream.Position + size;
			while (fileReader.BaseStream.Position < endPos)
			{
				if (fileReader.PeekByte() == 0)
				{
					fileReader.BaseStream.Position++;
				}
				else
				{
					adt.ModelFiles.Add(fileReader.ReadCString());
				}
			}
		}

		static void ReadMMID(BinaryReader fileReader, ADT adt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			uint count = size / 4;
			for (int i = 0; i < count; i++)
			{
				adt.ModelNameOffsets.Add(fileReader.ReadInt32());
			}
		}

		static void ReadMWMO(BinaryReader fileReader, ADT adt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			long endPos = fileReader.BaseStream.Position + size;
			while (fileReader.BaseStream.Position < endPos)
			{
				if (fileReader.PeekByte() == 0)
				{
					fileReader.BaseStream.Position++;
				}
				else
				{
					adt.ObjectFiles.Add(fileReader.ReadCString());
				}
			}
		}

		static void ReadMWID(BinaryReader fileReader, ADT adt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			uint count = size / 4;
			for (int i = 0; i < count; i++)
			{
				adt.ObjectFileOffsets.Add(fileReader.ReadInt32());
			}
		}

		static void ReadMDDF(BinaryReader fileReader, ADT adt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			var endPos = fileReader.BaseStream.Position + size;
			while (fileReader.BaseStream.Position < endPos)
			{
				var doodadDefinition = new MapDoodadDefinition();
				var nameIndex = fileReader.ReadInt32();
				doodadDefinition.FilePath = adt.ModelFiles[nameIndex]; // 4 bytes
				doodadDefinition.UniqueId = fileReader.ReadUInt32(); // 4 bytes
				var Y = fileReader.ReadSingle();
				var Z = fileReader.ReadSingle();
				var X = fileReader.ReadSingle();
				doodadDefinition.Position = new Vector3(X, Y, Z); // 12 bytes
				doodadDefinition.OrientationA = fileReader.ReadSingle(); // 4 Bytes
				doodadDefinition.OrientationB = fileReader.ReadSingle(); // 4 Bytes
				doodadDefinition.OrientationC = fileReader.ReadSingle(); // 4 Bytes
				doodadDefinition.Scale = fileReader.ReadUInt16() / 1024f; // 2 bytes
				doodadDefinition.Flags = fileReader.ReadUInt16(); // 2 bytes
				adt.DoodadDefinitions.Add(doodadDefinition);
			}
		}

		static void ReadMODF(BinaryReader fileReader, ADT adt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			long endPos = fileReader.BaseStream.Position + size;
			while (fileReader.BaseStream.Position < endPos)
			{
				var objectDef = new MapObjectDefinition();
				int nameIndex = fileReader.ReadInt32(); // 4 bytes
				objectDef.FilePath = adt.ObjectFiles[nameIndex];
				objectDef.UniqueId = fileReader.ReadUInt32(); // 4 bytes
				// This Position appears to be in the wrong order.
				// To get WoW coords, read it as: {Y, Z, X}
				var Y = fileReader.ReadSingle();
				var Z = fileReader.ReadSingle();
				var X = fileReader.ReadSingle();
				objectDef.Position = new Vector3(X, Y, Z); // 12 bytes
				objectDef.OrientationA = fileReader.ReadSingle(); // 4 Bytes
				objectDef.OrientationB = fileReader.ReadSingle(); // 4 Bytes
				objectDef.OrientationC = fileReader.ReadSingle(); // 4 Bytes

				var min = new Vector3();
				min.Y = fileReader.ReadSingle();
				min.Z = fileReader.ReadSingle();
				min.X = fileReader.ReadSingle();

				var max = new Vector3();
				max.Y = fileReader.ReadSingle();
				max.Z = fileReader.ReadSingle();
				max.X = fileReader.ReadSingle();
				objectDef.Extents = new BoundingBox(min, max); // 12*2 bytes
				objectDef.Flags = fileReader.ReadUInt16(); // 2 bytes
				objectDef.DoodadSetId = fileReader.ReadUInt16(); // 2 bytes
				objectDef.NameSet = fileReader.ReadUInt16(); // 2 bytes
				fileReader.ReadUInt16(); // padding

				adt.ObjectDefinitions.Add(objectDef);
			}
		}

		static void ReadMFBO(BinaryReader fileReader, ADT adt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();
		}

		static void ReadMCNK(BinaryReader br, ADT adt)
		{
			for (var i = 0; i < 256; i++)
			{
				var chunk = ADTChunkReader.Process(br, adt.MapChunkInfo[i].Offset, adt);
				adt.Chunks[chunk.Header.IndexY, chunk.Header.IndexX] = chunk;
			}
		}

		#region Liquids
		private struct MH20Header
		{
			public uint ofsData1;
			public uint LayerCount;
			public uint ofsData2;
		}

		static void ReadMH2O(BinaryReader fileReader, ADT adt)
		{
			var sig = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			long ofsMH2O = fileReader.BaseStream.Position;
			MH20Header[] mh20Header = new MH20Header[256];

			for (int i = 0; i < 256; i++)
			{
				mh20Header[i].ofsData1 = fileReader.ReadUInt32();
				mh20Header[i].LayerCount = fileReader.ReadUInt32();
				mh20Header[i].ofsData2 = fileReader.ReadUInt32();
			}

			// Rows
			for (int x = 0; x < 16; x++)
			{
				// Columns
				for (int y = 0; y < 16; y++)
				{
					// Indexing is [col, row]
					adt.LiquidInfo[x, y] = ProcessMH2O(fileReader, mh20Header[x * 16 + y], ofsMH2O);
				}
			}
		}

		private static MH2O ProcessMH2O(BinaryReader fileReader, MH20Header header, long waterSegmentBase)
		{
			var water = new MH2O();

			if (header.LayerCount == 0)
			{
				water.Header.Used = false;
				return water;
			}

			water.Header.Used = true;

			fileReader.BaseStream.Position = waterSegmentBase + header.ofsData1;

			water.Header.Flags = (MH2OFlags)fileReader.ReadUInt16();

			water.Header.Type = (FluidType)fileReader.ReadUInt16();
			water.Header.HeightLevel1 = fileReader.ReadSingle();
			water.Header.HeightLevel2 = fileReader.ReadSingle();
			water.Header.YOffset = fileReader.ReadByte();
			water.Header.XOffset = fileReader.ReadByte();
			water.Header.Width = fileReader.ReadByte();
			water.Header.Height = fileReader.ReadByte();

			var ofsWaterFlags = fileReader.ReadUInt32();
			var ofsWaterHeightMap = fileReader.ReadUInt32();

			water.RenderBitMap = new byte[water.Header.Height];
			if (ofsWaterFlags != 0)
			{
				fileReader.BaseStream.Position = waterSegmentBase + ofsWaterFlags;
				for (var i = 0; i < water.Header.Height; i++)
				{
					if (i < (ofsWaterHeightMap - ofsWaterFlags))
					{
						water.RenderBitMap[i] = fileReader.ReadByte();
					}
					else
					{
						water.RenderBitMap[i] = 0;
					}
				}
			}


			//var heightMapLen = (water.Header.Width + 1) * (water.Header.Height + 1);
			var heightMapLen = (TerrainConstants.UnitsPerChunkSide + 1) * (TerrainConstants.UnitsPerChunkSide + 1);
			water.HeightsArray = new float[heightMapLen];

			// If flags is 2, the chunk is for an ocean, and there is no heightmap
			if (ofsWaterHeightMap != 0 && (water.Header.Flags & MH2OFlags.Ocean) == 0)
			{
				fileReader.BaseStream.Position = waterSegmentBase + ofsWaterHeightMap;
				for (var i = 0; i < heightMapLen; i++)
				{
					water.HeightsArray[i] = fileReader.ReadSingle();
					if (water.HeightsArray[i] == 0)
					{
						water.HeightsArray[i] = water.Header.HeightLevel1;
					}
				}
			}
			else
			{
				for (var i = 0; i < heightMapLen; i++)
				{
					water.HeightsArray[i] = water.Header.HeightLevel1;
				}
			}

			return water;
		}
		#endregion


	}
}
