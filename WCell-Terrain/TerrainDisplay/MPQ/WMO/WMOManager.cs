using System;
using System.Collections.Generic;
using System.IO;
using TerrainDisplay.Collision;
using TerrainDisplay.Collision.QuadTree;
using WCell.Util.Graphics;
using TerrainDisplay.MPQ.ADT.Components;
using TerrainDisplay.MPQ.M2;
using TerrainDisplay.MPQ.WMO.Components;

namespace TerrainDisplay.MPQ.WMO
{
    public class WMOManager : IWMOManager
    {
        /// <summary>
        /// List of filenames managed by this WMOManager
        /// </summary>
        private readonly List<String> _fileNames = new List<String>();
        /// <summary>
        /// List of WMOs managed by this WMOManager
        /// </summary>
        public readonly List<WMORoot> WMOs = new List<WMORoot>();

        /// <summary>
        /// 1 degree = 0.0174532925 radians
        /// </summary>
        private const float RadiansPerDegree = 0.0174532925f;

        public List<string> FileNames
        {
            get { return _fileNames; }
        }

        private List<Vector3> wmoVertices;
        private List<int> wmoIndices;
        private List<Vector3> wmoM2Vertices;
        private List<int> wmoM2Indices;
        private List<Vector3> wmoLiquidVertices;
        private List<int> wmoLiquidIndices;

        public List<Vector3> WmoVertices
        {
            get
            {
                if (wmoVertices == null)
                {
                    GenerateRenderVerticesAndIndices();
                }
                return wmoVertices;
            }
            set
            {
                wmoVertices = value;
            }
        }

        public List<Vector3> WmoM2Vertices
        {
            get
            {
                if (wmoM2Vertices == null)
                {
                    GenerateRenderVerticesAndIndices();
                }
                return wmoM2Vertices;
            }
            set
            {
                wmoM2Vertices = value;
            }
        }

        public List<Vector3> WmoLiquidVertices
        {
            get
            {
                if (wmoLiquidVertices == null)
                {
                    GenerateRenderVerticesAndIndices();
                }
                return wmoLiquidVertices;
            }
            set
            {
                wmoLiquidVertices = value;
            }
        }

        public List<int> WmoIndices
        {
            get
            {
                if (wmoIndices == null)
                {
                    GenerateRenderVerticesAndIndices();
                }
                return wmoIndices;
            }
            set
            {
                wmoIndices = value;
            }
        }

        public List<int> WmoM2Indices
        {
            get
            {
                if (wmoM2Indices == null)
                {
                    GenerateRenderVerticesAndIndices();
                }
                return wmoM2Indices;
            }
            set
            {
                wmoM2Indices = value;
            }
        }

        public List<int> WmoLiquidIndices
        {
            get
            {
                if (wmoLiquidIndices == null)
                {
                    GenerateRenderVerticesAndIndices();
                }
                return wmoLiquidIndices;
            }
            set
            {
                wmoLiquidIndices = value;
            }
        }

        private void GenerateRenderVerticesAndIndices()
        {
            wmoVertices = new List<Vector3>();
            wmoM2Vertices = new List<Vector3>();
            wmoLiquidVertices = new List<Vector3>();
            wmoIndices = new List<int>();
            wmoM2Indices = new List<int>();
            wmoLiquidIndices = new List<int>();
            
            var offset = 0;
            foreach (var wmo in WMOs)
            {
                for (var v = 0; v < wmo.WmoVertices.Count; v++)
                {
                    wmoVertices.Add(wmo.WmoVertices[v]);
                }

                for (var i = 0; i < wmo.WmoIndices.Count; i++)
                {
                    wmoIndices.Add(wmo.WmoIndices[i] + offset);
                }
                offset = wmoVertices.Count;
            }
        }


