using WCell.Util.Logging;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Debugging
{
	public static class RegionExtensions
	{
		static Logger s_log = LogManager.GetCurrentClassLogger();

		public static int GetWaitDelay(this Region region)
		{
			return region.UpdateDelay + 200;
		}
	}
}