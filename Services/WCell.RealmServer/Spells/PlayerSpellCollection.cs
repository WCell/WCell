using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Castle.ActiveRecord;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.Items;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.Threading;
using WCell.RealmServer.Database;
using WCell.Util;
using WCell.RealmServer.Spells.Auras.Handlers;

namespace WCell.RealmServer.Spells
{
	public class PlayerSpellCollection : SpellCollection
	{
		private Timer m_offlineCooldownTimer;
		private object m_lock;
		private uint m_ownerId;

		/// <summary>
		/// Whether to send Update Packets
		/// </summary>
		protected bool m_sendPackets;

		/// <summary>
		/// Amount of currently added modifiers that require charges.
		/// If > 0, will iterate over modifiers and remove charges after SpellCasts.
		/// </summary>
		public int ModifiersWithCharges
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Flat modifiers of spells
		/// </summary>
		public readonly List<AddModifierEffectHandler> SpellModifiersFlat = new List<AddModifierEffectHandler>();

		/// <summary>
		/// Percent modifiers of spells
		/// </summary>
		public readonly List<AddModifierEffectHandler> SpellModifiersPct = new List<AddModifierEffectHandler>();

		/// <summary>
		/// Additional effects to be triggered when casting certain Spells
		/// </summary>
		public readonly List<AddTargetTriggerHandler> TargetTriggers = new List<AddTargetTriggerHandler>(1);

		/// <summary>
		/// All current Spell-cooldowns. 
		/// Each SpellId has an expiry time associated with it
		/// </summary>
		protected Dictionary<uint, ISpellIdCooldown> m_idCooldowns;
		/// <summary>
		/// All current category-cooldowns. 
		/// Each category has an expiry time associated with it
		/// </summary>
		protected Dictionary<uint, ISpellCategoryCooldown> m_categoryCooldowns;

		public PlayerSpellCollection(Character owner)
			: base(owner)
		{
			m_sendPackets = false;
		}

		public Dictionary<uint, ISpellIdCooldown> IdCooldowns
		{
			get { return m_idCooldowns; }
		}

		public Dictionary<uint, ISpellCategoryCooldown> CategoryCooldowns
		{
			get { return m_categoryCooldowns; }
		}

		/// <summary>
		/// If this is a player's 
		/// </summary>
		public Character OwnerChar
		{
			get { return Owner as Character; }
		}

		public void AddNew(Spell spell)
		{
			AddSpell(spell, true);
		}

		public override void AddSpell(Spell spell)
		{
			AddSpell(spell, true);
		}

		/// <summary>
		/// Adds the spell without doing any further checks nor adding any spell-related skills or showing animations (after load)
		/// </summary>
		internal void OnlyAdd(SpellRecord record)
		{
			var id = record.SpellId;
			if (!m_byId.ContainsKey(id))
			{
				//DeleteFromDB(id);
				var spell = SpellHandler.Get(id);
				m_byId[id] = spell;
			}
		}


		/// <summary>
		/// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
		/// </summary>
		void AddSpell(Spell spell, bool isNew)
		{
			// make sure the char knows the skill that this spell belongs to
			if (spell.Ability != null && !OwnerChar.Skills.Contains(spell.Ability.Skill.Id))
			{
				OwnerChar.Skills.Add(spell.Ability.Skill, true);
			}

			if (!m_byId.ContainsKey(spell.Id))
			{
				if (m_sendPackets && isNew)
				{
					SpellHandler.SendLearnedSpell(OwnerChar.Client, spell.Id);
					if (!spell.IsPassive)
					{
						SpellHandler.SendVisual(Owner, 362);
					}
				}
				OwnerChar.m_record.AddSpell(spell.Id);

				base.AddSpell(spell);
			}
		}

		public override void Clear()
		{
			foreach (var spell in m_byId.Values.ToArray())
			{
				OnRemove(spell);
				if (m_sendPackets)
				{
					SpellHandler.SendSpellRemoved(OwnerChar, spell.Id);
				}
			}

			m_byId.Clear();
		}

		/// <summary>
		/// Replaces or (if newSpell == null) removes oldSpell.
		/// </summary>
		public override void Replace(Spell oldSpell, Spell newSpell)
		{
			var hasOldSpell = oldSpell != null && m_byId.Remove(oldSpell.Id);
			if (hasOldSpell)
			{
				OnRemove(oldSpell);
				if (newSpell == null)
				{
					if (m_sendPackets)
					{
						SpellHandler.SendSpellRemoved(OwnerChar, oldSpell.Id);
						return;
					}
				}
			}

			if (newSpell != null)
			{
				if (m_sendPackets && hasOldSpell)
				{
					SpellHandler.SendSpellSuperceded(OwnerChar.Client, oldSpell.Id, newSpell.Id);
				}

				AddSpell(newSpell, !hasOldSpell);
			}
		}

