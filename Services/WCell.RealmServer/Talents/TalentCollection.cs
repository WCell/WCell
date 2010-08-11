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
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.Talents;
using WCell.Constants.Updates;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Talents
{
	/// <summary>
	/// Represents all Talents of a Character or Pet
	/// </summary>
	public class TalentCollection : IEnumerable<Talent>
	{
		internal Dictionary<TalentId, Talent> ById = new Dictionary<TalentId, Talent>();

		public readonly TalentTree[] Trees;
		internal readonly int[] m_treePoints;

		public TalentCollection(IHasTalents unit)
		{
			Owner = unit;

			Trees = TalentMgr.TreesByClass[(uint)unit.Class];
			m_treePoints = new int[Trees.Length];
		}

		internal void InitTalentPoints()
		{
			foreach (var spell in Owner.Spells)
			{
				if (spell.Talent != null)
				{
					m_treePoints[spell.Talent.Tree.TabIndex] ++;
				}
			}
		}

		public int TotalPointsSpent
		{
			get
			{
				var points = 0;
				foreach (var point in m_treePoints)
				{
					points += point;
				}
				return points;
			}
		}

		public int Count
		{
			get { return ById.Count; }
		}

		/// <summary>
		/// The amount of copper that it costs to reset all talents
		/// </summary>
		public uint ResetAllPrice
		{
			get { return (uint)Owner.GetTalentResetPrice(); }
		}

		public IHasTalents Owner
		{
			get;
			internal set;
		}

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
		public bool CanLearn(TalentEntry entry, int rank)
		{
			var tree = entry.Tree;
			var diff = rank - GetRank(entry.Id);

			if (tree.Class != Owner.Class || m_treePoints[tree.TabIndex] < entry.RequiredTreePoints ||
				rank > entry.Spells.Length || diff < 1)
			{
				return false;
			}

			if (tree.Class == ClassId.PetTalents)
			{
				switch (Owner.PetTalentType)
				{
					case (PetTalentType.Cunning):
						{
							if (tree.Id != TalentTreeId.PetTalentsCunning)
							{
								return false;
							}
							break;
						}
					case (PetTalentType.Ferocity):
						{
							if (tree.Id != TalentTreeId.PetTalentsFerocity)
							{
								return false;
							}
							break;
						}
					case (PetTalentType.Tenacity):
						{
							if (tree.Id != TalentTreeId.PetTalentsTenacity)
							{
								return false;
							}
							break;
						}
					default:
						{
							break;
						}
				}
			}

			// requires another talent?
			Talent reqTalent;

			return
				Owner.FreeTalentPoints >= diff &&
				(entry.RequiredId == TalentId.None || (ById.TryGetValue(entry.RequiredId, out reqTalent) &&
				(entry.RequiredRank == 0 || reqTalent.Rank >= entry.RequiredRank)));
		}
		#endregion

		#region Learn
		/// <summary>
		/// Tries to learn the given talent on the given rank
		/// </summary>
		/// <returns>Whether it was learnt</returns>
		public bool Learn(TalentId id, int rank)
		{
			var entry = TalentMgr.GetEntry(id);

			return entry != null && Learn(entry, rank);
		}

		public bool Learn(TalentId id, int rank, out SpellId learnedSpell)
		{
			var talent = TalentMgr.GetEntry(id);
			if (Learn(talent, rank, out learnedSpell))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Tries to learn the given talent on the given rank
		/// </summary>
		/// <returns>Whether it was learnt</returns>
		public bool Learn(TalentEntry entry, int rank)
		{
			if (!CanLearn(entry, rank))
			{
				return false;
			}

			Set(entry, rank);
			return true;
		}

		public bool Learn(TalentEntry entry, int rank, out SpellId learnedSpell)
		{
			if (Learn(entry, rank))
			{
				learnedSpell = entry.Spells[0].SpellId;
				return true;
			}
			learnedSpell = SpellId.None;
			return false;
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
			var points = Owner.FreeTalentPoints;
			Owner.FreeTalentPoints = 300;			// need extra Talent points to avoid internal checks to fail

			foreach (var talentTree in TalentMgr.TreesByClass[(int)clss])
			{
				if (talentTree == null) continue;
				foreach (var entry in talentTree)
				{
					if (entry == null) continue;
					Learn(entry, entry.MaxRank);
				}
			}

			Owner.FreeTalentPoints = points;
		}
		#endregion

		#region Set & Add
		/// <summary>
		/// Sets the given talent to the given rank without any checks.
		/// Make sure that the given TalentId is valid for this Character's class.
		/// </summary>
		public void Set(TalentId id, int rank)
		{
			TalentEntry talent = TalentMgr.GetEntry(id);

			Set(talent, rank);
		}

		/// <summary>
		/// Sets the given talent to the given rank without any checks
		/// </summary>
		public void Set(TalentEntry entry, int rank)
		{
			Talent talent;
			if (!ById.TryGetValue(entry.Id, out talent))
			{
				ById.Add(entry.Id, new Talent(this, entry, rank));
			}
			else
			{
				talent.Rank = rank;
			}
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

		public int GetFreePlayerTalentPoints(int level)
		{
			if (level < 10)
			{
				return -TotalPointsSpent;
			}
			return level - 9 - TotalPointsSpent;
		}

		public int GetFreePetTalentPoints(int level)
		{
			if (level < 20)
			{
				return -TotalPointsSpent;

			}
			return (level - 19) / 4 - TotalPointsSpent;
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
		/// Resets all talents
		/// </summary>
		public void ResetAll()
		{
			foreach (var talent in ById.Values)
			{
				talent.Remove();
			}
			ById.Clear();
		}

		/// <summary>
		/// Removes the given amount of arbitrarily selected talents (always removes higher level talents first)
		/// </summary>
		public void RemoveTalents(int count)
		{
			var trees = TalentMgr.GetTrees(Owner.Class);

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
										TalentHandler.SendTalentGroupList(Owner);
										return;
									}
								}
							}
						}
					}
				}
			}

			TalentHandler.SendTalentGroupList(Owner);
		}
		#endregion

		#region Dual Speccing

		public void ApplySpec(List<ITalentRecord> spec)
		{
			ResetAll();
			foreach (var record in spec)
			{
				Learn(record.Entry, record.Rank + 1);  // record stores a zero-based rank
			}
		}

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