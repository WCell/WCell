using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras
{
	public class PlayerAuraCollection : AuraCollection
	{
		/// <summary>
		/// Set of Auras that are only applied when certain items are equipped
		/// </summary>
		List<Aura> itemRestrictedAuras;

		/// <summary>
		/// Set of Auras that are only applied in certain shapeshift forms
		/// </summary>
		List<Aura> shapeshiftRestrictedAuras;

		/// <summary>
		/// Set of handlers to be updated about equipping/unequipping of Items
		/// </summary>
		private List<ItemEquipmentEventAuraHandler> itemEquipmentHandlers;

		public PlayerAuraCollection(Character owner)
			: base(owner)
		{
		}

		#region Overrides
		public override void AddAura(Aura aura, bool update)
		{
			base.AddAura(aura, update);
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
				if (aura.Spell.AllowedShapeshiftMask != 0)
				{
					ShapeshiftRestrictedAuras.Add(aura);
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

		internal void OnEquip(Item item)
		{
			if (itemRestrictedAuras != null)
			{
				var plr = (Character)m_owner;				// PlayerAuraCollection always has Character owner
				foreach (var aura in itemRestrictedAuras)
				{
					if (!aura.IsActive)
					{
						aura.IsActive = 
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
					if (aura.IsActive)
					{
						aura.IsActive =
							CheckRestrictions(aura, false) &&
							aura.Spell.CheckItemRestrictionsWithout(plr.Inventory, item) == SpellFailedReason.Ok;
					}
				}
			}
		}
		#endregion

		#region Shapeshift Restrictions
		internal void OnShapeshiftFormChanged()
		{
			if (shapeshiftRestrictedAuras != null)
			{
				foreach (var aura in shapeshiftRestrictedAuras)
				{
					if (aura.Spell.AllowedShapeshiftMask != 0)
					{
						// the entire Aura is toggled
						if (CheckRestrictions(aura))
						{
							aura.IsActive = true; // Aura is running
						}
						else
						{
							aura.IsActive = false;	// Aura is off
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

		/// <summary>
		/// Check all restrictions on that aura (optionally, exclude item check)
		/// </summary>
		private bool CheckRestrictions(Aura aura, bool inclItemCheck = true)
		{
			if (!aura.Spell.AllowedShapeshiftMask.HasAnyFlag(m_owner.ShapeshiftMask))
			{
				return false;
			}
			if (inclItemCheck && aura.Spell.CheckItemRestrictions(((Character)m_owner).Inventory) != SpellFailedReason.Ok)
			{
				return false;
			}
			return true;
		}
	}
}