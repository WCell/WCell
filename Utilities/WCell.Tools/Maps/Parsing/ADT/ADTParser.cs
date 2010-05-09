using System;
using System.IO;
using WCell.Constants;
using WCell.MPQTool;

namespace WCell.Tools.Maps.Parsing.ADT
{
	public static class ADTParser
	{
		public static ADTFile Process(MpqManager manager, string dataDirectory, string adtName)
		{
			var adtFilePath = Path.Combine(dataDirectory, adtName + ".adt");
			if (!manager.FileExists(adtFilePath)) return null;

			using (var fileReader = new BinaryReader(manager.OpenFile(adtFilePath)))
			{
				var adt = new ADTFile()
				{
					Name = adtName,
					Path = dataDirectory
				};

				ReadMVER(fileReader, adt);
				ReadMHDR(fileReader, adt);

				if (adt.Header.offsInfo != 0)
				{
					fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsInfo;
					ReadMCIN(fileReader, adt);
				}
				if (adt.Header.offsModels != 0)
				{
					fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsModels;
					ReadMMDX(fileReader, adt);
				}
				if (adt.Header.offsMapObjects != 0)
				{
					fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsMapObjects;
					ReadMWMO(fileReader, adt);
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
				if (adt.Header.offsMH2O != 0)
				{
					fileReader.BaseStream.Position = adt.Header.Base + adt.Header.offsMH2O;
					ReadMH2O(fileReader, adt);
				}

				ReadMCNKs(fileReader, adt);

				return adt;
			}
		}

		static void ReadMVER(BinaryReader fileReader, ADTFile adt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			adt.Version = fileReader.ReadInt32();
		}

		static void ReadMHDR(BinaryReader fileReader, ADTFile adt)
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

		static void ReadMCIN(BinaryReader fileReader, ADTFile adt)
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

		static void ReadMCNKs(BinaryReader br, ADTFile adt)
		{
			for (var i = 0; i < 256; i++)
			{
				var chunk = ProcessChunk(br, adt.MapChunkInfo[i].Offset);
				adt.MapChunks[chunk.IndexX, chunk.IndexY] = chunk;

				if (chunk.ofsHeight != 0)
				{
					adt.HeightMaps[chunk.IndexX, chunk.IndexY] = ProcessHeights(br, adt.MapChunkInfo[i].Offset + chunk.ofsHeight);
				}
			}
		}

		static void ReadMWMO(BinaryReader fileReader, ADTFile adt)
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

		static void ReadMMDX(BinaryReader fileReader, ADTFile adt)
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

		static void ReadMDDF(BinaryReader fileReader, ADTFile adt)
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
				doodadDefinition.Position = fileReader.ReadVector3(); // 12 bytes
				doodadDefinition.OrientationA = fileReader.ReadSingle(); // 4 Bytes
				doodadDefinition.OrientationB = fileReader.ReadSingle(); // 4 Bytes
				doodadDefinition.OrientationC = fileReader.ReadSingle(); // 4 Bytes
				doodadDefinition.Scale = fileReader.ReadUInt32() / 1024f; // 4 bytes
				adt.DoodadDefinitions.Add(doodadDefinition);
			}
		}

		static void ReadMODF(BinaryReader fileReader, ADTFile adt)
		{
			var type = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			var endPos = fileReader.BaseStream.Position + size;
			while (fileReader.BaseStream.Position < endPos)
			{
				var objectDef = new MapObjectDefinition();
				var nameIndex = fileReader.ReadInt32(); // 4 bytes
				objectDef.FileName = adt.ObjectFiles[nameIndex];
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

				adt.ObjectDefinitions.Add(objectDef);
			}
		}

		static void ReadMH2O(BinaryReader fileReader, ADTFile adt)
		{
			var sig = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();

			var ofsMH2O = fileReader.BaseStream.Position;
			var mh2oHeader = new MH2OHeader[256];

			for (var i = 0; i < 256; i++)
			{
				mh2oHeader[i] = new MH2OHeader
									{
										ofsData1 = fileReader.ReadUInt32(),
										LayerCount = fileReader.ReadUInt32(),
										ofsData2 = fileReader.ReadUInt32()
									};
			}

			for (var y = 0; y < 16; y++)
			{
				for (var x = 0; x < 16; x++)
				{
					adt.LiquidMaps[x, y] = ProcessMH2O(fileReader, mh2oHeader[y * 16 + x], ofsMH2O);
				}
			}
		}

