using WCell.RealmServer.Global;

namespace WCell.RealmServer.Debugging
{
	public static class MapExtensions
	{
		public static int GetWaitDelay(this Map map)
		{
			return map.UpdateDelay + 200;
		}
	}
}