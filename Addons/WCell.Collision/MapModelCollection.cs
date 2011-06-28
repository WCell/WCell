using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WCell.Collision.Addon;
using WCell.Constants;
using WCell.Constants.World;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.Collision
{
	/// <summary>
	/// Represents all models within a single map
	/// </summary>
	public class MapModelCollection
	{
		private const string fileType = "m2x";
		private readonly bool[,] tileLoaded;
		private readonly bool[,] tileInvalid;
		
		private readonly TreeReference<M2> Tree;

		public readonly MapId MapId;
		private readonly BoundingBox BoundingBox;

		public MapModelCollection(MapId map)
		{
			MapId = map;
			BoundingBox = World.GetMapBoundingBox(MapId);
			Tree = new TreeReference<M2>(new QuadTree<M2>(BoundingBox));
			tileLoaded = new bool[TerrainConstants.TilesPerMapSide,TerrainConstants.TilesPerMapSide];
			tileInvalid = new bool[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];
		}

		public float? GetModelHeight(Vector3 pos)
		{
			// Start at the character's head and send a ray pointed at the floor
			var startPos = pos + Vector3.Up * 2.0f; // what should the scalar be?
			var endPos = startPos + Vector3.Down * 2000.0f; // what should the scalar be?

			var models = GetPotentialColliders(ref startPos, ref endPos);
			if (models.Count == 0) return null;

			float tMax;
			var ray = CollisionHelper.CreateRay(startPos, endPos, out tMax);

			var dist = ShortestDistanceTo(models, tMax, ray);
			if (dist == null) return null;

			return (startPos.Z + Vector3.Down.Z * dist.Value);
		}

		internal bool HasLOS(Vector3 startPos, Vector3 endPos)
		{
			return !CheckIfCollides(startPos, endPos);
		}

		private bool CheckIfCollides(Vector3 startPos, Vector3 endPos)
		{
			var models = GetPotentialColliders(ref startPos, ref endPos);

			// No models intersect this footprint
			if (models.Count == 0) return false;

			float tMax;
			var ray = CollisionHelper.CreateRay(startPos, endPos, out tMax);

			return CollidesWithRay(models, tMax, ray);
		}

		private static bool CollidesWithRay(IList<M2> models, float tMax, Ray ray)
		{
			for (var i = 0; i < models.Count; i++)
			{
				var collides = models[i].CollidesWithRay(ref ray, ref tMax);
				if (!collides) continue;
				return true;
			}

			return false;
		}

		internal float? GetCollisionDistance(Vector3 startPos, Vector3 endPos)
		{
			var models = GetPotentialColliders(ref startPos, ref endPos);

			// No models intersect this footprint
			if (models.Count == 0) return null;

			float tMax;
			var ray = CollisionHelper.CreateRay(startPos, endPos, out tMax);

			return ShortestDistanceTo(models, tMax, ray);
		}

		private static float? ShortestDistanceTo(IList<M2> models, float tMax, Ray ray)
		{
			float? result = tMax;
			for (var i = 0; i < models.Count; i++)
			{
				var newResult = models[i].ShortestDistanceTo(ref ray, ref tMax);
				if (newResult == null) continue;

				result = Math.Min(result.Value, newResult.Value);
			}

			return result < tMax ? result : null;
		}

		#region Load Data
		private List<M2> GetPotentialColliders(ref Vector3 startPos, ref Vector3 endPos)
		{
			var startCoord = LocationHelper.GetTileXYForPos(startPos);
			var endCoord = LocationHelper.GetTileXYForPos(endPos);

			EnsureGroupLoaded(startCoord);
			if (startCoord != endCoord)
			{
				EnsureGroupLoaded(endCoord);
			}

			var footPrint = MakeBounds(ref startPos, ref endPos);
			return Tree.Tree.Query(footPrint);
		}

		/// <summary>
		/// Makes sure that the given tile and all it's neighbors are loaded
		/// </summary>
		private bool EnsureGroupLoaded(TileCoord tileCoord)
		{
			var result = true;

			var tile = new TileCoord
			{
				TileX = tileCoord.TileX,
				TileY = tileCoord.TileY
			};
			result = EnsureTileLoaded(tile);

			tile.TileX = tileCoord.TileX - 1;
			result = result && EnsureTileLoaded(tile);

			tile.TileX = tileCoord.TileX + 1;
			result = result && EnsureTileLoaded(tile);

			tile.TileX = tileCoord.TileX;
			tile.TileY = tileCoord.TileY - 1;
			result = result && EnsureTileLoaded(tile);

			tile.TileY = tileCoord.TileY + 1;
			result = result && EnsureTileLoaded(tile);

			return result;
		}

		private bool EnsureTileLoaded(TileCoord tile)
		{
			if (IsTileLoaded(tile) || NoTile(tile)) return true;
			
			if (!LoadTile(tile))
			{
				tileInvalid[tile.TileX, tile.TileY] = true;
				return false;
			}
			return true;
		}

		private bool LoadTile(TileCoord tileCoord)
		{
			var dir = Path.Combine(WorldMap.HeightMapFolder, ((int)MapId).ToString());
			if (!Directory.Exists(dir)) return false;

			var fileName = String.Format("{0}{1}.fub", tileCoord.TileX, tileCoord.TileY);
			var fullPath = Path.Combine(dir, fileName);
			if (!File.Exists(fullPath)) return false;

			return LoadTileModels(fullPath);
		}

		private bool IsTileLoaded(TileCoord tileCoord)
		{
			return tileLoaded[tileCoord.TileX, tileCoord.TileY];
		}

		private bool NoTile(TileCoord tileCoord)
		{
			return tileInvalid[tileCoord.TileX, tileCoord.TileY];
		}

		private string GenerateKey(MapId map, TileCoord tileCoord)
		{
			return String.Format("{0}_{1}_{2}", (int)map, tileCoord.TileX, tileCoord.TileY);
		}

		private static BoundingBox MakeBounds(ref Vector3 startPos, ref Vector3 endPos)
		{
			var newMin = Vector3.Min(startPos, endPos);
			var newMax = Vector3.Max(startPos, endPos);

			return new BoundingBox(newMin, newMax);
		}

		private bool LoadTileModels(string filePath)
		{
			using (var file = File.OpenRead(filePath))
			using (var br = new BinaryReader(file))
			{
				var key = br.ReadString();
				if (key != fileType)
				{
					Console.WriteLine("Invalid file format, suckah!");
				}

				ReadModels(br, Tree);

				br.Close();
			}


			return true;
		}

		private static void ReadModels(BinaryReader br, TreeReference<M2> tree)
		{
			var numModels = br.ReadInt32();
			for (var i = 0; i < numModels; i++)
			{
				var bounds = br.ReadBoundingBox();

				Vector3[] vertices;
				var numVertices = br.ReadInt32();
				if (numVertices > 0)
				{
					vertices = new Vector3[numVertices];
					for (var j = 0; j < numVertices; j++)
					{
						vertices[j] = br.ReadVector3();
					}
				}
				else
				{
					vertices = null;
				}

				Index3[] indices;
				var numIndices = br.ReadInt32();
				if (numIndices > 0)
				{
					indices = new Index3[numIndices];
					for (var j = 0; j < numIndices; j++)
					{
						indices[j] = br.ReadIndex3();
					}
				}
				else
				{
					indices = null;
				}

				var model = new M2
				{
					Bounds = bounds,
					Vertices = vertices,
					Triangles = indices
				};

				tree.Tree.Insert(model);
			}
		}
		#endregion
	}


}