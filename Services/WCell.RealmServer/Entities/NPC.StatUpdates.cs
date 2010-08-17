using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Items;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.NPCs.Pets;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// TODO: Move everything NPC-related from UnitUpdates in here
	/// </summary>
	public partial class NPC
	{
		protected internal override void UpdateStrength()
		{
			base.UpdateStrength();

			// Pet stat: Damage
			if (MainWeapon is GenericWeapon)
			{
				UpdatePetDamage((GenericWeapon)MainWeapon);
				this.UpdateMainDamage();
			}
			if (OffHandWeapon is GenericWeapon)
			{
				UpdatePetDamage((GenericWeapon)OffHandWeapon);
				this.UpdateOffHandDamage();
			}
		}

		protected internal override void UpdateStamina()
		{
			var stam = StaminaBuffPositive - StaminaBuffNegative;
			if (IsHunterPet)
			{
				// Pet stat: Stamina
				// "1 stamina gives 0.45 stamina untalented"
				var hunter = m_master as Character;
				if (hunter != null)
				{
					// recalculate base stamina for Pet
					var levelStatInfo = Entry.GetPetLevelStatInfo(Level);
					var baseStam = (hunter.Stamina * PetMgr.PetStaminaOfOwnerPercent + 50) / 100;
					if (levelStatInfo != null)
					{
						baseStam += levelStatInfo.BaseStats[(int)StatType.Stamina];
					}
					m_baseStats[(int)StatType.Stamina] = baseStam;

					stam += baseStam;
				}
			}
			else
			{
				stam += GetBaseStatValue(StatType.Stamina);
			}

			SetInt32(UnitFields.STAT2, stam);

			UpdateMaxHealth();
		}

		void UpdatePetDamage(GenericWeapon weapon)
		{
			var avg = (Strength - 20) / 2;
			weapon.Damages[0].Minimum = avg - avg / 5;
			weapon.Damages[0].Maximum = avg + avg / 5;
		}

		internal void UpdateSize()
		{
			var level = Level;
			if (HasMaster && m_entry.Family != null)
			{
				if (level >= m_entry.Family.MaxScaleLevel)
				{
					ScaleX = m_entry.Family.MaxScale * m_entry.Scale;
				}
				else
				{
					ScaleX = (m_entry.Family.MinScale + ((m_entry.Family.MaxScaleLevel - level) * m_entry.Family.ScaleStep)) *
						m_entry.Scale;
				}
			}
			else
			{
				ScaleX = m_entry.Scale;
			}
		}
	}
}
