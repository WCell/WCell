/*************************************************************************
 *
 *   file		: Faction.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-23 20:07:17 +0100 (on, 23 dec 2009) $

 *   revision		: $Rev: 1151 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Factions;
using WCell.Util.Data;

namespace WCell.RealmServer.Factions
{
	// Nice idea: Have factions change and engage into battle with each other or have peace.
	/// <summary>
	/// TODO: Load faction info completely at startup to avoid synchronization issues
	/// </summary>
	public class Faction
	{
		public static readonly HashSet<Faction> EmptySet = new HashSet<Faction>();
		public static readonly Faction NullFaction = new Faction(EmptySet, EmptySet, EmptySet);

		public readonly HashSet<Faction> Enemies = new HashSet<Faction>();
		public readonly HashSet<Faction> Friends = new HashSet<Faction>();
		public readonly HashSet<Faction> Neutrals = new HashSet<Faction>();

		public FactionGroup Group;
		public List<Faction> Children = new List<Faction>();

		public FactionEntry Entry;
		public readonly FactionTemplateEntry Template;

		public FactionId Id;
		public FactionReputationIndex ReputationIndex;

		/// <summary>
		/// whether this is a Player faction
		/// </summary>
		public bool IsPlayer;

		/// <summary>
		/// whether this is the Alliance or an Alliance faction
		/// </summary>
		public bool IsAlliance;

		/// <summary>
		/// whether this is the Horde or a Horde faction
		/// </summary>
		public bool IsHorde;

		/// <summary>
		/// whether this is a neutral faction (always stays neutral).
		/// </summary>
		public bool IsNeutral;

		/// <summary>
		/// Default ctor can be used for customizing your own Faction
		/// </summary>
		public Faction()
		{
		}

		private Faction(HashSet<Faction> enemies, HashSet<Faction> friends, HashSet<Faction> neutrals)
		{
			Enemies = enemies;
			Friends = friends;
			Neutrals = neutrals;
			Entry = new FactionEntry { Name = "Null Faction" };
			Template = new FactionTemplateEntry { EnemyFactions = new FactionId[0], FriendlyFactions = new FactionId[0] };
		}

		public Faction(FactionEntry entry, FactionTemplateEntry template)
		{
			Entry = entry;
			Template = template;
			Id = entry.Id;
			ReputationIndex = entry.FactionIndex;

            IsAlliance = template.FactionGroup.HasFlag(FactionGroupMask.Alliance);
		    IsHorde = template.FactionGroup.HasFlag(FactionGroupMask.Horde);
		}

		internal void Init()
		{
			if (Id == FactionId.Alliance || Entry.ParentId == FactionId.Alliance)
			{
				IsAlliance = true;
				Group = FactionGroup.Alliance;
			}
			else if (Id == FactionId.Horde || Entry.ParentId == FactionId.Horde)
			{
				IsHorde = true;
				Group = FactionGroup.Horde;
			}

			// friends 
			foreach (var faction in FactionMgr.ByTemplateId.Where(faction => faction != null))
			{
				if (IsPlayer && faction.Template.FriendGroup.HasAnyFlag(FactionGroupMask.Player))
				{
					Friends.Add(faction);
				}

				if (!Template.FriendGroup.HasAnyFlag(faction.Template.FactionGroup))
					continue;

				Friends.Add(faction);

				if (IsPlayer && faction.Template.FriendGroup != 0)
				{
					faction.Enemies.Add(this);
				}
			}

			var friends = Template.FriendlyFactions;
			foreach (var factionId in friends)
			{
				var friend = FactionMgr.Get(factionId);
				if (friend == null) continue;
				Friends.Add(friend);
				friend.Friends.Add(this);
			}

			// we are friends with ourselves
			Friends.Add(this);

			// enemies
			foreach (var faction in FactionMgr.ByTemplateId.Where(faction => faction != null))
			{
				if (IsPlayer && faction.Template.EnemyGroup.HasAnyFlag(FactionGroupMask.Player))
				{
					Enemies.Add(faction);
				}

				if (!Template.EnemyGroup.HasAnyFlag(faction.Template.FactionGroup)) continue;

				Enemies.Add(faction);

				if (IsPlayer && faction.Template.EnemyGroup != 0)
				{
					faction.Enemies.Add(this);
				}
			}

			var enemies = Template.EnemyFactions;
			for (var j = 0; j < friends.Length; j++)
			{
				var enemy = FactionMgr.Get(enemies[j]);
				if (enemy == null)
					continue;

				Enemies.Add(enemy);
				enemy.Enemies.Add(this);
			}

			// neutrals 
			foreach (var faction in FactionMgr.ByTemplateId.Where(faction => faction != null))
			{
				if( !Friends.Contains(faction) && !Enemies.Contains(faction))
					Neutrals.Add(faction);
			}

			if (Id == FactionId.Prey)
			{
				// For some reason, prey has predators as enemy (but they shouldnt ever attack)
				Enemies.Clear();
			}

		    IsNeutral = Enemies.Count == 0 && Friends.Count == 0;
		}
        
		/// <summary>
		/// Make this an alliance player faction
		/// </summary>
		internal void SetAlliancePlayer()
		{
			IsPlayer = true;
			Entry.ParentId = FactionId.Alliance;
			FactionMgr.AlliancePlayerFactions.Add(this);
		}

		/// <summary>
		/// Make this a horde player faction
		/// </summary>
		internal void SetHordePlayer()
		{
			IsPlayer = true;
			Entry.ParentId = FactionId.Horde;
			FactionMgr.HordePlayerFactions.Add(this);
		}

        public bool IsHostileTowards(Faction otherFaction)
		{
			if (Enemies.Contains(otherFaction))
			{
				return true;
			}

            // specific checks
            if (Template.EnemyFactions.Length > 0)
            {
                for (int i = 0; i < Template.EnemyFactions.Length; i++)
                {
                    // Are they an enemy?
                    if (Template.EnemyFactions[i] == otherFaction.Template.FactionId)
                        return true;

                    // or a friend?
                    if (Template.FriendlyFactions[i] == otherFaction.Template.FactionId)
                        return false;
                }
            }

            // Fall back to a general check
			return Template.EnemyGroup.HasAnyFlag(otherFaction.Template.FactionGroup);
        }

		public bool IsNeutralWith(Faction otherFaction)
		{
			return Neutrals.Contains(otherFaction);
		}

        public bool IsFriendlyTowards(Faction otherFaction)
        {
			if (Friends.Contains(otherFaction))
			{
				return true;
			}

            // specific checks
            if (Template.EnemyFactions.Length > 0)
            {
                for (int i = 0; i < Template.FriendlyFactions.Length; i++)
                {
                    // Are they a friend?
                    if (Template.FriendlyFactions[i] == otherFaction.Template.FactionId)
                        return true;
                    // or an enemy?
                    if (Template.EnemyFactions[i] == otherFaction.Template.FactionId)
                        return false;
                }
            }

            // Fall back to a general check
			return Template.FriendGroup.HasAnyFlag(otherFaction.Template.FactionGroup);
        }


	    public override int GetHashCode()
		{
			return (int)Id;
		}

		public override bool Equals(object obj)
		{
			if (obj is Faction)
				return Id == ((Faction) obj).Id;
			return false;
		}

		public override string ToString()
		{
			return Entry.Name + " (" + (int)Id + ")";
		}
	}

	public struct FactionReputationEntry
	{
		public FactionReputationIndex ReputationIndex;
		public int Value;
	}

	public struct IndexedFactionReputationEntry
	{
		[NotPersistent]
		public uint Index;
		public FactionReputationIndex FactionId;
		public int Value;
	}
}