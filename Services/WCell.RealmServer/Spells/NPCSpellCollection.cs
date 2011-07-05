using System;
using System.Collections.Generic;
using Cell.Core;
using WCell.Constants.Spells;
using WCell.Util.ObjectPools;
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

		protected HashSet<Spell> m_readySpells;
		protected List<CooldownRemoveTimer> m_cooldowns;

		#region Init & Cleanup
		private NPCSpellCollection()
		{
			m_readySpells = new HashSet<Spell>();
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
			foreach (var spell in m_readySpells)
			{
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
			AddReadySpell(spell);
		}

		/// <summary>
		/// Adds the given spell as ready. Once casted, the spell will be removed.
		/// This can be used to signal a one-time cast of a spell whose priority is to be
		/// compared to the other spells.
		/// </summary>
		public void AddReadySpell(Spell spell)
		{
			if (!spell.IsPassive)
			{
				m_readySpells.Add(spell);
			}
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
					AddCooldown(spell, millis);
				}
			}
			else
			{
				// add existing cooldown
				millis = Owner.Auras.GetModifiedInt(SpellModifierType.CooldownTime, spell, millis);
				AddCooldown(spell, millis);
			}
		}

		public void AddCooldown(Spell spell, DateTime cdTime)
		{
			var millis = (cdTime - DateTime.Now).ToMilliSecondsInt();
			AddCooldown(spell, millis);
		}

		private void AddCooldown(Spell spell, int millis)
		{
			if (millis <= 0) return;
			m_readySpells.Remove(spell);

			var action = new CooldownRemoveTimer(millis, spell);
			Owner.AddUpdateAction(action);
			if (m_cooldowns == null)
			{
				m_cooldowns = new List<CooldownRemoveTimer>();
			}
			m_cooldowns.Add(action);
		}

		public override void ClearCooldowns()
		{
			var context = Owner.ContextHandler;
			if (context != null)
			{
				context.AddMessage(() =>
				{
					if (m_cooldowns == null) return;
					foreach (var cd in m_cooldowns)
					{
						Owner.RemoveUpdateAction(cd);
						AddReadySpell(cd.Spell);
					}
				});
			}
		}

		public override bool IsReady(Spell spell)
		{
			return m_readySpells.Contains(spell);
		}

		public override void ClearCooldown(Spell spell, bool alsoCategory = true)
		{
			if (m_cooldowns != null)
			{
				for (var i = 0; i < m_cooldowns.Count; i++)
				{
					var cd = m_cooldowns[i];
					if (cd.Spell.Id != spell.Id) continue;

					m_cooldowns.Remove(cd);
					AddReadySpell(cd.Spell);
					break;
				}
			}
		}

		/// <summary>
		/// Returns the delay until the given spell has cooled down in milliseconds
		/// </summary>
		public int GetRemainingCooldownMillis(Spell spell)
		{
			if (m_cooldowns == null) return 0;

			for (var i = 0; i < m_cooldowns.Count; i++)
			{
				var cd = m_cooldowns[i];
				if (cd.Spell.Id != spell.Id) continue;

				return cd.GetDelayUntilNextExecution(Owner);
			}
			return 0;
		}

		protected class CooldownRemoveTimer : OneShotObjectUpdateTimer
		{
			public CooldownRemoveTimer(int millis, Spell spell) : base(millis, null)
			{
				Spell = spell;
				Callback = DoRemoveCooldown;
			}

			public Spell Spell
			{
				get;
				set;
			}

			void DoRemoveCooldown(WorldObject owner)
			{
				((NPCSpellCollection)((NPC)owner).Spells).AddReadySpell(Spell);
			}
		}
		#endregion
	}
}