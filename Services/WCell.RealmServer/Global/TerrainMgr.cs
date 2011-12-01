using WCell.Constants.World;
using WCell.Core.Terrain;

namespace WCell.RealmServer.Global
{
	public static class TerrainMgr
	{
		/// <summary>
		/// Use InitializationPass.First in Addon to set a custom provider
		/// </summary>
		public static ITerrainProvider Provider;

		public static readonly ITerrain[] Terrains = new ITerrain[(int)MapId.End + 1000];

		#region Init
		/// <summary>
		/// Called by World
		/// </summary>
		internal static void InitTerrain()
		{
			if (Provider == null)
			{
			    Provider = new DefaultTerrainProvider();
			}

			foreach (var rgn in World.MapTemplates)
			{
				if (rgn != null)
				{
					Terrains[(int) rgn.Id] = Provider.CreateTerrain(rgn.Id);
				}
			}
		}
		#endregion

		public static ITerrain GetTerrain(MapId map)
		{
			return Terrains[(int)map];
		}
	}
}