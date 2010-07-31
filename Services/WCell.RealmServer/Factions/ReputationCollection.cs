/*************************************************************************
 *
 *   file		: ReputationCollection.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-01 08:52:17 +0200 (ti, 01 sep 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1061 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Factions
{
	/// <summary>
	/// Represents the Reputation between a Player and all his known factions
	/// </summary>
	public class ReputationCollection
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		Character m_owner;
		Dictionary<FactionReputationIndex, Reputation> m_byIndex;

		public ReputationCollection(Character chr)
		{
			m_byIndex = new Dictionary<FactionReputationIndex, Reputation>();
			m_owner = chr;
		}

		public int Count
		{
			get
			{
				return m_byIndex.Count;
			}
		}

		public Character Owner
		{
			get
			{
				return m_owner;
			}
			set
			{
				m_owner = value;
			}
		}

		/// <summary>
		/// Initializes initial Factions of the owner (used when new Character is created)
		/// </summary>
		public void Initialize()
		{
			var factions = FactionMgr.ById[(uint)m_owner.Faction.Entry.ParentId].Children;
			foreach (var faction in factions)
			{
				if (m_byIndex.ContainsKey(faction.ReputationIndex))
				{
					log.Warn("Character {0} adding Reputation with {1} twice", m_owner, faction);
				}
				else
				{
					Create(faction.ReputationIndex);
				}
			}
		}

		/// <summary>
		/// Loads all Factions that this Character already knows from the DB
		/// </summary>
		public void Load()
		{
			foreach (var record in ReputationRecord.Load(m_owner.Record.Guid))
			{
				var fac = FactionMgr.Get(record.ReputationIndex);
				if (fac != null)
				{
					if (m_byIndex.ContainsKey(record.ReputationIndex))
					{
						log.Warn("Character {0} had Reputation with Faction {1} more than once.", m_owner, record.ReputationIndex);
					}
					else
					{
						var rep = new Reputation(record, fac);
						m_byIndex.Add(record.ReputationIndex, rep);
					}
				}
				else
				{
					log.Warn("Character {0} has saved Reputation with invalid Faction: {1}", m_owner, record.ReputationIndex);

					// record.DeleteAndFlush();
				}
			}
		}

		/// <summary>
		/// Sends all existing factions to the Client
		/// </summary>
		public void ResendAllFactions()
		{
			foreach (var rep in m_byIndex.Values)
			{
				FactionHandler.SendReputationStandingUpdate(m_owner.Client, rep);
			}
		}

		public bool IsHostile(Faction faction)
		{
			var rep = GetOrCreate(faction.ReputationIndex);
			return rep != null && rep.Hostile;
		}

		public bool CanAttack(Faction faction)
		{
			var rep = GetOrCreate(faction.ReputationIndex);
			return rep == null || rep.Hostile;
		}

		public Reputation this[FactionReputationIndex key]
		{
			get
			{
				Reputation rep;
				m_byIndex.TryGetValue(key, out rep);
				return rep;
			}
			set
			{
				// don't do anything
				throw new Exception("To modify the reputation with a specific faction, just modify the values of an already existing Reputation object.");
			}
		}

		#region Creation
		/// <summary>
		/// Returns the corresponding Reputation object. Creates a new one
		/// if the player didn't meet this faction yet.
		/// </summary>
		/// <param name="reputationIndex">The repListId of the faction</param>
		internal Reputation GetOrCreate(FactionReputationIndex reputationIndex)
		{
			Reputation rep;
			if (!m_byIndex.TryGetValue(reputationIndex, out rep))
			{
				rep = Create(reputationIndex);
			}
			return rep;
		}
        
		/// <summary>
		/// Creates a Reputation object that represents the relation to the given faction, or null
		/// </summary>
		/// <param name="factionIndex">The repListId of the faction</param>
		private Reputation Create(FactionReputationIndex factionIndex)
		{
			var fac = FactionMgr.Get(factionIndex);
			if (fac != null)
			{
				return Create(fac);
			}
			return null;
		}

		/// <summary>
		/// Creates a Reputation object that represents the relation to the given faction, or null
		/// </summary>
		/// <param name="faction">The Faction which the Reputation should be with</param>
		private Reputation Create(Faction faction)
		{
		    var defaultValue = GetDefaultReputationValue(faction);
		    var defaultFlags = GetDefaultReputationFlags(faction);
		    var newRecord = m_owner.Record.CreateReputationRecord();
		    var rep = new Reputation(newRecord, faction, defaultValue, defaultFlags);
			m_byIndex.Add(faction.ReputationIndex, rep);
            
            // For some reason, this also makes the faction visible ...
		    FactionHandler.SendReputationStandingUpdate(m_owner.Client, rep);
			return rep;
		}

        private ReputationFlags GetDefaultReputationFlags(Faction faction)
        {
            var entry = faction.Entry;
            for (int i = 0; i < 4; i++)
            {
				if ((entry.ClassMask[i] == 0 || entry.ClassMask[i].HasAnyFlag(Owner.ClassMask)) &&
					(entry.RaceMask[i] == 0 || entry.RaceMask[i].HasAnyFlag(Owner.RaceMask)))
                {
                    return (ReputationFlags) entry.BaseFlags[i];
                }
            }
            return ReputationFlags.None;
        }

	    private int GetDefaultReputationValue(Faction faction)
        {
            var entry = faction.Entry;
            for (int i = 0; i < 4; i++)
            {
				if ((entry.ClassMask[i] == 0 || entry.ClassMask[i].HasAnyFlag(Owner.ClassMask)) &&
					(entry.RaceMask[i] == 0 || entry.RaceMask[i].HasAnyFlag(Owner.RaceMask)))
                {
                    return entry.BaseRepValue[i];
                }
            }
            return 0;
        }
		#endregion

		#region Value
		public int GetValue(FactionReputationIndex reputationIndex)
		{
			Reputation rep;
			if (m_byIndex.TryGetValue(reputationIndex, out rep))
			{
				return rep.Value;
			}

			return 0;
		}

		public Reputation SetValue(FactionReputationIndex reputationIndex, int value)
		{
			Reputation rep = GetOrCreate(reputationIndex);
			if (rep != null)
			{
				SetValue(rep, value);
			}
			return rep;
		}

		public void SetValue(Reputation rep, int value)
		{
			if (rep.SetValue(value))
			{
				//UpdateHostile(rep, rep.Hostile);
			}
			FactionHandler.SendReputationStandingUpdate(m_owner.Client, rep);
		}

        public Reputation ModValue(FactionId factionId, int value)
        {
            var faction = FactionMgr.Get(factionId);
            return ModValue(faction.ReputationIndex, value);
        }

		public Reputation ModValue(FactionReputationIndex reputationIndex, int value)
		{
			var rep = GetOrCreate(reputationIndex);
			if (rep != null)
			{
				ModValue(rep, value);
			}
			return rep;
		}
        
		public void ModValue(Reputation rep, int value)
		{
			if (rep.SetValue(rep.Value + value))
			{
				//UpdateHostile(rep, rep.Hostile);
			}
			FactionHandler.SendReputationStandingUpdate(m_owner.Client, rep);
		}
		#endregion

		#region Standing
		public StandingLevel GetStandingLevel(FactionReputationIndex reputationIndex)
		{
			Reputation rep;
			if (m_byIndex.TryGetValue(reputationIndex, out rep))
			{
				return rep.StandingLevel;
			}
			return StandingLevel.Unknown;
		}
		#endregion

		#region Set/Update Hostility
		/// <summary>
		/// Only called if the player declared war
		/// </summary>
		public void DeclareWar(FactionReputationIndex reputationIndex, bool hostile, bool sendUpdate)
		{
			var rep = GetOrCreate(reputationIndex);

            // impossible to declare war on your own faction
            if (rep.IsForcedAtPeace)
                return;

            // this shouldnt be needed because of the previous check
            if (rep.Faction.Group == m_owner.Faction.Group)
            {
                // can't declare war on your own faction.
                return;
            }

			if (rep.DeclaredWar != hostile)
			{
				rep.DeclaredWar = hostile;
				if (sendUpdate && rep.DeclaredWar)
				{
					FactionHandler.SendSetAtWar(m_owner.Client, rep);
				}
			}
		}
		#endregion

        public void SetInactive(FactionReputationIndex reputationIndex, bool inactive)
        {
            var faction = GetOrCreate(reputationIndex);
            if (faction != null)
            {
                faction.IsInactive = true;
            }
        }

		#region GM's Love&Hate methods
		/// <summary>
		/// For GMs/Testers: Introduces the char to all Factions and sets
		/// Reputation to max.
		/// </summary>
		public void LoveAll()
		{
			foreach (Faction faction in FactionMgr.ByReputationIndex)
			{
				if (faction != null)
				{
					SetValue(faction.ReputationIndex, Reputation.Max);
				}
			}
		}

		/// <summary>
		/// For GMs/Testers: Introduces the char to all Factions and sets
		/// Reputation to min (oh boy are they gonna hate you).
		/// </summary>
		public void HateAll()
		{
			foreach (var faction in FactionMgr.ByReputationIndex)
			{
				if (faction != null)
				{
					SetValue(faction.ReputationIndex, Reputation.Min);
				}
			}
		}
		#endregion

		/// <summary>
		/// Returns the cost of this item after the reputation discount has been applied.
		/// </summary>
		public uint GetDiscountedCost(FactionReputationIndex reputationIndex, uint cost)
		{
			var lvl = GetStandingLevel(reputationIndex);
			return (cost * (100 - Reputation.GetReputationDiscountPct(lvl))) / 100;
		}

        /// <summary>
        /// Called when interacting with an NPC.
        /// </summary>
        public void OnTalkWith(NPC npc)
        {
            var reputationIndex = npc.Faction.ReputationIndex;
            
            // Does this faction even have a rep?
			if (reputationIndex < 0 || reputationIndex >= FactionReputationIndex.End) return;

            Reputation rep = GetOrCreate(reputationIndex);
            
            // Faction is now visible
            if (!rep.IsForcedInvisible)
            {
                rep.IsVisible = true;

                // Let the client know the Faction is visible
                FactionHandler.SendVisible(m_owner.Client, reputationIndex);
            }
        }

        /// <summary>
        /// Increases or Decreases reputation with the given faction.
        /// </summary>
        /// <param name="factionId">Faction Id.</param>
        /// <param name="value">Amount to add or decrease</param>
        /// <returns></returns>
        public Reputation GainReputation(FactionId factionId, int value)
        {
            value = value + (int)Math.Round(value * m_owner.ReputationGainModifierPercent / 100.0);
            return ModValue(factionId, value);
        }
	} 
}