        /// <summary>
        /// Adds a WMO to the manager
        /// </summary>
        /// <param name="currentMODF">MODF (placement information for this WMO)</param>
        public void AddWMO(MapObjectDefinition currentMODF)
        {
            _fileNames.Add(currentMODF.FilePath);

            // Parse the WMORoot
            var wmoRoot = WMORootParser.Process(MpqTerrainManager.MpqManager, currentMODF.FilePath);
            
            // Parse the WMOGroups
            for (var wmoGroup = 0; wmoGroup < wmoRoot.Header.GroupCount; wmoGroup++)
            {
                var newFile = wmoRoot.FilePath.Substring(0, wmoRoot.FilePath.LastIndexOf('.'));
                var currentFilePath = String.Format("{0}_{1:000}.wmo", newFile, wmoGroup);
                
                var group = WMOGroupParser.Process(MpqTerrainManager.MpqManager, currentFilePath, wmoRoot, wmoGroup);
                
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
                wmoRoot.WMOM2s = new List<M2.M2>((int) doodadSetCount);
                for (var i = doodadSetOffset; i < (doodadSetOffset + doodadSetCount); i++)
                {
                    var curDoodadDef = wmoRoot.DoodadDefinitions[i];
                    var curM2 = M2ModelParser.Process(MpqTerrainManager.MpqManager, curDoodadDef.FilePath);

                    var tempIndices = new List<int>();
                    for (var j = 0; j < curM2.BoundingTriangles.Length; j++)
                    {
                        var tri = curM2.BoundingTriangles[j];

                        tempIndices.Add(tri.Index2);
                        tempIndices.Add(tri.Index1);
                        tempIndices.Add(tri.Index0);
                    }

                    var rotatedM2 = TransformWMOM2(curM2, tempIndices, curDoodadDef);
                    wmoRoot.WMOM2s.Add(rotatedM2);
                }
            }

            TransformWMO(currentMODF, wmoRoot);

            var bounds = new BoundingBox(wmoRoot.WmoVertices);
            wmoRoot.Bounds = bounds;
            
            WMOs.Add(wmoRoot);
        }

