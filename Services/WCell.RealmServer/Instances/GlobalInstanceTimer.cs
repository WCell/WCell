using NHibernate.Criterion;
using WCell.Database;
using WCell.Constants.World;
using System;
using WCell.RealmServer.Database;
using WCell.Util;
using WCell.RealmServer.Global;
using WCell.Constants;

namespace WCell.RealmServer.Instances
{
	public class GlobalInstanceTimer
	{
		/// <summary>
		/// Load and verify timers
		/// </summary>
		public static GlobalInstanceTimer[] LoadTimers()
		{
            var arr = RealmWorldDBMgr.DatabaseProvider.FindAll<GlobalInstanceTimer>();
			var timers = new GlobalInstanceTimer[(int)MapId.End + 1000];
			foreach (var timer in arr)
			{
				timers[(int)timer.MapId] = timer;
			}

			var rgns = World.MapTemplates;
			for (var i = 0; i < rgns.Length; i++)
			{
				var rgn = rgns[i];
				var timer = timers[i];
				if (rgn != null && rgn.IsInstance)
				{
					if (timer != null && timer.LastResets.Length != InstanceMgr.MaxInstanceDifficulties)
					{
						// Invalid amount of reset times
						var resetTimes = timer.LastResets;
						Array.Resize(ref resetTimes, InstanceMgr.MaxInstanceDifficulties);
						timer.LastResets = resetTimes;
						for (var t = 0; t < timer.LastResets.Length; t++)
						{
							timer.LastResets[t] = DateTime.Now;
						}
						continue;
					}

					for (var d = 0; d < rgn.Difficulties.Length; d++)
					{
						var diff = rgn.Difficulties[d];
						if (diff != null)
						{
							if (timer == null && diff.ResetTime > 0)
							{
								// missing Timer
								timers[i] = timer = new GlobalInstanceTimer(rgn.Id);

								timer.LastResets[d] = DateTime.Now;
                                RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(timer);
							}
						}
					}
				}
				else if (timer != null)
				{
					// is not an instance (anymore)
                    RealmWorldDBMgr.DatabaseProvider.Delete(timer);
					timers[i] = null;
				}
			}
			return timers;
		}

		public MapId MapId;

		public GlobalInstanceTimer(MapId id)
		{
			MapId = id;
			LastResets = new DateTime[InstanceMgr.MaxInstanceDifficulties];
		}

		public GlobalInstanceTimer()
		{
		}

		public DateTime[] LastResets
		{
			get;
			set;
		}
	}
}