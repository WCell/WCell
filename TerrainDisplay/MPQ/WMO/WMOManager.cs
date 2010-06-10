using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using MPQNav.Collision;
using MPQNav.Collision._3D;
using MPQNav.MPQ.ADT.Components;
using MPQNav.MPQ.WMO.Components;
using Microsoft.Xna.Framework.Graphics;

namespace MPQNav.MPQ.WMO
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

        private readonly string _baseDirectory;

        public List<string> FileNames
        {
            get { return _fileNames; }
        }

        private List<VertexPositionNormalColored> _renderVertices;
        private List<int> _renderIndices;

        public List<VertexPositionNormalColored> RenderVertices
        {
            get
            {
                if (_renderVertices == null)
                {
                    GenerateRenderVerticesAndIndices();
                }
                return _renderVertices;
            }
            set
            {
                _renderVertices = value;
            }
        }

        public List<int> RenderIndices
        {
            get
            {
                if (_renderIndices == null)
                {
                    GenerateRenderVerticesAndIndices();
                }
                return _renderIndices;
            }
            set
            {
                _renderIndices = value;
            }
        }

        private void GenerateRenderVerticesAndIndices()
        {
            _renderVertices = new List<VertexPositionNormalColored>();
            _renderIndices = new List<int>();
            
            var offset = 0;
            foreach (var wmo in WMOs)
            {
                for (var v = 0; v < wmo.Vertices.Count; v++)
                {
                    _renderVertices.Add(wmo.Vertices[v]);
                }

                for (var i = 0; i < wmo.Indices.Count; i++)
                {
                    _renderIndices.Add(wmo.Indices[i] + offset);
                }
                offset = _renderVertices.Count;
            }
        }

        public WMOManager(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        /// <summary>
        /// Adds a WMO to the manager
        /// </summary>
        /// <param name="currentMODF">MODF (placement information for this WMO)</param>
        public void AddWMO(MapObjectDefinition currentMODF)
        {
            var fullFilePath = Path.Combine(_baseDirectory, currentMODF.FilePath);

            if (!File.Exists(fullFilePath))
            {
                throw new Exception("File does not exist: " + fullFilePath);
            }

            _fileNames.Add(currentMODF.FilePath);

            // Parse the WMORoot
            var wmoRoot = WMORootParser.Process(fullFilePath);
            wmoRoot.CreateAABB();

            // Parse the WMOGroups
            for (var wmoGroup = 0; wmoGroup < wmoRoot.Header.GroupCount; wmoGroup++)
            {
                var newFile = wmoRoot.FilePath.Substring(0, wmoRoot.FilePath.LastIndexOf('.'));
                var currentFileName = String.Format("{0}_{1:000}.wmo", newFile, wmoGroup);

                var group = WMOGroupParser.Process(_baseDirectory, currentFileName, wmoRoot, wmoGroup);
                
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
                    var curM2 = M2.M2ModelParser.Process(_baseDirectory, curDoodadDef.FilePath);

                    var tempVertices = new List<Vector3>(curM2.BoundingVertices);

                    var tempIndices = new List<int>();
                    for (var j = 0; j < curM2.BoundingTriangles.Length; j++)
                    {
                        var tri = curM2.BoundingTriangles[j];

                        tempIndices.Add(tri[2]);
                        tempIndices.Add(tri[1]);
                        tempIndices.Add(tri[0]);
                    }

                    var rotatedM2 = TransformWMOM2(tempVertices, tempIndices, curDoodadDef);
                    wmoRoot.WMOM2s.Add(rotatedM2);
                }
            }

            TransformWMO(currentMODF, wmoRoot);

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

                offset = currentWMO.Vertices.Count;
                foreach (var baseVector in currentGroup.Vertices)
                {
                    var rotatedVector = Vector3.Transform(baseVector, rotateZ);
                    var finalVector = rotatedVector + origin;

                    currentWMO.AddVertex(finalVector);
                }

                for (var index = 0; index < currentGroup.Indices.Count; index++)
                {
                    currentWMO.AddIndex(currentGroup.Indices[index].Index0 + offset);
                    currentWMO.AddIndex(currentGroup.Indices[index].Index1 + offset);
                    currentWMO.AddIndex(currentGroup.Indices[index].Index2 + offset);
                }
                
                
                if (currentGroup.Header.HasMLIQ)
                {
                    var liqInfo = currentGroup.LiquidInfo;
                    var liqOrigin = liqInfo.BaseCoordinates;

                    offset = currentWMO.Vertices.Count;
                    var tempListTop = new List<Vector3>();
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

                            currentWMO.Vertices.Add(new VertexPositionNormalColored(vecTop, Color.Blue, Vector3.Up));
                            tempListTop.Add(vecTop);
                        }
                    }

                    var boundsList = new List<Vector3>();
                    for (var row = 0; row < liqInfo.XTileCount; row++)
                    {
                        for (var col = 0; col < liqInfo.YTileCount; col++)
                        {
                            if ((liqInfo.LiquidTileFlags[row, col] & 0x0F) == 0x0F) continue;

                            var index = ((row + 1)*(liqInfo.YVertexCount) + col);
                            boundsList.Add(tempListTop[index]);
                            currentWMO.Indices.Add(offset + index);

                            index = (row*(liqInfo.YVertexCount) + col);
                            boundsList.Add(tempListTop[index]);
                            currentWMO.Indices.Add(offset + index);

                            index = (row*(liqInfo.YVertexCount) + col + 1);
                            boundsList.Add(tempListTop[index]);
                            currentWMO.Indices.Add(offset + index);

                            index = ((row + 1)*(liqInfo.YVertexCount) + col + 1);
                            boundsList.Add(tempListTop[index]);
                            currentWMO.Indices.Add(offset + index);

                            index = ((row + 1)*(liqInfo.YVertexCount) + col);
                            boundsList.Add(tempListTop[index]);
                            currentWMO.Indices.Add(offset + index);

                            index = (row*(liqInfo.YVertexCount) + col + 1);
                            boundsList.Add(tempListTop[index]);
                            currentWMO.Indices.Add(offset + index);
                        }
                    }

                    //var bounds = BoundingBox.CreateFromPoints(boundsList.ToArray());
                    //DrawBoundingBox(bounds, Color.Blue, currentWMO);
                }
            }

            //Rotate the M2s to the new orientation
            if (currentWMO.WMOM2s != null)
            {
                foreach (var currentM2 in currentWMO.WMOM2s)
                {
                    offset = currentWMO.Vertices.Count;
                    foreach (var baseVector in currentM2.Vertices)
                    {
                        var rotatedVector = Vector3.Transform(baseVector.Position, rotateZ);
                        var finalVector = rotatedVector + origin;

                        currentWMO.AddM2Vertex(finalVector);
                    }

                    foreach (var index in currentM2.Indices)
                    {
                        currentWMO.AddIndex(index + offset);
                    }
                }
            }

            // Generate the OBB
            //currentWMO.OrientatedBoundingBox = new OBB(currentWMO.AABB.Bounds.Center(), currentWMO.AABB.Bounds.Extents(), rotateZ);
        }

        private static void DrawBoundingBox(BoundingBox boundingBox, Color color, WMORoot currentWMO)
        {
            var min = boundingBox.Min;
            var max = boundingBox.Max;
            DrawBoundingBox(min, max, color, currentWMO);
        }

        private static void DrawBoundingBox(Vector3 min, Vector3 max, Color color, WMORoot currentWMO)
        {
            var zero = min;
            var one = new Vector3(min.X, max.Y, min.Z);
            var two = new Vector3(min.X, max.Y, max.Z);
            var three = new Vector3(min.X, min.Y, max.Z);
            var four = new Vector3(max.X, min.Y, min.Z);
            var five = new Vector3(max.X, max.Y, min.Z);
            var six = max;
            var seven = new Vector3(max.X, min.Y, max.Z);

            var offset = currentWMO.Vertices.Count;
            currentWMO.Vertices.Add(new VertexPositionNormalColored(zero, color, Vector3.Up));
            currentWMO.Vertices.Add(new VertexPositionNormalColored(one, color, Vector3.Up));
            currentWMO.Vertices.Add(new VertexPositionNormalColored(two, color, Vector3.Up));
            currentWMO.Vertices.Add(new VertexPositionNormalColored(three, color, Vector3.Up));
            currentWMO.Vertices.Add(new VertexPositionNormalColored(four, color, Vector3.Up));
            currentWMO.Vertices.Add(new VertexPositionNormalColored(five, color, Vector3.Up));
            currentWMO.Vertices.Add(new VertexPositionNormalColored(six, color, Vector3.Up));
            currentWMO.Vertices.Add(new VertexPositionNormalColored(seven, color, Vector3.Up));

            // Bottom Face
            currentWMO.Indices.Add(offset + 0);
            currentWMO.Indices.Add(offset + 1);
            currentWMO.Indices.Add(offset + 5);
            currentWMO.Indices.Add(offset + 0);
            currentWMO.Indices.Add(offset + 5);
            currentWMO.Indices.Add(offset + 4);

            // Front face
            currentWMO.Indices.Add(offset + 0);
            currentWMO.Indices.Add(offset + 1);
            currentWMO.Indices.Add(offset + 2);
            currentWMO.Indices.Add(offset + 0);
            currentWMO.Indices.Add(offset + 2);
            currentWMO.Indices.Add(offset + 3);

            // Left face
            currentWMO.Indices.Add(offset + 1);
            currentWMO.Indices.Add(offset + 2);
            currentWMO.Indices.Add(offset + 6);
            currentWMO.Indices.Add(offset + 1);
            currentWMO.Indices.Add(offset + 6);
            currentWMO.Indices.Add(offset + 5);

            // Back face
            currentWMO.Indices.Add(offset + 5);
            currentWMO.Indices.Add(offset + 6);
            currentWMO.Indices.Add(offset + 7);
            currentWMO.Indices.Add(offset + 5);
            currentWMO.Indices.Add(offset + 7);
            currentWMO.Indices.Add(offset + 4);

            // Right face
            currentWMO.Indices.Add(offset + 0);
            currentWMO.Indices.Add(offset + 3);
            currentWMO.Indices.Add(offset + 7);
            currentWMO.Indices.Add(offset + 0);
            currentWMO.Indices.Add(offset + 7);
            currentWMO.Indices.Add(offset + 4);

            // Top face
            currentWMO.Indices.Add(offset + 3);
            currentWMO.Indices.Add(offset + 2);
            currentWMO.Indices.Add(offset + 6);
            currentWMO.Indices.Add(offset + 3);
            currentWMO.Indices.Add(offset + 6);
            currentWMO.Indices.Add(offset + 7);
        }

        private static void DrawPositionPoint(Vector3 position, WMORoot currentWMO)
        {
            var color = Color.Green;
            var step = TerrainConstants.UnitSize/2;
            var topRight = new Vector3(position.X + step, position.Y + step, position.Z);
            var topLeft = new Vector3(position.X + step, position.Y - step, position.Z);
            var bottomRight = new Vector3(position.X - step, position.Y + step, position.Z);
            var bottomLeft = new Vector3(position.X - step, position.Y - step, position.Z);

            var offset = currentWMO.Vertices.Count;
            currentWMO.Vertices.Add(new VertexPositionNormalColored(bottomRight, color, Vector3.Up));
            currentWMO.Vertices.Add(new VertexPositionNormalColored(topRight, color, Vector3.Up));
            currentWMO.Vertices.Add(new VertexPositionNormalColored(topLeft, color, Vector3.Up));
            currentWMO.Indices.Add(offset + 0);
            currentWMO.Indices.Add(offset + 1);
            currentWMO.Indices.Add(offset + 2);

            offset = currentWMO.Vertices.Count;
            currentWMO.Vertices.Add(new VertexPositionNormalColored(bottomRight, color, Vector3.Up));
            currentWMO.Vertices.Add(new VertexPositionNormalColored(topLeft, color, Vector3.Up));
            currentWMO.Vertices.Add(new VertexPositionNormalColored(bottomLeft, color, Vector3.Up));
            currentWMO.Indices.Add(offset + 0);
            currentWMO.Indices.Add(offset + 1);
            currentWMO.Indices.Add(offset + 2);
        }

        private static M2.M2 TransformWMOM2(IList<Vector3> vectors, IEnumerable<int> indicies, DoodadDefinition modd)
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

            for (var i = 0; i < vectors.Count; i++)
            {
                // Scale and transform
                var vector = vectors[i];
                //PositionUtil.TransformToXNACoordSystem(ref vertex.Position);

                // Scale
                //Vector3 scaledVector;
                //Vector3.Transform(ref vector, ref scaleMatrix, out scaledVector);

                // Rotate
                Vector3 rotatedVector;
                //Vector3.Transform(ref scaledVector, ref rotMatrix, out rotatedVector);
                Vector3.Transform(ref vector, ref compositeMatrix, out rotatedVector);

                // Translate
                Vector3 finalVector;
                Vector3.Add(ref rotatedVector, ref origin, out finalVector);

                currentM2.Vertices.Add(new VertexPositionNormalColored(finalVector, Color.Red, Vector3.Up));
            }

            //currentM2.AABB = new AABB(currentM2.Vertices);
            //currentM2.OBB = new OBB(currentM2.AABB.Bounds.Center(), currentM2.AABB.Bounds.Extents(),
            //                         Matrix.CreateRotationY(mddf.OrientationB - 90));

            currentM2.Indices.AddRange(indicies);
            return currentM2;
        }
    }
}