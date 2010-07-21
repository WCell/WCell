using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.Util;

namespace WCell.RealmServer.Entities
{
	/// All damage-related UnitFields are to be found in this file
	public partial class Unit
	{
		protected IWeapon m_mainWeapon, m_offhandWeapon, m_RangedWeapon;

		/// <summary>
		/// Applies modifications to your attacks
		/// </summary>
		public readonly List<IAttackEventHandler> AttackEventHandlers = new List<IAttackEventHandler>(1);

		/// <summary>
		/// The maximum distance in yards to a valid attackable target
		/// </summary>
		public float CombatReach
		{
			get { return GetFloat(UnitFields.COMBATREACH); }
			set { SetFloat(UnitFields.COMBATREACH, value); }
		}

		public virtual float MaxAttackRange
		{
			get { return CombatReach + m_mainWeapon.MaxRange; }
		}

		#region Weapon Info
		/// <summary>
		/// Whether this Unit currently has a ranged weapon equipped
		/// </summary>
		public bool UsesRangedWeapon
		{
			get
			{
				return m_RangedWeapon != null && m_RangedWeapon.IsRanged;
			}
		}

		/// <summary>
		/// Whether this Character is currently using DualWield (attacking with 2 melee weapons)
		/// </summary>
		public bool UsesDualWield
		{
			get
			{
				return SheathType != SheathType.Ranged && m_offhandWeapon != null;
			}
		}

		/// <summary>
		/// The Unit's current mainhand weapon
		/// Is set by the Unit's ItemInventory
		/// </summary>
		public IWeapon MainWeapon
		{
			get
			{
				return m_mainWeapon;
			}
			set
			{
				if (value == m_mainWeapon)
				{
					return;
				}

				if (value == null)
				{
					// always make sure that a weapon is equipped
					value = GenericWeapon.Fists;
				}

				if (m_mainWeapon is Item)
				{
					((Item) m_mainWeapon).OnUnEquip(InventorySlot.MainHand);
				}

				m_mainWeapon = value;

				this.UpdateMainDamage();
				this.UpdateMainAttackTime();

				if (value is Item)
				{
					((Item) value).OnEquip();
				}
			}
		}

		/// <summary>
		/// The Unit's current ranged weapon or other kind of bonus item
		/// Is set by the Unit's ItemInventory
		/// </summary>
		public IWeapon RangedWeapon
		{
			get
			{
				return m_RangedWeapon;
			}
			internal set
			{
				if (value == m_RangedWeapon)
				{
					return;
				}

				if (m_RangedWeapon is Item)
				{
					((Item)m_RangedWeapon).OnUnEquip(InventorySlot.ExtraWeapon);
				}
				m_RangedWeapon = value;

				this.UpdateRangedAttackTime();
				this.UpdateRangedDamage();
				if (value is Item)
				{
					((Item)value).OnEquip();
				}
			}
		}

		/// <summary>
		/// The Unit's current offhand Weapon
		/// Is set by the Unit's ItemInventory
		/// </summary>
		public IWeapon OffHandWeapon
		{
			get
			{
				return m_offhandWeapon;
			}
			internal set
			{
				if (value == m_RangedWeapon)
				{
					return;
				}

				if (m_offhandWeapon is Item)
				{
					((Item)m_offhandWeapon).OnUnEquip(InventorySlot.OffHand);
				}

				m_offhandWeapon = value;

				this.UpdateOffHandDamage();
				this.UpdateOffHandAttackTime();
				if (value is Item)
				{
					((Item)value).OnEquip();
				}
			}
		}

		public IWeapon GetWeapon(EquipmentSlot slot)
		{
			switch (slot)
			{
				case EquipmentSlot.MainHand:
					return m_mainWeapon;
				case EquipmentSlot.ExtraWeapon:
					return m_RangedWeapon;
				case EquipmentSlot.OffHand:
					return m_offhandWeapon;
			}
			return null;
		}

		public IWeapon GetWeapon(InventorySlotType slot)
		{
			switch (slot)
			{
				case InventorySlotType.WeaponMainHand:
					return m_mainWeapon;
				case InventorySlotType.WeaponRanged:
					return m_RangedWeapon;
				case InventorySlotType.WeaponOffHand:
					return m_offhandWeapon;
			}
			return null;
		}

		public void SetWeapon(InventorySlotType slot, IWeapon weapon)
		{
			switch (slot)
			{
				case InventorySlotType.WeaponMainHand:
					MainWeapon = weapon;
					break;
				case InventorySlotType.WeaponRanged:
					RangedWeapon = weapon;
					break;
				case InventorySlotType.WeaponOffHand:
					OffHandWeapon = weapon;
					break;
			}
		}

