using System;
using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.MPQTool;
using WCell.Tools.Maps.Parsing;
using WCell.Tools.Maps.Parsing.ADT;
using WCell.Util.Graphics;
using WCell.Util.Toolshed;

namespace WCell.Tools.Maps
{
	public static class WorldObjectExtractor
	{
		private const string fileType = "m2x";
		private static readonly HashSet<uint> LoadedM2Ids = new HashSet<uint>(), LoadedWMOIds = new HashSet<uint>();
		private const float RadiansPerDegree = 0.0174532925f;

		public static void PrepareExtractor()
		{
			WDTParser.Parsed += ExtractM2s;
			WDTParser.Parsed += ExtractWMOs;
		}

		private static void ExtractM2s(WDTFile wdt)
		{
			var regionM2s = ExtractObjects<M2Object>(wdt, ExtractWDTM2s, ExtractTileM2s);
			LoadedM2Ids.Clear();

			var mapEntry = wdt.Entry;

			var max = regionM2s.HasTiles ? TerrainConstants.TilesPerMapSide : 1;
			for (var tileY = 0; tileY < max; tileY++)
			{
				for (var tileX = 0; tileX < max; tileX++)
				{
					//if (tileX != 36) continue;
					//if (tileY != 49) continue;

					if (regionM2s.ObjectsByTile[tileX, tileY] == null) continue;

					// Don't bother writing tiles with no models.
					var tileModels = regionM2s.ObjectsByTile[tileX, tileY];
					if (tileModels.Count == 0) continue;

					var path = Path.Combine(ToolConfig.M2Dir, mapEntry.Id.ToString());
					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}

					var file = File.Create(Path.Combine(path, TerrainConstants.GetM2File(tileX, tileY)));

					WriteTileM2s(file, tileModels);
					file.Close();
				}
			}
		}

		private static RegionObjects<O> ExtractObjects<O>(WDTFile wdt,
			Func<List<MapObjectDefinition>, TileObjects<O>> globalExtractor,
			Func<int, int, ADTFile, TileObjects<O>> tileExtractor)
			where O : class, new()
		{
			var objs = new RegionObjects<O>();

			if (wdt == null) return null;
			if (wdt.Header.Header1.HasFlag(WDTFlags.GlobalWMO))
			{
				objs.HasTiles = false;
				// No terrain, load the global WMO
				if (wdt.ObjectDefinitions == null) return null;

				objs.ObjectsByTile = new TileObjects<O>[1, 1];

				objs.ObjectsByTile[0, 0] = globalExtractor(wdt.ObjectDefinitions);
			}
			else
			{
				objs.HasTiles = true;
				objs.ObjectsByTile = new TileObjects<O>[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];
				for (var tileY = 0; tileY < TerrainConstants.TilesPerMapSide; tileY++)
				{
					for (var tileX = 0; tileX < TerrainConstants.TilesPerMapSide; tileX++)
					{
						if (!wdt.TileProfile[tileX, tileY]) continue;
						var adtName = TerrainConstants.GetADTFile(wdt.Name, tileX, tileY);
						var adt = ADTParser.Process(wdt.Manager, wdt.Path, adtName);
						if (adt == null) continue;

						objs.ObjectsByTile[tileX, tileY] = tileExtractor(tileX, tileY, adt);
					}
				}
			}

			return objs;
		}

		#region M2 Extract
		private static TileObjects<M2Object> ExtractWDTM2s(List<MapObjectDefinition> wmos)
		{
			var tileModels = new TileObjects<M2Object>();

			for (var i = 0; i < wmos.Count; i++)
			{
				var wmo = wmos[i];
				if (LoadedWMOIds.Contains(wmo.UniqueId)) continue;

				var filePath = wmo.FileName;
				var root = WMORootParser.Process(WDTParser.MpqManager, filePath);
				if (root == null)
				{
					Console.WriteLine("Invalid WMORoot returned.");
					return null;
				}

				LoadedWMOIds.Add(wmo.UniqueId);
				if (root.DoodadDefinitions == null)
				{
					Console.WriteLine("No models defined in Root.");
					return null;
				}

				ExtractM2s(wmo, root, tileModels);
			}

			return tileModels;
		}

