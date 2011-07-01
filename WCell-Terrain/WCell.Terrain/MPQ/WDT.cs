using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.World;
using WCell.MPQTool;
using WCell.Terrain.MPQ.ADTs;
using WCell.Terrain.MPQ.DBC;
using WCell.Terrain.MPQ.WMOs;
using WCell.Terrain.Serialization;
using WCell.Util;
using WCell.Util.Graphics;
using System.IO;

namespace WCell.Terrain.MPQ
{
	/// <summary>
	/// Represents a WDT entity and all it's associated data
	/// </summary>
	public class WDT : Terrain
	{
		public MapInfo Entry;

		public int Version;
		public string Name;
		public string Filename;
		public readonly WDTHeader Header = new WDTHeader();

		public readonly List<string> WmoFiles = new List<string>();
		public readonly List<MapObjectDefinition> WmoDefinitions = new List<MapObjectDefinition>();

		public M2[] M2s;
		public WMORoot[] WMOs;

		public WDT(MPQFinder finder, MapId mapId)
			: base(mapId)
		{
			Finder = finder;
		}

		public MPQFinder Finder
		{
			get;
			private set;
		}

		public override bool IsWMOOnly
		{
			get { return Header.IsWMOMap; }
		}

		protected override TerrainTile LoadTile(Point2D tileCoord)
		{
			var adt = ADTReader.ReadADT(Finder, this, tileCoord);

			WMOs = new WMORoot[adt.DoodadDefinitions.Count];
			for (var i = 0; i < adt.ObjectDefinitions.Count; i++)
			{
				var definition = adt.ObjectDefinitions[i];
				WMOs[i] = ReadWMO(definition);
			}

			M2s = new M2[adt.DoodadDefinitions.Count];
			for (var i = 0; i < adt.DoodadDefinitions.Count; i++)
			{
				var doodadDef = adt.DoodadDefinitions[i];
				M2s[i] = ReadM2(doodadDef);
			}

			adt.GenerateHeightVertexAndIndices();
			adt.GenerateLiquidVertexAndIndices();
			adt.BuildQuadTree();

			return adt;
		}

		/// <summary>
		/// Creates a dummy WDT and loads the given tile
		/// </summary>
		public static TerrainTile LoadTile(MapId map, Point2D coords)
		{
			var wdt = new WDT(MPQFinder.GetDefaultFinder(WCellTerrainSettings.WoWPath), map);
			wdt.TileProfile[coords.X, coords.Y] = true;
			return wdt.LoadTile(coords);
		}


		#region M2
		private M2 ReadM2(MapDoodadDefinition doodadDefinition)
		{
			var filePath = doodadDefinition.FilePath;
			var ext = Path.GetExtension(filePath);

			if (ext.Equals(".mdx") ||
				ext.Equals(".mdl"))
			{
				filePath = Path.ChangeExtension(filePath, ".m2");
			}

			var model = M2Reader.ReadM2(Finder, doodadDefinition.FilePath);

			var tempIndices = new List<int>();
			foreach (var tri in model.BoundingTriangles)
			{
				tempIndices.Add(tri.Index2);
				tempIndices.Add(tri.Index1);
				tempIndices.Add(tri.Index0);
			}

			var currentM2 = TransformM2(model, tempIndices, doodadDefinition);
			var tempVertices = currentM2.Vertices;
			var bounds = new BoundingBox(tempVertices.ToArray());
			currentM2.Bounds = bounds;

			return currentM2;
		}

		public static M2 TransformM2(M2Model model, IEnumerable<int> indicies, MapDoodadDefinition mddf)
		{
			var currentM2 = new M2();
			currentM2.Vertices.Clear();
			currentM2.Indices.Clear();

			var posX = (mddf.Position.X - TerrainConstants.CenterPoint) * -1;
			var posY = (mddf.Position.Y - TerrainConstants.CenterPoint) * -1;
			var origin = new Vector3(posX, posY, mddf.Position.Z);

			// Create the scale matrix used in the following loop.
			Matrix scaleMatrix;
			Matrix.CreateScale(mddf.Scale, out scaleMatrix);

			// Creation the rotations
			var rotateZ = Matrix.CreateRotationZ(MathHelper.ToRadians(mddf.OrientationB + 180));
			var rotateY = Matrix.CreateRotationY(MathHelper.ToRadians(mddf.OrientationA));
			var rotateX = Matrix.CreateRotationX(MathHelper.ToRadians(mddf.OrientationC));

			var worldMatrix = Matrix.Multiply(scaleMatrix, rotateZ);
			worldMatrix = Matrix.Multiply(worldMatrix, rotateX);
			worldMatrix = Matrix.Multiply(worldMatrix, rotateY);

			for (var i = 0; i < model.BoundingVertices.Length; i++)
			{
				var position = model.BoundingVertices[i];
				var normal = model.BoundingNormals[i];

				// Scale and Rotate
				Vector3 rotatedPosition;
				Vector3.Transform(ref position, ref worldMatrix, out rotatedPosition);

				Vector3 rotatedNormal;
				Vector3.Transform(ref normal, ref worldMatrix, out rotatedNormal);
				rotatedNormal.Normalize();

				// Translate
				Vector3 finalVector;
				Vector3.Add(ref rotatedPosition, ref origin, out finalVector);

				currentM2.Vertices.Add(finalVector);
			}

			currentM2.Indices.AddRange(indicies);
			return currentM2;
		}

