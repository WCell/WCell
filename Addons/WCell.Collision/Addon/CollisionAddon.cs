using System.Globalization;
using WCell.Core.Addons;
using WCell.Core.Initialization;
using WCell.RealmServer.Global;

namespace WCell.Collision.Addon
{
	public class CollisionAddon : WCellAddonBase
	{
		/// <summary>
		/// The culture-invariant name of this Addon
		/// </summary>
		public override string Name
		{
			get { return "Terrain Provider"; }
		}

		/// <summary>
		/// A shorthand name of the Addon that does not contain any spaces.
		///  Used as unique ID for this Addon.
		/// </summary>
		public override string ShortName
		{
			get { return "Terrain"; }
		}

		/// <summary>
		/// The name of the Author
		/// </summary>
		public override string Author
		{
			get { return "fubeca"; }
		}

		/// <summary>
		/// Website (where this Addon can be found)
		/// </summary>
		public override string Website
		{
			get { return "http://www.wcell.org"; }
		}

		/// <summary>
		/// The localized name, in the given culture
		/// </summary>
		public override string GetLocalizedName(CultureInfo culture)
		{
			return "Terrain Provider";
		}

		public override void TearDown()
		{

		}

		[Initialization(InitializationPass.First, "Initialize Terrain Addon")]
		public static void Init()
		{
			// TODO: Stream is read beyond end; memory inflation
            // TODO: Above is because map file format changed in terrain project, update wcell to use new structure
            // See TileExtractor.cs WriteChunkInfo line 500++
            // vs WorlMapTile.cs ReadHeightMap line 296++
			//TerrainMgr.Provider = new FullTerrainProvider();
		}
	}
}