		private static void ExtractM2s(MapObjectDefinition wmo, WMORoot root, TileObjects<M2Object> tileObjects)
		{
			var setIndices = new List<int> { 0 };
			if (wmo.DoodadSet != 0) setIndices.Add(wmo.DoodadSet);

			foreach (var index in setIndices)
			{
				var set = root.DoodadSets[index];
				for (var k = 0; k < set.InstanceCount; k++)
				{
					var dd = root.DoodadDefinitions[(int)set.FirstInstanceIndex + k];
					if (string.IsNullOrEmpty(dd.FilePath)) continue;

					var model = ExtractM2Object(wmo, dd);

					if (model.Vertices.Length < 1) continue;
					tileObjects.Add(model);
				}
			}
		}

		private static TileObjects<M2Object> ExtractTileM2s(int tileX, int tileY, ADTFile adt)
		{
			var list = adt.DoodadDefinitions;

			var objs = new TileObjects<M2Object>();

			foreach (var definition in list)
			{
				if (LoadedM2Ids.Contains(definition.UniqueId)) continue;
				if (string.IsNullOrEmpty(definition.FilePath)) continue;

				var obj = ExtractM2Object(definition);

				if (obj.Vertices.Length < 1) continue;
				objs.Add(obj);

				LoadedM2Ids.Add(definition.UniqueId);
			}

			var wmoList = adt.ObjectDefinitions;
			foreach (var wmo in wmoList)
			{
				if (LoadedWMOIds.Contains(wmo.UniqueId)) continue;

				var worldPos = new Vector3(TerrainConstants.CenterPoint - wmo.Position.X,
										   wmo.Position.Y,
										   TerrainConstants.CenterPoint - wmo.Position.Z);
				worldPos = CorrectWMOOrigin(worldPos);

				// If this WMO belongs to another tile, skip it.
				var wmoTileX = (int)((TerrainConstants.CenterPoint - worldPos.Y) / TerrainConstants.TileSize);
				var wmoTileY = (int)((TerrainConstants.CenterPoint - worldPos.X) / TerrainConstants.TileSize);
				if (wmoTileX != tileX || wmoTileY != tileY) continue;

				var root = WMORootParser.Process(WDTParser.MpqManager, wmo.FileName);

				ExtractM2s(wmo, root, objs);
				LoadedWMOIds.Add(wmo.UniqueId);
			}

			return objs;
		}

		private static M2Object ExtractM2Object(MapDoodadDefinition definition)
		{
			var filePath = definition.FilePath;
			var m2model = ExtractM2Model(filePath);

			return TransformM2Model(definition, m2model);
		}

		private static M2Object ExtractM2Object(MapObjectDefinition wmo, DoodadDefinition definition)
		{
			var filePath = definition.FilePath;
			var m2model = ExtractM2Model(filePath);

			return TransformM2Model(wmo, definition, m2model);
		}

		private static M2Model ExtractM2Model(string filePath)
		{
			if (Path.GetExtension(filePath).ToLower().Equals(".mdx") ||
				Path.GetExtension(filePath).ToLower().Equals(".mdl"))
			{
				filePath = filePath.Substring(0, filePath.LastIndexOf('.')) + ".m2";
			}

			var m2model = M2ModelParser.Process(WDTParser.MpqManager, filePath);
			if (m2model == null)
			{
				Console.WriteLine("Invalid M2Model returned.");
				return null;
			}
			return m2model;
		}

