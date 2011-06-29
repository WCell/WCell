using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
			if (!TerrainProgram.UseMultiThreadedLoading)
			{
				return LoadTileSerially(tileId);
			}

			var currentADT = ADTParser.Process(MpqTerrainManager.MpqManager, tileId);
			var taskCount = currentADT.ObjectDefinitions.Count + currentADT.DoodadDefinitions.Count;

			foreach (var objectDef in currentADT.ObjectDefinitions)
			{
				var reference = objectDef;
				//_mpqTerrainManager.WMOManager.AddWMO(reference);
				Task.Factory.StartNew(() =>
					{
						_mpqTerrainManager.WMOManager.AddWMO(reference);
						Console.WriteLine("{0} tasks left...", taskCount);
						if (Interlocked.Decrement(ref taskCount) == 0)
						{
							lock (this)
							{
								Monitor.Pulse(this);
							}
						}
					});
			}

			foreach (var doodadDef in currentADT.DoodadDefinitions)
			{
				var reference = doodadDef;
				//_mpqTerrainManager.M2Manager.Add(reference);
				Task.Factory.StartNew(() =>
					{
						_mpqTerrainManager.M2Manager.Add(reference);
						Console.WriteLine("{0} tasks left...", taskCount);
						if (Interlocked.Decrement(ref taskCount) == 0)
						{
							lock (this)
							{
								Monitor.Pulse(this);
							}
						}
					});
			}

			currentADT.GenerateHeightVertexAndIndices();
			currentADT.GenerateLiquidVertexAndIndices();
			currentADT.LoadQuadTree();

			_ADTs.Add(currentADT);

			lock (this)
			{
				Monitor.Wait(this);
			}

			return true;
		}

		/// <summary>
		/// Loads the given ADT on a single thread
		/// </summary>
		public bool LoadTileSerially(TileIdentifier tileId)
		{
			var currentADT = ADTParser.Process(MpqTerrainManager.MpqManager, tileId);

			foreach (var objectDef in currentADT.ObjectDefinitions)
			{
				var reference = objectDef;
				_mpqTerrainManager.WMOManager.AddWMO(reference);
			}

			foreach (var doodadDef in currentADT.DoodadDefinitions)
			{
				var reference = doodadDef;
				_mpqTerrainManager.M2Manager.Add(reference);
			}

			currentADT.GenerateHeightVertexAndIndices();
			currentADT.GenerateLiquidVertexAndIndices();
			currentADT.LoadQuadTree();

			_ADTs.Add(currentADT);

			return false;
		}
	}
}