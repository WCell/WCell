using System;
using Castle.ActiveRecord;
using WCell.Constants.World;
using WCell.Core.Database;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Instances
{
	[ActiveRecord]
	public class GlobalInstanceTimer : WCellRecord<GlobalInstanceTimer>
	{
		/// <summary>
		/// Load and verify timers
		/// </summary>
		public static GlobalInstanceTimer[] LoadTimers()
		{
			var arr = FindAll();
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

                    if (rgn.Difficulties != null)
                    {

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
                                    timer.Save();
                                }
                            }
                        }
                    }
				}
				else if (timer != null)
				{
					// is not an instance (anymore)
					timer.Delete();
					timers[i] = null;
				}
			}
			return timers;
		}

		public MapId MapId;

		public GlobalInstanceTimer(MapId id)
		{
			State = RecordState.New;
			MapId = id;
			LastResets = new DateTime[InstanceMgr.MaxInstanceDifficulties];
		}

		public GlobalInstanceTimer()
		{
		}

		[PrimaryKey(PrimaryKeyType.Assigned, "MapId")]
		private int m_MapId
		{
			get { return (int)MapId; }
			set { MapId = (MapId)value; }
		}

		[Property(NotNull = true)]
		public DateTime[] LastResets
		{
			get;
			set;
		}
	}
}