		/// <summary>
		/// Whether this Unit is allowed to melee at all
		/// </summary>
		public bool CanMelee
		{
			get;
			set;
		}
		#endregion

		#region Disarm
		private InventorySlotTypeMask m_DisarmMask;

		public bool MayCarry(InventorySlotTypeMask itemMask)
		{
			return (itemMask & DisarmMask) == 0;
		}

		/// <summary>
		/// The mask of slots of currently disarmed items.
		/// </summary>
		public InventorySlotTypeMask DisarmMask
		{
			get { return m_DisarmMask; }
		}

		/// <summary>
		/// Disarms the weapon of the given type (WeaponMainHand, WeaponRanged or WeaponOffHand)
		/// </summary>
		public void SetDisarmed(InventorySlotType type)
		{
			var m = type.ToMask();
			if (m_DisarmMask.HasAnyFlag(m))
			{
				return;
			}

			m_DisarmMask |= m;

			SetWeapon(type, null);
		}

		/// <summary>
		/// Rearms the weapon of the given type (WeaponMainHand, WeaponRanged or WeaponOffHand)
		/// </summary>
		public void UnsetDisarmed(InventorySlotType type)
		{
			var m = type.ToMask();
			if (!m_DisarmMask.HasAnyFlag(m))
			{
				return;
			}

			m_DisarmMask &= ~m;

			SetWeapon(type, GetOrInvalidateItem(type));
		}

		/// <summary>
		/// Finds the item for the given slot. 
		/// Unequips it and returns null, if it may not currently be used.
		/// </summary>
		protected virtual IWeapon GetOrInvalidateItem(InventorySlotType type)
		{
			return null;
		}
		#endregion

		#region Attack Times
		/// <summary>
		/// Time in millis between 2 Main-hand strikes
		/// </summary>
		public int MainHandAttackTime
		{
			get { return GetInt32(UnitFields.BASEATTACKTIME); }
			set { SetInt32(UnitFields.BASEATTACKTIME, value); }
		}

		/// <summary>
		/// Time in millis between 2 Off-hand strikes
		/// </summary>
		public int OffHandAttackTime
		{
			get { return GetInt32(UnitFields.BASEATTACKTIME + 1); }
			set { SetInt32(UnitFields.BASEATTACKTIME + 1, value); }
		}

		/// <summary>
		/// Time in millis between 2 ranged strikes
		/// </summary>
		public int RangedAttackTime
		{
			get { return GetInt32(UnitFields.RANGEDATTACKTIME); }
			set { SetInt32(UnitFields.RANGEDATTACKTIME, value); }
		}

		#endregion

		#region Damage Values
		public float MinDamage
		{
			get { return GetFloat(UnitFields.MINDAMAGE); }
			internal set { SetFloat(UnitFields.MINDAMAGE, value); }
		}

		public float MaxDamage
		{
			get { return GetFloat(UnitFields.MAXDAMAGE); }
			internal set { SetFloat(UnitFields.MAXDAMAGE, value); }
		}

		public float MinOffHandDamage
		{
			get { return GetFloat(UnitFields.MINOFFHANDDAMAGE); }
			internal set { SetFloat(UnitFields.MINOFFHANDDAMAGE, value); }
		}

		public float MaxOffHandDamage
		{
			get { return GetFloat(UnitFields.MAXOFFHANDDAMAGE); }
			internal set { SetFloat(UnitFields.MAXOFFHANDDAMAGE, value); }
		}

		public float MinRangedDamage
		{
			get { return GetFloat(UnitFields.MINRANGEDDAMAGE); }
			internal set { SetFloat(UnitFields.MINRANGEDDAMAGE, value); }
		}

		public float MaxRangedDamage
		{
			get { return GetFloat(UnitFields.MAXRANGEDDAMAGE); }
			internal set { SetFloat(UnitFields.MAXRANGEDDAMAGE, value); }
		}

		#endregion

		#region Melee Attack Power
		public virtual int MeleeAttackPower
		{
			get { return GetInt32(UnitFields.ATTACK_POWER); }
			internal set
			{
				SetInt32(UnitFields.ATTACK_POWER, value);
			}
		}

		public int MeleeAttackPowerModsPos
		{
			get { return GetUInt16Low(UnitFields.ATTACK_POWER_MODS); }
			set
			{
				SetUInt16Low(UnitFields.ATTACK_POWER_MODS, (ushort)value);
				this.UpdateMeleeAttackPower();
			}
		}

