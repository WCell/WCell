using System;
using WCell.Util.Graphics;

namespace WCell.Terrain.MPQ.M2.Components
{
    [Flags]
    public enum GlobalModelFlags
    {
        Flag_0x1_TiltX = 0x1,
        Flag_0x2_TiltY = 0x2,
        Flag_0x4 = 0x4,
        Flag_0x8_ExtraHeaderField = 0x8,
        Flag_0x10 = 0x10,
    }

    public class ModelHeader
    {
        public uint Magic;
        public uint Version;
        public int NameLength;
        public int NameOffset;

        public GlobalModelFlags GlobalModelFlags;

        public OffsetLocation GlobalSequences;
        public OffsetLocation Animations;
        public OffsetLocation AnimationLookup;
        public OffsetLocation Bones;
        public OffsetLocation KeyBoneLookup;
        public OffsetLocation Vertices;
        public uint ViewCount;
        public OffsetLocation Colors;
        public OffsetLocation Textures;
        public OffsetLocation Transparency;
        public OffsetLocation UVAnimation;
        public OffsetLocation TexReplace;
        public OffsetLocation RenderFlags;
        public OffsetLocation BoneLookupTable;
        public OffsetLocation TexLookup;
        public OffsetLocation TexUnits;
        public OffsetLocation TransLookup;
        public OffsetLocation UVAnimLookup;

        public BoundingBox VertexBox;
        public float VertexRadius;
        public BoundingBox BoundingBox;
        public float BoundingRadius;

        public OffsetLocation BoundingTriangles;
        public OffsetLocation BoundingVertices;
        public OffsetLocation BoundingNormals;
        public OffsetLocation Attachments;
        public OffsetLocation AttachLookup;
        public OffsetLocation Events;
        public OffsetLocation Lights;
        public OffsetLocation Cameras;
        public OffsetLocation CameraLookup;
        public OffsetLocation RibbonEmitters;
        public OffsetLocation ParticleEmitters;
        /// <summary>
        /// Only appears with GlobalFlag 0x8
        /// </summary>
        public OffsetLocation OptionalUnk;


        public bool HasUnknownFinalPart
        {
            get { return (GlobalModelFlags & GlobalModelFlags.Flag_0x8_ExtraHeaderField) != 0; }
        }
    }
}
