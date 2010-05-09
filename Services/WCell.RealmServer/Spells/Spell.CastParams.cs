using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.Constants.Items;

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
		/// Checks whether the given spell can be casted by the casting Unit
		/// </summary>
		public SpellFailedReason CheckCasterConstraints(Unit caster)
		{
			// check for combat
			if (caster.IsInCombat && RequiresCasterOutOfCombat)
			{
				return SpellFailedReason.AffectingCombat;
			}

			// Power Type			
			if (CostsMana && PowerType != caster.PowerType && PowerType != PowerType.Health)
			{
				return SpellFailedReason.NoPower;
			}

			// Stealth Required			
		    if (Attributes.Has(SpellAttributes.RequiresStealth) && caster.Stealthed < 1)
		    {
		        return SpellFailedReason.OnlyStealthed;
			}

			if (!caster.CanDoHarm && HasHarmfulEffects)
			{
				return SpellFailedReason.Pacified;
			}

		    // Not while silenced			
		    if (InterruptFlags.Has(InterruptFlags.OnSilence) && caster.IsUnderInfluenceOf(SpellMechanic.Silenced))
		    {
		        return SpellFailedReason.Silenced;
		    }

		    if (!AttributesExD.Has(SpellAttributesExD.UsableWhileStunned) && !caster.CanInteract)
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

			// shapeshift
			if (Attributes.Has(SpellAttributes.NotWhileShapeshifted) &&
				caster.ShapeShiftForm != ShapeShiftForm.Normal)
			{
				//return SpellFailedReason.NotShapeshift;
			}

			// Stealth Required			
			else if (Attributes.Has(SpellAttributes.RequiresStealth) && caster.Stealthed < 1)
			{
				return SpellFailedReason.OnlyStealthed;
			}

			// Not while silenced		
			else if (InterruptFlags.Has(InterruptFlags.OnSilence) &&
					 caster.IsUnderInfluenceOf(SpellMechanic.Silenced))
			{
				return SpellFailedReason.Silenced;
			}
			else if (!caster.CanDoHarm && HasHarmfulEffects)
			{
				return SpellFailedReason.Pacified;
			}
			else if (!AttributesExD.Has(SpellAttributesExD.UsableWhileStunned) && !caster.CanInteract)
			{
				return SpellFailedReason.Stunned;
			}
			else if (!caster.CanCastSpells)
			{
				return SpellFailedReason.Interrupted;
			}
			// Combo points			
			else if (IsFinishingMove && caster.ComboPoints == 0)
			{
				return SpellFailedReason.NoComboPoints;
			}

			// spell focus			
			if (!CheckSpellFocus(caster))
			{
				return SpellFailedReason.RequiresSpellFocus;
			}
			// shapeshift			
			//if (Attributes.Has(SpellAttributes.NotWhileShapeshifted) && caster.ShapeShiftForm != ShapeShiftForm.Normal)
			{
				//return SpellFailedReason.NotShapeshift;			
			}

			// AuraStates
			if (RequiredCasterAuraState != 0 || ExcludeCasterAuraState != 0)
			{
				// check AuraStates
				var state = caster.AuraState;
				if ((RequiredCasterAuraState != 0 && !state.Has(RequiredCasterAuraState)) ||
					(ExcludeCasterAuraState != 0 && state.Has(ExcludeCasterAuraState)))
				{
					return SpellFailedReason.CasterAurastate;
				}
			}

			// Required Auras
			if ((ExcludeCasterAuraId != 0 && caster.Auras.Contains(ExcludeCasterAuraId)) ||
				(RequiredCasterAuraId != 0 && !caster.Auras.Contains(RequiredCasterAuraId)))
			{
				return SpellFailedReason.CasterAurastate;
			}

			var spells = caster.Spells as PlayerSpellCollection;
			// check cooldown and power cost			
			if (spells != null && !spells.CheckCooldown(this))
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
			return RequiredSpellFocus == SpellFocus.None ||
			       caster.Region.GetGOWithSpellFocus(caster.Position, RequiredSpellFocus,
													 Range.MaxDist > 0 ? (Range.MaxDist + caster.CombatReach) : 5f, caster.Phase) != null;
		}

		public SpellFailedReason CheckItemRestrictions(Item usedItem, PlayerInventory inv)
		{
			if (RequiredItemClass != ItemClass.None)
			{
				if (EquipmentSlot != EquipmentSlot.End)
				{
					usedItem = inv[EquipmentSlot];
				}

				if (usedItem == null)
				{
					return SpellFailedReason.EquippedItem;

				}
				if (RequiredItemClass > 0)
				{
					if (usedItem.Template.Class != RequiredItemClass)
					{
						return SpellFailedReason.EquippedItemClass;
					}

					if (RequiredItemSubClassMask > 0 &&
						(usedItem.Template.SubClassMask & RequiredItemSubClassMask) == 0)
					{
						return SpellFailedReason.EquippedItemClass;
					}
				}
			}
			if (RequiredItemInventorySlotMask != InventorySlotTypeMask.None)
			{
				if (usedItem != null && (usedItem.Template.InventorySlotMask & RequiredItemInventorySlotMask) == 0)
				{
					return SpellFailedReason.EquippedItemClass;
				}
			}

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

			if (RequiredTotemCategories.Length > 0)
			{
				// Required totem category refers to tools that are required during the spell
				inv.CheckTotemCategories(RequiredTotemCategories);
			}

			// check for whether items must be equipped
			if (EquipmentSlot != EquipmentSlot.End)
			{
				var item = inv[EquipmentSlot];
				if (item == null)
				{
					return SpellFailedReason.EquippedItem;
				}
				if (AttributesExC.Has(SpellAttributesExC.RequiresWand) &&
					item.Template.SubClass != ItemSubClass.WeaponWand)
				{
					return SpellFailedReason.EquippedItem;
				}
				if (AttributesExC.Has(SpellAttributesExC.ShootRangedWeapon) &&
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
			if (target is Unit)
			{
				// AuraState
				if (RequiredTargetAuraState != 0 || ExcludeTargetAuraState != 0)
				{
					var state = ((Unit)target).AuraState;
					if ((RequiredTargetAuraState != 0 && !state.Has(RequiredTargetAuraState)) ||
						(ExcludeTargetAuraState != 0 && state.Has(ExcludeTargetAuraState)))
					{
						return SpellFailedReason.TargetAurastate;
					}
				}

				// Required Auras
				if ((ExcludeTargetAuraId != 0 && ((Unit)target).Auras.Contains(ExcludeTargetAuraId)) ||
					(RequiredTargetAuraId != 0 && !((Unit)target).Auras.Contains(RequiredTargetAuraId)))
				{
					return SpellFailedReason.TargetAurastate;
				}
			}

			// Make sure that we have a GameObject if the Spell requires one
			if (TargetFlags.Has(SpellTargetFlags.UnkUnit_0x100) &&
				(!(target is GameObject) || !target.IsInWorld))
			{
				return SpellFailedReason.BadTargets;
			}

			// CreatureTypes
			if (TargetCreatureTypes != TargetCreatureMask.None &&
				(!(target is NPC) || !((NPC)target).CheckCreatureType(TargetCreatureTypes)))
			{
				return SpellFailedReason.BadImplicitTargets;
			}
			if (!CanCastOnPlayer && target is Character)
			{
				return SpellFailedReason.BadImplicitTargets;
			}


			// Corpse target
			if (ReqDeadTarget)
			{
				if (TargetFlags.Has(SpellTargetFlags.PvPCorpse | SpellTargetFlags.Corpse))
				{
					if (!(target is Corpse) ||
						(TargetFlags.Has(SpellTargetFlags.PvPCorpse) && !caster.IsHostileWith(target)))
					{
						return SpellFailedReason.BadImplicitTargets;
					}
				}
				else if (!(target is NPC) || ((NPC)target).IsAlive || target.Loot != null)
				{
					// need to be dead and looted empty
					return SpellFailedReason.TargetNotDead;
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
			if (AttributesExB.Has(SpellAttributesExB.RequiresBehindTarget))
			{
				if (!caster.IsBehind(target))
				{
					return SpellFailedReason.NotBehind;
				}
			}

			if (AttributesExC.Has(SpellAttributesExC.NoInitialAggro))
			{
				if (target is Unit && ((Unit)target).IsInCombat)
				{
					return SpellFailedReason.TargetAffectingCombat;
				}
			}

			if (Range.MinDist > 0 &&
				//caster.IsInRadius(target, caster.GetSpellMinRange(Range.MinDist, target)))
				caster.IsInRadius(target, Range.MinDist))
			{
				return SpellFailedReason.TooClose;
			}

			return SpellFailedReason.Ok;
		}
		#endregion

		public int GetCooldown(Unit unit)
		{
            var cd = CooldownTime;
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
			else if (unit is Character)
			{
				cd = ((Character)unit).PlayerSpells.GetModifiedInt(SpellModifierType.CooldownTime, this, cd);
			}
            //return Math.Max(cd - unit.Region.UpdateDelay, 0);
			return cd;
		}
	}
}
