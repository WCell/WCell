using System;
using System.IO;
using NLog;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Tools.Maps.Structures;
using WCell.Tools.Maps.Utils;
using WCell.Util.Graphics;
using WCell.Util.Toolshed;

namespace WCell.Tools.Maps
{
	/// <summary>
	/// Reads information from the exported object files
	/// </summary>
	public static class ObjectFileReader
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private const string fileType = "wmo";

	    #region Read
		[Tool]
		public static void ViewRegionBuildings()
		{
            // Debug constraints here
			var regionBuildings = ReadRegionBuildings(MapId.EasternKingdoms);
			var buildings = regionBuildings.ObjectsByTile[36, 49];

			var highestVec = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			WMO highestWMO = null;
			foreach (var building in buildings)
			{
				var groups = building.BuildingGroups;
				foreach (var buildingGroup in groups)
				{
					var verts = buildingGroup.Vertices;
					foreach (var vector3 in verts)
					{
						if (vector3.Y < highestVec.Y) continue;
						highestVec = vector3;
						highestWMO = building;
					}
				}
			}

			Vector3 newVec;
			var worldVec = highestVec;
			if (highestWMO != null)
			{
				var center = (highestWMO.Bounds.Min + highestWMO.Bounds.Max) / 2.0f;
				newVec = Vector3.Transform(highestVec, Matrix.CreateRotationY(WCell.Util.Graphics.MathHelper.ToRadians(highestWMO.RotationModelY - 90)));

				// shift newVec to world space
				worldVec = new Vector3(newVec.Z, newVec.X, newVec.Y);
				worldVec += center;
			}

			Console.WriteLine(String.Format("X: {0}, Y: {1}, Z: {2}", worldVec.X, worldVec.Y, worldVec.Z));
		}

		public static RegionWMOs ReadRegionBuildings(MapId id)
		{
			var filePath = Path.Combine(ToolConfig.WMODir, ((int)id).ToString());
			if (!Directory.Exists(filePath))
			{
				throw new DirectoryNotFoundException(filePath);
			}

			var regionBuildings = new RegionWMOs();

			var count = 0;
			var max = TerrainConstants.TilesPerMapSide;
			for (var tileY = 0; tileY < max; tileY++)
			{
				for (var tileX = 0; tileX < max; tileX++)
				{
                    // Debug constraints here
					if (tileX != 36) continue;
					if (tileY != 49) continue;

					var fullPath = Path.Combine(filePath, TerrainConstants.GetWMOFile(tileX, tileY));
					if (!File.Exists(fullPath)) continue;

					var file = File.OpenRead(fullPath);
					var reader = new BinaryReader(file);

					var tileBuildings = ReadTileBuildings(reader);

					regionBuildings.ObjectsByTile[tileX, tileY] = tileBuildings;
					count++;

					file.Close();
				}
			}
			if (count > 1) regionBuildings.HasTiles = true;

			return regionBuildings;
		}

		public static TileObjects<WMO> ReadTileBuildings(BinaryReader reader)
		{
			var magix = reader.ReadString();
			if (magix != fileType)
			{
				Console.WriteLine("Invalid file format, suckah!");
				return null;
			}

			WMO[] wmos;
			var numBuildings = reader.ReadUInt16();
			if (numBuildings > 0)
			{
				wmos = new WMO[numBuildings];
				for (var i = 0; i < numBuildings; i++)
				{
					wmos[i] = ReadBuilding(reader);
				}
			}
			else
			{
				wmos = new WMO[0];
			}

			return new TileObjects<WMO>(wmos);
		}

		public static WMO ReadBuilding(BinaryReader reader)
		{
			var building = new WMO
			{
				Bounds = reader.ReadBoundingBox(),
				RotationModelY = reader.ReadSingle(),
				WorldPos = reader.ReadVector3()
			};

			WMOSubGroup[] groups;
			var numGroups = reader.ReadUInt16();
			if (numGroups > 0)
			{
				groups = new WMOSubGroup[numGroups];
				for (var i = 0; i < numGroups; i++)
				{
					groups[i] = ReadBuildingGroup(reader);
				}
			}
			else
			{
				groups = null;
			}
			building.BuildingGroups = groups;

			return building;
		}

		public static WMOSubGroup ReadBuildingGroup(BinaryReader reader)
		{
			var group = new WMOSubGroup
			{
				Bounds = reader.ReadBoundingBox()
			};

			Vector3[] vertices;
			var numVerts = reader.ReadUInt16();
			if (numVerts > 0)
			{
				vertices = new Vector3[numVerts];
				for (var i = 0; i < numVerts; i++)
				{
					vertices[i] = reader.ReadVector3();
				}
			}
			else
			{
				vertices = null;
			}
			group.Vertices = vertices;

			group.Tree = ReadBSPTree(reader);
			return group;
		}

		public static BSPTree ReadBSPTree(BinaryReader reader)
		{
			var rootId = reader.ReadInt16();
			BSPNode[] nodes;

			var numNodes = reader.ReadUInt16();
			if (numNodes > 0)
			{
				nodes = new BSPNode[numNodes];
				for (var i = 0; i < numNodes; i++)
				{
					nodes[i] = ReadBSPNode(reader);
				}
			}
			else
			{
				nodes = null;
			}

			return new BSPTree(rootId, nodes);
		}

		public static BSPNode ReadBSPNode(BinaryReader reader)
		{
			var node = new BSPNode
			{
				flags = ((BSPNodeFlags)reader.ReadByte()),
				negChild = reader.ReadInt16(),
				posChild = reader.ReadInt16(),
				planeDist = reader.ReadSingle()
			};

			var numIndices = reader.ReadUInt16();
			if (numIndices > 0)
			{
				var indices = new Index3[numIndices];
				for (var i = 0; i < numIndices; i++)
				{
					indices[i] = reader.ReadIndex3();
				}
				node.TriIndices = indices;
			}
			else
			{
				node.TriIndices = null;
			}

			return node;
		}
		#endregion
	}
}