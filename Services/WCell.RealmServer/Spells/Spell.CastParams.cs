using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.Constants.Items;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.Spells
{
	public partial class Spell
	{
		public bool CanCast(NPC npc)
		{
			return CheckCasterConstraints(npc) == SpellFailedReason.Ok;
		}

		#region Check Caster
		/// <summary>
		/// Checks whether the given spell can be casted by the casting Unit.
		/// Does not do range checks.
		/// </summary>
		public SpellFailedReason CheckCasterConstraints(Unit caster)
		{
			// check for combat
			if (caster.IsInCombat && RequiresCasterOutOfCombat)
			{
				return SpellFailedReason.AffectingCombat;
			}

			// Power Type			
			if (CostsPower &&
				PowerType != caster.PowerType &&
				PowerType != PowerType.Health &&
				!AttributesExB.HasFlag(SpellAttributesExB.DoesNotNeedShapeshift))
			{
				return SpellFailedReason.OnlyShapeshift;
			}

			if (!caster.CanDoHarm && HasHarmfulEffects)
			{
				return SpellFailedReason.Pacified;
			}

			// Not while silenced			
            if (SpellInterrupts != null && SpellInterrupts.InterruptFlags.HasFlag(InterruptFlags.OnSilence) && caster.IsUnderInfluenceOf(SpellMechanic.Silenced))
			{
				return SpellFailedReason.Silenced;
			}

			// Check if castable while stunned
			if (!AttributesExD.HasFlag(SpellAttributesExD.UsableWhileStunned) && !caster.CanInteract)
			{
				return SpellFailedReason.CantDoThatRightNow;
			}
			// Combo points			
			if (IsFinishingMove && caster.ComboPoints == 0)
			{
				return SpellFailedReason.NoComboPoints;
			}

			// spell focus
			if (!CheckSpellFocus(caster))
			{
				return SpellFailedReason.RequiresSpellFocus;
			}

			// Not while silenced		
			else if (!caster.CanCastSpells &&
					(!IsPhysicalAbility ||
                    (SpellInterrupts != null && SpellInterrupts.InterruptFlags.HasFlag(InterruptFlags.OnSilence) &&
					 caster.IsUnderInfluenceOf(SpellMechanic.Silenced))))
			{
				return SpellFailedReason.Silenced;
			}
			// cannot use physical ability or not do harm at all
			else if ((!caster.CanDoPhysicalActivity && IsPhysicalAbility) ||
					(!caster.CanDoHarm && HasHarmfulEffects))
			{
				return SpellFailedReason.Pacified;
			}
			else if (!AttributesExD.HasFlag(SpellAttributesExD.UsableWhileStunned) && !caster.CanInteract)
			{
				return SpellFailedReason.Stunned;
			}
			// Combo points			
			else if (IsFinishingMove && caster.ComboPoints == 0)
			{
				return SpellFailedReason.NoComboPoints;
			}

			// AuraStates
            if (SpellAuraRestrictions != null)
            {
                if (SpellAuraRestrictions.RequiredCasterAuraState != 0 || SpellAuraRestrictions.ExcludeCasterAuraState != 0)
                {
                    // check AuraStates
                    var state = caster.AuraState;
                    if ((SpellAuraRestrictions.RequiredCasterAuraState != 0 && !state.HasAnyFlag(SpellAuraRestrictions.RequiredCasterAuraState)) ||
                        (SpellAuraRestrictions.ExcludeCasterAuraState != 0 && state.HasAnyFlag(SpellAuraRestrictions.ExcludeCasterAuraState)))
                    {
                        return SpellFailedReason.CasterAurastate;
                    }
                }

                // Required Auras
                if ((SpellAuraRestrictions.ExcludeCasterAuraId != 0 && caster.Auras.Contains(SpellAuraRestrictions.ExcludeCasterAuraId)) ||
                    (SpellAuraRestrictions.RequiredCasterAuraId != 0 && !caster.Auras.Contains(SpellAuraRestrictions.RequiredCasterAuraId)))
                {
                    return SpellFailedReason.CasterAurastate;
                }
            }

			// Shapeshift
            if (SpellShapeshift != null)
            {
                var shapeshiftMask = caster.ShapeshiftMask;
                bool ignoreShapeshiftRequirement = false;	// use this to allow for lazy requirement lookup
                if (SpellShapeshift.ExcludeShapeshiftMask.HasAnyFlag(shapeshiftMask))
                {
                    if (!(ignoreShapeshiftRequirement = caster.Auras.IsShapeshiftRequirementIgnored(this)))
                    {
                        return SpellFailedReason.NotShapeshift;
                    }
                }
                else if (!SpellShapeshift.RequiredShapeshiftMask.HasAnyFlag(shapeshiftMask))
                {
                    // our mask did not pass -> do the default checks
                    var shapeshiftEntry = caster.ShapeshiftEntry;
                    var shapeshifted = shapeshiftEntry != null && (shapeshiftEntry.Flags & ShapeshiftInfoFlags.NotActualShapeshift) == 0;

                    if (shapeshifted)
                    {
                        if (SpellShapeshift.RequiredShapeshiftMask != 0)
                        {
                            // When shapeshifted, can only use spells that allow this form
                            if (!(ignoreShapeshiftRequirement = caster.Auras.IsShapeshiftRequirementIgnored(this)))
                            {
                                return SpellFailedReason.OnlyShapeshift;
                            }
                        }
                        else if (Attributes.HasAnyFlag(SpellAttributes.NotWhileShapeshifted))
                        {
                            if (!(ignoreShapeshiftRequirement = caster.Auras.IsShapeshiftRequirementIgnored(this)))
                            {
                                // cannot cast this spell when shapeshifted
                                return SpellFailedReason.NotShapeshift;
                            }
                        }
                    }

                    if (Attributes.HasFlag(SpellAttributes.RequiresStealth) && caster.Stealthed < 1)
                    {
                        if (!caster.Auras.IsShapeshiftRequirementIgnored(this))
                        {
                            // Stealth Required, but not stealthed and not ignored by a SPELL_AURA_MOD_IGNORE_SHAPESHIFT aura
                            return SpellFailedReason.OnlyStealthed;
                        }
                    }
                }
            }

			var spells = caster.Spells as PlayerSpellCollection;

			// check cooldown and power cost			
			if (spells != null && !spells.IsReady(this))
			{
				return SpellFailedReason.NotReady;
			}

			if (!IsPassive)
			{
				if (!caster.HasEnoughPowerToCast(this, null))
				{
					return SpellFailedReason.NoPower;
				}
			}
			return SpellFailedReason.Ok;
		}

		private bool CheckSpellFocus(Unit caster)
		{
            if (SpellCastingRequirements == null)
                return true;
			var range = caster.GetSpellMaxRange(this);
            return SpellCastingRequirements.RequiredSpellFocus == SpellFocus.None ||
                   caster.Map.GetGOWithSpellFocus(caster.Position, SpellCastingRequirements.RequiredSpellFocus, range > 0 ? (range) : 5f, caster.Phase) != null;
		}

		/// <summary>
		/// Whether this spell has certain requirements on items
		/// </summary>
		public bool HasItemRequirements
		{
			get
			{
                if (SpellEquippedItems == null)
                    return false;
                return (SpellEquippedItems.RequiredItemClass != 0 && SpellEquippedItems.RequiredItemClass != ItemClass.None) ||
                  SpellEquippedItems.RequiredItemInventorySlotMask != InventorySlotTypeMask.None ||
				  RequiredTools != null ||
                  (SpellTotems != null && SpellTotems.RequiredToolCategories.Length > 0) ||
				  EquipmentSlot != EquipmentSlot.End;
			}
		}

		public SpellFailedReason CheckItemRestrictions(Item usedItem, PlayerInventory inv)
		{
            if (SpellEquippedItems != null && SpellEquippedItems.RequiredItemClass != ItemClass.None)
			{
				if (EquipmentSlot != EquipmentSlot.End)
				{
					usedItem = inv[EquipmentSlot];
				}

				if (usedItem == null)
				{
					return SpellFailedReason.EquippedItem;

				}
                if (SpellEquippedItems.RequiredItemClass > 0)
				{
                    if (usedItem.Template.Class != SpellEquippedItems.RequiredItemClass)
					{
						return SpellFailedReason.EquippedItemClass;
					}

                    if (SpellEquippedItems.RequiredItemSubClassMask > 0 &&
                        !usedItem.Template.SubClassMask.HasAnyFlag(SpellEquippedItems.RequiredItemSubClassMask))
					{
						return SpellFailedReason.EquippedItemClass;
					}
				}
			}
            if (SpellEquippedItems != null &&  SpellEquippedItems.RequiredItemInventorySlotMask != InventorySlotTypeMask.None)
			{
                if (usedItem != null && (usedItem.Template.InventorySlotMask & SpellEquippedItems.RequiredItemInventorySlotMask) == 0)
				// don't use Enum.HasFlag!
				{
					return SpellFailedReason.EquippedItemClass;
				}
			}

			return CheckGeneralItemRestrictions(inv);
		}

		/// <summary>
		/// Checks whether the given inventory satisfies this Spell's item restrictions
		/// </summary>
		public SpellFailedReason CheckItemRestrictions(PlayerInventory inv)
		{
			return CheckItemRestrictionsWithout(inv, null);
		}

		/// <summary>
		/// Checks whether the given inventory satisfies this Spell's item restrictions
		/// </summary>
		public SpellFailedReason CheckItemRestrictionsWithout(PlayerInventory inv, Item exclude)
		{
            if (SpellEquippedItems == null)
                return CheckGeneralItemRestrictions(inv);

            if (SpellEquippedItems.RequiredItemClass == ItemClass.Armor || SpellEquippedItems.RequiredItemClass == ItemClass.Weapon)
			{
				Item item;
				if (EquipmentSlot != EquipmentSlot.End)
				{
					item = inv[EquipmentSlot];

					if (item == null || item == exclude)
					{
						return SpellFailedReason.EquippedItem;
					}

					if (!CheckItemRestriction(item))
					{
						return SpellFailedReason.EquippedItemClass;
					}
				}
				else
				{
                    if (inv.Iterate(ItemMgr.EquippableInvSlotsByClass[(int)SpellEquippedItems.RequiredItemClass], i => i == exclude || !CheckItemRestriction(i)))
					{
						return SpellFailedReason.EquippedItemClass;
					}
				}
			}

            if (SpellEquippedItems.RequiredItemInventorySlotMask != InventorySlotTypeMask.None)
			{
                if (inv.Iterate(SpellEquippedItems.RequiredItemInventorySlotMask, item => item == exclude || (item.Template.InventorySlotMask & SpellEquippedItems.RequiredItemInventorySlotMask) == 0
				))
				{
					// iterated over all matching items and did not find the right one
					return SpellFailedReason.EquippedItemClass;
				}
			}

			return CheckGeneralItemRestrictions(inv);
		}

		bool CheckItemRestriction(Item item)
		{
            if (item.Template.Class != SpellEquippedItems.RequiredItemClass)
			{
				return false;
			}

            if (SpellEquippedItems.RequiredItemSubClassMask > 0 && !item.Template.SubClassMask.HasAnyFlag(SpellEquippedItems.RequiredItemSubClassMask))
			{
				return false;
			}
			return true;
		}

		public SpellFailedReason CheckGeneralItemRestrictions(PlayerInventory inv)
		{
			// check for special tools
			if (RequiredTools != null)
			{
				foreach (var tool in RequiredTools)
				{
					if (!inv.Contains(tool.Id, 1, false))
					{
						return SpellFailedReason.ItemNotFound;
					}
				}
			}

            if (SpellTotems != null && SpellTotems.RequiredToolCategories.Length > 0)
			{
				// Required totem category refers to tools that are required during the spell
				if (!inv.CheckTotemCategories(SpellTotems.RequiredToolCategories))
				{
					return SpellFailedReason.TotemCategory;
				}
			}

			// check for whether items must be equipped
			if (EquipmentSlot != EquipmentSlot.End)
			{
				var item = inv[EquipmentSlot];
				if (item == null)
				{
					return SpellFailedReason.EquippedItem;
				}
				if (AttributesExC.HasFlag(SpellAttributesExC.RequiresWand) &&
					item.Template.SubClass != ItemSubClass.WeaponWand)
				{
					return SpellFailedReason.EquippedItem;
				}
				if (AttributesExC.HasFlag(SpellAttributesExC.ShootRangedWeapon) &&
					!item.Template.IsRangedWeapon)
				{
					return SpellFailedReason.EquippedItem;
				}
			}
			return SpellFailedReason.Ok;
		}
		#endregion

		#region Check Target
		/// <summary>
		/// Checks whether the given target is valid for the given caster.
		/// Is called automatically when SpellCast selects Targets.
		/// Does not do maximum range check.
		/// </summary>
		public SpellFailedReason CheckValidTarget(WorldObject caster, WorldObject target)
		{
			if (AttributesEx.HasAnyFlag(SpellAttributesEx.CannotTargetSelf) && target == caster)
			{
				return SpellFailedReason.NoValidTargets;
			}
            if (target is Unit && SpellAuraRestrictions != null)
			{
				// AuraState
                if (SpellAuraRestrictions.RequiredTargetAuraState != 0 || SpellAuraRestrictions.ExcludeTargetAuraState != 0)
				{
					var state = ((Unit)target).AuraState;
                    if ((SpellAuraRestrictions.RequiredTargetAuraState != 0 && !state.HasAnyFlag(SpellAuraRestrictions.RequiredTargetAuraState)) ||
                        (SpellAuraRestrictions.ExcludeTargetAuraState != 0 && state.HasAnyFlag(SpellAuraRestrictions.ExcludeTargetAuraState)))
					{
						return SpellFailedReason.TargetAurastate;
					}
				}

				// Required Auras
                if ((SpellAuraRestrictions.ExcludeTargetAuraId != 0 && ((Unit)target).Auras.Contains(SpellAuraRestrictions.ExcludeTargetAuraId)) ||
                    (SpellAuraRestrictions.RequiredTargetAuraId != 0 && !((Unit)target).Auras.Contains(SpellAuraRestrictions.RequiredTargetAuraId)))
				{
					return SpellFailedReason.TargetAurastate;
				}
			}

			// Make sure that we have a GameObject if the Spell requires one
			if (SpellTargetRestrictions != null && SpellTargetRestrictions.TargetFlags.HasAnyFlag(SpellTargetFlags.UnkUnit_0x100) &&
				(!(target is GameObject) || !target.IsInWorld))
			{
				return SpellFailedReason.BadTargets;
			}

			// CreatureTypes
            if (SpellTargetRestrictions != null && SpellTargetRestrictions.CreatureMask != CreatureMask.None &&
                (!(target is NPC) || !((NPC)target).CheckCreatureType(SpellTargetRestrictions.CreatureMask)))
			{
				return SpellFailedReason.BadImplicitTargets;
			}
			if (!CanCastOnPlayer && target is Character)
			{
				return SpellFailedReason.BadImplicitTargets;
			}


			// Corpse target
			if (RequiresDeadTarget)
			{
                if (SpellTargetRestrictions != null && SpellTargetRestrictions.TargetFlags.HasAnyFlag(SpellTargetFlags.PvPCorpse | SpellTargetFlags.Corpse))
				{
					if (!(target is Corpse) ||
                        (SpellTargetRestrictions.TargetFlags.HasAnyFlag(SpellTargetFlags.PvPCorpse) && caster != null && !caster.IsHostileWith(target)))
					{
						return SpellFailedReason.BadImplicitTargets;
					}
				}
				else if ((target is NPC))
				{
					// need to be dead and looted empty
                    if (((NPC)target).IsAlive || target.Loot != null)
                    {
                        return SpellFailedReason.TargetNotDead;
                    }
				}
                else if((target is Character))
                {
                    if(((Character)target).IsAlive)
                    {
                        return SpellFailedReason.TargetNotDead;
                    }
                }
			}
			else
			{
				if (target is Unit && !((Unit)target).IsAlive)
				{
					return SpellFailedReason.TargetsDead;
				}
			}

			// Rogues do it from behind
			if (AttributesExB.HasFlag(SpellAttributesExB.RequiresBehindTarget))
			{
				if (caster != null && !caster.IsBehind(target))
				{
					return SpellFailedReason.NotBehind;
				}
			}

			//if (AttributesExC.HasFlag(SpellAttributesExC.NoInitialAggro))
			//{
			//    if (target is Unit && ((Unit)target).IsInCombat)
			//    {
			//        return SpellFailedReason.TargetAffectingCombat;
			//    }
			//}

			if (Range.MinDist > 0 && caster != null &&
				//caster.IsInRadius(target, caster.GetSpellMinRange(Range.MinDist, target)))
				caster.IsInRadius(target, Range.MinDist))
			{
				return SpellFailedReason.TooClose;
			}

			return SpellFailedReason.Ok;
		}
		#endregion

		#region Check Proc
		public bool CanProcBeTriggeredBy(Unit owner, IUnitAction action, bool active)
		{
			//if (CheckCasterConstraints(owner) != SpellFailedReason.Ok)
			//{
			//    return false;
			//}

			if (action.Spell != null)
			{
				if (active)
				{
					// owner == attacker
					if (CasterProcSpells != null)
					{
						return CasterProcSpells.Contains(action.Spell);
					}
				}
				else if (TargetProcSpells != null)
				{
					// owner == victim
					return TargetProcSpells.Contains(action.Spell);
				}
				if (action.Spell == this)
				{
					// Proc spell can't trigger itself
					return false;
				}
			}

            if (SpellEquippedItems != null && SpellEquippedItems.RequiredItemClass != ItemClass.None)
			{
				// check for weapon
				if (!(action is DamageAction))
				{
					return false;
				}

				var aAction = (DamageAction)action;
				if (aAction.Weapon == null || !(aAction.Weapon is Item))
				{
					return false;
				}

				var weapon = ((Item)aAction.Weapon).Template;

                return weapon.Class == SpellEquippedItems.RequiredItemClass &&
                       (SpellEquippedItems.RequiredItemSubClassMask == 0 || weapon.SubClassMask.HasAnyFlag(SpellEquippedItems.RequiredItemSubClassMask));
			}
			return true;
		}
		#endregion

		#region Cooldown
		public int GetCooldown(Unit unit)
		{
			int cd;
			if (unit is NPC && AISettings.Cooldown.MaxDelay > 0)
			{
				cd = AISettings.Cooldown.GetRandomCooldown();
			}
			else
			{
				cd = SpellCooldowns != null ? SpellCooldowns.CooldownTime : 0;
			}

			if (cd == 0)
			{
				if (HasIndividualCooldown)
				{
					// add weapon delay as Cooldown
					if (unit is Character)
					{
						var weapon = ((Character)unit).Inventory[EquipmentSlot];
						if (weapon != null)
						{
							cd = weapon.AttackTime;
						}
					}
					else if (unit is NPC)
					{
						cd = ((NPC)unit).Entry.AttackTime;
					}
				}
			}
			else
			{
				cd = GetModifiedCooldown(unit, cd);
			}
			//return Math.Max(cd - unit.Map.UpdateDelay, 0);
			return cd;
		}

		public int GetModifiedCooldown(Unit unit, int cd)
		{
			return unit.Auras.GetModifiedInt(SpellModifierType.CooldownTime, this, cd);
		}
		#endregion
	}
}