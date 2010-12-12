using WCell.Constants;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Data;
using WCell.Constants.World;
using WCell.RealmServer.Content;
using WCell.Util.Graphics;
using WCell.Util.Variables;
using System;
using System.Linq;
using WCell.RealmServer.Entities;
using WCell.Core;

namespace WCell.RealmServer.Battlegrounds
{
	/// <summary>
	/// Battleground information
	/// </summary>
	[DataHolder]
	public class BattlegroundTemplate : IDataHolder
	{
		public BattlegroundId Id;

		[NotPersistent]
		public MapId RegionId;

		public int MinPlayersPerTeam, MaxPlayersPerTeam;

		public int MinLevel, MaxLevel;

		/// <summary>
		/// Load pos from DBCs
		/// </summary>
		public int AllianceStartPosIndex, HordeStartPosIndex;

		public Vector3 AllianceStartPosition, HordeStartPosition;
		public float AllianceStartOrientation, HordeStartOrientation;

		[NotPersistent]
		public BattlegroundCreator Creator;

		[NotPersistent]
		public RegionTemplate RegionTemplate;

		[NotPersistent]
		public GlobalBattlegroundQueue[] Queues;

        [NotPersistent]
        public PvPDifficultyEntry[] Difficulties;

		public int MinPlayerCount
		{
			get { return MinPlayersPerTeam * 2; }
		}

		public uint GetId()
		{
			return (uint) Id;
		}
        
		public DataHolderState DataHolderState
		{
			get;
			set;
		}

		public void FinalizeDataHolder()
		{
			RegionId = BattlegroundMgr.BattlemasterListReader.Entries[(int)Id].MapId;

			RegionTemplate = World.GetRegionTemplate(RegionId);
			if (RegionTemplate == null)
			{
				ContentMgr.OnInvalidDBData("BattlegroundTemplate had invalid RegionId: {0} (#{1})",
					RegionId, (int)RegionId);
				return;
			}

			if (BattlegroundMgr.Templates.Length <= (int)Id)
			{
				ContentMgr.OnInvalidDBData("BattlegroundTemplate had invalid BG-Id: {0} (#{1})",
					Id, (int)Id);
				return;
			}

            Difficulties = new PvPDifficultyEntry[BattlegroundMgr.PVPDifficultyReader.Entries.Values.Count(entry => (entry.mapId == RegionId))];

            foreach (var entry in BattlegroundMgr.PVPDifficultyReader.Entries.Values.Where(entry => (entry.mapId == RegionId)))
            {
                    Difficulties[entry.bracketId] = entry;
            }
            
            MinLevel = RegionTemplate.MinLevel = Difficulties.First().minLevel;
            MaxLevel = RegionTemplate.MaxLevel = Difficulties.Last().maxLevel;
			BattlegroundMgr.Templates[(int)Id] = this;

			CreateQueues();
            SetStartPos();
		}

        public int GetBracketIdForLevel(int level)
        {
            var diff = Difficulties.FirstOrDefault(entry => (level >= entry.minLevel && level <= entry.maxLevel));
			if (diff != null)
			{
				return diff.bracketId;
			}
			else
			{
				return -1;
			}
        }

		private void CreateQueues()
		{
            Queues = new GlobalBattlegroundQueue[Difficulties.Length];
            foreach (var entry in Difficulties)
            {
				if (entry != null)
				{
					AddQueue(new GlobalBattlegroundQueue(this, entry.bracketId, entry.minLevel, entry.maxLevel));
				}
            }
		}

		void AddQueue(GlobalBattlegroundQueue queue)
		{
				Queues[queue.BracketId] = queue;
		}

		/// <summary>
		/// Gets the appropriate queue for the given character.
		/// </summary>
		/// <param name="chr">the character</param>
		/// <returns>the appropriate queue for the given character</returns>
		public GlobalBattlegroundQueue GetQueue(Character character)
		{
			return GetQueue(character.Level);
		}

		/// <summary>
		/// Gets the appropriate queue for a character of the given level.
		/// </summary>
		/// <param name="level">the level of the character</param>
		/// <returns>the appropriate queue for the given character level</returns>
		public GlobalBattlegroundQueue GetQueue(int level)
		{
            return Queues.Get((uint)GetBracketIdForLevel(level));
		}

		#region Enqueue
		/// <summary>
		/// Enqueues a single Character to the global Queue of this Template
		/// </summary>
		/// <param name="chr"></param>
		/// <returns></returns>
		public BattlegroundRelation EnqueueCharacter(Character chr, BattlegroundSide side)
		{
			return GetQueue(chr).Enqueue(chr, side);
		}

		/// <summary>
		/// Tries to enqueue the given Character or -if specified- his/her entire Group
		/// </summary>
		/// <param name="chr"></param>
		/// <param name="asGroup"></param>
		public void TryEnqueue(Character chr, bool asGroup)
		{
			TryEnqueue(chr, asGroup, chr.FactionGroup.GetBattlegroundSide());
		}

		/// <summary>
		/// Tries to enqueue the given Character or -if specified- his/her entire Group
		/// </summary>
		/// <param name="chr"></param>
		/// <param name="asGroup"></param>
		public void TryEnqueue(Character chr, bool asGroup, BattlegroundSide side)
		{
			TryEnqueue(chr, asGroup, 0u, side);
		}