		/// <summary>
		/// Enqueues a new task to remove that spell from DB
		/// </summary>
		private void OnRemove(Spell spell)
		{
			if (spell.Skill != null)
			{
				OwnerChar.Skills.Remove(spell.SkillId);
			}
			OwnerChar.m_record.RemoveSpell(spell.Id);
		}

		/// <summary>
		/// Called when the player logs out
		/// </summary>
		internal void OnOwnerLoggedOut()
		{
			m_ownerId = Owner.EntityId.Low;
			Owner = null;
			m_sendPackets = false;
			m_lock = new object();
			SpellHandler.PlayerSpellCollections[m_ownerId] = this;

			m_offlineCooldownTimer = new Timer(FinalizeCooldowns);
			m_offlineCooldownTimer.Change(SpellHandler.DefaultCooldownSaveDelay, TimeSpan.Zero);
		}

		/// <summary>
		/// Called when the player logs back in
		/// </summary>
		internal void OnReconnectOwner(Character owner)
		{
			lock (m_lock)
			{
				if (m_offlineCooldownTimer != null)
				{
					m_offlineCooldownTimer.Change(Timeout.Infinite, Timeout.Infinite);
					m_offlineCooldownTimer = null;
				}
			}
			Owner = owner;
		}

		internal void PlayerInitialize()
		{
			// re-apply passive effects
			var chr = OwnerChar;
			foreach (var spell in m_byId.Values)
			{
				if (spell.IsPassive && !spell.HasHarmfulEffects)
				{
					chr.SpellCast.Start(spell, true, Owner);
				}
				if (spell.Talent != null)
				{
					chr.Talents.AddExisting(spell.Talent, spell.Rank);
				}
			}

			m_sendPackets = true;
		}

		public override void AddDefaults()
		{
			// add the default Spells for the race/class
			for (var i = 0; i < OwnerChar.Archetype.Spells.Count; i++)
			{
				var spell = OwnerChar.Archetype.Spells[i];
				AddNew(spell);
			}

			// add all default Spells of all Skills the Char already has
			//foreach (var skill in OwnerChar.Skills)
			//{
			//    for (var i = 0; i < skill.SkillLine.InitialAbilities.Count; i++)
			//    {
			//        var ability = skill.SkillLine.InitialAbilities[i];
			//        AddNew(ability.Spell);
			//    }
			//}
		}

		#region Enhancers
		public void RemoveEnhancer(SpellEffect effect)
		{

		}

		/// <summary>
		/// Returns the modified value (modified by certain talents) of the given type for the given spell (as int)
		/// </summary>
		public int GetModifiedInt(SpellModifierType type, Spell spell, int value)
		{
			var flatMod = GetModifierFlat(type, spell);
			var percentMod = GetModifierPercent(type, spell);
			return ((value + flatMod) * (100 + percentMod)) / 100;
		}

		/// <summary>
		/// Returns the modified value (modified by certain talents) of the given type for the given spell (as float)
		/// </summary>
		public float GetModifiedFloat(SpellModifierType type, Spell spell, float value)
		{
			var flatMod = GetModifierFlat(type, spell);
			var percentMod = GetModifierPercent(type, spell);
			return (value + flatMod) * (1 + (percentMod / 100f));
		}

		/// <summary>
		/// Returns the percent modifier (through certain talents) of the given type for the given spell
		/// </summary>
		public int GetModifierPercent(SpellModifierType type, Spell spell)
		{
			var amount = 0;
			for (var i = 0; i < SpellModifiersPct.Count; i++)
			{
				var modifier = SpellModifiersPct[i];
				if ((SpellModifierType)modifier.SpellEffect.MiscValue == type &&
					spell.SpellClassSet == modifier.SpellEffect.Spell.SpellClassSet &&
					spell.MatchesMask(modifier.SpellEffect.AffectMask))
				{
					amount += modifier.SpellEffect.ValueMin;
				}
			}
			return amount;
		}

