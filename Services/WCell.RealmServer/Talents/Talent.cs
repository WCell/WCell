/*************************************************************************
 *
 *   file		: Talent.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-24 00:08:42 +0100 (sø, 24 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1212 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Talents;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Talents
{
	public class Talent
	{
		public readonly TalentCollection Talents;
		public readonly TalentEntry Entry;

		int m_rank;

		internal Talent(TalentCollection talents, TalentEntry entry)
		{
			Talents = talents;
			Entry = entry;
		}

		public Talent(TalentCollection talents, TalentEntry entry, int rank)
		{
			m_rank = -1;
			Talents = talents;
			Entry = entry;
			Rank = rank;
		}

		public Spell Spell
		{
			get { return Entry.Spells[m_rank]; }
		}

		/// <summary>
		/// The actual rank, as displayed in the GUI
		/// </summary>
		public int ActualRank
		{
			get { return Rank + 1; }
			set
			{
				Rank = value - 1;
			}
		}

		/// <summary>
		/// Current zero-based rank of this Talent. 
		/// The rank displayed in the GUI is Rank+1.
		/// </summary>
		public int Rank
		{
			get { return m_rank; }
			set
			{
				int diff;
				if (m_rank > value)
				{
					// remove Ranks
					if (value < -1)
					{
						value = -1;
					}
					diff = m_rank - value;

					Talents.UpdateFreeTalentPointsSilently(diff);

					for (var i = m_rank; i >= value + 1; i--)
					{
						// remove higher ranks
						var spell = Entry.Spells[i];
						Talents.Owner.Spells.Remove(spell);
					}

					if (value < 0)
					{
						// remove from TalentCollection
						Talents.ById.Remove(Entry.Id);
					}
				}
				else if (value > m_rank)
				{
					// add Ranks
					if (value > Entry.MaxRank - 1)
					{
						value = Entry.MaxRank - 1;
					}

					diff = value - m_rank;

					for (var i = m_rank + 1; i <= value; i++)
					{
						Talents.Owner.Spells.AddSpell(Entry.Spells[value]);
					}

					// take points
					Talents.UpdateFreeTalentPointsSilently(-diff);
				}
				else
				{
					return;
				}
				Talents.m_treePoints[Entry.Tree.TabIndex] += diff;
				m_rank = value;
			}
		}

		/// <summary>
		/// Sets the rank without sending any packets or doing checks.
		/// </summary>
		internal void SetRankSilently(int rank)
		{
			m_rank = rank;
			Talents.m_treePoints[Entry.Tree.TabIndex] += rank + 1;
		}

		public void Remove()
		{
			Remove(true);
		}

		/// <summary>
		/// Removes all ranks of this talent.
		/// </summary>
		internal void Remove(bool update)
		{
			Rank = -1;
			if (update)
			{
				TalentHandler.SendTalentGroupList(Talents);
			}
		}
	}

	public struct SimpleTalentDescriptor
	{
		public TalentId TalentId;
		public int Rank;
	}
}