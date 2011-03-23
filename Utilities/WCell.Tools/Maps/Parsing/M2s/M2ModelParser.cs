using System.IO;
using WCell.Util.Logging;
using WCell.MPQTool;
using WCell.Tools.Maps.Parsing.M2s.Components;
using WCell.Tools.Maps.Structures;
using WCell.Tools.Maps.Utils;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps.Parsing.M2s
{
    public class M2ModelParser
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static M2Model Process(MpqManager mpqManager, string filePath)
        {
            if (!mpqManager.FileExists(filePath))
            {
                var altFilePath = Path.ChangeExtension(filePath, ".m2");
                if (!mpqManager.FileExists(altFilePath))
                {
                    log.Error("M2 file does not exist: ", altFilePath);
                }

                filePath = altFilePath;
            }

            var model = new M2Model();

            using (var br = new BinaryReader(mpqManager.OpenFile(filePath)))
            {
                ReadHeader(br, model);
                ReadGlobalSequences(br, model);
                ReadAnimations(br, model);
                ReadAnimationLookup(br, model);
                ReadBones(br, model);
                ReadKeyBoneLookup(br, model);
                ReadVertices(br, model);
                ReadColors(br, model);
                ReadTextures(br, model);
                ReadTransparency(br, model);
                ReadUVAnimation(br, model);
                ReadTexReplace(br, model);
                ReadRenderFlags(br, model);
                ReadBoneLookupTable(br, model);
                ReadTexLookup(br, model);
                ReadTexUnits(br, model);
                ReadTransLookup(br, model);
                ReadUVAnimLookup(br, model);
                ReadBoundingTriangles(br, model);
                ReadBoundingVertices(br, model);
                ReadBoundingNormals(br, model);
                ReadAttachments(br, model);
                ReadAttachLookups(br, model);
                ReadEvents(br, model);
                ReadLights(br, model);
                ReadCameras(br, model);
                ReadCameraLookup(br, model);
                ReadRibbonEmitters(br, model);
                ReadParticleEmitters(br, model);

                if (model.Header.HasUnknownFinalPart)
                {
                    ReadOptionalSection(br, model);
                }
            }

            model.FilePath = filePath;
            return model;
        }

        static void ReadHeader(BinaryReader br, M2Model model)
        {
            var header = model.Header = new ModelHeader();

            header.Magic = br.ReadUInt32();
            header.Version = br.ReadUInt32();
            header.NameLength = br.ReadInt32();
            header.NameOffset = br.ReadInt32();
            header.GlobalModelFlags = (GlobalModelFlags) br.ReadUInt32();

            br.ReadOffsetLocation(ref header.GlobalSequences);
            br.ReadOffsetLocation(ref header.Animations);
            br.ReadOffsetLocation(ref header.AnimationLookup);
            br.ReadOffsetLocation(ref header.Bones);
            br.ReadOffsetLocation(ref header.KeyBoneLookup);
            br.ReadOffsetLocation(ref header.Vertices);
            header.ViewCount = br.ReadUInt32();
            br.ReadOffsetLocation(ref header.Colors);
            br.ReadOffsetLocation(ref header.Textures);
            br.ReadOffsetLocation(ref header.Transparency);
            br.ReadOffsetLocation(ref header.UVAnimation);
            br.ReadOffsetLocation(ref header.TexReplace);
            br.ReadOffsetLocation(ref header.RenderFlags);
            br.ReadOffsetLocation(ref header.BoneLookupTable);
            br.ReadOffsetLocation(ref header.TexLookup);
            br.ReadOffsetLocation(ref header.TexUnits);
            br.ReadOffsetLocation(ref header.TransLookup);
            br.ReadOffsetLocation(ref header.UVAnimLookup);

            header.VertexBox = br.ReadBoundingBox();
            header.VertexRadius = br.ReadSingle();
            header.BoundingBox = br.ReadBoundingBox();
            header.BoundingRadius = br.ReadSingle();

            br.ReadOffsetLocation(ref header.BoundingTriangles);
            br.ReadOffsetLocation(ref header.BoundingVertices);
            br.ReadOffsetLocation(ref header.BoundingNormals);
            br.ReadOffsetLocation(ref header.Attachments);
            br.ReadOffsetLocation(ref header.AttachLookup);
            br.ReadOffsetLocation(ref header.Events);
            br.ReadOffsetLocation(ref header.Lights);
            br.ReadOffsetLocation(ref header.Cameras);
            br.ReadOffsetLocation(ref header.CameraLookup);
            br.ReadOffsetLocation(ref header.RibbonEmitters);
            br.ReadOffsetLocation(ref header.ParticleEmitters);

            if (header.HasUnknownFinalPart)
            {
                br.ReadOffsetLocation(ref header.OptionalUnk);
            }


            br.BaseStream.Position = model.Header.NameOffset;
            //model.Name = Encoding.UTF8.GetString(br.ReadBytes(model.Header.NameLength));
            model.Name = br.ReadCString();
        }
        static void ReadGlobalSequences(BinaryReader br, M2Model model)
        {
            var gsInfo = model.Header.GlobalSequences;
            model.GlobalSequenceTimestamps = new uint[gsInfo.Count];

            br.BaseStream.Position = gsInfo.Offset;

            for (int i = 0; i < model.GlobalSequenceTimestamps.Length; i++)
            {
                model.GlobalSequenceTimestamps[i] = br.ReadUInt32();
            }
        }
        static void ReadAnimations(BinaryReader br, M2Model model)
        {
        }
        static void ReadAnimationLookup(BinaryReader br, M2Model model)
        {
        }
        static void ReadBones(BinaryReader br, M2Model model)
        {
        }
        static void ReadKeyBoneLookup(BinaryReader br, M2Model model)
        {
        }
        static void ReadVertices(BinaryReader br, M2Model model)
        {
            //var vertInfo = model.Header.Vertices;

            //model.Vertices = new ModelVertices[vertInfo.Count];

            //br.BaseStream.Position = vertInfo.Offset;
            //for (int i = 0; i < vertInfo.Count; i++)
            //{
            //    var mv = new ModelVertices
            //                 {
            //                     Position = br.ReadVector3(),
            //                     BoneWeight = br.ReadBytes(4),
            //                     BoneIndices = br.ReadBytes(4),
            //                     Normal = br.ReadVector3(),
            //                     TextureCoordinates = br.ReadVector2(),
            //                     Float_1 = br.ReadSingle(),
            //                     Float_2 = br.ReadSingle()
            //                 };

            //    model.Vertices[i] = mv;
            //}
        }
        static void ReadColors(BinaryReader br, M2Model model)
        {
        }
        static void ReadTextures(BinaryReader br, M2Model model)
        {
        }
        static void ReadTransparency(BinaryReader br, M2Model model)
        {
        }
        static void ReadUVAnimation(BinaryReader br, M2Model model)
        {
        }
        static void ReadTexReplace(BinaryReader br, M2Model model)
        {
        }
        static void ReadRenderFlags(BinaryReader br, M2Model model)
        {
        }
        static void ReadBoneLookupTable(BinaryReader br, M2Model model)
        {
        }
        static void ReadTexLookup(BinaryReader br, M2Model model)
        {
        }
        static void ReadTexUnits(BinaryReader br, M2Model model)
        {
        }
        static void ReadTransLookup(BinaryReader br, M2Model model)
        {
        }
        static void ReadUVAnimLookup(BinaryReader br, M2Model model)
        {
        }
        static void ReadBoundingTriangles(BinaryReader br, M2Model model)
        {
            var btInfo = model.Header.BoundingTriangles;
            model.BoundingTriangles = new Index3[btInfo.Count/3];

            br.BaseStream.Position = btInfo.Offset;

            for (int i = 0; i < model.BoundingTriangles.Length; i++)
            {
                model.BoundingTriangles[i] = new Index3 {
                                                            Index2 = br.ReadInt16(),
                                                            Index1 = br.ReadInt16(),
                                                            Index0 = br.ReadInt16()
                                                        };
                
            }
        }
        static void ReadBoundingVertices(BinaryReader br, M2Model model)
        {
            var bvInfo = model.Header.BoundingVertices;

            model.BoundingVertices = new Vector3[bvInfo.Count];
            br.BaseStream.Position = bvInfo.Offset;

            for (var i = 0; i < bvInfo.Count; i++)
            {
                model.BoundingVertices[i] = br.ReadVector3();
            }
        }
        static void ReadBoundingNormals(BinaryReader br, M2Model model)
        {
            //var bnInfo = model.Header.BoundingVertices;

            //model.BoundingNormals = new Vector3[bnInfo.Count];
            //br.BaseStream.Position = bnInfo.Offset;

            //for (var i = 0; i < bnInfo.Count; i++)
            //{
            //    model.BoundingNormals[i] = br.ReadVector3();
            //}
        }
        static void ReadAttachments(BinaryReader br, M2Model model)
        {
        }
        static void ReadAttachLookups(BinaryReader br, M2Model model)
        {
        }
        static void ReadEvents(BinaryReader br, M2Model model)
        {
        }
        static void ReadLights(BinaryReader br, M2Model model)
        {
        }
        static void ReadCameras(BinaryReader br, M2Model model)
        {
        }
        static void ReadCameraLookup(BinaryReader br, M2Model model)
        {
        }
        static void ReadRibbonEmitters(BinaryReader br, M2Model model)
        {
        }
        static void ReadParticleEmitters(BinaryReader br, M2Model model)
        {
        }
        static void ReadOptionalSection(BinaryReader br, M2Model model)
        {
        }
    }
}