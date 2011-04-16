using System.Collections.Generic;
using WCell.Constants;
using WCell.Tools.Maps.Parsing.M2s;
using WCell.Tools.Maps.Parsing.WMO.Components;
using WCell.Tools.Maps.Structures;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps.Parsing.WMO
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

        public BoundingBox AABB;

        public WMORoot(string filePath)
        {
            FilePath = filePath;
        }
    }
}