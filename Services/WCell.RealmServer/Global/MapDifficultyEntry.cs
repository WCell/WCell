using WCell.RealmServer.Instances;

namespace WCell.RealmServer.Global
{
	public class MapDifficultyEntry : MapDifficultyDBCEntry
	{
		public static readonly int HeroicResetTime = 86400;
		public static readonly int MaxDungeonPlayerCount = 5;
		
		public MapTemplate Map;

		public bool IsHeroic;

		public bool IsRaid;

		/// <summary>
		/// Softly bound instances can always be reset but you only x times per hour.
		/// </summary>
		public BindingType BindingType;

		internal void Finalize(MapTemplate map)
		{
			Map = map;
			if (ResetTime == 0)
			{
				ResetTime = map.DefaultResetTime;
			}

			//if (Map.Type == MapType.Dungeon)
			//{
			//    IsHeroic = Index == 1;
			//}
			//else if (MaxPlayerCount != 0)
			//{
			//    // use heuristics to determine whether we have heroic difficulty:
			//    foreach (var diff in Map.Difficulties)
			//    {
			//        if (diff != null && diff.MaxPlayerCount == MaxPlayerCount)
			//        {
			//            // Second entry with the same player count -> Probably heroic
			//            IsHeroic = true;
			//            break;
			//        }
			//    }
			//}
			IsHeroic = ResetTime == HeroicResetTime;
			IsRaid = MaxPlayerCount == MaxDungeonPlayerCount;

			BindingType = IsDungeon ? BindingType.Soft : BindingType.Hard;
		}

		public bool IsDungeon
		{
			get { return !IsHeroic && !IsRaid; }
		}
	}
}