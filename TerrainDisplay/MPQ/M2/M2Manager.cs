using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrainDisplay.Collision;
using TerrainDisplay.Collision._3D;
using TerrainDisplay.MPQ.ADT.Components;

namespace TerrainDisplay.MPQ.M2
{
    public class M2Manager : IM2Manager
    {
        private static Color M2Color = Color.SlateGray;
        #region variables

        /// <summary>
        /// 1 degree = 0.0174532925 radians
        /// </summary>
        private const float RadiansPerDegree = 0.0174532925f;

        /// <summary>
        /// List of filenames managed by this M2Manager
        /// </summary>
        private readonly List<String> _names = new List<String>();

        /// <summary>
        /// List of WMOs managed by this WMOManager
        /// </summary>
        public List<M2> M2s = new List<M2>();

        #endregion

        private readonly string _baseDirectory;

        public M2Manager(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
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
            foreach (var m2 in M2s)
            {
                for (var v = 0; v < m2.Vertices.Count; v++)
                {
                    _renderVertices.Add(m2.Vertices[v]);
                }
                for (var i = 0; i < m2.Indices.Count; i++)
                {
                    _renderIndices.Add(m2.Indices[i] + offset);
                }
                offset = _renderVertices.Count;
            }
        }

        /// <summary>
        /// Add a M2 to this manager.
        /// </summary>
        /// <param name="doodadDefinition">MDDF (placement information) for this M2</param>
        public void Add(MapDoodadDefinition doodadDefinition)
        {
            string filePath = Path.Combine(_baseDirectory, doodadDefinition.FilePath);

            if (Path.GetExtension(filePath).Equals(".mdx") || 
                Path.GetExtension(filePath).Equals(".mdl"))
            {
                filePath = Path.ChangeExtension(filePath, ".m2");
                //filePath = filePath.Substring(0, filePath.LastIndexOf('.')) + ".m2";
                //filePath = Path.GetFileNameWithoutExtension(filePath) + ".m2";
            }

            _names.Add(filePath);
            Process(doodadDefinition);
        }

        private void Process(MapDoodadDefinition doodadDefinition)
        {
            var model = M2ModelParser.Process(_baseDirectory, doodadDefinition.FilePath);

            var tempVertices = new List<Vector3>(model.BoundingVertices);
            
            var tempIndices = new List<int>();
            for (var i = 0; i < model.BoundingTriangles.Length; i++)
            {
                var tri = model.BoundingTriangles[i];
                
                tempIndices.Add(tri[2]);
                tempIndices.Add(tri[1]);
                tempIndices.Add(tri[0]);
            }

            var currentM2 = Transform(model, tempIndices, doodadDefinition);
           
            M2s.Add(currentM2);
        }

        private static M2 Transform(M2Model model, IEnumerable<int> indicies, MapDoodadDefinition mddf)
        {
            var currentM2 = new M2();
            currentM2.Vertices.Clear();
            currentM2.Indices.Clear();

            var posX = (mddf.Position.X - TerrainConstants.CenterPoint)*-1;
            var posY = (mddf.Position.Y - TerrainConstants.CenterPoint)*-1;
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

                currentM2.Vertices.Add(new VertexPositionNormalColored(finalVector, M2Color, rotatedNormal));
            }

            //currentM2.AABB = new AABB(currentM2.Vertices);
            //currentM2.OBB = new OBB(currentM2.AABB.Bounds.Center(), currentM2.AABB.Bounds.Extents(),
            //                         Matrix.CreateRotationY(mddf.OrientationB - 90));

            currentM2.Indices.AddRange(indicies);
            return currentM2;
        }
    }
}