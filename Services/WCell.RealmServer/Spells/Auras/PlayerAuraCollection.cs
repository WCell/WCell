using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras
{
	/// <summary>
	/// AuraCollection for Character objects. 
	/// Contains a lot of modifications and bookkeeping that is not required for NPCs.
	/// </summary>
	public class PlayerAuraCollection : AuraCollection
	{
		#region Fields
		/// <summary>
		/// Amount of currently added modifiers that require charges.
		/// If > 0, will iterate over modifiers and remove charges after SpellCasts.
		/// </summary>
		public int ModifierWithChargesCount
		{
			get;
			protected set;
		}

		/// <summary>
		/// Flat modifiers of spells
		/// </summary>
		internal readonly List<AddModifierEffectHandler> SpellModifiersFlat = new List<AddModifierEffectHandler>(5);

		/// <summary>
		/// Percent modifiers of spells
		/// </summary>
		internal readonly List<AddModifierEffectHandler> SpellModifiersPct = new List<AddModifierEffectHandler>(5);

		/// <summary>
		/// Mask of spells that are allowed to crit hit, although they are not allowed to, by default
		/// </summary>
		internal readonly uint[] CriticalStrikeEnabledMask = new uint[SpellConstants.SpellClassMaskSize];

		/// <summary>
		/// Set of Auras that are only applied when certain items are equipped
		/// </summary>
		List<Aura> itemRestrictedAuras;

		/// <summary>
		/// Set of Auras that are only applied in certain shapeshift forms
		/// </summary>
		List<Aura> shapeshiftRestrictedAuras;

		/// <summary>
		/// Set of Auras that are only applied in certain AuraStates
		/// </summary>
		List<Aura> auraStateRestrictedAuras;
		#endregion

		public PlayerAuraCollection(Character owner)
			: base(owner)
		{
		}

		#region Overrides
		public override void AddAura(Aura aura, bool start)
		{
			base.AddAura(aura, start);
			if (aura.Spell.IsPassive)
			{
				if (aura.Spell.HasItemRequirements)
				{
					ItemRestrictedAuras.Add(aura);
				}
				if (aura.Spell.IsModalShapeshiftDependentAura)
				{
					ShapeshiftRestrictedAuras.Add(aura);
				}
				if (aura.Spell.RequiredCasterAuraState != 0)
				{
					AuraStateRestrictedAuras.Add(aura);
				}
			}
		}

		protected internal override void Cancel(Aura aura)
		{
			base.Cancel(aura);
			if (aura.Spell.IsPassive)
			{
				if (aura.Spell.HasItemRequirements)
				{
					ItemRestrictedAuras.Remove(aura);
				}
				if (aura.Spell.IsModalShapeshiftDependentAura)
				{
					ShapeshiftRestrictedAuras.Remove(aura);
				}
				if (aura.Spell.RequiredCasterAuraState != 0)
				{
					AuraStateRestrictedAuras.Remove(aura);
				}
			}
		}
		#endregion

		#region Item Restrictions
		List<Aura> ItemRestrictedAuras
		{
			get
			{
				if (itemRestrictedAuras == null)
				{
					itemRestrictedAuras = new List<Aura>(3);
				}
				return itemRestrictedAuras;
			}
		}

		internal void OnEquip(Item item)
		{
			if (itemRestrictedAuras != null)
			{
				var plr = (Character)m_owner;				// PlayerAuraCollection always has Character owner
				foreach (var aura in itemRestrictedAuras)
				{
					if (!aura.IsActivated)
					{
						aura.IsActivated = 
							CheckRestrictions(aura, false) &&
							aura.Spell.CheckItemRestrictions(item, plr.Inventory) == SpellFailedReason.Ok;
					}
				}
			}
		}

		internal void OnBeforeUnEquip(Item item)
		{
			if (itemRestrictedAuras != null)
			{
				var plr = (Character)m_owner;				// PlayerAuraCollection always has Character owner
				foreach (var aura in itemRestrictedAuras)
				{
					if (aura.IsActivated)
					{
						aura.IsActivated =
							CheckRestrictions(aura, false) &&
							aura.Spell.CheckItemRestrictionsWithout(plr.Inventory, item) == SpellFailedReason.Ok;
					}
				}
			}
		}
		#endregion

		#region Shapeshift Restrictions
		List<Aura> ShapeshiftRestrictedAuras
		{
			get
			{
				if (shapeshiftRestrictedAuras == null)
				{
					shapeshiftRestrictedAuras = new List<Aura>(3);
				}
				return shapeshiftRestrictedAuras;
			}
		}

		internal void OnShapeshiftFormChanged()
		{
			if (shapeshiftRestrictedAuras != null)
			{
				foreach (var aura in shapeshiftRestrictedAuras)
				{
					if (aura.Spell.RequiredShapeshiftMask != 0)
					{
						// the entire Aura is toggled
						if (CheckRestrictions(aura))
						{
							aura.IsActivated = true; // Aura is running
						}
						else
						{
							aura.IsActivated = false;	// Aura is off
							continue;
						}
					}
					else if (aura.Spell.HasShapeshiftDependentEffects)
					{
						// the Aura state itself did not change
						aura.ReEvaluateNonPeriodicEffects();
					}
				}
			}
		}
		#endregion

		#region AuraState Restrictions
		List<Aura> AuraStateRestrictedAuras
		{
			get
			{
				if (auraStateRestrictedAuras == null)
				{
					auraStateRestrictedAuras = new List<Aura>(2);
				}
				return auraStateRestrictedAuras;
			}
		}

		internal void OnAuraStateChanged()
		{
			if (auraStateRestrictedAuras != null)
			{
				var state = Owner.AuraState;
				foreach (var aura in auraStateRestrictedAuras)
				{
					// Is Aura not in the right state?
					if (state.HasAnyFlag(aura.Spell.RequiredCasterAuraState) != aura.IsActivated)
					{
						// Toggle activation
						aura.IsActivated = !aura.IsActivated;
					}
				}
			}
		}
		#endregion

		/// <summary>
		/// Check all restrictions on that aura (optionally, exclude item check)
		/// </summary>
		private bool CheckRestrictions(Aura aura, bool inclItemCheck = true)
		{
			if (!aura.Spell.RequiredShapeshiftMask.HasAnyFlag(m_owner.ShapeshiftMask))
			{
				return false;
			}
			if (inclItemCheck && aura.Spell.CheckItemRestrictions(((Character)m_owner).Inventory) != SpellFailedReason.Ok)
			{
				return false;
			}
			return true;
		}

		#region Spell Modifiers
		public void AddSpellModifierPercent(AddModifierEffectHandler modifier)
		{
			if (modifier.Charges > 0)
			{
				ModifierWithChargesCount++;
			}
			SpellModifiersFlat.Add(modifier);
			OnModifierChange(modifier);
			AuraHandler.SendModifierUpdate((Character)m_owner, modifier.SpellEffect, true);
		}

		public void AddSpellModifierFlat(AddModifierEffectHandler modifier)
		{
			if (modifier.Charges > 0)
			{
				ModifierWithChargesCount++;
			}
			SpellModifiersFlat.Add(modifier);
			OnModifierChange(modifier);
			AuraHandler.SendModifierUpdate((Character)m_owner, modifier.SpellEffect, true);
		}

		public void RemoveSpellModifierPercent(AddModifierEffectHandler modifier)
		{
			if (modifier.Charges > 0)
			{
				ModifierWithChargesCount--;
			}
			SpellModifiersFlat.Add(modifier);
			OnModifierChange(modifier);
			AuraHandler.SendModifierUpdate((Character)m_owner, modifier.SpellEffect, false);
		}

		public void RemoveSpellModifierFlat(AddModifierEffectHandler modifier)
		{
			if (modifier.Charges > 0)
			{
				ModifierWithChargesCount--;
			}
			SpellModifiersFlat.Add(modifier);
			OnModifierChange(modifier);
			AuraHandler.SendModifierUpdate((Character)m_owner, modifier.SpellEffect, false);
		}

		private void OnModifierChange(AddModifierEffectHandler modifier)
		{
			foreach (var aura in Owner.Auras)
			{
				if (aura.IsActivated && !aura.Spell.IsEnhancer && modifier.SpellEffect.MatchesSpell(aura.Spell))
				{
					// activated, passive Aura, affected by this modifier -> Needs to re-apply
					aura.ReApplyNonPeriodicEffects();
				}
			}
		}

		/// <summary>
		/// Returns the modified value (modified by certain talent bonusses) of the given type for the given spell (as int)
		/// </summary>
		public override int GetModifiedInt(SpellModifierType type, Spell spell, int value)
		{
			var flatMod = GetModifierFlat(type, spell);
			var percentMod = GetModifierPercent(type, spell);
			return (((value + flatMod) * (100 + percentMod)) + 50) / 100;		// rounded
		}

		/// <summary>
		/// Returns the given value minus bonuses through certain talents, of the given type for the given spell (as int)
		/// </summary>
		public override int GetModifiedIntNegative(SpellModifierType type, Spell spell, int value)
		{
			var flatMod = GetModifierFlat(type, spell);
			var percentMod = GetModifierPercent(type, spell);
			return (((value - flatMod) * (100 - percentMod)) + 50) / 100;		// rounded
		}

		/// <summary>
		/// Returns the modified value (modified by certain talents) of the given type for the given spell (as float)
		/// </summary>
		public override float GetModifiedFloat(SpellModifierType type, Spell spell, float value)
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
					modifier.SpellEffect.MatchesSpell(spell))
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
					modifier.SpellEffect.MatchesSpell(spell))
				{
					amount += modifier.SpellEffect.ValueMin;
				}
			}
			return amount;
		}
		#endregion

		#region OnCasted
		public override void OnCasted(SpellCast cast)
		{
			var spell = cast.Spell;
			if (ModifierWithChargesCount > 0)
			{
				var toRemove = new List<Aura>(3);
				for (var i = 0; i < SpellModifiersFlat.Count; i++)
				{
					var modifier = SpellModifiersFlat[i];
					if (modifier.SpellEffect.MatchesSpell(spell))
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
					if (modifier.SpellEffect.MatchesSpell(spell))
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

		/// <summary>
		/// Returns wehther the given spell is allowed to crit, if it was not
		/// allowed to crit by default. (Due to Talents that override Spell behavior)
		/// </summary>
		public bool CanSpellCrit(Spell spell)
		{
			return spell.MatchesMask(CriticalStrikeEnabledMask);
		}
	}
}