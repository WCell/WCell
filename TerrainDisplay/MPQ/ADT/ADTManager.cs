using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TerrainDisplay.MPQ.ADT
{
    /// <summary>
    /// The ADTManager is responsible for handling all the different ADTs that we are going to be loading up.
    /// </summary>
    public class ADTManager : IADTManager
    {
        #region variables
        /// <summary>
        /// List of all ADTs managed by this ADT manager
        /// </summary>
        private readonly List<ADTBase> _ADTs = new List<ADTBase>();

        public IList<ADTBase> MapTiles
        {
            get { return _ADTs; }
        }
        #endregion

        private readonly MpqTerrainManager _mpqTerrainManager;

        #region constructors

        /// <summary>
        /// Creates a new instance of the ADT manager.
        /// </summary>
        /// <param name="continent">Continent of the ADT</param>
        /// <param name="dataDirectory">Base directory for all MPQ data WITH TRAILING SLASHES</param>
        /// <param name="mpqTerrainManager">Handles organization of all terrain elements</param>
        /// <example>ADTManager myADTManager = new ADTManager(continent.Azeroth, "C:\\mpq\\");</example>
        public ADTManager(MpqTerrainManager mpqTerrainManager)
        {
            _mpqTerrainManager = mpqTerrainManager;
        }

        #endregion

        /// <summary>
        /// Loads an ADT into the manager.
        /// </summary>
        /// <param name="tileId">The <see cref="TileIdentifier"/> describing the tile to load.</param>
        public bool LoadTile(TileIdentifier tileId)
        {
            var currentADT = ADTParser.Process(MpqTerrainManager.MpqManager, tileId);

            foreach (var objectDef in currentADT.ObjectDefinitions)
            {
                _mpqTerrainManager.WMOManager.AddWMO(objectDef);
            }

            foreach (var doodadDef in currentADT.DoodadDefinitions)
            {
                _mpqTerrainManager.M2Manager.Add(doodadDef);
            }

            currentADT.GenerateHeightVertexAndIndices();
            currentADT.GenerateLiquidVertexAndIndices();

            _ADTs.Add(currentADT);

            return false;
        }
    }
}