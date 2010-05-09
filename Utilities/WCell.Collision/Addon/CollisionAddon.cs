using System.Globalization;
using WCell.Core.Addons;
using WCell.Core.Initialization;
using WCell.RealmServer.Global;

namespace WCell.Collision.Addon
{
    public class CollisionAddon : WCellAddonBase
    {
        private const string name = "Terrain Provider";
        private const string shortName = "TerrainProvider";
        private const string author = "fubeca";
        private const string website = "www.wcell.org";

        /// <summary>
        /// The culture-invariant name of this Addon
        /// </summary>
        public override string Name
        {
            get { return name; }
        }

        /// <summary>
        /// A shorthand name of the Addon that does not contain any spaces.
        ///  Used as unique ID for this Addon.
        /// </summary>
        public override string ShortName
        {
            get { return shortName; }
        }

        /// <summary>
        /// The name of the Author
        /// </summary>
        public override string Author
        {
            get { return author; }
        }

        /// <summary>
        /// Website (where this Addon can be found)
        /// </summary>
        public override string Website
        {
            get { return website; }
        }

        /// <summary>
        /// The localized name, in the given culture
        /// </summary>
        public override string GetLocalizedName(CultureInfo culture)
        {
            return name;
        }

    	public override void TearDown()
    	{
    		
    	}

    	[Initialization(InitializationPass.First, "Initialize Terrain Addon")]
        public static void Init()
        {
            TerrainMgr.Provider = new FullTerrainProvider();
        }
    }
}
