using WCell.Constants;
using WCell.Constants.Misc;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// TODO: Move everything Character-related from UnitUpdates in here
	/// </summary>
	public partial class Character
	{
		protected internal override void UpdateStamina()
		{
			base.UpdateStamina();
			
			if (m_MeleeAPModByStat != null)
			{
				this.UpdateAllAttackPower();
			}

			// update pet
			if (m_activePet != null && m_activePet.IsHunterPet)
			{
				m_activePet.UpdateStamina();
			}
		}

		protected internal override void UpdateIntellect()
		{
			base.UpdateIntellect();

			if (PowerType == PowerType.Mana)
			{
				// spell caster crit chance depends on intellect
				this.UpdateSpellCritChance();
			}

			// TODO Update spell power: AddDamageMod & HealingDoneMod

			this.UpdatePowerRegen();
			if (m_MeleeAPModByStat != null)
			{
				this.UpdateAllAttackPower();
			}
		}

		protected internal override void UpdateSpirit()
		{
			base.UpdateSpirit();

			if (m_MeleeAPModByStat != null)
			{
				this.UpdateAllAttackPower();
			}
		}

		protected internal override int IntellectManaBonus
		{
			get
			{
				var intelBase = Archetype.FirstLevelStats.Intellect;
				return intelBase + (Intellect - intelBase) * ManaPerIntelligence;
			}
		}

		#region CombatRatings
		private void UpdateChancesByCombatRating(CombatRating rating)
		{
			// TODO: Update influence
			switch (rating)
			{
				case CombatRating.Dodge:
					UnitUpdates.UpdateDodgeChance(this);
					break;
				case CombatRating.Parry:
					UnitUpdates.UpdateParryChance(this);
					break;
				case CombatRating.Block:
					UnitUpdates.UpdateBlockChance(this);
					break;
				case CombatRating.MeleeCritChance:
					UnitUpdates.UpdateCritChance(this);
					break;
				case CombatRating.RangedCritChance:
					UnitUpdates.UpdateCritChance(this);
					break;
				case CombatRating.SpellCritChance:
					UnitUpdates.UpdateSpellCritChance(this);
					break;
				case CombatRating.DefenseSkill:
					UnitUpdates.UpdateDefense(this);
					break;
				case CombatRating.MeleeHitChance:
					UnitUpdates.UpdateMeleeHitChance(this);
					break;
				case CombatRating.RangedHitChance:
					UnitUpdates.UpdateRangedHitChance(this);
					break;
				case CombatRating.Expertise:
					UnitUpdates.UpdateExpertise(this);
					break;
			}
		}
		#endregion
	}
}
