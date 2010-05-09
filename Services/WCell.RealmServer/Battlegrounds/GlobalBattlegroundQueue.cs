using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cell.Core.Collections;
using WCell.RealmServer.Entities;
using WCell.Constants;
using WCell.RealmServer.GameObjects;
using NLog;

namespace WCell.RealmServer.Battlegrounds
{
	/// <summary>
	/// A <see cref="GlobalBattlegroundQueue"/> contains all instances of a particular level-range
	/// of one <see cref="BattlegroundTemplate"/>.
	/// </summary>
	public class GlobalBattlegroundQueue : BattlegroundQueue
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private static int defaultBGCreationPlayerThresholdPct = 80;

		/// <summary>
		/// A new BG is created and invitation will start once the queue contains at least this percentage
		/// of the BG's max player limit.
		/// </summary>
		public static int DefaultBGCreationPlayerThresholdPct
		{
			get { return defaultBGCreationPlayerThresholdPct; }
			set
			{
				defaultBGCreationPlayerThresholdPct = value;
				foreach (var template in BattlegroundMgr.Templates)
				{
					if (template != null)
					{
						foreach (var q in template.Queues)
						{
							if (q != null)
							{
								q.SetThreshold();
							}
						}
					}
				}
			}
		}


		private int m_CreationPlayerThreshold;

		/// <summary>
		/// All <see cref="Battleground"/>-instances of this queue's <see cref="BattlegroundTemplate"/>
		/// </summary>
		public readonly ImmutableList<Battleground> Instances = new ImmutableList<Battleground>();

		public GlobalBattlegroundQueue(BattlegroundId bgid)
			: this(BattlegroundMgr.GetTemplate(bgid))
		{
		}

		public GlobalBattlegroundQueue(BattlegroundTemplate template) : 
			this(template, 0, 0, RealmServerConfiguration.MaxCharacterLevel)
		{
		}

		public GlobalBattlegroundQueue(BattlegroundTemplate template, int lvlBracket, int minLevel, int maxLevel) :
			base(template, lvlBracket, minLevel, maxLevel)
		{
			SetThreshold();
		}

		protected override BattlegroundTeamQueue CreateTeamQueue(BattlegroundSide side)
		{
			return new GlobalBGTeamQueue(this, side);
		}

		private void SetThreshold()
		{
			m_CreationPlayerThreshold = (Template.RegionInfo.MaxPlayerCount * defaultBGCreationPlayerThresholdPct) / 100;
		}

		public int CreationPlayerThreshold
		{
			get { return m_CreationPlayerThreshold; }
		}

		public int CharacterCount
		{
			get
			{
				var count = 0;
				foreach (var q in TeamQueues)
				{
					count += q.CharacterCount;
				}
				return count;
			}
		}

		public override bool RequiresLocking
		{
			get { return true; }
		}

		public Battleground GetBattleground(uint instanceId)
		{
			foreach (var bg in Instances)
			{
				if (bg.InstanceId == instanceId)
				{
					return bg;
				}
			}
			return null;
		}

		public void CheckBGCreation()
		{
			if (CharacterCount >= CreationPlayerThreshold)
			{
				CreateBattleground();
			}
		}

		bool CheckBGRequirements()
		{
			if (!GOMgr.Loaded)
			{
				log.Warn("Tried to create Battleground without GOs loaded.");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Make sure to load GOs before calling this method
		/// </summary>
		/// <typeparam name="B"></typeparam>
		/// <returns></returns>
		public Battleground CreateBattleground()
		{
			if (!CheckBGRequirements() || m_Template.Creator == null)
			{
				return null;
			}

			var bg = m_Template.Creator();
			InitBG(bg);
			return bg;
		}

		/// <summary>
		/// Make sure to load GOs before calling this method
		/// </summary>
		/// <typeparam name="B"></typeparam>
		/// <returns></returns>
		public B CreateBattleground<B>()
			where B : Battleground, new()
		{
			if (!CheckBGRequirements())
			{
				return null;
			}

			var bg = new B();
			InitBG(bg);
			return bg;
		}

		private void InitBG(Battleground bg)
		{
			Instances.Add(bg);
			bg.ParentQueue = this;
			bg.InitRegion(m_Template.RegionInfo);
		}

		/// <summary>
		/// Enqueues the given Character(s) for the given side
		/// </summary>
		public BattlegroundRelation Enqueue(ICharacterSet chrs, BattlegroundSide side)
		{
			var queue = GetTeamQueue(side);
			var request = new BattlegroundRelation(queue, chrs);

			queue.Enqueue(request);

			return request;
		}

		internal void OnRemove(Battleground bg)
		{
			Instances.Remove(bg);
		}

		public override Battleground Battleground
		{
			get { return null; }
		}

		protected internal override void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
