using System.Globalization;

using WCell.Core.Addons;
using WCell.Core.Initialization;


namespace WCell.Addons
{
    public class GossipMenus : WCellAddonBase
    {
        public override string Name
        {
            get { return "WCell Gossip Menus"; }
        }

        public override string ShortName
        {
            get { return "Gossip"; }
        }

        public override string Author
        {
            get { return "Froid"; }
        }

        public override string Website
        {
            get { return ""; }
        }

        public override void TearDown()
        {
            // ?
        }

        public override string GetLocalizedName(CultureInfo culture)
        {
            return "WCell Gossip Menus";
        }

        [Initialization(InitializationPass.Last, "Initialize Gossip Addon")]
        public static void Init()
        {

        }
    }
}