		private static M2 TransformWMOM2(M2Model model, IEnumerable<int> indicies, DoodadDefinition modd)
		{
			var currentM2 = new M2();

			currentM2.Vertices.Clear();
			currentM2.Indices.Clear();

			var origin = new Vector3(modd.Position.X, modd.Position.Y, modd.Position.Z);

			// Create the scalar
			var scalar = modd.Scale;
			var scaleMatrix = Matrix.CreateScale(scalar);

			// Create the rotations
			var quatX = modd.Rotation.X;
			var quatY = modd.Rotation.Y;
			var quatZ = modd.Rotation.Z;
			var quatW = modd.Rotation.W;

			var rotQuat = new Quaternion(quatX, quatY, quatZ, quatW);
			var rotMatrix = Matrix.CreateFromQuaternion(rotQuat);

			var compositeMatrix = Matrix.Multiply(scaleMatrix, rotMatrix);

			for (var i = 0; i < model.BoundingVertices.Length; i++)
			{
				// Scale and transform
				var basePosVector = model.BoundingVertices[i];
				var baseNormVector = model.BoundingNormals[i];
				//PositionUtil.TransformToXNACoordSystem(ref vertex.Position);

				// Scale
				//Vector3 scaledVector;
				//Vector3.Transform(ref vector, ref scaleMatrix, out scaledVector);

				// Rotate
				Vector3 rotatedPosVector;
				Vector3.Transform(ref basePosVector, ref compositeMatrix, out rotatedPosVector);

				Vector3 rotatedNormVector;
				Vector3.Transform(ref baseNormVector, ref compositeMatrix, out rotatedNormVector);
				rotatedNormVector.Normalize();

				// Translate
				Vector3 finalPosVector;
				Vector3.Add(ref rotatedPosVector, ref origin, out finalPosVector);

				currentM2.Vertices.Add(finalPosVector);
			}

			currentM2.Indices.AddRange(indicies);
			return currentM2;
		}
		#endregion

		#region WMOs

		/// <summary>
		/// Adds a WMO to the manager
		/// </summary>
		/// <param name="currentMODF">MODF (placement information for this WMO)</param>
		public WMORoot ReadWMO(MapObjectDefinition currentMODF)
		{
			// Parse the WMORoot
			var wmoRoot = WMORootParser.ReadWMO(Finder, currentMODF.FilePath);

			// Parse the WMOGroups
			for (var wmoGroup = 0; wmoGroup < wmoRoot.Header.GroupCount; wmoGroup++)
			{
				var newFile = wmoRoot.FilePath.Substring(0, wmoRoot.FilePath.LastIndexOf('.'));
				var currentFilePath = String.Format("{0}_{1:000}.wmo", newFile, wmoGroup);

				var group = WMOGroupReader.Process(Finder, currentFilePath, wmoRoot, wmoGroup);

				wmoRoot.Groups[wmoGroup] = group;
			}

			//wmoRoot.DumpLiqChunks();

			// Parse in the WMO's M2s
			var curDoodadSet = currentMODF.DoodadSetId;

			var setIndices = new List<int> { 0 };
			if (curDoodadSet > 0) setIndices.Add(curDoodadSet);

			foreach (var index in setIndices)
			{
				var doodadSetOffset = wmoRoot.DoodadSets[index].FirstInstanceIndex;
				var doodadSetCount = wmoRoot.DoodadSets[index].InstanceCount;
				wmoRoot.WMOM2s = new M2[(int)doodadSetCount];
				for (var i = doodadSetOffset; i < (doodadSetOffset + doodadSetCount); i++)
				{
					var curDoodadDef = wmoRoot.DoodadDefinitions[i];
					var curM2 = M2Reader.ReadM2(Finder, curDoodadDef.FilePath);

					var tempIndices = new List<int>();
					for (var j = 0; j < curM2.BoundingTriangles.Length; j++)
					{
						var tri = curM2.BoundingTriangles[j];

						tempIndices.Add(tri.Index2);
						tempIndices.Add(tri.Index1);
						tempIndices.Add(tri.Index0);
					}

					var rotatedM2 = TransformWMOM2(curM2, tempIndices, curDoodadDef);
					wmoRoot.WMOM2s[i - doodadSetOffset] = rotatedM2;
				}
			}

			TransformWMO(currentMODF, wmoRoot);

			var bounds = new BoundingBox(wmoRoot.WmoVertices);
			wmoRoot.Bounds = bounds;
			return wmoRoot;
		}

