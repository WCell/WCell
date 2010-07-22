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
			if (Attributes.HasAnyFlag(SpellAttributes.RequiresStealth) && caster.Stealthed < 1)
			{
				return SpellFailedReason.OnlyStealthed;
			}

			if (!caster.CanDoHarm && HasHarmfulEffects)
			{
				return SpellFailedReason.Pacified;
			}

			// Not while silenced			
			if (InterruptFlags.HasFlag(InterruptFlags.OnSilence) && caster.IsUnderInfluenceOf(SpellMechanic.Silenced))
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
					(InterruptFlags.HasFlag(InterruptFlags.OnSilence) &&
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

			// spell focus			
			if (!CheckSpellFocus(caster))
			{
				return SpellFailedReason.RequiresSpellFocus;
			}

			// AuraStates
			if (RequiredCasterAuraState != 0 || ExcludeCasterAuraState != 0)
			{
				// check AuraStates
				var state = caster.AuraState;
				if ((RequiredCasterAuraState != 0 && !state.HasAnyFlag(RequiredCasterAuraState)) ||
					(ExcludeCasterAuraState != 0 && state.HasAnyFlag(ExcludeCasterAuraState)))
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

			// Shapeshift
			var shapeshiftMask = caster.ShapeshiftMask;
			bool ignoreShapeshiftRequirement = false;	// use this to allow for lazy requirement lookup
			if (ExcludeShapeshiftMask.HasAnyFlag(shapeshiftMask))
			{
				if (!(ignoreShapeshiftRequirement = caster.Auras.IsShapeshiftRequirementIgnored(this)))
				{
					return SpellFailedReason.NotShapeshift;
				}
			}
			else if (!AllowedShapeshiftMask.HasAnyFlag(shapeshiftMask))
			{
				// our mask did not pass -> do the default checks
				var shapeshiftEntry = caster.ShapeshiftEntry;
				var shapeshifted = shapeshiftEntry != null && (shapeshiftEntry.Flags & ShapeshiftInfoFlags.NotActualShapeshift) == 0;

				if (shapeshifted)
				{
					if (AllowedShapeshiftMask != 0)
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
					if (ignoreShapeshiftRequirement || caster.Auras.IsShapeshiftRequirementIgnored(this))
					{
						// Stealth Required, but not stealthed
						return SpellFailedReason.OnlyStealthed;
					}
				}
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
			var range = caster.GetSpellMaxRange(this);
			return RequiredSpellFocus == SpellFocus.None ||
				   caster.Region.GetGOWithSpellFocus(caster.Position, RequiredSpellFocus, range > 0 ? (range) : 5f, caster.Phase) != null;
		}

		/// <summary>
		/// Whether this spell has certain requirements on items
		/// </summary>
		public bool HasItemRequirements
		{
			get
			{
				return (RequiredItemClass != 0 && RequiredItemClass != ItemClass.None) ||
				  RequiredItemInventorySlotMask != InventorySlotTypeMask.None ||
				  RequiredTools != null ||
				  RequiredTotemCategories.Length > 0 ||
				  EquipmentSlot != EquipmentSlot.End;
			}
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
						!usedItem.Template.SubClassMask.HasAnyFlag(RequiredItemSubClassMask))
					{
						return SpellFailedReason.EquippedItemClass;
					}
				}
			}
			if (RequiredItemInventorySlotMask != InventorySlotTypeMask.None)
			{
				if (usedItem != null && (usedItem.Template.InventorySlotMask & RequiredItemInventorySlotMask) == 0)
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
		public SpellFailedReason CheckItemRestrictionsWithout(Item exclude, PlayerInventory inv)
		{
			if (RequiredItemClass == ItemClass.Armor || RequiredItemClass == ItemClass.Weapon)
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
					if (inv.Iterate(ItemMgr.EquippableInvSlotsByClass[(int)RequiredItemClass], i => i == exclude || !CheckItemRestriction(i)))
					{
						return SpellFailedReason.EquippedItemClass;
					}
				}
			}

			if (RequiredItemInventorySlotMask != InventorySlotTypeMask.None)
			{
				if (inv.Iterate(RequiredItemInventorySlotMask, item => item == exclude || (item.Template.InventorySlotMask & RequiredItemInventorySlotMask) == 0
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
			if (item.Template.Class != RequiredItemClass)
			{
				return false;
			}

			if (RequiredItemSubClassMask > 0 && !item.Template.SubClassMask.HasAnyFlag(RequiredItemSubClassMask))
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

			if (RequiredTotemCategories.Length > 0)
			{
				// Required totem category refers to tools that are required during the spell
				if (!inv.CheckTotemCategories(RequiredTotemCategories))
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
			if (target is Unit)
			{
				// AuraState
				if (RequiredTargetAuraState != 0 || ExcludeTargetAuraState != 0)
				{
					var state = ((Unit)target).AuraState;
					if ((RequiredTargetAuraState != 0 && !state.HasAnyFlag(RequiredTargetAuraState)) ||
						(ExcludeTargetAuraState != 0 && state.HasAnyFlag(ExcludeTargetAuraState)))
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
			if (TargetFlags.HasAnyFlag(SpellTargetFlags.UnkUnit_0x100) &&
				(!(target is GameObject) || !target.IsInWorld))
			{
				return SpellFailedReason.BadTargets;
			}

			// CreatureTypes
			if (CreatureMask != CreatureMask.None &&
				(!(target is NPC) || !((NPC)target).CheckCreatureType(CreatureMask)))
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
				if (TargetFlags.HasAnyFlag(SpellTargetFlags.PvPCorpse | SpellTargetFlags.Corpse))
				{
					if (!(target is Corpse) ||
						(TargetFlags.HasAnyFlag(SpellTargetFlags.PvPCorpse) && !caster.IsHostileWith(target)))
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
			if (AttributesExB.HasFlag(SpellAttributesExB.RequiresBehindTarget))
			{
				if (!caster.IsBehind(target))
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

			if (Range.MinDist > 0 &&
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
			if (CheckCasterConstraints(owner) != SpellFailedReason.Ok)
			{
				return false;
			}

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

			if (RequiredItemClass != ItemClass.None)
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

				return weapon.Class == RequiredItemClass &&
					   (RequiredItemSubClassMask == 0 || weapon.SubClassMask.HasAnyFlag(RequiredItemSubClassMask));
			}
			return true;
		}
		#endregion


		#region Cooldown
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
		#endregion
	}
}