        /// <summary>
        /// Transforms an M2s vertices into World Coordinates
        /// This function is for M2s present on the world map
        /// </summary>
        private static M2Object TransformM2Model(MapDoodadDefinition definition, M2Model m2Model)
		{
			var model = new M2Object
			{
				Vertices = new Vector3[m2Model.BoundingVertices.Length]
			};

			// Rotate
			var origin = new Vector3(
				(TerrainConstants.CenterPoint - definition.Position.X),
				definition.Position.Y,
				(TerrainConstants.CenterPoint - definition.Position.Z)
			);

			for (var i = 0; i < model.Vertices.Length; i++)
			{
				// Reverse the previos coord transform for the rotation.
				var vector = TransformToIntermediateCoords(m2Model.BoundingVertices[i]);

				// Create the rotation matrices
				var rotateX = Matrix.CreateRotationX(definition.OrientationC * RadiansPerDegree);
				var rotateY = Matrix.CreateRotationY((definition.OrientationB - 90) * RadiansPerDegree);
				var rotateZ = Matrix.CreateRotationZ(-definition.OrientationA * RadiansPerDegree);

				// Rotate the vector
				var rotatedVector = Vector3.Transform(vector, rotateY);
				rotatedVector = Vector3.Transform(rotatedVector, rotateZ);
				rotatedVector = Vector3.Transform(rotatedVector, rotateX);


				var finalVector = rotatedVector + origin;

				model.Vertices[i] = finalVector;
			}


			// Add the triangle indices to the model
			model.Triangles = new Index3[m2Model.BoundingTriangles.Length];
			for (var i = 0; i < m2Model.BoundingTriangles.Length; i++)
			{
				var tri = m2Model.BoundingTriangles[i];
				model.Triangles[i] = new Index3
				{
					Index0 = (short)tri[2],
					Index1 = (short)tri[1],
					Index2 = (short)tri[0]
				};
			}

			// Calculate the boundingbox
			model.Bounds = new BoundingBox(model.Vertices);

			return model;
		}

        /// <summary>
        /// Transforms an M2s vertices into Building Coordinates
        /// This function is for M2s which decorate buildings
        /// </summary>
        private static M2Object TransformM2Model(MapObjectDefinition wmo, DoodadDefinition definition, M2Model m2Model)
		{
			var model = new M2Object
			{
				Vertices = new Vector3[m2Model.BoundingVertices.Length]
			};

			var origin = new Vector3(-definition.Position.X, definition.Position.Z, definition.Position.Y);
			var wmoOrigin = CorrectWMOOrigin(wmo.Position);
			var rotation = definition.Rotation;
			var rotateY = Matrix.CreateRotationY((wmo.OrientationB - 90) * RadiansPerDegree);
			for (var i = 0; i < m2Model.BoundingVertices.Length; i++)
			{
				var vector = m2Model.BoundingVertices[i];
				var rotatedVector = Vector3.Transform(vector, rotation);
				rotatedVector = TransformToIntermediateCoords(rotatedVector);
				var finalModelVector = rotatedVector + origin;

				var rotatedWMOVector = Vector3.Transform(finalModelVector, rotateY);
				var finalWMOVector = rotatedWMOVector + wmoOrigin;

				model.Vertices[i] = finalWMOVector;
			}

			// Add the triangle indices to the model
			model.Triangles = new Index3[m2Model.BoundingTriangles.Length];
			for (var i = 0; i < m2Model.BoundingTriangles.Length; i++)
			{
				var tri = m2Model.BoundingTriangles[i];
				model.Triangles[i] = new Index3
				{
					Index0 = (short)tri[2],
					Index1 = (short)tri[1],
					Index2 = (short)tri[0]
				};
			}

			// Calculate the boundingbox
			model.Bounds = new BoundingBox(model.Vertices);

			return model;
		}

		private static Vector3 CorrectWMOOrigin(Vector3 origin)
		{
			var posX = TerrainConstants.CenterPoint - origin.X;
			var posZ = TerrainConstants.CenterPoint - origin.Z;

			return new Vector3(posX, origin.Y, posZ);
		}

		private static Vector3 TransformToIntermediateCoords(Vector3 vector)
		{
			var newX = -vector.X;
			var newZ = vector.Y;
			var newY = vector.Z;

			return new Vector3(newX, newY, newZ);
		}
		#endregion

		#region M2 Write
		private static void WriteTileM2s(Stream file, ICollection<M2Object> models)
		{
			if (models == null)
			{
				Console.WriteLine("Cannot write null TileModels to file.");
				return;
			}

			var writer = new BinaryWriter(file);
			writer.Write(fileType);

			writer.Write(models.Count);
			foreach (var model in models)
			{
				WriteModel(writer, model);
			}
		}

