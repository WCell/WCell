using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using WCell.Constants;
using WCell.MPQTool;
using WCell.Tools.Maps.Parsing;
using WCell.Tools.Maps.Parsing.ADT;
using WCell.Tools.Maps.Parsing.ADT.Components;
using WCell.Tools.Maps.Parsing.M2s;
using WCell.Tools.Maps.Parsing.WDT;
using WCell.Tools.Maps.Parsing.WMO;
using WCell.Tools.Maps.Parsing.WMO.Components;
using WCell.Tools.Maps.Structures;
using WCell.Tools.Maps.Utils;
using WCell.Util.Graphics;
using WCell.Util.Toolshed;

namespace WCell.Tools.Maps
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
            WDTParser.Parsed += ExtractMapObjects;
		}

        private static void ExtractMapObjects(WDTFile wdt)
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
                        if (MapTileExtractor.TerrainInfo != null)
                        {
                            adt = MapTileExtractor.TerrainInfo[tileY, tileX];
                        }

                        if (adt == null)
                        {
                            adt = ADTParser.Process(wdt.Manager, wdt.Entry, tileY, tileX);
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
            if (MapTileExtractor.ModelsToIgnore.Contains(filePath)) return;
            if (LoadedM2Models.ContainsKey(filePath)) return;

            var m2Model = M2ModelParser.Process(WDTParser.MpqManager, filePath);
            if (m2Model == null) return;

            if (m2Model.BoundingVertices.IsNullOrEmpty())
            {
                MapTileExtractor.ModelsToIgnore.Add(filePath);
                return;
            }

            LoadedM2Models.Add(filePath, m2Model);
        }
        #endregion

		#region M2 Write
		private static void WriteMapM2s(DBCMapEntry mapEntry)
		{
            var path = Path.Combine(ToolConfig.M2Dir, mapEntry.Id.ToString());
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
            foreach (var def in list)
            {
                if (string.IsNullOrEmpty(def.FilePath)) continue;

                WMORoot root;
                if (!LoadedWMORoots.TryGetValue(def.FilePath, out root))
                {
                    root = WMORootParser.Process(WDTParser.MpqManager, def.FilePath);
                    LoadedWMORoots.Add(def.FilePath, root);
                }

                ExtractWMOGroups(def.FilePath, root);
                ExtractWMOM2s(root);
            }
		}

        private static void ExtractWMOGroups(string filePath, WMORoot root)
        {
            for (var wmoGroup = 0; wmoGroup < root.Header.GroupCount; wmoGroup++)
            {
                var newFile = filePath.Substring(0, filePath.LastIndexOf('.'));
                var newFilePath = String.Format("{0}_{1:000}.wmo", newFile, wmoGroup);

                var group = WMOGroupParser.Process(WDTParser.MpqManager, newFilePath, root, wmoGroup);
                if (group == null) continue;

                root.Groups[wmoGroup] = group;
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
                    foreach (var dRefId in wmoGroup.DoodadReferences)
                    {
                        var def = root.DoodadDefinitions[dRefId];
                        if (def == null) continue;
                        if (string.IsNullOrEmpty(def.FilePath)) continue;
                        if (MapTileExtractor.ModelsToIgnore.Contains(def.FilePath)) continue;

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
        public static void WriteMapWMOs(DBCMapEntry entry)
		{
            var path = Path.Combine(ToolConfig.M2Dir, entry.Id.ToString());
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
                    if (MapTileExtractor.ModelsToIgnore.Contains(def.FilePath)) continue;

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
                if (MapTileExtractor.ModelsToIgnore.Contains(def.FilePath)) continue;

                list.Add(defId);
            }

            writer.Write(list);
        }

		private static void WriteBSPTree(BinaryWriter writer, WMOGroup group)
		{
		    var tree = new BSPTree(group.BSPNodes);
            
			writer.Write(tree.rootId);

			writer.Write(tree.nodes.Length);
			for (var i = 0; i < tree.nodes.Length; i++)
			{
				WriteBSPNode(writer, tree.nodes[i]);
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

	public class RegionWMOs : RegionObjects<WMO>
	{
	}

	public class RegionObjects<O>
	{
		public bool HasTiles = true;
		public TileObjects<O>[,] ObjectsByTile;
	}

	public class TileObjects<O> : List<O>
	{
		public TileObjects()
		{
		}

		public TileObjects(int capacity)
			: base(capacity)
		{
		}

		public TileObjects(IEnumerable<O> collection)
			: base(collection)
		{
		}
	}

	public class M2Object
	{
		public BoundingBox Bounds;
		public Vector3[] Vertices;
		public Index3[] Triangles;
	}

	public class WMO
	{
		public BoundingBox Bounds;
		public float RotationModelY;
		public Vector3 WorldPos;
		public WMOSubGroup[] BuildingGroups;
	}

	public class WMOSubGroup
	{
		public BoundingBox Bounds;
		public Vector3[] Vertices;
		public BSPTree Tree;
	}
}
