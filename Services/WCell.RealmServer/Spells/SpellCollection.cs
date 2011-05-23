/*************************************************************************
 *
 *   file		: SpellCollection.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-29 04:07:03 +0100 (fr, 29 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1232 $
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
using Castle.ActiveRecord;
using Cell.Core;
using NLog;
using WCell.Constants.Achievements;
using WCell.Constants.Spells;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.Util.NLog;
using WCell.Util.Threading;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class SpellCollection : IEnumerable<Spell>
	{
		public static readonly ObjectPool<List<Spell>> SpellListPool = ObjectPoolMgr.CreatePool(() => new List<Spell>(), true);

		/// <summary>
		/// All spells by id
		/// </summary>
		protected Dictionary<SpellId, Spell> m_byId;

		/// <summary>
		/// Additional effects to be triggered when casting certain Spells
		/// </summary>
		private List<AddTargetTriggerHandler> m_TargetTriggers;

		#region Init & Cleanup
		protected SpellCollection()
		{
			m_byId = new Dictionary<SpellId, Spell>();
		}

		protected virtual void Initialize(Unit owner)
		{
			Owner = owner;
		}

		protected internal virtual void Recycle()
		{
			Owner = null;

			m_byId.Clear();
			if (m_TargetTriggers != null)
			{
				m_TargetTriggers.Clear();
			}
		}
		#endregion

		/// <summary>
		/// Required by SpellCollection
		/// </summary>
		public Unit Owner
		{
			get;
			internal protected set;
		}

		/// <summary>
		/// The amount of Spells in this Collection
		/// </summary>
		public int Count
		{
			get { return m_byId.Count; }
		}

		public bool HasSpells
		{
			get { return m_byId.Count > 0; }
		}

		public IEnumerable<Spell> AllSpells
		{
			get { return m_byId.Values; }
		}

		/// <summary>
		/// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
		/// </summary>
		public void AddSpell(uint spellId)
		{
			var spell = SpellHandler.ById[spellId];
			AddSpell(spell);
		}

		/// <summary>
		/// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
		/// </summary>
		public void AddSpell(SpellId spellId)
		{
			var spell = SpellHandler.Get(spellId);
			AddSpell(spell);
		}

		/// <summary>
		/// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
		/// </summary>
		public virtual void AddSpell(Spell spell)
		{
			//Add(id, spell, true);
			m_byId[spell.SpellId] = spell;
			OnAdd(spell);
		}

		protected void OnAdd(Spell spell)
		{
			if (spell.IsPassive)
			{
				Owner.SpellCast.TriggerSelf(spell);
			}
			if (spell.AdditionallyTaughtSpells.Count > 0)
			{
				foreach (var spe in spell.AdditionallyTaughtSpells)
				{
					AddSpell(spe);
				}
			}
            if (Owner is Character)
            {
                ((Character)Owner).Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.LearnSpell, spell.Id);
            }
		}

		/// <summary>
		/// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
		/// </summary>
		public void AddSpell(IEnumerable<SpellId> spells)
		{
			foreach (var spell in spells)
			{
				AddSpell(spell);
			}
		}

		/// <summary>
		/// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
		/// </summary>
		public void AddSpell(params SpellId[] spells)
		{
			foreach (var spell in spells)
			{
				AddSpell(spell);
			}
		}

		/// <summary>
		/// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
		/// </summary>
		public void AddSpell(IEnumerable<Spell> spells)
		{
			foreach (var spell in spells)
			{
				AddSpell(spell);
			}
		}

		/// <summary>
		/// Adds the spell without doing any further checks or adding any spell-related skills or showing animations
		/// </summary>
		public void OnlyAdd(SpellId id)
		{
			m_byId.Add(id, SpellHandler.ById.Get((uint)id));
		}

		public void OnlyAdd(Spell spell)
		{
			m_byId.Add(spell.SpellId, spell);
		}

		public bool Contains(uint id)
		{
			return m_byId.ContainsKey((SpellId)id);
		}

		public bool Contains(SpellId id)
		{
			return m_byId.ContainsKey(id);
		}

		public Spell this[SpellId id]
		{
			get
			{
				Spell spell;
				m_byId.TryGetValue(id, out spell);
				return spell;
			}
		}

		public Spell this[uint id]
		{
			get
			{
				Spell spell;
				m_byId.TryGetValue((SpellId)id, out spell);
				return spell;
			}
		}

		/// <summary>
		/// Gets the highest rank of the line that this SpellCollection contains
		/// </summary>
		public Spell GetHighestRankOf(SpellLineId lineId)
		{
			return GetHighestRankOf(lineId.GetLine());
		}

		/// <summary>
		/// Gets the highest rank of the line that this SpellCollection contains
		/// </summary>
		public Spell GetHighestRankOf(SpellLine line)
		{
			var rank = line.HighestRank;
			do
			{
				if (Contains(rank.SpellId))
				{
					return rank;
				}
			} while ((rank = rank.PreviousRank) != null);
			return null;
		}

		public void Remove(SpellId spellId)
		{
			Replace(SpellHandler.Get(spellId), null);
		}

		public bool Remove(uint spellId)
		{
			Remove((SpellId)spellId);
			return true;
		}

		public virtual bool Remove(Spell spell)
		{
			return Replace(spell, null);
		}

		public virtual void Clear()
		{
			foreach (var spell in m_byId.Values)
			{
				if (spell.IsPassive)
				{
					Owner.Auras.Remove(spell);
				}
			}
			m_byId.Clear();
		}

		/// <summary>
		/// Only works if you have 2 valid spell ids and oldSpellId already exists.
		/// </summary>
		public void Replace(SpellId oldSpellId, SpellId newSpellId)
		{
			Spell oldSpell, newSpell = SpellHandler.Get(newSpellId);
			if (m_byId.TryGetValue(oldSpellId, out oldSpell))
			{
				Replace(oldSpell, newSpell);
			}
		}

		/// <summary>
		/// Replaces or (if newSpell == null) removes oldSpell; does nothing if oldSpell doesn't exist.
		/// </summary>
		public virtual bool Replace(Spell oldSpell, Spell newSpell)
		{
			//if (m_byId.Remove((uint)oldSpell))
			if (m_byId.Remove(oldSpell.SpellId))
			{
				if (oldSpell.IsPassive)
				{
					Owner.Auras.Remove(oldSpell);
				}
				if (newSpell != null)
				{
					AddSpell(newSpell);
				}
				return true;
			}

			if (newSpell != null)
			{
				AddSpell(newSpell);
			}
			return false;
		}

		public virtual void AddDefaultSpells()
		{
		}

		public abstract void AddCooldown(Spell spell, Item casterItem);

		public abstract void ClearCooldowns();

		public abstract bool IsReady(Spell spell);

		/// <summary>
		/// Clears the cooldown for the given spell
		/// </summary>
		public void ClearCooldown(SpellId spellId, bool alsoClearCategory = true)
		{
			var spell = SpellHandler.Get(spellId);
			if (spell == null)
			{
				try
				{
					throw new ArgumentException("No spell given for cooldown", "spellId");
				}
				catch (Exception e)
				{
					LogUtil.WarnException(e);
				}
				return;
			}
			ClearCooldown(spell, alsoClearCategory);
		}

		public abstract void ClearCooldown(Spell cooldownSpell, bool alsoClearCategory = true);

		#region Special Spell Casting behavior

		public List<AddTargetTriggerHandler> TargetTriggers
		{
			get
			{
				if (m_TargetTriggers == null)
				{
					m_TargetTriggers = new List<AddTargetTriggerHandler>(3);
				}
				return m_TargetTriggers;
			}
		}

		/// <summary>
		/// Trigger all spells that might be triggered by the given Spell
		/// </summary>
		/// <param name="spell"></param>
		internal void TriggerSpellsFor(SpellCast cast)
		{
			if (m_TargetTriggers == null) return;

			int val;
			var spell = cast.Spell;
			for (var i = 0; i < m_TargetTriggers.Count; i++)
			{
				var triggerHandler = m_TargetTriggers[i];
				var effect = triggerHandler.SpellEffect;
				if (spell.SpellClassOptions.SpellClassSet == effect.Spell.SpellClassOptions.SpellClassSet &&
					effect.MatchesSpell(spell) &&
					(((val = effect.CalcEffectValue(Owner)) >= 100) || Utility.Random(0, 101) <= val) &&
					spell != effect.TriggerSpell)	// prevent inf loops
				{
					var caster = triggerHandler.Aura.CasterUnit;
					if (caster != null)
					{
						//cast.Trigger(effect.TriggerSpell, cast.Targets.MakeArray());
						cast.Trigger(effect.TriggerSpell);
					}
				}
			}
		}
		#endregion

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<Spell> GetEnumerator()
		{
			foreach (var spell in m_byId.Values)
			{
				yield return spell;
			}
		}
	}
}