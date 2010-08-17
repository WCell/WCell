using System;
using System.Collections.Generic;
using Cell.Core;
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
	/// </summary>
	public class NPCSpellCollection : SpellCollection
	{
		/// <summary>
		/// Cooldown of NPC spells, if they don't have one
		/// </summary>
		public static int DefaultNPCSpellCooldownMillis = 1500;

		static readonly ObjectPool<NPCSpellCollection> NPCSpellCollectionPool =
			new ObjectPool<NPCSpellCollection>(() => new NPCSpellCollection());

		public static NPCSpellCollection Obtain(NPC npc)
		{
			var spells = NPCSpellCollectionPool.Obtain();
			spells.Initialize(npc);
			return spells;
		}

		protected List<Spell> m_readySpells;
		protected List<CooldownRemoveAction> m_cooldowns;
		
		#region Init & Cleanup
		private NPCSpellCollection()
		{
			m_readySpells = new List<Spell>(5);
		}

		protected internal override void Recycle()
		{
			base.Recycle();

			m_readySpells.Clear();
			if (m_cooldowns != null)
			{
				m_cooldowns.Clear();
			}

			NPCSpellCollectionPool.Recycle(this);
		}
		#endregion

		public NPC OwnerNPC
		{
			get { return Owner as NPC; }
		}

		public IEnumerable<Spell> ReadySpells
		{
			get { return m_readySpells; }
		}

		public int ReadyCount
		{
			get { return m_readySpells.Count; }
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
			if (!m_byId.ContainsKey(spell.SpellId))
			{
				//NPCOwner.Brain.ActionCollection.AddFactory
				base.AddSpell(spell);

				OnNewSpell(spell);
			}
		}

		void OnNewSpell(Spell spell)
		{
			if (!spell.IsAreaSpell && !spell.IsAura && spell.HasHarmfulEffects)
			{
				MaxCombatSpellRange = Math.Max(MaxCombatSpellRange, Owner.GetSpellMaxRange(spell, null));
			}
			if (!spell.IsPassive)
			{
				AddReadySpell(spell);
			}
		}

		/// <summary>
		/// Adds the given spell as ready. Once casted, the spell will be removed.
		/// This can be used to signal a one-time cast of a spell whose priority is to be
		/// compared to the other spells.
		/// </summary>
		public void AddReadySpell(Spell spell)
		{
			m_readySpells.Add(spell);
		}

		public override void Clear()
		{
			base.Clear();
			m_readySpells.Clear();
		}

		public override bool Remove(Spell spell)
		{
			if (base.Remove(spell))
			{
				m_readySpells.Remove(spell);

				if (Owner.GetSpellMaxRange(spell, null) >= MaxCombatSpellRange)
				{
					// find new max combat spell range
					MaxCombatSpellRange = 0f;
					foreach (var sp in m_byId.Values)
					{
						if (sp.Range.MaxDist > MaxCombatSpellRange)
						{
							MaxCombatSpellRange = Owner.GetSpellMaxRange(sp, null);
						}
					}
				}
				return true;
			}
			return false;
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
			if (millis <= 0)
			{
				if (spell.CastDelay == 0 && spell.Durations.Max == 0)
				{
					// no cooldown, no cast delay, no duration: Add default cooldown
					millis = Owner.Auras.GetModifiedInt(SpellModifierType.CooldownTime, spell, DefaultNPCSpellCooldownMillis);
					ProcessCooldown(spell, millis);
				}
			}
			else
			{
				// add existing cooldown
				millis = Owner.Auras.GetModifiedInt(SpellModifierType.CooldownTime, spell, millis);
				ProcessCooldown(spell, millis);
			}
		}

		public void RestoreCooldown(Spell spell, DateTime cdTime)
		{
			var millis = (cdTime - DateTime.Now).ToMilliSecondsInt();
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
				context.AddMessage(() =>
				{
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

		public override void ClearCooldown(Spell spell, bool alsoCategory)
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