		private static void WriteModel(BinaryWriter writer, M2Object m2Object)
		{
			if (m2Object == null)
			{
				Console.WriteLine("Cannot write null Building to file.");
				return;
			}

			writer.Write(m2Object.Bounds);

			writer.Write(m2Object.Vertices.Length);
			for (var i = 0; i < m2Object.Vertices.Length; i++)
			{
				writer.Write(m2Object.Vertices[i]);
			}

			writer.Write(m2Object.Triangles.Length);
			for (var i = 0; i < m2Object.Triangles.Length; i++)
			{
				writer.Write(m2Object.Triangles[i]);
			}
		}
		#endregion

		#region WMO Read

		public static TileObjects<WMO> ExtractWDTWMOs(List<MapObjectDefinition> wmos)
		{
			var buildings = new TileObjects<WMO>(wmos.Count);

			for (var i = 0; i < wmos.Count; i++)
			{
				var wmo = wmos[i];
				if (LoadedWMOIds.Contains(wmo.UniqueId)) continue;

				var filePath = wmo.FileName;
				var building = ExtractWMO(filePath);

				// Correct the World Positioning
				var calcExtents = GetWMOBounds(wmo, building.BuildingGroups);
				var worldExtents = ToWorldCoords(calcExtents);

				building.Bounds = worldExtents;
				building.RotationModelY = wmo.OrientationB;
				var worldPos = new Vector3(TerrainConstants.CenterPoint - wmo.Position.X,
										   wmo.Position.Y,
										   TerrainConstants.CenterPoint - wmo.Position.Z);
				building.WorldPos = ToWorldCoords(ref worldPos);

				buildings.Add(building);

				LoadedWMOIds.Add(wmo.UniqueId);
			}

			return buildings;
		}

		public static TileObjects<WMO> ExtractTileWMOs(int tileX, int tileY, ADTFile adt)
		{
			var wmos = adt.ObjectDefinitions;
			var buildings = new TileObjects<WMO>(wmos.Count);

			for (var i = 0; i < wmos.Count; i++)
			{
				var wmo = wmos[i];
				if (LoadedWMOIds.Contains(wmo.UniqueId)) continue;
				var worldPos = new Vector3(TerrainConstants.CenterPoint - wmo.Position.X,
										   wmo.Position.Y,
										   TerrainConstants.CenterPoint - wmo.Position.Z);
				worldPos = ToWorldCoords(ref worldPos);

				// If this WMO belongs to another tile, skip it.
				var wmoTileX = (int)((TerrainConstants.CenterPoint - worldPos.Y) / TerrainConstants.TileSize);
				var wmoTileY = (int)((TerrainConstants.CenterPoint - worldPos.X) / TerrainConstants.TileSize);
				if (wmoTileX != tileX || wmoTileY != tileY) continue;

				var filePath = wmo.FileName;
				var building = ExtractWMO(filePath);

				// Correct the World Positioning
				var calcExtents = GetWMOBounds(wmo, building.BuildingGroups);
				var worldExtents = ToWorldCoords(calcExtents);

				building.Bounds = worldExtents;
				building.RotationModelY = wmo.OrientationB;
				building.WorldPos = worldPos;

				buildings.Add(building);

				LoadedWMOIds.Add(wmo.UniqueId);
			}

			return buildings;
		}

		public static WMO ExtractWMO(string path)
		{
			var root = WMORootParser.Process(WDTParser.MpqManager, path);

			if (root == null)
			{
				Console.WriteLine("Invalid WMORoot returned.");
				return null;
			}

			var building = new WMO
			{
				BuildingGroups = new WMOSubGroup[root.Header.GroupCount]
			};

			building.BuildingGroups = ExtractGroups(path, root.Header.GroupCount, root);


			return building;
		}

