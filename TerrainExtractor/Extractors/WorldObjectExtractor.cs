using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using TerrainDisplay;
using TerrainDisplay.MPQ.ADT;
using TerrainDisplay.MPQ.ADT.Components;
using TerrainDisplay.MPQ.M2;
using TerrainDisplay.MPQ.WDT;
using TerrainDisplay.MPQ.WMO;
using TerrainDisplay.MPQ.WMO.Components;
using TerrainDisplay.Util;
using TerrainDisplay.World.DBC;
using TerrainExtractor.Parsers;
using WCell.Util.Graphics;

namespace TerrainExtractor.Extractors
{
    public static class WorldObjectExtractor
	{
		private const string m2FileType = "m2x";
        private const string wmoFileType = "wmo";
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static Dictionary<string, M2Model> LoadedM2Models = new Dictionary<string, M2Model>();
        public static Dictionary<string, WMORoot> LoadedWMORoots = new Dictionary<string, WMORoot>();
        

		public static void PrepareExtractor()
		{
            WDTExtractor.Parsed += ExtractMapObjects;
		}

        private static void ExtractMapObjects(WDT wdt)
        {
            if (wdt == null) return;
            if (wdt.Header.Header1.HasFlag(WDTFlags.GlobalWMO))
            {
                // No terrain, load the global WMO
                if (wdt.WmoDefinitions == null) return;
                ExtractWMOs(wdt.WmoDefinitions);
            }
            else
            {
                for (var tileX = 0; tileX < TerrainConstants.TilesPerMapSide; tileX++)
                {
                    for (var tileY = 0; tileY < TerrainConstants.TilesPerMapSide; tileY++)
                    {
                        if (!wdt.TileProfile[tileY, tileX]) continue;

                        ADT adt = null;
                        if (TileExtractor.TerrainInfo != null)
                        {
                            adt = TileExtractor.TerrainInfo[tileY, tileX];
                        }

                        if (adt == null)
                        {
                            var tileId = new TileIdentifier
                            {
                                MapId = wdt.Entry.Id,
                                MapName = wdt.Entry.InternalName,
                                TileX = tileX,
                                TileY = tileY
                            };
                            adt = ADTParser.Process(wdt.Manager, tileId);
                        }
                        if (adt == null) continue;

                        ExtractTileWMOs(adt);
                        ExtractTileM2s(adt);
                    }
                }
            }

            PrepareMapWMOs();
            PrepareMapM2s();

            WriteMapM2s(wdt.Entry);
            WriteMapWMOs(wdt.Entry);
        }
		
        #region M2 Extract
        private static void ExtractWMOM2s(WMORoot root)
		{
		    foreach (var def in root.DoodadDefinitions)
		    {
		        ExtractM2Model(def.FilePath);
		    }
		}

	    private static void ExtractTileM2s(ADT adt)
		{
			foreach (var def in adt.DoodadDefinitions)
			{
                ExtractM2Model(def.FilePath);
			}
		}

        private static void ExtractM2Model(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            if (TileExtractor.ModelsToIgnore.Contains(filePath)) return;
            if (LoadedM2Models.ContainsKey(filePath)) return;

            var m2Model = M2ModelParser.Process(WDTExtractor.MpqManager, filePath);
            if (m2Model == null) return;

            if (m2Model.BoundingVertices.IsNullOrEmpty())
            {
                TileExtractor.ModelsToIgnore.Add(filePath);
                return;
            }

            LoadedM2Models.Add(filePath, m2Model);
        }
        #endregion

