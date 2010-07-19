using System;
using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.Util.Threading;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using NHibernate.Mapping;
using NHibernate.Engine;
using WCell.Util;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// NPC spell collection
	/// TODO: is it necessary to create a dictionary if npc doesn't have spells?
	/// </summary>
	public class NPCSpellCollection : SpellCollection
	{
		protected bool m_defaultSpells;

		protected List<Spell> m_readySpells;
		protected List<CooldownRemoveAction> m_cooldowns;

		public NPCSpellCollection(NPC owner)
			: base(owner, false)
		{
			m_byId = owner.Entry.Spells;
			m_readySpells = new List<Spell>();
			if (m_byId != null)
			{
				m_defaultSpells = true;
				foreach (var spell in m_byId.Values)
				{
					AddReadySpell(spell);
					OnNewSpell(spell);
				}
			}
			else
			{
				m_defaultSpells = false;
				m_byId = new Dictionary<uint, Spell>(5);
			}
		}

		public NPC OwnerNPC
		{
			get
			{
				return Owner as NPC;
			}
		}

		public List<Spell> ReadySpells
		{
			get { return m_readySpells; }
		}

		/// <summary>
		/// The max combat of any 1vs1 combat spell
		/// </summary>
		public float MaxCombatSpellRange
		{
			get;
			private set;
		}

		/// <summary>
		/// Shuffles all currently ready Spells
		/// </summary>
		public void ShuffleReadySpells()
		{
			Utility.Shuffle(m_readySpells);
		}

		public Spell GetReadySpell(SpellId spellId)
		{
			for (int i = 0; i < m_readySpells.Count; i++)
			{
				var spell = m_readySpells[i];
				if (spell.SpellId == spellId)
				{
					return spell;
				}
			}
			return null;
		}

		#region Add / Remove
		public override void AddSpell(Spell spell)
		{
			EnsureDefaultSpellsWontChange();
			//NPCOwner.Brain.ActionCollection.AddFactory
			AddReadySpell(spell);

			base.AddSpell(spell);
		}

		void OnNewSpell(Spell spell)
		{
			if (!spell.IsAreaSpell && !spell.IsAura && spell.HasHarmfulEffects)
			{
				MaxCombatSpellRange = Math.Max(MaxCombatSpellRange, Owner.GetSpellMaxRange(spell, null));
			}
		}

		void AddReadySpell(Spell spell)
		{
			if (!spell.IsPassive)
			{
				m_readySpells.Add(spell);
			}
		}

		public override void Clear()
		{
			EnsureDefaultSpellsWontChange();
			m_readySpells.Clear();

			base.Clear();
		}

		public override void Remove(Spell spell)
		{
			EnsureDefaultSpellsWontChange();

			m_readySpells.Remove(spell);
			base.Remove(spell);

			if (Owner.GetSpellMaxRange(spell, null) >= MaxCombatSpellRange)
			{
				MaxCombatSpellRange = 0f;
				foreach (var sp in m_byId.Values)
				{
					if (sp.Range.MaxDist > MaxCombatSpellRange)
					{
						MaxCombatSpellRange = Owner.GetSpellMaxRange(sp, null);
					}
				}
			}
		}
		/// <summary>
		/// Create a new collection if we are currently sharing the default Spells for this
		/// NPC-type.
		/// </summary>
		private void EnsureDefaultSpellsWontChange()
		{
			if (m_defaultSpells)
			{
				m_defaultSpells = false;
				var newDictionary = new Dictionary<uint, Spell>(m_byId.Count);
				foreach (var pair in m_byId)
				{
					newDictionary.Add(pair.Key, pair.Value);
				}

				m_byId = newDictionary;
			}
		}
		#endregion

		#region Cooldowns
		/// <summary>
		/// When NPC Spells cooldown, they get removed from the list of
		/// ready Spells.
		/// </summary>
		public override void AddCooldown(Spell spell, Item item)
		{
            var millis = Math.Max(spell.GetCooldown(Owner), spell.CategoryCooldownTime);
			ProcessCooldown(spell, millis);
		}

        public void RestoreCooldown(Spell spell, DateTime cdTime)
        {
            var millis = (cdTime - DateTime.Now).Milliseconds;
            ProcessCooldown(spell, millis);
        }

	    private void ProcessCooldown(Spell spell, int millis)
		{
			if (millis <= 0) return;
	        m_readySpells.Remove(spell);
	        
            var ticks = millis / Owner.Region.UpdateDelay;
	        var action = new CooldownRemoveAction(ticks, spell, owner => m_readySpells.Add(spell));
	        Owner.CallPeriodically(action);
	        if (m_cooldowns == null)
	        {
	            m_cooldowns = new List<CooldownRemoveAction>();
	        }
	        m_cooldowns.Add(action);
	    }
        
		public override void ClearCooldowns()
		{
			var context = Owner.ContextHandler;
			if (context != null)
				context.AddMessage(() => {
					if (m_cooldowns != null)
					{
						for (var i = 0; i < m_cooldowns.Count; i++)
						{
							var cd = m_cooldowns[i];
							Owner.RemoveUpdateAction(cd);
							m_readySpells.Add(cd.Spell);
						}
					}
				});
		}

		public override bool IsReady(Spell spell)
		{
			return m_readySpells.Contains(spell);
		}

		public override void ClearCooldown(Spell spell)
		{
			if (m_cooldowns != null)
			{
				for (var i = 0; i < m_cooldowns.Count; i++)
				{
					var cd = m_cooldowns[i];
					if (cd.Spell.Id == spell.Id)
					{
					    m_cooldowns.Remove(cd);
						m_readySpells.Add(cd.Spell);
						break;
					}
				}
			}
		}

        public int TicksUntilCooldown(Spell spell)
        {
            if (m_cooldowns == null) return 0;

            for (var i = 0; i < m_cooldowns.Count; i++)
            {
                var cd = m_cooldowns[i];
                if (cd.Spell.Id != spell.Id) continue;

                return ((Owner.Region.TickCount + (int)Owner.EntityId.Low) % cd.Ticks);
            }
            return 0;
        }

		protected class CooldownRemoveAction : OneShotUpdateObjectAction
		{
			public CooldownRemoveAction(int ticks, Spell spell, Action<WorldObject> action)
				: base(ticks, action)
			{
				Spell = spell;
			}

			public Spell Spell
			{
				get;
				set;
			}
		}

		#endregion
	}
}