		public static WMOSubGroup[] ExtractGroups(string path, uint count, WMORoot root)
		{
			var groups = new WMOSubGroup[count];
			for (var wmoGroup = 0; wmoGroup < count; wmoGroup++)
			{
				var newFile = path.Substring(0, path.LastIndexOf('.'));
				var filePath = String.Format("{0}_{1:000}.wmo", newFile, wmoGroup);

				var group = WMOGroupParser.Process(WDTParser.MpqManager, filePath, root, wmoGroup);

				var bounds = GetGroupBounds(group.Verticies);

				var newGroup = new WMOSubGroup
				{
					Bounds = bounds,
					Vertices = group.Verticies.ToArray(),
					Tree = new BSPTree(group.BSPNodes)
				};

				groups[wmoGroup] = newGroup;
			}
			return groups;
		}

		private static BoundingBox ToWorldCoords(BoundingBox boundingBox)
		{
			var newMin = ToWorldCoords(ref boundingBox.Min);
			var newMax = ToWorldCoords(ref boundingBox.Max);
			return new BoundingBox(Vector3.Min(newMin, newMax), Vector3.Max(newMin, newMax));
		}

		private static Vector3 ToWorldCoords(ref Vector3 vec)
		{
			var newX = vec.Z;
			var newY = vec.X;
			var newZ = vec.Y;

			return new Vector3(newX, newY, newZ);
		}

		private static BoundingBox GetWMOBounds(MapObjectDefinition wmoDef, IEnumerable<WMOSubGroup> groups)
		{
			if (groups == null) return BoundingBox.INVALID;

			var boxen = new List<BoundingBox>();
			foreach (var buildingGroup in groups)
			{
				boxen.Add(buildingGroup.Bounds);
			}
			var newBox = GetOuterBounds(boxen.ToArray());

			// Translate the world position
			var worldCenter = new Vector3(TerrainConstants.CenterPoint - wmoDef.Position.X,
									 wmoDef.Position.Y,
									 TerrainConstants.CenterPoint - wmoDef.Position.Z);

			// Rotate to the model's final position
			var rotateY = Matrix.CreateRotationY((wmoDef.OrientationB - 90) * RadiansPerDegree);
			var rotMin = Vector3.Transform(newBox.Min, rotateY) + worldCenter;
			var rotMax = Vector3.Transform(newBox.Max, rotateY) + worldCenter;

			// Make a new axis-aligned bounding box
			var newMin = Vector3.Min(rotMin, rotMax);
			var newMax = Vector3.Max(rotMin, rotMax);

			return new BoundingBox(newMin, newMax);
		}

		private static BoundingBox GetOuterBounds(BoundingBox[] boxen)
		{
			if (boxen == null)
				return BoundingBox.INVALID;
			if (boxen.Length < 1)
				return BoundingBox.INVALID;
			if (boxen.Length < 2)
				return boxen[0];

			var newBox = BoundingBox.Join(ref boxen[0], ref boxen[1]);
			for (var i = 2; i < boxen.Length; i++)
			{
				newBox = BoundingBox.Join(ref newBox, ref boxen[i]);
			}
			return newBox;
		}

		private static BoundingBox GetGroupBounds(IList<Vector3> vertices)
		{
			var min = new Vector3(float.MaxValue);
			var max = new Vector3(float.MinValue);

			for (var i = 0; i < vertices.Count; i++)
			{
				var vertex = vertices[i];
				min = Vector3.Min(vertex, min);
				max = Vector3.Max(vertex, max);
			}

			return new BoundingBox(min, max);
		}
		#endregion



		#region WMO Write
		public static void ExtractWMOs(WDTFile wdt)
		{
			var wmos = ExtractObjects<WMO>(wdt, ExtractWDTWMOs, ExtractTileWMOs);
			LoadedWMOIds.Clear();

			var max = wmos.HasTiles ? TerrainConstants.TilesPerMapSide : 1;
			for (var tileY = 0; tileY < max; tileY++)
			{
				for (var tileX = 0; tileX < max; tileX++)
				{
					//if (tileX != 39) continue;
					//if (tileY != 27) continue;

					if (wmos.ObjectsByTile[tileX, tileY] == null) continue;

					// Don't bother writing tiles with no buildings.
					var tileBuildings = wmos.ObjectsByTile[tileX, tileY];
					if (tileBuildings.Count == 0) continue;

					var path = Path.Combine(ToolConfig.WMODir, wdt.Entry.Id.ToString());
					if (!Directory.Exists(path))
						Directory.CreateDirectory(path);
					var fileName = TerrainConstants.GetWMOFile(tileX, tileY);
					var file = File.Create(Path.Combine(path, fileName));

					WriteTileBuildings(file, tileBuildings);

					file.Close();
				}
			}
		}

