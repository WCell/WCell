using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.RealmServer.NPCs.Pets;
using WCell.Util;

namespace WCell.RealmServer.Entities
{
	public partial class NPC
	{
		private int[] m_damageDoneMods;
		private float[] m_damageDoneFactors;

		#region Damage Mods
		/// <summary>
		/// Modifies the damage for the given school by the given delta.
		/// Requires a call to <see cref="UnitUpdates.UpdateAllDamages"/> afterwards.
		/// </summary>
		/// <param name="school"></param>
		/// <param name="delta"></param>
		protected internal override void AddDamageDoneModSilently(DamageSchool school, int delta)
		{
			if (m_damageDoneMods == null)
			{
				m_damageDoneMods = new int[(int)DamageSchool.Count];
			}
			m_damageDoneMods[(int)school] += delta;
		}

		/// <summary>
		/// Modifies the damage for the given school by the given delta.
		/// Requires a call to <see cref="UnitUpdates.UpdateAllDamages"/> afterwards.
		/// </summary>
		/// <param name="school"></param>
		/// <param name="delta"></param>
		protected internal override void RemoveDamageDoneModSilently(DamageSchool school, int delta)
		{
			if (m_damageDoneMods == null)
			{
				return;
			}
			m_damageDoneMods[(int)school] -= delta;
		}

		protected internal override void ModDamageDoneFactorSilently(DamageSchool school, float delta)
		{
			if (m_damageDoneFactors == null)
			{
				m_damageDoneFactors = new float[(int)DamageSchool.Count];
			}
			m_damageDoneFactors[(int)school] += delta;
		}

		public override int GetDamageDoneMod(DamageSchool school)
		{
			var amount = 0;

			if (IsHunterPet && m_master != null)
			{
				if (school != DamageSchool.Physical)
				{
					// "0.13% spell damage (0.18 spell damage with 2/2 Wild Hunt)"	
					var bonus = m_master.GetDamageDoneMod(school);
					amount += (bonus * PetMgr.PetSpellDamageOfOwnerPercent + 50) / 100;	 // rounding
				}
			}
			if (m_damageDoneMods != null)
			{
				amount += m_damageDoneMods[(int)school];
			}
			return amount;
		}

		public override float GetDamageDoneFactor(DamageSchool school)
		{
			if (m_damageDoneFactors == null)
			{
				return 1;
			}
			return 1 + m_damageDoneFactors[(int)school];
		}
		#endregion


		public override float GetResiliencePct()
		{
			if (HasPlayerMaster)
			{
				var chr = (Character)m_master;
				var resilience = chr.GetCombatRating(CombatRating.MeleeResilience);
				return resilience / GameTables.GetCRTable(CombatRating.MeleeResilience).GetMax((uint)Level - 1);
			}
			return 5;
		}
	}
}
