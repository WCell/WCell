using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using WCell.Constants;
using WCell.MPQTool;
using WCell.Terrain.Legacy;
using WCell.Terrain.MPQ;
using WCell.Terrain.MPQ.ADTs;
using WCell.Terrain.MPQ.WMOs;
using WCell.Util;
using WCell.Util.Graphics;
using Material = WCell.Terrain.MPQ.WMOs.Material;

namespace WCell.Terrain.Serialization
{
    public static class WMOReader
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static WMORoot ReadWMO(MpqLibrarian mpqLibrarian, string filePath)
        {
            var root = new WMORoot(filePath);

            if (!mpqLibrarian.FileExists(filePath))
            {
                log.Error("WMO file does not exist: ", filePath);
            	return null;
            }

            using (var stream = mpqLibrarian.OpenFile(filePath))
            using (var fileReader = new BinaryReader(stream))
            {
                uint type = 0;
                uint size = 0;
                long curPos = AdvanceToNextChunk(fileReader, 0, ref type, ref size);

                if (type == Signatures.MVER)
                {
                    // Version
                    ReadMVER(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MVER");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOHD)
                {
                    // Root Header
                    ReadMOHD(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOHD");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOTX)
                {
                    // Texture Names
                    ReadMOTX(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOTX");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOMT)
                {
                    // Materials
                    ReadMOMT(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOMT");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOGN)
                {
                    ReadMOGN(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOGN");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOGI)
                {
                    // Group Information
                    ReadMOGI(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOGI");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOSB)
                {
                    // Skybox (always 0 now, its no longer handled in WMO)
                    ReadMOSB(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOSB");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOPV)
                {
                    // Portal Vertices
                    ReadMOPV(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOPV");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOPT)
                {
                    // Portal Information
                    ReadMOPT(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOPT");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOPR)
                {
                    // Portal Relations
                    ReadMOPR(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOPR");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOVV)
                {
                    // Visible Vertices
                    ReadMOVV(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOVV");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOVB)
                {
                    // Visible Blocks
                    ReadMOVB(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOVB");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MOLT)
                {
                    // Lights
                    ReadMOLT(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MOLT");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MODS)
                {
                    // Doodad Set
                    ReadMODS(fileReader, root);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MODS");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MODN)
                {
                    // Doodad Names
                    ReadMODN(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MODN");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MODD)
                {
                    // Doodad Definitions
                    ReadMODD(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MODD");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                if (type == Signatures.MFOG)
                {
                    // Fog info
                    ReadMFOG(fileReader, root, size);
                }
                else
                {
                    Console.WriteLine("WMO Root missing required chunk MFOG");
                }
                curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);

                // Theres only 1 optional chunk in a WMO root
                if (fileReader.BaseStream.Position < fileReader.BaseStream.Length)
                {
                    if (type == Signatures.MCVP)
                    {
                        ReadMCVP(fileReader, root, size);
                    }

                    //curPos = AdvanceToNextChunk(fileReader, curPos, ref type, ref size);
                }

            }


            return root;
        }

        static long AdvanceToNextChunk(BinaryReader br, long curPos, ref uint type, ref uint size)
        {
            br.BaseStream.Seek(curPos + size, SeekOrigin.Begin);
            if (br.BaseStream.Position == br.BaseStream.Length)
            {
                return br.BaseStream.Length;
            }

            type = br.ReadUInt32();
            size = br.ReadUInt32();
            long newCurPos = br.BaseStream.Position;
            return newCurPos;
        }

        static void ReadMVER(BinaryReader br, WMORoot wmo)
        {
            wmo.Version = br.ReadInt32();
        }

        /// <summary>
        /// Reads the header for the root file
        /// </summary>
        static void ReadMOHD(BinaryReader br, WMORoot wmo)
        {
            wmo.Header.TextureCount = br.ReadUInt32();
            wmo.Header.GroupCount = br.ReadUInt32();
            wmo.Header.PortalCount = br.ReadUInt32();
            wmo.Header.LightCount = br.ReadUInt32();
            wmo.Header.ModelCount = br.ReadUInt32();
            wmo.Header.DoodadCount = br.ReadUInt32();
            wmo.Header.DoodadSetCount = br.ReadUInt32();

            wmo.Header.AmbientColor = br.ReadColor4();
            wmo.Header.WMOId = br.ReadUInt32();

            wmo.Header.BoundingBox = new BoundingBox(br.ReadWMOVector3(), br.ReadWMOVector3());

            wmo.Header.Flags = (WMORootHeaderFlags)br.ReadUInt32();

            wmo.Groups = new WMOGroup[wmo.Header.GroupCount];
        }

        static void ReadMOTX(BinaryReader br, WMORoot wmo, uint size)
        {
            wmo.Textures = new Dictionary<int, string>();

            long endPos = br.BaseStream.Position + size;
            while (br.BaseStream.Position < endPos)
            {
                if (br.PeekByte() == 0)
                {
                    br.BaseStream.Position++;
                }
                else
                {
                    wmo.Textures.Add((int) (size - (endPos - br.BaseStream.Position)), br.ReadCString());
                }
            }
        }

        static void ReadMOMT(BinaryReader br, WMORoot wmo)
        {
            wmo.Materials = new Material[wmo.Header.TextureCount];

            for (int i = 0; i < wmo.Materials.Length; i++)
            {
                var mt = new Material
                             {
                                 Flags = (MaterialFlags)br.ReadUInt32(),
                                 Int_1 = br.ReadUInt32(),
                                 BlendMode = br.ReadInt32(),
                                 TextureNameStart = br.ReadInt32(),
                                 SidnColor = br.ReadColor4(),
                                 FrameSidnColor = br.ReadColor4(),
                                 TextureNameEnd = br.ReadInt32(),
                                 DiffColor = br.ReadColor4(),
                                 GroundType = br.ReadInt32(),
                                 Float_1 = br.ReadSingle(),
                                 Float_2 = br.ReadSingle(),
                                 Int_2 = br.ReadInt32(),
                                 Int_3 = br.ReadInt32(),
                                 Int_4 = br.ReadInt32()
                             };

                // these 2 are set in RAM in the client to the associated HTEXTUREs
                br.ReadUInt32();// 0x38
                br.ReadUInt32();// 0x3C

                wmo.Materials[i] = mt;

                //if (mt.Flags != 0)
                    //Console.WriteLine();
            }
        }

        static void ReadMOGN(BinaryReader br, WMORoot wmo, uint size)
        {
            wmo.GroupNames = new Dictionary<int, string>();

            long endPos = br.BaseStream.Position + size;
            while (br.BaseStream.Position < endPos)
            {
                if (br.PeekByte() == 0)
                {
                    br.BaseStream.Position++;
                }
                else
                {
                    wmo.GroupNames.Add((int)(size - (endPos - br.BaseStream.Position)), br.ReadCString());
                }
            }
        }

        static void ReadMOGI(BinaryReader br, WMORoot wmo)
        {
            wmo.GroupInformation = new GroupInformation[wmo.Header.GroupCount];

            for (int i = 0; i < wmo.GroupInformation.Length; i++)
            {
                var g = new GroupInformation {
                                                 Flags = (WMOGroupFlags) br.ReadUInt32(),
                                                 BoundingBox = new BoundingBox(br.ReadWMOVector3(), br.ReadWMOVector3()),
                                                 NameIndex = br.ReadInt32()
                                             };

                wmo.GroupInformation[i] = g;
            }
        }

        static void ReadMOSB(BinaryReader br, WMORoot wmo)
        {
            // skyboxes moved to light.dbc
        }

        static void ReadMOPV(BinaryReader br, WMORoot wmo)
        {
            // PortalCount of 4 x Vector3 to form a rectangle for the doorway
            uint vertexCount = wmo.Header.PortalCount*4;
            wmo.PortalVertices = new Vector3[vertexCount];

            for (var i=0; i<vertexCount; i++)
            {
                wmo.PortalVertices[i] = br.ReadVector3();
            }
        }

        static void ReadMOPT(BinaryReader br, WMORoot wmo)
        {
            wmo.PortalInformation = new PortalInformation[wmo.Header.PortalCount];

            for (var i = 0; i < wmo.Header.PortalCount; i++)
            {
                var p = new PortalInformation
                            {
                                Vertices = new VertexSpan
                                               {
                                                   StartVertex = br.ReadInt16(),
                                                   VertexCount = br.ReadInt16(),
                                               },
                                Plane = br.ReadPlane()
                            };
                wmo.PortalInformation[i] = p;
            }
        }

        static void ReadMOPR(BinaryReader br, WMORoot wmo)
        {
            wmo.PortalRelations = new PortalRelation[wmo.Header.PortalCount];

            for (int i = 0; i < wmo.PortalRelations.Length; i++)
            {
                var r = new PortalRelation
                {
                    PortalIndex = br.ReadUInt16(),
                    GroupIndex = br.ReadUInt16(),
                    Side = br.ReadInt16()
                };
                br.ReadInt16(); // filler

                wmo.PortalRelations[i] = r;
            }
        }

        static void ReadMOVV(BinaryReader br, WMORoot wmo, uint size)
        {
            wmo.VisibleVertices = new Vector3[size/12]; // 12 = sizeof(Vector3), but we can't use sizeof with Vector3

            for (int i = 0; i < wmo.VisibleVertices.Length; i++)
            {
                wmo.VisibleVertices[i] = br.ReadVector3();
            }
        }

        static void ReadMOVB(BinaryReader br, WMORoot wmo, uint size)
        {
            uint blockCount = size/4;
            wmo.VisibleBlocks = new VertexSpan[blockCount];

            for (int i = 0; i < blockCount; i++)
            {
                wmo.VisibleBlocks[i] = new VertexSpan
                                           {
                                               StartVertex = br.ReadInt16(),
                                               VertexCount = br.ReadInt16()
                                           };
            }
        }
        
        static void ReadMOLT(BinaryReader br, WMORoot wmo)
        {
            wmo.LightInfo = new LightInformation[wmo.Header.LightCount];

            for (int i = 0; i < wmo.LightInfo.Length; i++)
            {
                var light = new LightInformation
                                {
                                    Byte_1 = br.ReadByte(),
                                    Byte_2 = br.ReadByte(),
                                    Byte_3 = br.ReadByte(),
                                    Byte_4 = br.ReadByte(),
                                    Color = br.ReadColor4(),
                                    Position = br.ReadVector3(),
                                    Intensity = br.ReadSingle(),
                                    AttenStart = br.ReadSingle(),
                                    AttenEnd = br.ReadSingle(),
                                    Float_4 = br.ReadSingle(),
                                    Float_5 = br.ReadSingle(),
                                    Float_6 = br.ReadSingle(),
                                    Float_7 = br.ReadSingle()
                                };

                //FixVector3(ref light.position);

                wmo.LightInfo[i] = light;
            }
        }

        /// <summary>
        /// Translates a Vector3 read by the ReadVector3 extension method 
        /// into a vector in the coordinate space used by WoW
        /// (X, Y, Z) -> (X, Z, -Y)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="final"></param>
        static void FixVector3(ref Vector3 source)
        {
            var y = source.Y;
            source.Y = source.Z;
            source.Z = -y;
        }

        static void ReadMODS(BinaryReader br, WMORoot wmo)
        {
            wmo.DoodadSets = new DoodadSet[wmo.Header.DoodadSetCount];

            for (int i = 0; i < wmo.Header.DoodadSetCount; i++)
            {
                var d = new DoodadSet
                {
                    SetName = br.ReadFixedString(20),
                    FirstInstanceIndex = br.ReadUInt32(),
                    InstanceCount = br.ReadUInt32()
                };
                br.ReadInt32(); // padding

                wmo.DoodadSets[i] = d;
            }
        }

        static void ReadMODN(BinaryReader br, WMORoot wmo, uint size)
        {
            wmo.DoodadFiles = new Dictionary<int, string>();

            long endPos = br.BaseStream.Position + size;
            while (br.BaseStream.Position < endPos)
            {
                if (br.PeekByte() == 0)
                {
                    br.BaseStream.Position++;
                }
                else
                {
                    //doodadNames.Add(size - (endPos - br.BaseStream.Position), br.ReadCString());
                    wmo.DoodadFiles.Add((int) (size - (endPos - br.BaseStream.Position)), br.ReadCString());
                }
            }
        }

        static void ReadMODD(BinaryReader br, WMORoot wmo, uint size)
        {
            // Why oh why is wmo.Header.DoodadCount wrong sometimes
            // 40 is the size of DoodadDefinition
            wmo.DoodadDefinitions = new DoodadDefinition[size / 40];

            for (var i = 0; i < wmo.DoodadDefinitions.Length; i++)
            {
                var dd = new DoodadDefinition
                             {
                                 NameIndex = br.ReadInt32(),
                                 Position = br.ReadVector3(),
                                 Rotation = br.ReadQuaternion(),
                                 Scale = br.ReadSingle(),
                                 Color = br.ReadColor4()
                             };

                if (dd.NameIndex != -1)
                {
                    if(!wmo.DoodadFiles.TryGetValue(dd.NameIndex, out dd.FilePath))
                    {
                        dd.FilePath = "";
                        log.Error(String.Format("Doodad File Path for index: {0} missing from the Dictionary!", dd.NameIndex));
                    }
                }

                wmo.DoodadDefinitions[i] = dd;
            }
        }

        static void ReadMFOG(BinaryReader br, WMORoot wmo, uint size)
        {
            wmo.Fogs = new Fog[size/48];

            for (int i = 0; i < wmo.Fogs.Length; i++)
            {
                var fog = new Fog
                              {
                                  Flags = (FogFlags)br.ReadUInt32(),
                                  Position = br.ReadVector3(),
                                  Start = br.ReadSingle(),
                                  End = br.ReadSingle(),
                                  FogInfo_FOG = new FogInfo
                                                 {
                                                     End = br.ReadSingle(),
                                                     StartScalar = br.ReadSingle(),
                                                     Color = br.ReadColor4()
                                                 },
                                  FogInfo_UWFOG = new FogInfo
                                                 {
                                                     End = br.ReadSingle(),
                                                     StartScalar = br.ReadSingle(),
                                                     Color = br.ReadColor4()
                                                 }
                              };

                wmo.Fogs[i] = fog;
            }
        }

        static void ReadMCVP(BinaryReader br, WMORoot wmo, uint size)
        {
            wmo.ComplexVolumePlanes = new Plane[size/(4*sizeof (float))];

            for (int i = 0; i < wmo.ComplexVolumePlanes.Length; i++)
            {
                wmo.ComplexVolumePlanes[i] = br.ReadPlane();
            }
        }



		/// <summary>
		/// Adds a WMO to the manager
		/// </summary>
		public static WMORoot ReadWMO(MpqLibrarian librarian, MapObjectDefinition currentMODF)
		{
			// Parse the WMORoot
			var wmoRoot = WMOReader.ReadWMO(librarian, currentMODF.FilePath);

			if (wmoRoot == null) return null;

			// Parse the WMOGroups
			for (var wmoGroup = 0; wmoGroup < wmoRoot.Header.GroupCount; wmoGroup++)
			{
				var newFile = wmoRoot.FilePath.Substring(0, wmoRoot.FilePath.LastIndexOf('.'));
				var currentFilePath = String.Format("{0}_{1:000}.wmo", newFile, wmoGroup);

				var group = WMOGroupReader.Process(librarian, currentFilePath, wmoRoot, wmoGroup);

				wmoRoot.Groups[wmoGroup] = group;
			}

			//wmoRoot.DumpLiqChunks();

			// Parse in the WMO's M2s
			var curDoodadSet = currentMODF.DoodadSetId;

			var setIndices = new List<int> { 0 };
			if (curDoodadSet > 0) setIndices.Add(curDoodadSet);
		    var setDefs = new List<DoodadSet>(setIndices.Count);
		    foreach (var index in setIndices)
		    {
				if (index >= wmoRoot.DoodadSets.Length)
				{
					log.Error("Invalid index {0} into wmoRoot.DoodadSet array with id", index, curDoodadSet);
					continue;
				}
		    	setDefs.Add(wmoRoot.DoodadSets[index]);
		    }

		    var m2List = new List<M2>();
            foreach (var def in setDefs)
			{
				var doodadSetOffset = def.FirstInstanceIndex;
				var doodadSetCount = def.InstanceCount;
				for (var i = doodadSetOffset; i < (doodadSetOffset + doodadSetCount); i++)
				{
					var curDoodadDef = wmoRoot.DoodadDefinitions[i];
					if (string.IsNullOrEmpty(curDoodadDef.FilePath))
					{
						log.Error("Encountered Doodad with empty file path");
						continue;
					}
					var curM2 = M2Reader.ReadM2(librarian, curDoodadDef.FilePath, true);

					var tempIndices = new List<int>();
					for (var j = 0; j < curM2.BoundingTriangles.Length; j++)
					{
						var tri = curM2.BoundingTriangles[j];

						tempIndices.Add(tri.Index2);
						tempIndices.Add(tri.Index1);
						tempIndices.Add(tri.Index0);
					}

					var rotatedM2 = TransformWMOM2(curM2, tempIndices, curDoodadDef);
					m2List.Add(rotatedM2);
				}
			}

		    wmoRoot.WMOM2s = m2List.ToArray();
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

				offset = currentWMO.WmoLiquidVertices.Count;
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
					offset = currentWMO.WmoM2Vertices.Count;
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
		private static M2 TransformWMOM2(M2Model model, List<int> indices, DoodadDefinition modd)
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

            currentM2.Indices.AddRange(indices);
            return currentM2;
		}
    }

}
