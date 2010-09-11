using WCell.Constants;
using WCell.RealmServer.Instances;

namespace WCell.RealmServer.Global
{
	public class MapDifficultyEntry : MapDifficultyDBCEntry
	{
		public RegionTemplate Region;

		public bool IsHeroic;

		/// <summary>
		/// Softly bound instances can always be reset but you only x times per hour.
		/// </summary>
		public BindingType BindingType;

		internal void Finalize(RegionTemplate region)
		{
			Region = region;
			if (ResetTime == 0)
			{
				ResetTime = region.DefaultResetTime;
			}

			if (MaxPlayerCount != 0)
			{
				// use heuristics to determine whether we have a heroic difficulty:
				foreach (var diff in Region.Difficulties)
				{
					if (diff != null && diff.MaxPlayerCount == MaxPlayerCount)
					{
						// Second entry with the same player count -> Probably heroic
						IsHeroic = true;
						break;
					}
				}
			}

			BindingType = !IsHeroic && Region.Type == MapType.Dungeon ? BindingType.Soft : BindingType.Hard;
		}
	}
}