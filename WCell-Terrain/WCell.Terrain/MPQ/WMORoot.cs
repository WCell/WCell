using System.Collections.Generic;
using System.IO;
using WCell.Terrain.Legacy;
using WCell.Terrain.MPQ.WMOs;
using WCell.Util.Graphics;
using OBB = WCell.Util.Graphics.OBB;

namespace WCell.Terrain.MPQ
{
    public class WMORoot
    {
        public readonly string FilePath;

        public WMOGroup[] Groups;

        /// <summary>
        /// MVER
        /// </summary>
        public int Version;
        /// <summary>
        /// MOHD
        /// </summary>
        public readonly RootHeader Header = new RootHeader();
        /// <summary>
        /// MOGI
        /// </summary>
        public GroupInformation[] GroupInformation;
        /// <summary>
        /// MOPT chunks
        /// </summary>
        public PortalInformation[] PortalInformation;
        /// <summary>
        /// MOPR chunks
        /// </summary>
        public PortalRelation[] PortalRelations;
        /// <summary>
        /// MOPV
        /// </summary>
        public Vector3[] PortalVertices;
        /// <summary>
        /// MODN
        /// </summary>
        public Dictionary<int,string> DoodadFiles;
        /// <summary>
        /// MODS
        /// </summary>
        public DoodadSet[] DoodadSets;
        /// <summary>
        /// MODD
        /// </summary>
        public DoodadDefinition[] DoodadDefinitions;
        /// <summary>
        /// MOTX
        /// </summary>
        public Dictionary<int,string> Textures;
        /// <summary>
        /// MOGN
        /// </summary>
        public Dictionary<int, string> GroupNames;
        /// <summary>
        /// MCVP
        /// </summary>
        public Plane[] ComplexVolumePlanes;
        /// <summary>
        /// MOVV
        /// </summary>
        public Vector3[] VisibleVertices;
        /// <summary>
        /// MOVB
        /// </summary>
        public VertexSpan[] VisibleBlocks;
        /// <summary>
        /// MOLT
        /// </summary>
        public LightInformation[] LightInfo;
        /// <summary>
        /// MOMT
        /// </summary>
        public Material[] Materials;
        /// <summary>
        /// MFOG
        /// </summary>
        public Fog[] Fogs;

        /// <summary>
        /// The Orientated Bounding Box for this WMO
        /// </summary>
        public OBB OrientatedBoundingBox;

        public BoundingBox Bounds;

        /// <summary>
        /// The M2 models that decorate this WMO
        /// </summary>
        public M2[] WMOM2s;

        #region Rendering Variables
        /// <summary>
        /// List of vertices used for rendering this WMO in World Space
        /// </summary>
        public readonly List<Vector3> WmoVertices = new List<Vector3>();
        public readonly List<Vector3> WmoM2Vertices = new List<Vector3>();
        public readonly List<Vector3> WmoLiquidVertices = new List<Vector3>();
        /// <summary>
        /// List of indicies used for rendering this WMO in World Space
        /// </summary>
        public List<int> WmoIndices = new List<int>();
        public List<int> WmoM2Indices = new List<int>();
        public List<int> WmoLiquidIndices = new List<int>();
        #endregion

        public WMORoot(string filePath)
        {
            FilePath = filePath;
        }

        public void ClearCollisionData()
        {
            OrientatedBoundingBox = new OBB();
            WmoVertices.Clear();
            WmoIndices.Clear();
        }

        internal void DumpLiqChunks()
        {
            var name = Path.GetFileName(FilePath);
            name = Path.ChangeExtension(name, ".liquid.txt");
            var f = new StreamWriter(name);
            
            f.WriteLine("Dumping WMOGroup Liquid Information...");
            
            for (var i = 0; i < Groups.Length; i++)
            {
                var group = Groups[i];
                if (group == null) continue;
                if (group.LiquidInfo == null) continue;

                f.WriteLine(string.Format("Group: {0:00}", i));
                group.LiquidInfo.Dump(f);
                f.WriteLine();
                f.WriteLine();
            }

            f.Close();
        }
    }
}