		public int MeleeAttackPowerModsNeg
		{
			get { return GetUInt16Low(UnitFields.ATTACK_POWER_MODS); }
			set
			{
				SetUInt16High(UnitFields.ATTACK_POWER_MODS, (ushort)value);
				this.UpdateMeleeAttackPower();
			}
		}

		public float MeleeAttackPowerMultiplier
		{
			get { return GetFloat(UnitFields.ATTACK_POWER_MULTIPLIER); }
			set
			{
				SetFloat(UnitFields.ATTACK_POWER_MULTIPLIER, value);
				this.UpdateMeleeAttackPower();
			}
		}

		public int TotalMeleeAP
		{
			get
			{
				var value = MeleeAttackPower;
				value += MeleeAttackPowerModsPos;
				value -= MeleeAttackPowerModsNeg;

				value = ((1 + MeleeAttackPowerMultiplier) * value).RoundInt();
				return value;
			}
		}
		#endregion

		#region Ranged Attack Power

		public int RangedAttackPower
		{
			get { return GetInt32(UnitFields.RANGED_ATTACK_POWER); }
			internal set
			{
				SetInt32(UnitFields.RANGED_ATTACK_POWER, value);
			}
		}

		public int RangedAttackPowerModsPos
		{
			get { return GetInt16Low(UnitFields.RANGED_ATTACK_POWER_MODS); }
			set
			{
				SetInt16Low(UnitFields.RANGED_ATTACK_POWER_MODS, (short)value);
				this.UpdateRangedAttackPower();
			}
		}

		public int RangedAttackPowerModsNeg
		{
			get { return GetInt16High(UnitFields.RANGED_ATTACK_POWER_MODS); }
			set
			{
				SetInt16High(UnitFields.RANGED_ATTACK_POWER_MODS, (short)value);
				this.UpdateRangedAttackPower();
			}
		}

		public float RangedAttackPowerMultiplier
		{
			get { return GetFloat(UnitFields.RANGED_ATTACK_POWER_MULTIPLIER); }
			set
			{
				SetFloat(UnitFields.RANGED_ATTACK_POWER_MULTIPLIER, value);
				this.UpdateRangedAttackPower();
			}
		}

		public int TotalRangedAP
		{
			get
			{
				var value = RangedAttackPower;
				value += RangedAttackPowerModsPos;
				value -= RangedAttackPowerModsNeg;

				value = ((1 + RangedAttackPowerMultiplier) * value).RoundInt();
				return value;
			}
		}
		#endregion



		/// <summary>
		/// Deals environmental damage to this Unit (cannot be resisted)
		/// </summary>
		public void DoEnvironmentalDamage(EnviromentalDamageType dmgType, int amount)
		{
			//if (dmgType == EnviromentalDamageType.Fall)
			//{
			//    amount -= (int)(SafeFall * (amount / 100f));
			//}

			DoRawDamage(new SimpleDamageAction { Damage = amount, Victim = this });
			CombatLogHandler.SendEnvironmentalDamage(this, dmgType, (uint)amount);
		}

		public void CalcFallingDamage(int speed)
		{
		}

		/// <summary>
		/// Deals damage, cancels damage-sensitive Auras, checks for spell interruption etc
		/// </summary>
		public void DoRawDamage(IDamageAction action)
		{
			// Default on damage stuff
			action.Victim.OnDamageAction(action);

			// events
			if (action.Attacker is Character)
			{
				Character.NotifyHitDeliver(action);
			}
			else if (action.Attacker is NPC)
			{
				((NPC)action.Attacker).Entry.NotifyHitDeliver(action);
			}

			if (action.Victim is Character)
			{
				Character.NotifyHitReceive(action);
			}
			else if (action.Victim is NPC)
			{
				((NPC)action.Victim).Entry.NotifyHitReceive(action);
			}

			if (action.Attacker != null && action.Attacker.Brain != null)
			{
				action.Attacker.Brain.OnDamageDealt(action);
			}

			if (m_brain != null)
			{
				m_brain.OnDamageReceived(action);
			}

			// deal damage
			var dmg = action.ActualDamage;
			if (dmg > 0)
			{
				if (action.Attacker != null && action.Victim.ManaShieldAmount > 0)
				{
					action.Victim.DrainManaShield(ref dmg);
				}

				var health = action.Victim.Health;

				if (dmg >= health)
				{
					LastKiller = action.Attacker;
				}

				action.Victim.Health = health - dmg;
			}
		}
	}
}