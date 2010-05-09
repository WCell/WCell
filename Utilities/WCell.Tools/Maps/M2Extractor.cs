using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using WCell.Constants;
using WCell.MPQTool;
using WCell.Tools.Maps.Parsing;
using WCell.Tools.Maps.Parsing.ADT;
using WCell.Util.Graphics;
using WCell.Util.Toolshed;

namespace WCell.Tools.Maps
{
	public static class M2Extractor
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private static MpqManager manager;
		private const string baseDir = "WORLD\\maps\\";
		private const string fileType = "m2x";
		private static List<uint> LoadedM2Ids;
		private const float RadiansPerDegree = 0.0174532925f;

		[Tool]
		public static void WriteM2Files()
		{
			log.Info("Extracting partial M2 world objects - This might take a few minutes.....");
			var startTime = DateTime.Now;
			var wowRootDir = ToolConfig.Instance.GetWoWDir();
			var outputDir = ToolConfig.M2Dir;

			manager = new MpqManager(wowRootDir);

			var entryList = DBCMapReader.GetMapEntries();
			if (entryList == null)
			{
				log.Error("Could not retrieve MapEntries.");
				return;
			}

			foreach (var mapEntry in entryList)
			{
				//if (mapEntry.Id != (int)MapId.EasternKingdoms) continue;

				var dir = mapEntry.MapDirName;
				var wdtDir = Path.Combine(baseDir, dir);
				var wdtName = dir;

				var wdt = WDTParser.Process(manager, wdtDir, wdtName);
				if (wdt == null) continue;

				var regionM2s = ExtractRegionM2s(wdt);

				var count = 0;
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
						if (tileModels.Models.Count == 0) continue;

						var path = Path.Combine(outputDir, mapEntry.Id.ToString());
						if (!Directory.Exists(path))
							Directory.CreateDirectory(path);
						var file = File.Create(Path.Combine(path, TerrainConstants.GetM2File(tileX, tileY)));

						WriteTileM2s(file, tileModels);
						count++;
						file.Close();
					}
				}
				log.Info(count);
			}

			log.Info("Done ({0})", DateTime.Now - startTime);

		}

		#region Extract
		private static RegionModels ExtractRegionM2s(WDTFile wdt)
		{
			LoadedM2Ids = new List<uint>(250);
			RegionModels models;

			if (wdt == null) return null;
			if ((wdt.Header.Header1 & WDTFlags.GlobalWMO) != 0)
			{
				// No terrain, load the global WMO
				if (wdt.ObjectDefinitions == null) return null;

				models = new RegionModels
				{
					HasTiles = false,
					ObjectsByTile = new TileModels[1, 1]
				};

				models.ObjectsByTile[0, 0] = ExtractWDTM2s(wdt.ObjectDefinitions);

				LoadedM2Ids.Clear();
				return models;
			}


			models = new RegionModels
			{
				HasTiles = true
			};

			for (var tileY = 0; tileY < TerrainConstants.TilesPerMapSide; tileY++)
			{
				for (var tileX = 0; tileX < TerrainConstants.TilesPerMapSide; tileX++)
				{
					//if (tileX != 36) continue;
					//if (tileY != 49) continue;

					if (!wdt.TileProfile[tileX, tileY]) continue;
					var adtName = string.Format("{0}_{1:00}_{2:00}", wdt.Name, tileX, tileY);
					var adt = ADTParser.Process(manager, wdt.Path, adtName);
					if (adt == null) continue;

					models.ObjectsByTile[tileX, tileY] = ExtractTileM2s(tileX, tileY, adt);
				}
			}

			LoadedM2Ids.Clear();
			return models;
		}

		private static TileModels ExtractWDTM2s(IList<MapObjectDefinition> wmos)
		{
			var LoadedWMOIds = new List<uint>();
			var tileModels = new TileModels
			{
				Models = new List<TileModel>()
			};

			for (var i = 0; i < wmos.Count; i++)
			{
				var wmo = wmos[i];
				if (LoadedWMOIds.Contains(wmo.UniqueId)) continue;

				var filePath = wmo.FileName;
				var root = WMORootParser.Process(manager, filePath);
				if (root == null)
				{
					Console.WriteLine("Invalid WMORoot returned.");
					return null;
				}

				LoadedWMOIds.AddUnique(wmo.UniqueId);
				if (root.DoodadDefinitions == null)
				{
					Console.WriteLine("No models defined in Root.");
					return null;
				}

				ExtractWMOM2s(wmo, root, tileModels);
			}

			return tileModels;
		}

		private static void ExtractWMOM2s(MapObjectDefinition wmo, WMORoot root, TileModels tileModels)
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

					var model = ExtractModel(wmo, dd);

					if (model.Vertices.Length < 1) continue;
					tileModels.Models.Add(model);
				}
			}
		}

		private static TileModels ExtractTileM2s(int tileX, int tileY, ADTFile adt)
		{
			var LoadedWMOIds = new List<uint>();
			var list = adt.DoodadDefinitions;

			var tileModels = new TileModels
			{
				Models = new List<TileModel>(list.Count)
			};

			foreach (var definition in list)
			{
				if (LoadedM2Ids.Contains(definition.UniqueId)) continue;
				if (string.IsNullOrEmpty(definition.FilePath)) continue;

				var model = ExtractModel(definition);

				if (model.Vertices.Length < 1) continue;
				tileModels.Models.Add(model);

				LoadedM2Ids.AddUnique(definition.UniqueId);
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

				var filePath = wmo.FileName;
				var root = WMORootParser.Process(manager, filePath);

				ExtractWMOM2s(wmo, root, tileModels);
				LoadedWMOIds.AddUnique(wmo.UniqueId);
			}

			return tileModels;
		}

		private static TileModel ExtractModel(MapDoodadDefinition definition)
		{
			var filePath = definition.FilePath;
			var m2model = ExtractM2Model(filePath);

			return TransformM2Model(definition, m2model);
		}

		private static TileModel ExtractModel(MapObjectDefinition wmo, DoodadDefinition definition)
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

			var m2model = M2ModelParser.Process(manager, filePath);
			if (m2model == null)
			{
				Console.WriteLine("Invalid M2Model returned.");
				return null;
			}
			return m2model;
		}

		private static TileModel TransformM2Model(MapDoodadDefinition definition, M2Model m2model)
		{
			var model = new TileModel
			{
				Vertices = new Vector3[m2model.BoundingVertices.Length]
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
				var vector = TransformToIntermediateCoords(m2model.BoundingVertices[i]);

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
			model.Triangles = new Index3[m2model.BoundingTriangles.Length];
			for (var i = 0; i < m2model.BoundingTriangles.Length; i++)
			{
				var tri = m2model.BoundingTriangles[i];
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

		private static TileModel TransformM2Model(MapObjectDefinition wmo, DoodadDefinition definition, M2Model m2model)
		{
			var model = new TileModel
			{
				Vertices = new Vector3[m2model.BoundingVertices.Length]
			};

			var origin = new Vector3(-definition.Position.X, definition.Position.Z, definition.Position.Y);
			var wmoOrigin = CorrectWMOOrigin(wmo.Position);
			var rotation = definition.Rotation;
			var rotateY = Matrix.CreateRotationY((wmo.OrientationB - 90) * RadiansPerDegree);
			for (var i = 0; i < m2model.BoundingVertices.Length; i++)
			{
				var vector = m2model.BoundingVertices[i];
				var rotatedVector = Vector3.Transform(vector, rotation);
				rotatedVector = TransformToIntermediateCoords(rotatedVector);
				var finalModelVector = rotatedVector + origin;

				var rotatedWMOVector = Vector3.Transform(finalModelVector, rotateY);
				var finalWMOVector = rotatedWMOVector + wmoOrigin;

				model.Vertices[i] = finalWMOVector;
			}

			// Add the triangle indices to the model
			model.Triangles = new Index3[m2model.BoundingTriangles.Length];
			for (var i = 0; i < m2model.BoundingTriangles.Length; i++)
			{
				var tri = m2model.BoundingTriangles[i];
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

		#region Write
		private static void WriteTileM2s(Stream file, TileModels models)
		{
			if (models == null)
			{
				Console.WriteLine("Cannot write null TileModels to file.");
				return;
			}

			var writer = new BinaryWriter(file);
			writer.Write(fileType);

			writer.Write(models.Models.Count);
			foreach (var model in models.Models)
			{
				WriteModel(writer, model);
			}
		}

		private static void WriteModel(BinaryWriter writer, TileModel tileModel)
		{
			if (tileModel == null)
			{
				Console.WriteLine("Cannot write null Building to file.");
				return;
			}

			writer.Write(tileModel.Bounds);

			writer.Write(tileModel.Vertices.Length);
			for (var i = 0; i < tileModel.Vertices.Length; i++)
			{
				writer.Write(tileModel.Vertices[i]);
			}

			writer.Write(tileModel.Triangles.Length);
			for (var i = 0; i < tileModel.Triangles.Length; i++)
			{
				writer.Write(tileModel.Triangles[i]);
			}
		}
		#endregion
	}

	public class RegionModels
	{
		public bool HasTiles = true;
		public TileModels[,] ObjectsByTile = new TileModels[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];
	}

	public class TileModels
	{
		public List<TileModel> Models;
	}

	public class TileModel
	{
		public BoundingBox Bounds;
		public Vector3[] Vertices;
		public Index3[] Triangles;
	}
}