		/// <summary>
		/// Tries to enqueue the given Character or -if specified- his/her entire Group
		/// </summary>
		/// <param name="chr"></param>
		/// <param name="asGroup"></param>
		public void TryEnqueue(Character chr, bool asGroup, uint instanceId)
		{
			TryEnqueue(chr, asGroup, instanceId, chr.FactionGroup.GetBattlegroundSide());
		}

		/// <summary>
		/// Tries to enqueue the given character or his whole group for the given battleground.
		/// </summary>
		/// <param name="chr">the character who enqueued</param>
		/// <param name="asGroup">whether or not to enqueue the character or his/her group</param>
		public void TryEnqueue(Character chr, bool asGroup, uint instanceId, BattlegroundSide side)
		{
			// specific instance given
			if (instanceId != 0)
			{
				var queue = GetQueue(chr);
				if (queue != null)
				{
					var bg = queue.GetBattleground(instanceId);
					if (bg != null && bg.HasQueue)
					{
						bg.TryJoin(chr, asGroup, side);
					}
				}
			}
			else
			{
				var teamQueue = GetQueue(chr).GetTeamQueue(side);
				teamQueue.Enqueue(chr, asGroup);
			}
		}
		#endregion

		public override string ToString()
		{
			return GetType().Name +
				string.Format(" (Id: {0} (#{1}), Map: {2} (#{3})",
				Id, (int)Id, RegionId, (int)RegionId);
		}

        public void SetStartPos()
        {
            WorldSafeLocation allianceStartPos;
            BattlegroundMgr.WorldSafeLocs.TryGetValue(AllianceStartPosIndex, out allianceStartPos);
            if (allianceStartPos != null)
            {
                var allianceStartPosVector = new Vector3(allianceStartPos.X, allianceStartPos.Y, allianceStartPos.Z);

                if (allianceStartPosVector.X != 0.0f && allianceStartPosVector.Y != 0.0f && allianceStartPosVector.Z != 0.0f)
                    AllianceStartPosition = allianceStartPosVector;

            }

            WorldSafeLocation hordeStartPos;
            BattlegroundMgr.WorldSafeLocs.TryGetValue(HordeStartPosIndex, out hordeStartPos);
            if(hordeStartPos != null)
            {
                var hordeStartPosVector = new Vector3(hordeStartPos.X, hordeStartPos.Y, hordeStartPos.Z);

                if (hordeStartPosVector.X != 0.0f && hordeStartPosVector.Y != 0.0f && hordeStartPosVector.Z != 0.0f)
                    HordeStartPosition = hordeStartPosVector;
            }
        }
	}
}


/*
 * 
	Logic required to enqueue all group-members (into different Queues):
 *		public void EnqueueGroup(Group group, BattlegroundSide side)
		{
			var chrs = group.GetCharacters();
			if (chrs.Length == 0)
			{
				// Group is empty
				return;
			}

			var faction = group.FactionGroup;

			List<KeyValuePair<GlobalBattlegroundQueue, SynchronizedCharacterList>> lists = null;
			GlobalBattlegroundQueue queue = null;
			for (var i = 0; i < chrs.Length; i++)
			{
				var groupChr = chrs[i];
				var q = GetQueue(groupChr);
				if (queue == null)
				{
					queue = q;
				}
				else if (queue != q || lists != null || i >= MaxPlayersPerTeam)
				{
					// Not all Characters go into the same Queue -> Create lists
					AddToList(ref lists, chrs, i, q, groupChr, faction);
					//BattlegroundHandler.SendBattlegroundError(groupChr, BattlegroundJoinError.GroupJoinedNotEglible);
				}
			}

			if (lists == null)
			{
				// everyone goes in the same Queue

				queue.Enqueue(new SynchronizedCharacterList(faction, chrs), true, side);
			}
			else
			{
			    // more than one queue
			    for (var i = 0; i < lists.Count; i++)
			    {
			        var pair = lists[i];
			        pair.Key.Enqueue(pair.Value, true, side);
			    }
			}
		}
 * 
		/// <summary>
		/// Adds a new Character to the set of request-lists, according to the default ruleset
		/// </summary>
		private void AddToList(ref List<KeyValuePair<BattlegroundQueue, SynchronizedCharacterList>> lists,
			Character[] chrs, int index, BattlegroundQueue q, Character groupChr, FactionGroup faction)
		{
			if (lists == null)
			{
				// first list
				lists = new List<KeyValuePair<BattlegroundQueue, SynchronizedCharacterList>>(2);

				// add all those that have not been added yet
				var firstList = new KeyValuePair<BattlegroundQueue, SynchronizedCharacterList>(
					GetQueue(chrs[0]),
					new SynchronizedCharacterList(faction)
					);
				lists.Add(firstList);
				for (var i = 0; i < index; i++)
				{
					var chr = chrs[i];
					firstList.Value.Add(chr);
				}

				// add first char of the second Queue
				lists.Add(new KeyValuePair<BattlegroundQueue, SynchronizedCharacterList>(
							q,
							new SynchronizedCharacterList(faction) { groupChr }
							));
			}
			else
			{
				// find correct list
				var found = false;
				for (var j = 0; j < lists.Count; j++)
				{
					var l = lists[j];
					if (l.Key == q && l.Value.Count < MaxPlayersPerTeam)
					{
						l.Value.Add(groupChr);
						found = true;
						break;
					}
				}

				if (!found)
				{
					// didn't find a suitable list -> Create new List
					lists.Add(new KeyValuePair<BattlegroundQueue, SynchronizedCharacterList>(
								q,
								new SynchronizedCharacterList(faction) { groupChr }
								));
				}
			}
		}
*/