using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using TerrainDisplay.Collision._3D;
using TerrainDisplay.MPQ.WMO.Components;

namespace TerrainDisplay.MPQ.WMO
{
    public struct Index3
    {
        public short Index0;
        public short Index1;
        public short Index2;
    }

    public class WMOGroup
    {
        public uint Version;
        public readonly GroupHeader Header = new GroupHeader();

        /// <summary>
        /// MOVT - Verticies
        /// </summary>
        public readonly List<Vector3> Vertices = new List<Vector3>();
        /// <summary>
        /// Normals
        /// </summary>
        public readonly List<Vector3> Normals = new List<Vector3>();

        /// <summary>
        /// MOVI - Triangle Indices.
        /// Rendering Indices information for the Group.
        /// </summary>
        public readonly List<Index3> Indices = new List<Index3>();

        /// <summary>
        /// Flags that affect the Triangles in this Group.
        /// </summary>
        public MOPY[] TriangleMaterials;
        /// <summary>
        /// The index into the WMOs GroupList that refers to this WMOGroup.
        /// </summary>
        public int Index;
        /// <summary>
        /// A list of the resulting Triangles from this WMOGroup.
        /// </summary>
        public readonly List<Triangle> Triangles = new List<Triangle>();

        public BSPNode[] BSPNodes;
        public ushort[] MOBR;

        public uint TriangleCount;
        public uint VertexCount;

        public readonly WMORoot Root;
        public string Name;
        public string DescriptiveName;

        /// <summary>
        /// MLIQ Info
        /// </summary>
        public LiquidInfo LiquidInfo;
        /// <summary>
        /// MOTV1
        /// </summary>
        public Vector2[] TextureVertices1;
        /// <summary>
        /// MOCV1
        /// </summary>
        public Color4[] VertexColors1;
        /// <summary>
        /// MOTV2
        /// </summary>
        public Vector2[] TextureVertices2;
        /// <summary>
        /// MOCV2
        /// </summary>
        public Color4[] VertexColors2;
        /// <summary>
        /// MOBA
        /// </summary>
        public RenderBatch[] Batches;
        /// <summary>
        /// MOLR
        /// </summary>
        public ushort[] LightReferences;
        /// <summary>
        /// MODR
        /// </summary>
        public ushort[] DoodadReferences;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="root"></param>
        /// <param name="index">The index into the WMOs GroupList that refers to this WMOGroup.</param>
        public WMOGroup(WMORoot root, int index)
        {
            Root = root;
            Index = index;
        }

        internal void DumpBSPNodes()
        {
            using (var f = new StreamWriter(Name + ".bspnodes.txt"))
            {
                for (var i = 0; i < BSPNodes.Length; i++)
                {
                    var node = BSPNodes[i];
                    f.WriteLine("Node: " + i);
                    //node.Dump(f);
                }
            }
        }

        #region Internal Classes


        #endregion
    }


}
