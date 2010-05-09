using System;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Formulas
{
	/// <summary>
	/// <see href="http://www.wowwiki.com/Rest"/>
	/// </summary>
	public static class RestGenerator
	{
		/// <summary>
		/// The amount of time in which a user can gain 5% (100 / <see cref="AverageRestingRatePct"/>) rest in seconds.
		/// Default: 8 * 3600 = 8 hours.
		/// </summary>
		public static int AverageRestingPeriod = 8 * 3600;

		/// <summary>
		/// Average percentage of Rest generated per <see cref="AverageRestingPeriod"/>
		/// </summary>
		public static int AverageRestingRatePct = 5;

		/// <summary>
		/// the amount of xp to the following level
		/// </summary>
		/// <remarks>By default, rest accumulates 4 times faster when in an Inn or other kind of resting area.</remarks>
		/// <returns></returns>
		public static Func<TimeSpan, Character, int> GetRestXp = (TimeSpan time, Character chr) => {
			var nextLevelXp = chr.NextLevelXP;
			if (nextLevelXp > 0)
			{
				var seconds = (int)time.TotalSeconds;
				var bonus = (nextLevelXp * seconds * AverageRestingRatePct) / (100 * AverageRestingPeriod);	// 5% per 8 hours
				if (chr.RestTrigger == null)
				{
					bonus /= 4;	// resting is 4 times slower if not in a resting area
				}

				// cannot exceed 150%
				var max = (nextLevelXp * 3) / 2;
				return MathUtil.ClampMinMax(bonus, 0, max - chr.RestXp);
			}
			return 0;
		};
	}
}