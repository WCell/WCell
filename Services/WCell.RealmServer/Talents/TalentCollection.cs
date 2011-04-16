/*************************************************************************
 *
 *   file		: TalentCollection.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-21 18:48:36 +0100 (ma, 21 dec 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1149 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.Talents;
using WCell.Constants.Updates;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.NPCs.Pets;

namespace WCell.RealmServer.Talents
{
	/// <summary>
	/// Represents all Talents of a Character or Pet
	/// </summary>
	public abstract class TalentCollection : IEnumerable<Talent>
	{
		internal Dictionary<TalentId, Talent> ById = new Dictionary<TalentId, Talent>();

		internal readonly int[] m_treePoints;

		internal TalentCollection(Unit unit)
		{
			Owner = unit;

			m_treePoints = new int[Trees.Length];
		}

		internal void CalcSpentTalentPoints()
		{
            for (var i = 0; i < Trees.Length; i++)
            {
                m_treePoints[i] = 0;
            }

			foreach (var talent in this)
			{
                UpdateTreePoint(talent.Entry.Tree.TabIndex, talent.ActualRank);
			}
		}

        internal void UpdateTreePoint(uint tabIndex, int diff)
        {
            m_treePoints[tabIndex] += diff;
        }

		#region Default Properties
		public TalentTree[] Trees
		{
			get { return TalentMgr.GetTrees(Owner.Class); }
		}

		public int TotalPointsSpent
		{
			get { return m_treePoints.Sum(); }
		}

		public int Count
		{
			get { return ById.Count; }
		}

		public Unit Owner
		{
			get;
			internal set;
		}
		#endregion

		#region CanLearn
		/// <summary>
		/// Whether the given talent can be learned by this Character
		/// </summary>
		public bool CanLearn(TalentId id, int rank)
		{
			var talent = TalentMgr.GetEntry(id);

			return talent != null && CanLearn(talent, rank);
		}

		/// <summary>
		/// Whether the given talent can be learned by an average player.
		/// Does not check for available Talent points, since that is checked when the Rank is changed.
		/// </summary>
		public virtual bool CanLearn(TalentEntry entry, int rank)
		{
			var tree = entry.Tree;
			var diff = rank - GetRank(entry.Id);

			if (tree.Class != Owner.Class || m_treePoints[tree.TabIndex] < entry.RequiredTreePoints ||
				rank > entry.Spells.Length || diff < 1)
			{
				return false;
			}

			// requires another talent?
			Talent reqTalent;

			return
				FreeTalentPoints >= diff &&
				(entry.RequiredId == TalentId.None || (ById.TryGetValue(entry.RequiredId, out reqTalent) &&
				(entry.RequiredRank == 0 || reqTalent.Rank >= entry.RequiredRank)));
		}
		#endregion

		#region Learn
		public Talent Learn(TalentId id, int rank)
		{
			var entry = TalentMgr.GetEntry(id);
			if (entry != null)
			{
				return Learn(entry, rank);
			}
			return null;
		}

		/// <summary>
		/// Tries to learn the given talent on the given rank
		/// </summary>
		/// <returns>Whether it was learnt</returns>
		public Talent Learn(TalentEntry entry, int rank)
		{
			if (!CanLearn(entry, rank))
			{
				return null;
			}

			return Set(entry, rank);
		}

		/// <summary>
		/// Learn all talents of your own class
		/// </summary>
		public void LearnAll()
		{
			LearnAll(Owner.Class);
		}

		/// <summary>
		/// Learns all talents of the given class
		/// </summary>
		public void LearnAll(ClassId clss)
		{
			var points = FreeTalentPoints;
			FreeTalentPoints = 300;			// need extra Talent points to avoid internal checks to fail

			foreach (var talentTree in TalentMgr.TreesByClass[(int)clss])
			{
				if (talentTree == null) continue;
				foreach (var entry in talentTree)
				{
					if (entry == null) continue;
					Learn(entry, entry.MaxRank);
				}
			}

			FreeTalentPoints = points;
		}
		#endregion

		#region Set & Add
		/// <summary>
		/// Sets the given talent to the given rank without any checks.
		/// Make sure that the given TalentId is valid for this Character's class.
		/// </summary>
		public Talent Set(TalentId id, int rank)
		{
			var entry = TalentMgr.GetEntry(id);
			return Set(entry, rank);
		}

		/// <summary>
		/// Sets the given talent to the given rank without any checks
		/// </summary>
		public Talent Set(TalentEntry entry, int rank)
		{
			Talent talent;
			if (!ById.TryGetValue(entry.Id, out talent))
			{
				ById.Add(entry.Id, talent = new Talent(this, entry, rank));
			}
			else
			{
				talent.Rank = rank;
			}
			return talent;
		}

		internal void AddExisting(TalentEntry entry, int rank)
		{
			var talent = new Talent(this, entry);
			rank = Math.Max(0, rank - 1);
			talent.SetRankSilently(rank);

			ById[entry.Id] = talent;
		}
		#endregion


		#region Getters
		/// <summary>
		/// Returns the current rank that this player has of this talent
		/// </summary>
		public Talent GetTalent(TalentId id)
		{
			Talent talent;
			ById.TryGetValue(id, out talent);
			return talent;
		}

		/// <summary>
		/// Returns the current rank that this player has of this talent
		/// </summary>
		public int GetRank(TalentId id)
		{
			Talent talent;
			if (ById.TryGetValue(id, out talent))
			{
				return talent.Rank;
			}

			return -1;
		}

		/// <summary>
		/// Whether this Owner has a certain Talent.
		/// </summary>
		/// <param name="id">The TalentId of the Talent</param>
		/// <returns>True if the Owner has the specified Talent</returns>
		public bool HasTalent(TalentId id)
		{
			return ById.ContainsKey(id);
		}

		/// <summary>
		/// Whether this Owner has a certain Talent.
		/// </summary>
		/// <param name="talent">The Talent in question.</param>
		/// <returns>True if the Owner has the specified Talent</returns>
		public bool HasTalent(Talent talent)
		{
			return ById.ContainsKey(talent.Entry.Id);
		}
		#endregion

		#region Remove & Reset
		public bool Remove(TalentId id)
		{
			var talent = GetTalent(id);
			if (talent != null)
			{
				talent.Remove();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes the given amount of arbitrarily selected talents (always removes higher level talents first)
		/// </summary>
		public void RemoveTalents(int count)
		{
			var trees = Trees;

			// TODO: Remove depth-first, instead of breadth-first
			for (var i = 0; i < trees.Length; i++)
			{
				var tree = trees[i];
				while (m_treePoints[i] > 0 && count > 0)
				{
					for (var r = tree.TalentTable.Length - 1; r >= 0; r--)
					{
						var row = tree.TalentTable[r];
						foreach (var entry in row)
						{
							if (entry != null)
							{
								var talent = GetTalent(entry.Id);
								if (talent != null)
								{
									if (count >= talent.ActualRank)
									{
										count -= talent.ActualRank;
										talent.Remove();
									}
									else
									{
										talent.ActualRank -= count;
										count = 0;
										TalentHandler.SendTalentGroupList(this);
										return;
									}
								}
							}
						}
					}
				}
			}

			TalentHandler.SendTalentGroupList(this);
		}

		/// <summary>
		/// Resets all talents for free
		/// </summary>
		public void ResetAllForFree()
		{
			foreach (var talent in ById.Values.ToArray())
			{
				talent.Remove();
			}
			ById.Clear();
			FreeTalentPoints = GetFreeTalentPointsForLevel(Owner.Level);
			LastResetTime = DateTime.Now;
		}

		/// <summary>
		/// 
		/// </summary>
		public int CalculateCurrentResetTier()
		{
			if (OwnerCharacter.GodMode)
			{
				return 0;
			}

			var tier = CurrentResetTier;
			var lastResetTime = LastResetTime;

			if (lastResetTime == null)
			{
				return 0;
			}

			// count down the tier
			var timeLapse = DateTime.Now - lastResetTime.Value;
			var numDiscounts = (int)timeLapse.TotalHours / ResetTierDecayHours;
			var newPriceTier = tier - numDiscounts;
			if (newPriceTier < 0)
			{
				return 0;
			}

			return newPriceTier;
		}

		/// <summary>
		/// The amount of copper that it costs to reset all talents.
		/// Updates the current tier, according to the amount of time passed.
		/// </summary>
		public uint GetResetPrice()
		{
			var tier = CalculateCurrentResetTier();
			var tiers = ResetPricesPerTier;
			if (tier >= tiers.Length)
			{
				tier = tiers.Length - 1;
			}
			return tiers[tier];
		}

		/// <summary>
		/// Returns whether reseting succeeded or if it failed (due to not having enough gold)
		/// </summary>
		public bool ResetTalents()
		{
			var tier = CalculateCurrentResetTier();
			var tiers = ResetPricesPerTier;
			if (tier >= tiers.Length)
			{
				tier = tiers.Length - 1;
			}

			var price = tiers[tier];
			var chr = OwnerCharacter;
			if (price > chr.Money || chr.GodMode)
			{
				ResetAllForFree();

				chr.Money -= price;
				CurrentResetTier = tier + 1;
				return true;
			}
			return false;
		}
		#endregion

		#region Abstract & Virtual
		/// <summary>
		/// The Owner of this TalentCollection or the owning pet's Master
		/// </summary>
		public abstract Character OwnerCharacter
		{
			get;
		}

		public abstract int FreeTalentPoints
		{
			get;
			set;
		}

		public virtual int SpecProfileCount
		{
			get { return 1; }
			internal set
			{
				/* don't do nothing */
				LogManager.GetCurrentClassLogger().Warn("Tried to set Talents.TalentGroupCount for: {0}", Owner);
			}
		}

		/// <summary>
		/// The index of the currently used group of talents (one can have multiple groups due to multi spec)
		/// </summary>
		public abstract int CurrentSpecIndex
		{
			get;
		}

		public abstract uint[] ResetPricesPerTier
		{
			get;
		}

		protected abstract int CurrentResetTier
		{
			get;
			set;
		}

		public abstract DateTime? LastResetTime
		{
			get;
			set;
		}

		/// <summary>
		/// Amount of hours that it takes the reset price tier to go down by one
		/// </summary>
		public abstract int ResetTierDecayHours
		{
			get;
		}

		public abstract int GetFreeTalentPointsForLevel(int level);

		//TODO
		public abstract void UpdateFreeTalentPointsSilently(int delta);
		#endregion

		#region Enumerator
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<Talent> GetEnumerator()
		{
			return ById.Values.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion
	}
}