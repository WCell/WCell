using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MPQNav.MPQ.ADT
{
    /// <summary>
    /// The ADTManager is responsible for handling all the different ADTs that we are going to be loading up.
    /// </summary>
    public class ADTManager : IADTManager
    {
        #region variables
        /// <summary>
        /// Default ADT Path
        /// </summary>
        private const string _adtPath = "World\\Maps\\";

        /// <summary>
        /// List of all ADTs managed by this ADT manager
        /// </summary>
        private readonly List<ADTBase> _ADTs = new List<ADTBase>();

        public IList<ADTBase> MapTiles
        {
            get { return _ADTs; }
        }

        /// <summary>
        /// Base directory for all MPQ data.
        /// </summary>
        private readonly string _basePath;

        /// <summary>
        /// Continent of the ADT Manager
        /// </summary>
        private readonly ContinentType _continent;

        /// <summary>
        /// Boolean result stating if this manager is loaded or not.
        /// </summary>
        private readonly bool _loaded;

        #endregion

        private readonly MpqTerrainManager _mpqTerrainManager;

        #region constructors

        /// <summary>
        /// Creates a new instance of the ADT manager.
        /// </summary>
        /// <param name="c">Continent of the ADT</param>
        /// <param name="dataDirectory">Base directory for all MPQ data WITH TRAILING SLASHES</param>
        /// <param name="mpqTerrainManager">Handles organization of all terrain elements</param>
        /// <example>ADTManager myADTManager = new ADTManager(continent.Azeroth, "C:\\mpq\\");</example>
        public ADTManager(string dataDirectory, ContinentType c, MpqTerrainManager mpqTerrainManager)
        {
            if (Directory.Exists(dataDirectory))
            {
                _loaded = true;
                _basePath = Path.Combine(dataDirectory, _adtPath);
                _continent = c;
                _mpqTerrainManager = mpqTerrainManager;
            }
            else
            {
                MessageBox.Show("Invalid data directory entered. Please exit and update your app.CONFIG file",
                                "Invalid Data Directory");
            }
        }

        #endregion

        /// <summary>
        /// Loads an ADT into the manager.
        /// </summary>
        /// <param name="tileX">X coordinate of the ADT in the 64 x 64 Grid 
        /// (The x-axis points into the screen and represents rows in the grid.)</param>
        /// <param name="tileY">Y coordinate of the ADT in the 64 x 64 grid 
        /// (The y-axis points left and also represents columns in the grid.)</param>
        public bool LoadTile(int tileX, int tileY)
        {
            if (_loaded == false)
            {
                MessageBox.Show("ADT Manager not loaded, aborting loading ADT file.", "ADT Manager not loaded.");
                return false;
            }
            var continentPath = Path.Combine(_basePath, _continent.ToString());

            if (!Directory.Exists(continentPath))
            {
                throw new Exception("Continent data missing");
            }

            // Tiles and Chunks are indexed as [col, row] for some wierd reason.
            var filePath = string.Format("{0}\\{1}_{2:00}_{3:00}.adt", continentPath, _continent, tileY, tileX);

            if (!File.Exists(filePath))
            {
                throw new Exception("ADT Doesn't exist: " + filePath);
            }
            
            var currentADT = ADTParser.Process(filePath, _mpqTerrainManager);

            foreach (var objectDef in currentADT.ObjectDefinitions)
            {
                //if (objectDef.UniqueId != 15377) continue;
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