		private static void TransformWMO(MapObjectDefinition currentMODF, WMORoot currentWMO)
		{
			currentWMO.ClearCollisionData();
			var position = currentMODF.Position;
			var posX = (position.X - TerrainConstants.CenterPoint) * -1;
			var posY = (position.Y - TerrainConstants.CenterPoint) * -1;
			var origin = new Vector3(posX, posY, position.Z);
			//origin = new Vector3(0.0f);

			//DrawWMOPositionPoint(origin, currentWMO);
			//DrawBoundingBox(currentMODF.Extents, Color.Purple, currentWMO);

			//var rotateZ = Matrix.CreateRotationZ(0*RadiansPerDegree);
			var rotateZ = Matrix.CreateRotationZ((currentMODF.OrientationB + 180) * MathUtil.RadiansPerDegree);
			//var rotateX = Matrix.CreateRotationX(currentMODF.OrientationC * RadiansPerDegree);
			//var rotateY = Matrix.CreateRotationY(currentMODF.OrientationA * RadiansPerDegree);

			int offset;


			foreach (var currentGroup in currentWMO.Groups)
			{
				if (currentGroup == null) continue;
				//if (!currentGroup.Header.HasMLIQ) continue;

				var usedTris = new HashSet<Index3>();
				var wmoTrisUnique = new List<Index3>();

				foreach (var node in currentGroup.BSPNodes)
				{
					if (node.TriIndices == null) continue;
					foreach (var triangle in node.TriIndices)
					{
						if (usedTris.Contains(triangle)) continue;

						usedTris.Add(triangle);
						wmoTrisUnique.Add(triangle);
					}
				}

				var newIndices = new Dictionary<int, int>();
				foreach (var tri in wmoTrisUnique)
				{
					// add all vertices, uniquely
					int newIndex;
					if (!newIndices.TryGetValue(tri.Index0, out newIndex))
					{
						newIndex = currentWMO.WmoVertices.Count;
						newIndices.Add(tri.Index0, newIndex);

						var basePosVec = currentGroup.Vertices[tri.Index0];
						var rotatedPosVec = Vector3.Transform(basePosVec, rotateZ);
						var finalPosVector = rotatedPosVec + origin;
						currentWMO.WmoVertices.Add(finalPosVector);
					}
					currentWMO.WmoIndices.Add(newIndex);

					if (!newIndices.TryGetValue(tri.Index1, out newIndex))
					{
						newIndex = currentWMO.WmoVertices.Count;
						newIndices.Add(tri.Index1, newIndex);

						var basePosVec = currentGroup.Vertices[tri.Index1];
						var rotatedPosVec = Vector3.Transform(basePosVec, rotateZ);
						var finalPosVector = rotatedPosVec + origin;
						currentWMO.WmoVertices.Add(finalPosVector);
					}
					currentWMO.WmoIndices.Add(newIndex);

					if (!newIndices.TryGetValue(tri.Index2, out newIndex))
					{
						newIndex = currentWMO.WmoVertices.Count;
						newIndices.Add(tri.Index2, newIndex);

						var basePosVec = currentGroup.Vertices[tri.Index2];
						var rotatedPosVec = Vector3.Transform(basePosVec, rotateZ);
						var finalPosVector = rotatedPosVec + origin;
						currentWMO.WmoVertices.Add(finalPosVector);
					}
					currentWMO.WmoIndices.Add(newIndex);
				}

				//for (var i = 0; i < currentGroup.Vertices.Count; i++)
				//{
				//    var basePosVector = currentGroup.Vertices[i];
				//    var rotatedPosVector = Vector3.Transform(basePosVector, rotateZ);
				//    var finalPosVector = rotatedPosVector + origin;

				//    //var baseNormVector = currentGroup.Normals[i];
				//    //var rotatedNormVector = Vector3.Transform(baseNormVector, rotateZ);

				//    currentWMO.WmoVertices.Add(finalPosVector);
				//}

				//for (var index = 0; index < currentGroup.Indices.Count; index++)
				//{
				//    currentWMO.WmoIndices.Add(currentGroup.Indices[index].Index0 + offset);
				//    currentWMO.WmoIndices.Add(currentGroup.Indices[index].Index1 + offset);
				//    currentWMO.WmoIndices.Add(currentGroup.Indices[index].Index2 + offset);
				//}

				// WMO Liquids
				if (!currentGroup.Header.HasMLIQ) continue;

				var liqInfo = currentGroup.LiquidInfo;
				var liqOrigin = liqInfo.BaseCoordinates;

				offset = currentWMO.WmoVertices.Count;
				for (var xStep = 0; xStep < liqInfo.XVertexCount; xStep++)
				{
					for (var yStep = 0; yStep < liqInfo.YVertexCount; yStep++)
					{
						var xPos = liqOrigin.X + xStep * TerrainConstants.UnitSize;
						var yPos = liqOrigin.Y + yStep * TerrainConstants.UnitSize;
						var zPosTop = liqInfo.HeightMapMax[xStep, yStep];

						var liqVecTop = new Vector3(xPos, yPos, zPosTop);

						var rotatedTop = Vector3.Transform(liqVecTop, rotateZ);
						var vecTop = rotatedTop + origin;

						currentWMO.WmoLiquidVertices.Add(vecTop);
					}
				}

				for (var row = 0; row < liqInfo.XTileCount; row++)
				{
					for (var col = 0; col < liqInfo.YTileCount; col++)
					{
						if ((liqInfo.LiquidTileFlags[row, col] & 0x0F) == 0x0F) continue;

						var index = ((row + 1) * (liqInfo.YVertexCount) + col);
						currentWMO.WmoLiquidIndices.Add(offset + index);

						index = (row * (liqInfo.YVertexCount) + col);
						currentWMO.WmoLiquidIndices.Add(offset + index);

						index = (row * (liqInfo.YVertexCount) + col + 1);
						currentWMO.WmoLiquidIndices.Add(offset + index);

						index = ((row + 1) * (liqInfo.YVertexCount) + col + 1);
						currentWMO.WmoLiquidIndices.Add(offset + index);

						index = ((row + 1) * (liqInfo.YVertexCount) + col);
						currentWMO.WmoLiquidIndices.Add(offset + index);

						index = (row * (liqInfo.YVertexCount) + col + 1);
						currentWMO.WmoLiquidIndices.Add(offset + index);
					}
				}
			}

			//Rotate the M2s to the new orientation
			if (currentWMO.WMOM2s != null)
			{
				foreach (var currentM2 in currentWMO.WMOM2s)
				{
					offset = currentWMO.WmoVertices.Count;
					for (var i = 0; i < currentM2.Vertices.Count; i++)
					{
						var basePosition = currentM2.Vertices[i];
						var rotatedPosition = Vector3.Transform(basePosition, rotateZ);
						var finalPosition = rotatedPosition + origin;

						//var rotatedNormal = Vector3.Transform(basePosition, rotateZ);

						currentWMO.WmoM2Vertices.Add(finalPosition);
					}

					foreach (var index in currentM2.Indices)
					{
						currentWMO.WmoM2Indices.Add(index + offset);
					}
				}
			}
		}
		#endregion