		/// <summary>
		/// Returns the flat modifier (through certain talents) of the given type for the given spell
		/// </summary>
		public int GetModifierFlat(SpellModifierType type, Spell spell)
		{
			var amount = 0;
			for (var i = 0; i < SpellModifiersFlat.Count; i++)
			{
				var modifier = SpellModifiersFlat[i];
				if ((SpellModifierType)modifier.SpellEffect.MiscValue == type &&
					spell.SpellClassSet == modifier.SpellEffect.Spell.SpellClassSet &&
					spell.MatchesMask(modifier.SpellEffect.AffectMask))
				{
					amount += modifier.SpellEffect.ValueMin;
				}
			}
			return amount;
		}

		/// <summary>
		/// Trigger all spells that might be triggered by the given Spell
		/// </summary>
		/// <param name="spell"></param>
		public void TriggerSpellsFor(SpellCast cast)
		{
			int val;
			var spell = cast.Spell;
			for (var i = 0; i < TargetTriggers.Count; i++)
			{
				var triggerHandler = TargetTriggers[i];
				var effect = triggerHandler.SpellEffect;
				if (spell.SpellClassSet == effect.Spell.SpellClassSet &&
					spell.MatchesMask(effect.AffectMask) &&
					(((val = effect.CalcEffectValue(Owner)) >= 100) ||
					Utility.Random(0, 101) <= val))
				{
					var caster = triggerHandler.Aura.Caster;
					if (caster != null)
					{
						cast.Trigger(effect.TriggerSpell, cast.Targets.MakeArray());
					}
				}
			}
		}

		public void OnCasted(SpellCast cast)
		{
			TriggerSpellsFor(cast);
			var spell = cast.Spell;
			if (ModifiersWithCharges > 0)
			{
				var toRemove = new List<Aura>(3);
				for (var i = 0; i < SpellModifiersFlat.Count; i++)
				{
					var modifier = SpellModifiersFlat[i];
					if (spell.SpellClassSet == modifier.SpellEffect.Spell.SpellClassSet &&
						spell.MatchesMask(modifier.SpellEffect.AffectMask))
					{
						if (modifier.Charges > 0)
						{
							modifier.Charges--;
							if (modifier.Charges < 1)
							{
								toRemove.Add(modifier.Aura);
							}
						}
					}
				}
				for (var i = 0; i < SpellModifiersPct.Count; i++)
				{
					var modifier = SpellModifiersPct[i];
					if (spell.SpellClassSet == modifier.SpellEffect.Spell.SpellClassSet &&
						spell.MatchesMask(modifier.SpellEffect.AffectMask))
					{
						if (modifier.Charges > 0)
						{
							modifier.Charges--;
							if (modifier.Charges < 1)
							{
								toRemove.Add(modifier.Aura);
							}
						}
					}
				}

				foreach (var aura in toRemove)
				{
					aura.Remove(false);
				}
			}
		}
		#endregion

		#region Spell Constraints
		/// <summary>
		/// Add everything to the caster that this spell requires
		/// </summary>
		public void SatisfyConstraintsFor(Spell spell)
		{
			var chr = OwnerChar;
			// add reagents
			foreach (var reagent in spell.Reagents)
			{
				var templ = reagent.Template;
				if (templ != null)
				{
					var amt = reagent.Amount * 10;
					chr.Inventory.Ensure(templ, amt);
				}
			}

			// add tools
			if (spell.RequiredTools != null)
			{
				foreach (var tool in spell.RequiredTools)
				{
					chr.Inventory.Ensure(tool.Template, 1);
				}
			}
			if (spell.RequiredTotemCategories != null)
			{
				foreach (var cat in spell.RequiredTotemCategories)
				{
					var tool = ItemMgr.GetFirstTotemCat(cat);
					if (tool != null)
					{
						chr.Inventory.Ensure(tool, 1);
					}
				}
			}

			// Profession
			if (spell.Skill != null)
			{
			    chr.Skills.TryLearn(spell.SkillId);
			}


			// add spellfocus object (if not present)
			if (spell.RequiredSpellFocus != 0)
			{
				var go = chr.Region.GetGOWithSpellFocus(chr.Position, spell.RequiredSpellFocus,
					spell.Range.MaxDist > 0 ? (spell.Range.MaxDist) : 5f, chr.Phase);

				if (go == null)
				{
					foreach (var entry in GOMgr.Entries.Values)
					{
						if (entry is GOSpellFocusEntry &&
							((GOSpellFocusEntry)entry).SpellFocus == spell.RequiredSpellFocus)
						{
							entry.Spawn(chr, chr);
							break;
						}
					}
				}
			}
		}
		#endregion