        private static void TransformWMO(MapObjectDefinition currentMODF, WMORoot currentWMO)
        {
            currentWMO.ClearCollisionData();
            var position = currentMODF.Position;
            var posX = (position.X - TerrainConstants.CenterPoint)*-1;
            var posY = (position.Y - TerrainConstants.CenterPoint)*-1;
            var origin = new Vector3(posX, posY, position.Z);
            //origin = new Vector3(0.0f);

            //DrawWMOPositionPoint(origin, currentWMO);
            //DrawBoundingBox(currentMODF.Extents, Color.Purple, currentWMO);

            //var rotateZ = Matrix.CreateRotationZ(0*RadiansPerDegree);
            var rotateZ = Matrix.CreateRotationZ((currentMODF.OrientationB + 180)*RadiansPerDegree);
            //var rotateX = Matrix.CreateRotationX(currentMODF.OrientationC * RadiansPerDegree);
            //var rotateY = Matrix.CreateRotationY(currentMODF.OrientationA * RadiansPerDegree);

            int offset;

            
            foreach (var currentGroup in currentWMO.Groups)
            {
                if (currentGroup == null) continue;
                //if (!currentGroup.Header.HasMLIQ) continue;

                var usedTris = new Dictionary<Index3, int>();
                var wmoTrisUnique = new List<Index3>();

                foreach (var node in currentGroup.BSPNodes)
                {
                    if (node.TriIndices == null) continue;
                    foreach (var index3 in node.TriIndices)
                    {
                        if (usedTris.ContainsKey(index3)) continue;

                        usedTris.Add(index3, 0);
                        wmoTrisUnique.Add(index3);
                    }
                }

                var newIndices = new Dictionary<int, int>();
                foreach (var tri in wmoTrisUnique)
                {
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

                        var index = ((row + 1)*(liqInfo.YVertexCount) + col);
                        currentWMO.WmoLiquidIndices.Add(offset + index);

                        index = (row*(liqInfo.YVertexCount) + col);
                        currentWMO.WmoLiquidIndices.Add(offset + index);

                        index = (row*(liqInfo.YVertexCount) + col + 1);
                        currentWMO.WmoLiquidIndices.Add(offset + index);

                        index = ((row + 1)*(liqInfo.YVertexCount) + col + 1);
                        currentWMO.WmoLiquidIndices.Add(offset + index);

                        index = ((row + 1)*(liqInfo.YVertexCount) + col);
                        currentWMO.WmoLiquidIndices.Add(offset + index);

                        index = (row*(liqInfo.YVertexCount) + col + 1);
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

        //private static void DrawBoundingBox(BoundingBox boundingBox, Color color, WMORoot currentWMO)
        //{
        //    var min = boundingBox.Min;
        //    var max = boundingBox.Max;
        //    DrawBoundingBox(min, max, color, currentWMO);
        //}

        //private static void DrawBoundingBox(Vector3 min, Vector3 max, Color color, WMORoot currentWMO)
        //{
        //    var zero = min;
        //    var one = new Vector3(min.X, max.Y, min.Z);
        //    var two = new Vector3(min.X, max.Y, max.Z);
        //    var three = new Vector3(min.X, min.Y, max.Z);
        //    var four = new Vector3(max.X, min.Y, min.Z);
        //    var five = new Vector3(max.X, max.Y, min.Z);
        //    var six = max;
        //    var seven = new Vector3(max.X, min.Y, max.Z);

        //    var offset = currentWMO.WmoVertices.Count;
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(zero, color, Vector3.Up));
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(one, color, Vector3.Up));
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(two, color, Vector3.Up));
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(three, color, Vector3.Up));
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(four, color, Vector3.Up));
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(five, color, Vector3.Up));
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(six, color, Vector3.Up));
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(seven, color, Vector3.Up));

        //    // Bottom Face
        //    currentWMO.WmoIndices.Add(offset + 0);
        //    currentWMO.WmoIndices.Add(offset + 1);
        //    currentWMO.WmoIndices.Add(offset + 5);
        //    currentWMO.WmoIndices.Add(offset + 0);
        //    currentWMO.WmoIndices.Add(offset + 5);
        //    currentWMO.WmoIndices.Add(offset + 4);

        //    // Front face
        //    currentWMO.WmoIndices.Add(offset + 0);
        //    currentWMO.WmoIndices.Add(offset + 1);
        //    currentWMO.WmoIndices.Add(offset + 2);
        //    currentWMO.WmoIndices.Add(offset + 0);
        //    currentWMO.WmoIndices.Add(offset + 2);
        //    currentWMO.WmoIndices.Add(offset + 3);

        //    // Left face
        //    currentWMO.WmoIndices.Add(offset + 1);
        //    currentWMO.WmoIndices.Add(offset + 2);
        //    currentWMO.WmoIndices.Add(offset + 6);
        //    currentWMO.WmoIndices.Add(offset + 1);
        //    currentWMO.WmoIndices.Add(offset + 6);
        //    currentWMO.WmoIndices.Add(offset + 5);

        //    // Back face
        //    currentWMO.WmoIndices.Add(offset + 5);
        //    currentWMO.WmoIndices.Add(offset + 6);
        //    currentWMO.WmoIndices.Add(offset + 7);
        //    currentWMO.WmoIndices.Add(offset + 5);
        //    currentWMO.WmoIndices.Add(offset + 7);
        //    currentWMO.WmoIndices.Add(offset + 4);

        //    // Right face
        //    currentWMO.WmoIndices.Add(offset + 0);
        //    currentWMO.WmoIndices.Add(offset + 3);
        //    currentWMO.WmoIndices.Add(offset + 7);
        //    currentWMO.WmoIndices.Add(offset + 0);
        //    currentWMO.WmoIndices.Add(offset + 7);
        //    currentWMO.WmoIndices.Add(offset + 4);

        //    // Top face
        //    currentWMO.WmoIndices.Add(offset + 3);
        //    currentWMO.WmoIndices.Add(offset + 2);
        //    currentWMO.WmoIndices.Add(offset + 6);
        //    currentWMO.WmoIndices.Add(offset + 3);
        //    currentWMO.WmoIndices.Add(offset + 6);
        //    currentWMO.WmoIndices.Add(offset + 7);
        //}

        //private static void DrawPositionPoint(Vector3 position, WMORoot currentWMO)
        //{
        //    var color = Color.Green;
        //    var step = TerrainConstants.UnitSize/2;
        //    var topRight = new Vector3(position.X + step, position.Y + step, position.Z);
        //    var topLeft = new Vector3(position.X + step, position.Y - step, position.Z);
        //    var bottomRight = new Vector3(position.X - step, position.Y + step, position.Z);
        //    var bottomLeft = new Vector3(position.X - step, position.Y - step, position.Z);

        //    var offset = currentWMO.WmoVertices.Count;
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(bottomRight, color, Vector3.Up));
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(topRight, color, Vector3.Up));
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(topLeft, color, Vector3.Up));
        //    currentWMO.WmoIndices.Add(offset + 0);
        //    currentWMO.WmoIndices.Add(offset + 1);
        //    currentWMO.WmoIndices.Add(offset + 2);

        //    offset = currentWMO.WmoVertices.Count;
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(bottomRight, color, Vector3.Up));
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(topLeft, color, Vector3.Up));
        //    currentWMO.WmoVertices.Add(new VertexPositionNormalColored(bottomLeft, color, Vector3.Up));
        //    currentWMO.WmoIndices.Add(offset + 0);
        //    currentWMO.WmoIndices.Add(offset + 1);
        //    currentWMO.WmoIndices.Add(offset + 2);
        //}

        private static M2.M2 TransformWMOM2(M2Model model, IEnumerable<int> indicies, DoodadDefinition modd)
        {
            var currentM2 = new M2.M2();

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
    }
}