		//public void LoadZone(int zoneId)
		//{
		//    // Rows
		//    for (var x = 0; x < 64; x++)
		//    {
		//        // Columns
		//        for (var y = 0; y < 64; y++)
		//        {
		//            var tileExists = _wdtFile.TileProfile[y, x];
		//            if (!tileExists) continue;

		//            var _adtPath = "WORLD\\MAPS";
		//            var _basePath = Path.Combine(_baseDirectory, _adtPath);
		//            var _continent = "Azeroth";
		//            var continentPath = Path.Combine(_basePath, _continent);

		//            if (!Directory.Exists(continentPath))
		//            {
		//                throw new Exception("Continent data missing");
		//            }

		//            var filePath = string.Format("{0}\\{1}_{2:00}_{3:00}.adt", continentPath, _continent, x, y);

		//            if (!File.Exists(filePath))
		//            {
		//                throw new Exception("ADT Doesn't exist: " + filePath);
		//            }

		//            var adt = ADTParser.Process(filePath, this);

		//            for (var j = 0; j < 16; j++)
		//            {
		//                for (var i = 0; i < 16; i++)
		//                {
		//                    var mapChunk = adt.MapChunks[i, j];
		//                    if (mapChunk == null) continue;
		//                    if (mapChunk.Header.AreaId != zoneId) continue;
		//                }
		//            }

		//            _adtManager.MapTiles.Clear();
		//        }
		//    }
		//}

	}
}
