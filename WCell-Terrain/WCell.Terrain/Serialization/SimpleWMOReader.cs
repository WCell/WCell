using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Terrain.MPQ.WMOs;
using WCell.Terrain.Simple.WMO;
using WCell.Constants.World;
using WCell.Util;

namespace WCell.Terrain.Serialization
{
	public static class SimpleWMOReader
	{
		public static SimpleWMO Process(string basePath, MapId mapId, string path)
		{
			basePath = Path.Combine(basePath, mapId.ToString());
			var filePath = Path.Combine(basePath, path);
			filePath = Path.ChangeExtension(filePath, ".wmo");

			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("Extracted M2 file not found: {0}", filePath);
			}

			var wmo = new SimpleWMO();

			using (var file = File.OpenRead(filePath))
			using (var reader = new BinaryReader(file))
			{
				var type = reader.ReadString();
				if (type != WorldObjectWriter.WMOFileTypeId)
				{
					throw new InvalidDataException(string.Format("WMO file in invalid format: {0}", filePath));
				}

				var version = reader.ReadInt32();
				if (version != ADTWriter.Version)
				{
					throw new InvalidDataException(string.Format(
						"WMO file \"{0}\" has wrong version: {1} - Expected: {2}",
						filePath, version, WorldObjectWriter.Version));
				}


				wmo.Extents = reader.ReadBoundingBox();
				wmo.WMOId = reader.ReadUInt32();

				ReadWMODoodadDefs(reader, wmo);

				ReadWMOGroups(reader, wmo);
			}
			return wmo;
		}

		private static void ReadWMODoodadDefs(BinaryReader br, SimpleWMO wmo)
		{
			var numSets = br.ReadInt32();
			var setList = new List<Dictionary<int, SimpleWMOM2Definition>>(numSets);
			for (var i = 0; i < numSets; i++)
			{
				var numDefs = br.ReadInt32();
				var defDict = new Dictionary<int, SimpleWMOM2Definition>(numDefs);
				for (var j = 0; j < numDefs; j++)
				{
					var key = br.ReadInt32();
					var def = new SimpleWMOM2Definition
					{
						FilePath = br.ReadString(),
						Position = br.ReadVector3(),
						Extents = br.ReadBoundingBox(),
						WMOToModel = br.ReadMatrix(),
						ModeltoWMO = br.ReadMatrix()
					};
					defDict.Add(key, def);
				}
				setList.Add(defDict);
			}
			wmo.WMOM2Defs = setList;
		}

		private static void ReadWMOGroups(BinaryReader br, SimpleWMO wmo)
		{
			var groupCount = br.ReadInt32();
			var groupList = new List<SimpleWMOGroup>(groupCount);
			for (int i = 0; i < groupCount; i++)
			{
				var group = new SimpleWMOGroup();
				group.Flags = (WMOGroupFlags)br.ReadUInt32();
				group.Bounds = br.ReadBoundingBox();
				group.GroupId = br.ReadUInt32();
				group.ModelRefs = br.ReadInt32List();

				group.HasLiquid = br.ReadBoolean();
				if (group.HasLiquid)
				{
					ReadWMOGroupLiquidInfo(br, group);
				}
				group.WmoVertices = br.ReadVector3List();

				ReadBSPTree(br, group);

				groupList.Add(group);
			}
			wmo.Groups = groupList;
		}

		private static void ReadWMOGroupLiquidInfo(BinaryReader br, SimpleWMOGroup group)
		{
			group.LiquidBaseCoords = br.ReadVector3();
			group.LiqTileCountX = br.ReadInt32();
			group.LiqTileCountY = br.ReadInt32();

			group.LiquidTileMap = new bool[group.LiqTileCountX, group.LiqTileCountY];
			for (var y = 0; y < group.LiqTileCountY; y++)
			{
				for (var x = 0; x < group.LiqTileCountX; x++)
				{
					group.LiquidTileMap[x, y] = br.ReadBoolean();
				}
			}

			group.LiquidHeights = new float[group.LiqTileCountX + 1, group.LiqTileCountY + 1];
			for (var y = 0; y < group.LiqTileCountY + 1; y++)
			{
				for (var x = 0; x < group.LiqTileCountX + 1; x++)
				{
					group.LiquidHeights[x, y] = br.ReadSingle();
				}
			}
		}

		private static void ReadBSPTree(BinaryReader br, SimpleWMOGroup group)
		{
			var rootId = br.ReadInt16();
			var nodeCount = br.ReadInt32();
			var nodeList = new List<WMOBSPNode>(nodeCount);
			for (var i = 0; i < nodeCount; i++)
			{
				var node = new WMOBSPNode
				{
					flags = (BSPNodeFlags)br.ReadByte(),
					negChild = br.ReadInt16(),
					posChild = br.ReadInt16(),
					planeDist = br.ReadSingle(),
					TriIndices = br.ReadIndex3List()
				};

				nodeList.Add(node);
			}

			group.Tree = new WMOBSPTree(nodeList, rootId);
		}
	}
}