		#region Cooldowns
		/// <summary>
		/// Tries to add the given Spell to the cooldown List.
		/// Returns false if Spell is still cooling down.
		/// </summary>
		public bool CheckCooldown(Spell spell)
		{
			// check for individual cooldown
			ISpellIdCooldown idCooldown = null;
			if (m_idCooldowns != null)
			{
				if (m_idCooldowns.TryGetValue(spell.Id, out idCooldown))
				{
					if (idCooldown.Until > DateTime.Now)
					{
						return false;
					}
					m_idCooldowns.Remove(spell.Id);
				}
			}

			// check for category cooldown
			ISpellCategoryCooldown catCooldown = null;
			if (spell.CategoryCooldownTime > 0)
			{
				if (m_categoryCooldowns != null)
				{
					if (m_categoryCooldowns.TryGetValue(spell.Category, out catCooldown))
					{
						if (catCooldown.Until > DateTime.Now)
						{
							return false;
						}
						m_categoryCooldowns.Remove(spell.Category);
					}
				}
			}

			// enqueue delete task for consistent cooldowns
			if (idCooldown is ConsistentSpellIdCooldown || catCooldown is ConsistentSpellCategoryCooldown)
			{
				var removedId = idCooldown as ConsistentSpellIdCooldown;
				var removedCat = catCooldown as ConsistentSpellCategoryCooldown;
				RealmServer.Instance.AddMessage(new Message(() =>
				{
					if (removedId != null)
					{
						removedId.Delete();
					}
					if (removedCat != null)
					{
						removedCat.Delete();
					}
				}));
			}
			return true;
		}

		public override void AddCooldown(Spell spell, Item casterItem)
		{
			// TODO: Add cooldown mods
			var itemSpell = casterItem != null && casterItem.Template.UseSpell != null;

			var cd = 0;
			if (itemSpell)
			{
				cd = casterItem.Template.UseSpell.Cooldown;
			}
			if (cd == 0)
			{
				cd = spell.GetCooldown(Owner);
			}

			var catCd = 0;
			if (itemSpell)
			{
				catCd = casterItem.Template.UseSpell.CategoryCooldown;
			}
			if (catCd == 0)
			{
				catCd = spell.CategoryCooldownTime;
			}

			if (cd > 0)
			{
				if (m_idCooldowns == null)
				{
					m_idCooldowns = new Dictionary<uint, ISpellIdCooldown>();
				}
				var idCooldown = new SpellIdCooldown
				{
					SpellId = spell.Id,
					Until = (DateTime.Now + TimeSpan.FromMilliseconds(cd))
				};

				if (itemSpell)
				{
					idCooldown.ItemId = casterItem.Template.Id;
				}
				m_idCooldowns[spell.Id] = idCooldown;
			}

			if (spell.CategoryCooldownTime > 0)
			{
				if (m_categoryCooldowns == null)
				{
					m_categoryCooldowns = new Dictionary<uint, ISpellCategoryCooldown>();
				}
				var catCooldown = new SpellCategoryCooldown
				{
					SpellId = spell.Id,
					Until = DateTime.Now.AddMilliseconds(catCd)
				};

				if (itemSpell)
				{
					catCooldown.CategoryId = casterItem.Template.UseSpell.CategoryId;
					catCooldown.ItemId = casterItem.Template.Id;
				}
				else
				{
					catCooldown.CategoryId = spell.Category;
				}
				m_categoryCooldowns[spell.Category] = catCooldown;
			}

		}