		private static MH2O ProcessMH2O(BinaryReader fileReader, MH2OHeader header, long waterSegmentBase)
		{
			var water = new MH2O();

			if (header.LayerCount == 0)
			{
				water.Used = false;
				return water;
			}

			water.Used = true;

			fileReader.BaseStream.Position = waterSegmentBase + header.ofsData1;

			water.Flags = (MH2OFlags)fileReader.ReadUInt16();

			water.Type = (FluidType)fileReader.ReadUInt16();
			water.HeightLevel1 = fileReader.ReadSingle();
			water.HeightLevel2 = fileReader.ReadSingle();
			water.XOffset = fileReader.ReadByte();
			water.YOffset = fileReader.ReadByte();
			water.Width = fileReader.ReadByte();
			water.Height = fileReader.ReadByte();

			var ofsWaterFlags = fileReader.ReadUInt32();
			var ofsWaterHeightMap = fileReader.ReadUInt32();

			var heightMapLen = (water.Width + 1) * (water.Height + 1);
			var heights = new float[heightMapLen];

			// If flags is 2, the chunk is for an ocean, and there is no heightmap
			if (ofsWaterHeightMap != 0 && (water.Flags & MH2OFlags.Ocean) == 0)
			{
				fileReader.BaseStream.Position = waterSegmentBase + ofsWaterHeightMap;

				for (var i = 0; i < heightMapLen; i++)
				{
					heights[i] = fileReader.ReadSingle();
					if (heights[i] == 0)
					{
						heights[i] = water.HeightLevel1;
					}
				}

			}
			else
			{
				for (var i = 0; i < heightMapLen; i++)
				{
					heights[i] = water.HeightLevel1;
				}
			}

			water.Heights = new float[water.Height + 1, water.Width + 1];
			for (var r = 0; r <= water.Height; r++)
			{
				for (var c = 0; c <= water.Width; c++)
				{

					water.Heights[r, c] = heights[c + r * c];
				}
			}

			return water;
		}

		static MCNK ProcessChunk(BinaryReader fileReader, uint mcnkOffset)
		{
			var mcnk = new MCNK();

			fileReader.BaseStream.Position = mcnkOffset;

			var sig = fileReader.ReadUInt32();
			if (sig != Signatures.MCNK)
			{
				Console.WriteLine("Invalid Chunk offset found.");
			}

			var mcnkSize = fileReader.ReadUInt32();

			mcnk.Flags = fileReader.ReadInt32();
			mcnk.IndexX = fileReader.ReadInt32();
			mcnk.IndexY = fileReader.ReadInt32();
			mcnk.nLayers = fileReader.ReadUInt32(); //0xC
			mcnk.nDoodadRefs = fileReader.ReadUInt32(); //0x10
			mcnk.ofsHeight = fileReader.ReadUInt32(); //0x14
			mcnk.ofsNormal = fileReader.ReadUInt32(); //0x18
			mcnk.ofsLayer = fileReader.ReadUInt32(); //0x1C
			mcnk.ofsRefs = fileReader.ReadUInt32(); //0x20
			mcnk.ofsAlpha = fileReader.ReadUInt32(); //0x24
			mcnk.sizeAlpha = fileReader.ReadUInt32(); //0x28
			mcnk.ofsShadow = fileReader.ReadUInt32(); //0x2C
			mcnk.sizeShadow = fileReader.ReadUInt32(); //0x30
			mcnk.AreaId = fileReader.ReadUInt32(); //0x34
			mcnk.nMapObjRefs = fileReader.ReadUInt32(); //0x38

			mcnk.Holes = fileReader.ReadUInt16();
			fileReader.ReadUInt16(); // pad

			mcnk.predTex = new ushort[8];
			for (var i = 0; i < 8; i++)
			{
				mcnk.predTex[i] = fileReader.ReadUInt16();
			}

			mcnk.nEffectDoodad = new byte[8];
			for (var i = 0; i < 8; i++)
			{
				mcnk.nEffectDoodad[i] = fileReader.ReadByte();
			}
			mcnk.ofsSndEmitters = fileReader.ReadUInt32(); //0x58
			mcnk.nSndEmitters = fileReader.ReadUInt32(); //0x5C
			mcnk.ofsLiquid = fileReader.ReadUInt32(); //0x60
			mcnk.sizeLiquid = fileReader.ReadUInt32(); //0x64
			mcnk.Z = fileReader.ReadSingle();
			mcnk.X = fileReader.ReadSingle();
			mcnk.Y = fileReader.ReadSingle();
			mcnk.offsColorValues = fileReader.ReadInt32();
			mcnk.props = fileReader.ReadInt32();
			mcnk.effectId = fileReader.ReadInt32();

			return mcnk;
		}

		static MCVT ProcessHeights(BinaryReader fileReader, uint mcvtOffset)
		{
			fileReader.BaseStream.Position = mcvtOffset;
			var heights = new MCVT();
			var sig = fileReader.ReadUInt32();
			var size = fileReader.ReadUInt32();
			for (var i = 0; i < 145; i++)
			{
				heights.Heights[i] = fileReader.ReadSingle();
			}
			return heights;
		}
	}
}