		#region M2 Write
		private static void WriteMapM2s(MapInfo mapEntry)
		{
            var path = Path.Combine(TerrainDisplayConfig.M2Dir, mapEntry.Id.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

		    foreach (var m2ModelPair in LoadedM2Models)
		    {
		        var filePath = Path.Combine(path, m2ModelPair.Key);
		        var dirPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                var fullFilePath = Path.ChangeExtension(filePath, TerrainConstants.M2FileExtension);
                
		        var file = File.Create(fullFilePath);
                var writer = new BinaryWriter(file);
                
                WriteModel(writer, m2ModelPair.Value);
                file.Close();
		    }
		}

		private static void WriteModel(BinaryWriter writer, M2Model m2Model)
		{
			if (m2Model == null)
			{
				Console.WriteLine("Cannot write null Model to file.");
				return;
			}

            writer.Write(m2FileType);
			writer.Write(m2Model.Header.BoundingBox);
            writer.Write(m2Model.BoundingVertices);
			writer.Write(m2Model.BoundingTriangles);
		}
		#endregion

		#region WMO Read
        public static void ExtractTileWMOs(ADT adt)
        {
            var list = adt.ObjectDefinitions;
            ExtractWMOs(list);
        }

		public static void ExtractWMOs(List<MapObjectDefinition> list)
		{
		    foreach (var def in list.Where(def => !string.IsNullOrEmpty(def.FilePath)))
		    {
		        WMORoot root;
		        if (!LoadedWMORoots.TryGetValue(def.FilePath, out root))
		        {
		            root = WMORootParser.Process(WDTExtractor.MpqManager, def.FilePath);
		            LoadedWMORoots.Add(def.FilePath, root);
		        }

		        ExtractWMOGroups(def.FilePath, root);
		        ExtractWMOM2s(root);
		    }
		}

        private static void ExtractWMOGroups(string filePath, WMORoot root)
        {
            for (var grpIndex = 0; grpIndex < root.Header.GroupCount; grpIndex++)
            {
                var newFile = filePath.Substring(0, filePath.LastIndexOf('.'));
                var newFilePath = String.Format("{0}_{1:000}.wmo", newFile, grpIndex);

                var group = WMOGroupParser.Process(WDTExtractor.MpqManager, newFilePath, root, grpIndex);
                if (group == null) continue;

                root.Groups[grpIndex] = group;
            }
        }
		#endregion

        #region WMO & M2 Prepare
        private static void PrepareMapWMOs()
        {
            // Reposition the m2s contained within the wmos
            foreach (var root in LoadedWMORoots.Values)
            {
                foreach (var wmoGroup in root.Groups)
                {
                    if (wmoGroup.DoodadReferences.IsNullOrEmpty()) continue;
                    PrepareWMOGroupDoodadReferences(root, wmoGroup);

                    // Determine the liquid type for any liquid chunks
                    if (!wmoGroup.Header.HasMLIQ) continue;
                    uint interimLiquidType;
                    if ((root.Header.Flags & WMORootHeaderFlags.Flag_0x4) != 0)
                    {
                        interimLiquidType = wmoGroup.Header.LiquidType;
                    }
                    else
                    {
                        if (wmoGroup.Header.LiquidType == 15) // Green Lava
                            interimLiquidType = 0u; // :( no green lava
                        else
                            interimLiquidType = wmoGroup.Header.LiquidType + 1; // eh?
                    }
                    var liquidType = interimLiquidType;
                    if (interimLiquidType < 21) // Less than Naxxramas - Slime. After 20 they start being special types
                    {
                        if (interimLiquidType > 0)
                        {
                            switch ((interimLiquidType - 1) & 3)
                            {
                                case 0u:
                                    var flags = wmoGroup.Header.Flags; // same as the MOGP header flags
                                    liquidType = ((flags & WMOGroupFlags.Flag_0x80000) != 0) ? 14u : 13u;
                                        // WMO Water normally, but with the 0x80000 flag it's WMO Ocean
                                    break;
                                case 1u:
                                    liquidType = 14u; // WMO Ocean
                                    break;
                                case 2u:
                                    liquidType = 19u; // WMO Magma
                                    break;
                                case 3u:
                                    liquidType = 20u; // WMO Slime
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    wmoGroup.LiquidInfo.LiquidType = liquidType;
                }
            }
        }

	    private static void PrepareWMOGroupDoodadReferences(WMORoot root, WMOGroup wmoGroup)
	    {
	        foreach (var dRefId in wmoGroup.DoodadReferences)
	        {
	            var def = root.DoodadDefinitions[dRefId];
	            if (def == null) continue;
	            if (string.IsNullOrEmpty(def.FilePath)) continue;
	            if (TileExtractor.ModelsToIgnore.Contains(def.FilePath)) continue;

	            // Calculate and store the models' transform matrices
	            Matrix scaleMatrix;
	            Matrix.CreateScale(def.Scale, out scaleMatrix);

	            Matrix rotMatrix;
	            Matrix.CreateFromQuaternion(ref def.Rotation, out rotMatrix);

	            Matrix modelToWMO;
	            Matrix.Multiply(ref scaleMatrix, ref rotMatrix, out modelToWMO);

	            Matrix wmoToModel;
	            Matrix.Invert(ref modelToWMO, out wmoToModel);
	            def.ModelToWMO = modelToWMO;
	            def.WMOToModel = wmoToModel;


	            M2Model model;
	            if (!LoadedM2Models.TryGetValue(def.FilePath, out model))
	            {
	                log.Error(String.Format("M2Model file: {0} missing from the Dictionary!", def.FilePath));
	                continue;
	            }

	            // Calculate the wmoSpace bounding box for this model
	            CalculateModelsWMOSpaceBounds(def, model);
	        }
	    }


	    private static void CalculateModelsWMOSpaceBounds(DoodadDefinition def, M2Model model)
	    {
	        var wmoSpaceVecs = new List<Vector3>(model.BoundingVertices.Length);
	        for (var j = 0; j < model.BoundingVertices.Length; j++)
	        {
	            Vector3 rotated;
	            Vector3.Transform(ref model.BoundingVertices[j], ref def.ModelToWMO, out rotated);

	            Vector3 final;
	            Vector3.Add(ref rotated, ref def.Position, out final);
	            wmoSpaceVecs.Add(final);
	        }
	        def.Extents = new BoundingBox(wmoSpaceVecs.ToArray());
	    }

        private static void PrepareMapM2s()
        {
            
        }
        #endregion

        #region WMO Write
        public static void WriteMapWMOs(MapInfo entry)
		{
            var path = Path.Combine(TerrainDisplayConfig.M2Dir, entry.Id.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

		    foreach (var wmoPair in LoadedWMORoots)
		    {
		        var filePath = Path.Combine(path, wmoPair.Key);
		        var dirPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                var fullFilePath = Path.ChangeExtension(filePath, TerrainConstants.WMOFileExtension);
                
                var file = File.Create(fullFilePath);
                var writer = new BinaryWriter(file);

                WriteWMO(writer, wmoPair.Value);
                file.Close();
            }
		}

		private static void WriteWMO(BinaryWriter writer, WMORoot root)
		{
			if (root == null)
			{
				Console.WriteLine("Cannot write null WMORoot to file.");
				return;
			}

            // Magic fileType
            writer.Write(wmoFileType);

			writer.Write(root.Header.BoundingBox);
            writer.Write(root.Header.WMOId);

		    WriteDoodadDefinitions(writer, root);
		    
		    WriteGroups(writer, root);
		}
        
        private static void WriteDoodadDefinitions(BinaryWriter writer, WMORoot root)
        {
            var doodadSets = new List<List<int>>();
            foreach(var set in root.DoodadSets)
            {
                var list = new List<int>((int)set.InstanceCount);
                for (var i = 0; i < set.InstanceCount; i++)
                {
                    var idx = set.FirstInstanceIndex + i;
                    var def = root.DoodadDefinitions[idx];
                    if (def == null) continue;
                    if (TileExtractor.ModelsToIgnore.Contains(def.FilePath)) continue;

                    list.Add((int)idx);
                }

                doodadSets.Add(list);
            }

            writer.Write(doodadSets.Count);
            foreach (var doodadSet in doodadSets)
            {
                writer.Write(doodadSet.Count);
                foreach (var defId in doodadSet)
                {
                    var def = root.DoodadDefinitions[defId];

                    writer.Write(defId);
                    writer.Write(def.FilePath);
                    writer.Write(def.Position);
                    writer.Write(def.Extents);
                    writer.Write(def.WMOToModel);
                    writer.Write(def.ModelToWMO);
                }
            }
        }

        private static void WriteGroups(BinaryWriter writer, WMORoot root)
        {
            if (root.GroupInformation == null) return;
            if (root.Groups == null) return;

            writer.Write(root.GroupInformation.Length);
            for (var i = 0; i < root.GroupInformation.Length; i++)
            {
                var info = root.GroupInformation[i];
                var group = root.Groups[i];

                writer.Write((uint)info.Flags);
                writer.Write(info.BoundingBox);
                writer.Write(group.Header.WMOGroupId);

                WriteGroupDoodadRefs(writer, root, group);

                writer.Write(group.Header.HasMLIQ);
                if (group.Header.HasMLIQ)
                {
                    WriteGroupLiquidInfo(writer, group);
                }

                writer.Write(group.Vertices);
                WriteBSPTree(writer, group);
            }
        }

        private static void WriteGroupDoodadRefs(BinaryWriter writer, WMORoot root, WMOGroup group)
        {
            if (group.DoodadReferences.IsNullOrEmpty())
            {
                writer.Write(0);
                return;
            }

            var list = new List<int>();
            foreach (var defId in group.DoodadReferences)
            {
                var def = root.DoodadDefinitions[defId];
                if (def == null) continue;
                if (TileExtractor.ModelsToIgnore.Contains(def.FilePath)) continue;

                list.Add(defId);
            }

            writer.Write(list);
        }

        private static void WriteGroupLiquidInfo(BinaryWriter writer, WMOGroup group)
        {
            // The zero point on the map
            writer.Write(group.LiquidInfo.BaseCoordinates);

            // Dimensions of the Map
            writer.Write(group.LiquidInfo.XTileCount);
            writer.Write(group.LiquidInfo.YTileCount);

            // Write the LiquidTile Map
            for (var y = 0; y < group.LiquidInfo.YTileCount; y++)
            {
                for (var x = 0; x < group.LiquidInfo.XTileCount; x++)
                {
                    writer.Write((group.LiquidInfo.LiquidTileFlags[x, y] & 0x0F) != 0x0F);
                }
            }

            for (var y = 0; y < group.LiquidInfo.YVertexCount; y++)
            {
                for (var x = 0; x < group.LiquidInfo.XVertexCount; x++)
                {
                    writer.Write(group.LiquidInfo.HeightMapMax[x, y]);
                }
            }
        }

		private static void WriteBSPTree(BinaryWriter writer, WMOGroup group)
		{
		    var tree = new BSPTree(group.BSPNodes);
            
			writer.Write(tree.rootId);

			writer.Write(tree.nodes.Count);
			foreach (var node in tree.nodes)
			{
			    WriteBSPNode(writer, node);
			}
		}

		private static void WriteBSPNode(BinaryWriter writer, BSPNode node)
		{
			writer.Write((byte)node.flags);
			writer.Write(node.negChild);
			writer.Write(node.posChild);
			writer.Write(node.planeDist);

			if (node.TriIndices != null)
			{
                writer.Write(node.TriIndices);
			}
			else
			{
				writer.Write(0);
			}
		}
		#endregion
    }
}