		/// <summary>
		/// Returns whether the given spell is still cooling down
		/// </summary>
		public override bool IsReady(Spell spell)
		{
			ISpellCategoryCooldown catCooldown;
			if (m_categoryCooldowns != null)
			{
				if (m_categoryCooldowns.TryGetValue(spell.Category, out catCooldown))
				{
					if (catCooldown.Until > DateTime.Now)
					{
						return true;
					}

					m_categoryCooldowns.Remove(spell.Category);
					if (catCooldown is ActiveRecordBase)
					{
						RealmServer.Instance.AddMessage(new Message(() => ((ActiveRecordBase)catCooldown).Delete()));
					}
				}
			}

			ISpellIdCooldown idCooldown;
			if (m_idCooldowns != null)
			{
				if (m_idCooldowns.TryGetValue(spell.Id, out idCooldown))
				{
					if (idCooldown.Until > DateTime.Now)
					{
						return true;
					}

					m_idCooldowns.Remove(spell.Id);
					if (idCooldown is ActiveRecordBase)
					{
						RealmServer.Instance.AddMessage(() => ((ActiveRecordBase)idCooldown).Delete());
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Clears all pending spell cooldowns.
		/// </summary>
		/// <remarks>Requires IO-Context.</remarks>
		public override void ClearCooldowns()
		{
			// send cooldown updates to client
			if (m_idCooldowns != null)
			{
				foreach (var pair in m_idCooldowns)
				{
					SpellHandler.SendClearCoolDown(OwnerChar, (SpellId)pair.Key);
				}
				m_idCooldowns.Clear();
			}
			if (m_categoryCooldowns != null)
			{
				foreach (var spell in m_byId.Values)
				{
					if (m_categoryCooldowns.ContainsKey(spell.Category))
					{
						SpellHandler.SendClearCoolDown(OwnerChar, spell.SpellId);
					}
				}
			}

			// remove and delete all cooldowns
			var cds = m_idCooldowns;
			var catCds = m_categoryCooldowns;
			m_idCooldowns = null;
			m_categoryCooldowns = null;
			RealmServer.Instance.AddMessage(new Message(() =>
			{
				if (cds != null)
				{
					foreach (var cooldown in cds.Values)
					{
						if (cooldown is ActiveRecordBase)
						{
							((ActiveRecordBase)cooldown).Delete();
						}
					}
					cds.Clear();
				}
				if (catCds != null)
				{
					foreach (var cooldown in catCds.Values)
					{
						if (cooldown is ActiveRecordBase)
						{
							((ActiveRecordBase)cooldown).Delete();
						}
					}
					catCds.Clear();
				}
			}));
		}

		/// <summary>
		/// Clears the cooldown for this spell and all spells in its category
		/// </summary>
		public override void ClearCooldown(Spell cooldownSpell)
		{
			var ownerChar = OwnerChar;
			if (ownerChar != null)
			{
				// send cooldown update to client
				SpellHandler.SendClearCoolDown(ownerChar, cooldownSpell.SpellId);
				if (cooldownSpell.Category != 0)
				{
					foreach (var spell in m_byId.Values)
					{
						if (spell.Category == cooldownSpell.Category)
						{
							SpellHandler.SendClearCoolDown(ownerChar, spell.SpellId);
						}
					}
				}
			}

			// remove and delete
			ISpellIdCooldown idCooldown;
			ISpellCategoryCooldown catCooldown;
			if (m_idCooldowns != null)
			{
				if (m_idCooldowns.TryGetValue(cooldownSpell.Id, out idCooldown))
				{
					m_idCooldowns.Remove(cooldownSpell.Id);
				}
			}
			else
			{
				idCooldown = null;
			}

			if (m_categoryCooldowns != null)
			{
				if (m_categoryCooldowns.TryGetValue(cooldownSpell.Category, out catCooldown))
				{
					m_categoryCooldowns.Remove(cooldownSpell.Id);
				}
			}
			else
			{
				catCooldown = null;
			}

			if (idCooldown is ActiveRecordBase || catCooldown is ActiveRecordBase)
			{
				RealmServer.Instance.AddMessage(new Message(() =>
				{
					if (idCooldown is ActiveRecordBase)
					{
						((ActiveRecordBase)idCooldown).Delete();
					}
					if (catCooldown is ActiveRecordBase)
					{
						((ActiveRecordBase)catCooldown).Delete();
					}
				}
				));
			}
		}

		private void FinalizeCooldowns(object sender)
		{
			lock (m_lock)
			{
				if (m_offlineCooldownTimer != null)
				{
					m_offlineCooldownTimer = null;
					FinalizeCooldowns(ref m_idCooldowns);
					FinalizeCooldowns(ref m_categoryCooldowns);
				}
			}
		}

		private void FinalizeCooldowns<T>(ref Dictionary<uint, T> cooldowns) where T : ICooldown
		{
			if (cooldowns == null)
				return;

			Dictionary<uint, T> newCooldowns = null;
			foreach (ICooldown cooldown in cooldowns.Values)
			{
				if (cooldown.Until < DateTime.Now + TimeSpan.FromMinutes(1))
				{
					// already expired or will expire very soon
					if (cooldown is ActiveRecordBase)
					{
						// delete
						((ActiveRecordBase)cooldown).Delete();
					}
				}
				else
				{
					if (newCooldowns == null)
					{
						newCooldowns = new Dictionary<uint, T>();
					}

					var cd = cooldown.AsConsistent();
					if (cd.CharId != m_ownerId)
					{
						cd.CharId = m_ownerId;
					}
					cd.SaveAndFlush();		// update or create
					newCooldowns.Add(cd.Identifier, (T)cd);
				}
			}
			cooldowns = newCooldowns;
		}
		#endregion
	}
}