		private static void WriteTileBuildings(Stream file, TileObjects<WMO> tileBuildings)
		{
			if (tileBuildings == null)
			{
				Console.WriteLine("Cannot write null TileBuildings to file.");
				return;
			}

			// The magic fileType
			var writer = new BinaryWriter(file);
			writer.Write(fileType);

			// Number of Buildings we're storing
			writer.Write(tileBuildings.Count);
			foreach (var building in tileBuildings)
			{
				WriteBuilding(writer, building);
			}
		}

		private static void WriteBuilding(BinaryWriter writer, WMO wmo)
		{
			if (wmo == null)
			{
				Console.WriteLine("Cannot write null Building to file.");
				return;
			}

			//BoundingBox, store it
			writer.Write(wmo.Bounds);

			// Rotation Degrees, store them
			writer.Write(wmo.RotationModelY);

			// WorldPos, store it
			writer.Write(wmo.WorldPos);

			// Write the number of Groups
			writer.Write(wmo.BuildingGroups.Length);
			for (var i = 0; i < wmo.BuildingGroups.Length; i++)
			{
				WriteBuildingGroup(writer, wmo.BuildingGroups[i]);
			}
		}

		private static void WriteBuildingGroup(BinaryWriter writer, WMOSubGroup buildingGroup)
		{
			if (buildingGroup == null)
			{
				Console.WriteLine("Cannot write null BuildingGroup to file.");
				return;
			}

			// Store the Group's Bounds
			writer.Write(buildingGroup.Bounds);

			writer.Write(buildingGroup.Vertices.Length);
			for (var i = 0; i < buildingGroup.Vertices.Length; i++)
			{
				writer.Write(buildingGroup.Vertices[i]);
			}

			WriteBSPTree(writer, buildingGroup.Tree);
		}

		private static void WriteBSPTree(BinaryWriter writer, BSPTree tree)
		{
			writer.Write(tree.rootId);

			writer.Write(tree.nodes.Length);
			for (var i = 0; i < tree.nodes.Length; i++)
			{
				WriteBSPNode(writer, tree.nodes[i]);
			}
		}

		private static void WriteBSPNode(BinaryWriter writer, BSPNode node)
		{
			writer.Write((byte)node.flags);
			writer.Write(node.negChild);
			writer.Write(node.posChild);
			writer.Write(node.planeDist);

			if (node.TriIndices != null)
			{
				writer.Write(node.TriIndices.Length);
				for (var i = 0; i < node.TriIndices.Length; i++)
				{
					writer.Write(node.TriIndices[i]);
				}
			}
			else
			{
				// Consider a null TriIndex array to have 0 length.
				writer.Write(0);
			}
		}
		#endregion
	}

	public class RegionWMOs : RegionObjects<WMO>
	{
	}

	public class RegionObjects<O>
	{
		public bool HasTiles = true;
		public TileObjects<O>[,] ObjectsByTile;
	}

	public class TileObjects<O> : List<O>
	{
		public TileObjects()
		{
		}

		public TileObjects(int capacity)
			: base(capacity)
		{
		}

		public TileObjects(IEnumerable<O> collection)
			: base(collection)
		{
		}
	}

	public class M2Object
	{
		public BoundingBox Bounds;
		public Vector3[] Vertices;
		public Index3[] Triangles;
	}

	public class WMO
	{
		public BoundingBox Bounds;
		public float RotationModelY;
		public Vector3 WorldPos;
		public WMOSubGroup[] BuildingGroups;
	}

	public class WMOSubGroup
	{
		public BoundingBox Bounds;
		public Vector3[] Vertices;
		public BSPTree Tree;
	}
}
