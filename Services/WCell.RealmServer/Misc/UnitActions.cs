using System;
using NLog;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;

namespace WCell.RealmServer.Misc
{
	#region IUnitAction
	/// <summary>
	/// Any kind of Action a Unit can perform
	/// </summary>
	public interface IUnitAction
	{
		/// <summary>
		/// The Attacker or Caster
		/// </summary>
		Unit Attacker { get; }

		/// <summary>
		/// Victim or Target or Receiver
		/// </summary>
		Unit Victim { get; }

		/// <summary>
		/// Whether this was a critical action (might be meaningless for some actions)
		/// </summary>
		bool IsCritical { get; }

		Spell Spell { get; }

		/// <summary>
		/// Reference count is used to support pooling
		/// </summary>
		int ReferenceCount
		{
			get;
			set;
		}
	}
	#endregion

	#region IDamageAction
	public interface IDamageAction : IUnitAction
	{
		SpellEffect SpellEffect
		{
			get;
		}

		int ActualDamage
		{
			get;
		}

		int Damage
		{
			get;
			set;
		}

		bool IsDot
		{
			get;
		}

		DamageSchool UsedSchool
		{
			get;
		}

		IWeapon Weapon
		{
			get;
		}
	}
	#endregion

	#region SimpleUnitAction
	public class SimpleUnitAction : IUnitAction
	{
		public Unit Attacker
		{
			get;
			set;
		}

		public Unit Victim
		{
			get;
			set;
		}

		public bool IsCritical
		{
			get;
			set;
		}

		public Spell Spell
		{
			get;
			set;
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		public int ReferenceCount
		{
			get { return 0; }
			set { }
		}
	}
	#endregion

	#region HealAction
	public class HealAction : SimpleUnitAction
	{
		public int Value
		{
			get;
			set;
		}

		/// <summary>
		/// Heal over time
		/// </summary>
		public bool IsHot
		{
			get;
			set;
		}
	}
	#endregion

	#region TrapTriggerAction
	public class TrapTriggerAction : SimpleUnitAction
	{
	}
	#endregion

	#region AuraRemovedAction
	public class AuraAction : IUnitAction
	{
		public Unit Attacker
		{
			get;
			set;
		}

		public Unit Victim
		{
			get;
			set;
		}

		public bool IsCritical
		{
			get { return false; }
		}

		public Aura Aura
		{
			get;
			set;
		}

		public Spell Spell
		{
			get { return Aura.Spell; }
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		public int ReferenceCount
		{
			get { return 0; }
			set { }
		}
	}
	#endregion

	#region SimpleDamageAction
	public class SimpleDamageAction : IDamageAction
	{
		public Unit Attacker
		{
			get { return null; }
		}

		public Unit Victim
		{
			get;
			set;
		}

		public SpellEffect SpellEffect
		{
			get { return null; }
		}

		public int Damage
		{
			get;
			set;
		}

		public int ActualDamage
		{
			get { return Damage; }
		}

		public bool IsDot
		{
			get { return false; }
		}

		public bool IsCritical
		{
			get { return false; }
		}

		public DamageSchool UsedSchool
		{
			get { return DamageSchool.Physical; }
		}

		public IWeapon Weapon
		{
			get { return null; }
		}

		public Spell Spell
		{
			get { return null; }
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		public int ReferenceCount
		{
			get { return 0; }
			set { }
		}
	}
	#endregion

	/// <summary>
	/// Contains all information related to any direct attack that deals positive damage
	/// </summary>
	public class DamageAction : IDamageAction
	{
		/// <summary>
		/// During Combat: The default delay in milliseconds between CombatTicks
		/// </summary>
		public static int DefaultCombatTickDelay = 600;

		public DamageAction(Unit attacker)
		{
			Attacker = attacker;
		}

		/// <summary>
		/// The Attacker or null if this is not an actual Attack
		/// </summary>
		public Unit Attacker
		{
			get;
			internal set;
		}

		/// <summary>
		/// The Unit that is being attacked
		/// </summary>
		public Unit Victim
		{
			get;
			set;
		}

		public void ModDamagePercent(int pct)
		{
			m_Damage += (m_Damage * pct + 50) / 100;
		}

		/// <summary>
		/// Returns the given percentage of the applied damage
		/// </summary>
		public int GetDamagePercent(int percent)
		{
			return (m_Damage * percent + 50) / 100;
		}

		private int m_Damage;

		public int Damage
		{
			get { return m_Damage; }
			set
			{
				if (value < 0)
				{
					// no negative damage
					value = 0;
				}
				m_Damage = value;
			}
		}

		public bool IsCritical
		{
			get;
			set;
		}

		/// <summary>
		/// Damage over time has different implications than normal damage
		/// </summary>
		public bool IsDot
		{
			get;
			set;
		}

		public IWeapon Weapon
		{
			get;
			set;
		}

		public SpellEffect SpellEffect
		{
			get;
			set;
		}

		public Spell Spell
		{
			get { return SpellEffect != null ? SpellEffect.Spell : null; }
		}

		public DamageSchoolMask Schools;

		/// <summary>
		/// Can only be modified in AddDamageMods, not later.
		/// Value between 0 and 100.
		/// </summary>
		public float ResistPct;

		public int Absorbed, Resisted, Blocked;

		public VictimState VictimState;

		public HitFlags HitFlags;

		private ProcHitFlags ProcHitFlags;

		/// <summary>
		/// Actions that are marked in use, will not be recycled
		/// </summary>
		public int ReferenceCount
		{
			get;
			set;
		}

		#region Situational Properties
		public DamageSchool UsedSchool
		{
			get;
			set;
		}

		/// <summary>
		/// White damage or strike ability (Heroic Strike, Ranged, Throw etc)
		/// </summary>
		public bool IsWeaponAttack
		{
			get
			{
				return Weapon != null &&
					   (SpellEffect == null || SpellEffect.Spell.IsPhysicalAbility);
			}
		}

		/// <summary>
		/// Any attack that involves a spell
		/// </summary>
		public bool IsSpellCast
		{
			get { return SpellEffect != null; }
		}

		/// <summary>
		/// Pure spell attack, no weapon involved
		/// </summary>
		public bool IsMagic
		{
			get { return !IsWeaponAttack && SpellEffect != null; }
		}

		public bool IsRangedAttack
		{
			get
			{
				return Weapon != null ? Weapon.IsRanged : false;
			}
		}

		public bool IsAutoshot
		{
			get
			{
				return Weapon != null && SpellEffect != null &&
					SpellEffect.Spell.IsAutoRepeating;
			}
		}

		public bool IsMeleeAttack
		{
			get
			{
				return Weapon != null ? !Weapon.IsRanged : false;
			}
		}

		public bool CanDodge
		{
			get { return SpellEffect == null || !SpellEffect.Spell.Attributes.HasFlag(SpellAttributes.CannotDodgeBlockParry); }
		}

		public bool CanBlockParry
		{
			get
			{
				return CanDodge && !Victim.IsStunned && Attacker.IsInFrontOf(Victim);
			}
		}

		/// <summary>
		/// An action can crit if there is no spell involved,
		/// the given spell is allowed to crit by default,
		/// or the Attacker has a modifier that allows the spell to crit.
		/// </summary>
		public bool CanCrit
		{
			get
			{
				return (SpellEffect == null ||
						((!Spell.AttributesExB.HasFlag(SpellAttributesExB.CannotCrit) && !IsDot)) ||
						(Attacker is Character && ((Character)Attacker).PlayerAuras.CanSpellCrit(SpellEffect.Spell)));
			}
		}

		public int ActualDamage
		{
			get { return Damage - Absorbed - Resisted - Blocked; }
		}
		#endregion

		#region Attack
		/// <summary>
		/// Does a melee/ranged/wand physical attack.
		/// Calculates resistances/attributes (resilience, hit chance) and takes them into account.
		/// </summary>
		/// <returns>ProcHitFlags containing hit result</returns>
		public ProcHitFlags DoAttack()
		{
			if (Victim == null)
			{
				LogManager.GetCurrentClassLogger().Error("{0} tried to attack with no Target selected.", Attacker);
				return ProcHitFlags;
			}

			if (Victim.IsEvading)
			{
				Evade();
				return ProcHitFlags;
			}
			else if (Victim.IsImmune(UsedSchool) || Victim.IsInvulnerable)
			{
				MissImmune();
				return ProcHitFlags;
			}

			//foreach (var mod in Attacker.AttackModifiers)
			//{
			//    mod.ModPreAttack(this);
			//}

			if (CanCrit && Victim.StandState != StandState.Stand)
			{
				StrikeCritical();
				return ProcHitFlags;
			}

			//hitinfo declarations
			var hitChance = CalcHitChance();

			var random = Utility.Random(1, 10000);
			if (random > hitChance)
			{
				// missed the target
				Miss();
				return ProcHitFlags;
			}
			else
			{
				int dodgeParry;
				if (!IsRangedAttack)
				{
					// can only dodge or parry when using melee
					var dodge = CanDodge ? CalcDodgeChance() : 0;
					var parry = CanBlockParry ? CalcParryChance() : 0;
					dodgeParry = dodge + parry;

					if (random > (hitChance - dodge))
					{
						// dodge
						Dodge();
						return ProcHitFlags;
					}
					else if (random > (hitChance - dodge - parry))
					{
						// parry
						Parry();
						return ProcHitFlags;
					}
				}
				else
				{
					dodgeParry = 0;
				}


				var glancingblow = CalcGlancingBlowChance();
				if (random > (hitChance - dodgeParry - glancingblow))
				{
					// TODO: glancing blow
					StrikeGlancing();
					return ProcHitFlags;
				}
				else
				{
					var crushingblow = CalcCrushingBlowChance();
					var critical = CalcCritChance();

					if (random > (hitChance - dodgeParry - glancingblow - crushingblow))
					{
						//crushing blow
						StrikeCrushing();
						return ProcHitFlags;
					}
					else if (random > (hitChance - dodgeParry - glancingblow - crushingblow - critical))
					{
						// critical hit
						StrikeCritical();
						return ProcHitFlags;
					}
					else
					{
						if (CanBlockParry && random > (hitChance - dodgeParry - glancingblow - critical - crushingblow - CalcBlockChance()))
						{
							// block
							Block();
							return ProcHitFlags;
						}
						else
						{
							// normal attack
							StrikeNormal();
							return ProcHitFlags;
						}
					}
				}
			}
		}
		#endregion

		#region Miss & Strike
		public void MissImmune()
		{
			Damage = 0;
			VictimState = VictimState.Immune;
			ProcHitFlags |= ProcHitFlags.Immune;
			DoStrike();
		}

		public void Miss()
		{
			Damage = 0;
			ProcHitFlags |= ProcHitFlags.Miss;
			DoStrike();
		}

		public void Dodge()
		{
			Damage = 0;
			VictimState = VictimState.Dodge;
			HitFlags = HitFlags.Miss;
			ProcHitFlags |= ProcHitFlags.Dodge;
			Blocked = 0;
			IsCritical = false;
			DoStrike();
		}

		public void Block()
		{
			HitFlags = HitFlags.PlayWoundAnimation | HitFlags.Block;
			VictimState = VictimState.Block;
			ProcHitFlags |= ProcHitFlags.Block;
			Blocked = CalcBlockDamage();
			if (Damage == Blocked)
			{
				ProcHitFlags |= ProcHitFlags.FullBlock;
			}
			IsCritical = false;
			DoStrike();
		}

		public void Evade()
		{
			Damage = 0;
			VictimState = VictimState.Evade;
			ProcHitFlags |= ProcHitFlags.Evade;
			DoStrike();
		}

		public void Parry()
		{
			Damage = 0;
			VictimState = VictimState.Parry;
			HitFlags = HitFlags.Miss;
			ProcHitFlags |= ProcHitFlags.Parry;
			Blocked = 0;
			IsCritical = false;
			DoStrike();
		}

		public void StrikeCrushing()
		{
			Damage = (Damage * 10 + 5) / 15;		// == Damage * 1.5f
			HitFlags = HitFlags.PlayWoundAnimation | HitFlags.Crushing;
			VictimState = VictimState.Wound;
			ProcHitFlags |= ProcHitFlags.NormalHit;
			Blocked = 0;
			IsCritical = false;
			DoStrike();
		}

		public void StrikeCritical()
		{
			IsCritical = Victim.StandState == StandState.Stand;
			SetCriticalDamage();
			HitFlags = HitFlags.PlayWoundAnimation | HitFlags.ResistType1 | HitFlags.ResistType2 | HitFlags.CriticalStrike;
			VictimState = VictimState.Wound;
			ProcHitFlags |= ProcHitFlags.CriticalHit;
			Blocked = 0;
			// Automatic double damage against sitting target - but doesn't proc crit abilities
			DoStrike();
		}

		public void SetCriticalDamage()
		{
			Damage = MathUtil.RoundInt(Attacker.CalcCritDamage(Damage, Victim, SpellEffect));
		}

		public void StrikeGlancing()
		{
			Damage = (int)(Damage * CalcGlancingBlowDamageFactor());
			VictimState = VictimState.Wound;
			HitFlags = HitFlags.PlayWoundAnimation | HitFlags.Glancing;
			ProcHitFlags |= ProcHitFlags.NormalHit;
			Blocked = 0;
			IsCritical = false;
			DoStrike();
		}

		public void StrikeNormal()
		{
			HitFlags = HitFlags.PlayWoundAnimation;
			VictimState = VictimState.Wound;
			ProcHitFlags |= ProcHitFlags.NormalHit;
			Blocked = 0;
			IsCritical = false;
			DoStrike();
		}

		/// <summary>
		/// Strikes the target
		/// </summary>
		public void DoStrike()
		{
			if (Damage > 0)
			{
				var level = Attacker.Level;
				var res = Victim.GetResistance(UsedSchool) - Attacker.GetTargetResistanceMod(UsedSchool);

				if (res > 0)
				{
					ProcHitFlags |= ProcHitFlags.Resist;

					// This formula only applies for armor
					if (UsedSchool == DamageSchool.Physical)
					{
						if (level < 60)
						{
							ResistPct = (res / (res + 400f + 85f * level)) * 100f;
						}
						else
						{
							ResistPct = (res / (res - 22167.5f + 467.5f * level)) * 100f;
						}

					}
					else
					{
						// Magical damageschool
						ResistPct = Victim.GetResistChancePct(Attacker, UsedSchool);
					}
				}
				else
				{
					ResistPct = 0;
				}

				if (ResistPct > 75)
				{
					ResistPct = 75;
				}
				if (ResistPct < 0)
				{
					ResistPct = 0;
				}

				Victim.DeathPrevention++;
				Attacker.DeathPrevention++;
				try
				{
					// add mods and call events
					AddDamageMods();
					Victim.OnDefend(this);
					Attacker.OnAttack(this);

					Resisted = MathUtil.RoundInt(ResistPct * Damage / 100f);
					if (Absorbed > 0)
					{
						HitFlags |= HitFlags.AbsorbType1 | HitFlags.AbsorbType2;
						ProcHitFlags |= ProcHitFlags.Absorb;
					}
					else
					{
						Absorbed = Resisted = 0;
					}

					if (Weapon == Attacker.OffHandWeapon)
					{
						HitFlags |= HitFlags.OffHand;
					}

					Victim.DoRawDamage(this);
				}
				finally
				{
					Victim.DeathPrevention--;
					Attacker.DeathPrevention--;
				}
			}

			//if ()
			//CombatHandler.SendMeleeDamage(attacker, this, schools, hitInfo, (uint)totalDamage,
			//(uint)absorbed, (uint)resisted, (uint)blocked, victimState);
			if (SpellEffect != null)
			{
				CombatLogHandler.SendMagicDamage(this);
			}
			else
			{
				CombatHandler.SendAttackerStateUpdate(this);
			}

			TriggerProcOnStrike();
		}

		void TriggerProcOnStrike()
		{
			if (Weapon != null && SpellEffect == null)
			{
				var attackerProcTriggerFlags = ProcTriggerFlags.None;
				var victimProcTriggerFlags = ProcTriggerFlags.None;

				if (Weapon.IsMelee)
				{
					attackerProcTriggerFlags |= ProcTriggerFlags.DoneMeleeAutoAttack;
					victimProcTriggerFlags |= ProcTriggerFlags.ReceivedMeleeAutoAttack;
				}
				else if (Weapon.IsRanged)
				{
					attackerProcTriggerFlags |= ProcTriggerFlags.DoneRangedAutoAttack;
					victimProcTriggerFlags |= ProcTriggerFlags.ReceivedRangedAutoAttack;
				}

				if (Attacker != null && Attacker.IsAlive)
				{
					Attacker.Proc(attackerProcTriggerFlags, Attacker, this, true, ProcHitFlags);
				}

				if (Victim != null && Victim.IsAlive)
				{
					Victim.Proc(victimProcTriggerFlags, Attacker, this, true, ProcHitFlags);
				}
			}
		}
		#endregion

		#region Chances
		/// <summary>
		/// Calculated in UnitUpdates.UpdateBlockChance
		/// </summary>
		public int CalcBlockDamage()
		{
			// Mobs should be able to block as well, right?
			if (!(Victim is Character))
			{
				// mobs can't block
				return 0;
			}

			var target = (Character)Victim;
			return (int)target.BlockValue;
		}

		/// <summary>
		/// Calculates the damage reduced by a glancing blow.
		/// Reusable in the case glancing blows also happen against players
		/// Currently they don't.
		/// </summary>
		/// <returns>The random damage reduction factor between 0.01 and 0.99 depending on weapon skill/defense</returns>
		public double CalcGlancingBlowDamageFactor()
		{
			// The value will be capped to Level * 5 
			// otherwise items which increase skill could push glancing blows/dmg off the attacktable
			var attackerSkill = Victim is Character ? ((Character)Attacker).Skills.GetValue(Weapon.Skill) : (uint)Victim.Level * 5;
			if (attackerSkill > Attacker.Level * 5)
			{
				attackerSkill = (uint)Attacker.Level * 5;
			}

			var defenseSkill = Victim is Character ? ((Character)Victim).Skills.GetValue(SkillId.Defense) : (uint)Victim.Level * 5;
			var diff = defenseSkill - attackerSkill;
			var lowValue = 1.3 - 0.05 * (diff);
			var highValue = 1.2 - 0.03 * (diff);
			if (SpellEffect != null)
			{
				lowValue -= 0.7;
				lowValue = Math.Min(0.6, lowValue);
				highValue -= 0.3;
			}
			else
			{
				lowValue = Math.Min(0.91, lowValue);
			}
			if (lowValue < 0.01)
			{
				lowValue = 0.01;
			}
			if (highValue < 0.2)
			{
				highValue = 0.2;
			}
			if (highValue > 0.99)
			{
				highValue = 0.99;
			}

			return Utility.Random(lowValue, highValue);
		}

		/// <summary>
		/// Calculates the percentage of damage reduction from armor.
		/// Between 1-7500 (capped at 75%)
		/// </summary>
		/// <returns></returns>
		public double CalcArmorReductionPrct()
		{
			//return (int) (100/((467.5*Attacker.Level - 22167.5)/Victim.Armor + 1));
			double levelMod = Attacker.Level;
			if (levelMod > 59)
			{
				levelMod = (levelMod + (4.5 * (levelMod - 59)));
			}
			var reduction = 0.1 * Victim.Armor / (8.5 * levelMod + 40);
			reduction = reduction / (1 + reduction);

			// Damage reduction cap
			if (reduction > 0.75)
			{
				return 75;
			}

			// No reduction?!
			if (reduction < 0)
			{
				return 0;
			}
			return reduction * 100;
		}

		/// <summary>
		/// Gives the chance to hit between 0-10000
		/// </summary>
		public int CalcHitChance()
		{
			int hitchance = 0;
			int skillBonus;

			//uhm gotta set the variables for skills
			if (Victim is Character)
			{
				skillBonus = (int)((Character)Victim).Skills.GetValue(SkillId.Defense);
			}
			else
			{
				skillBonus = Victim.Level * 5; // defskill of mobs depends on their lvl.
			}

			// attacker hit mods
			var attackHitChanceMod = Victim.GetIntMod(IsRangedAttack ? StatModifierInt.AttackerRangedHitChance : StatModifierInt.AttackerMeleeHitChance);
			hitchance += attackHitChanceMod * 100;

			if (Attacker is Character)
			{

				var atk = Attacker as Character;
				hitchance += (IsRangedAttack ? (int)atk.RangedHitChance : (int)atk.HitChance) * 100;
				skillBonus -= (int)atk.Skills.GetValue(Weapon.Skill);
			}
			else
			{
				skillBonus -= Attacker.Level * 5; // attack skill of mobs depends on their lvl.
			}

			//uhh think that pve and pvp is the same calculations...
			if (skillBonus <= 10)
			{
				if (Attacker.UsesDualWield)
				{
					hitchance += (10000 - (2400 + (skillBonus) * 10));
				}
				else
				{
					hitchance += (10000 - (500 + (skillBonus) * 10));
				}
			}
			else
			{
				if (Attacker.UsesDualWield)
				{
					hitchance += (10000 - (2600 + (skillBonus - 1000) * 40));
				}
				else
				{
					hitchance += (10000 - (700 + (skillBonus - 1000) * 40));
				}
			}

			//returning something that makes somewhat sense....
			if (hitchance > 10000)
				return 10000;
			else
				return hitchance;
		}

		/// <summary>
		/// 2.1 calculation (3.30 is ~10% lower (24%))
		/// Is between 1 - 10000, 
		/// </summary>
		/// <returns></returns>
		public int CalcGlancingBlowChance()
		{
			if (Attacker is Character)
			{
				var weaponSkill = ((Character)Attacker).Skills.GetValue(Weapon.Skill);
				if (weaponSkill > Attacker.Level * 5)
				{
					weaponSkill = (uint)Attacker.Level * 5;
				}
				var chance = (10 + (int)(Victim.Level * 5 - weaponSkill)) * 100;
				return MathUtil.ClampMinMax(chance, 0, 10000);
			}
			return 0;
		}

		/// <summary>
		/// Calculates the chance of a crushing blow.
		/// </summary>
		/// <returns>The chance multiplied by 100, 54% = 5400, 100% = 10000</returns>
		public int CalcCrushingBlowChance()
		{
			//var chance = 0;

			//if (Attacker is NPC && Victim is Character)
			//{
			//    var def = (Character)Victim;

			//    var weaponskill = ((Character)Attacker).Skills.GetValue(Weapon.Skill);
			//    var defense = def.GetCombatRatingMod(CombatRating.DefenseSkill);
			//    defense = Math.Max(defense, Victim.Level * 5);

			//    chance = (int) ((weaponskill - defense) * 2 - 15);

			//    if (chance > 0)
			//        chance *= 100;
			//    else chance = 0;
			//}
			//return chance;

			var chance = 0;

			if (Attacker is NPC && Victim is Character)
			{
				var chr = Victim as Character;
				var playerDefense = chr.Skills.GetValue(SkillId.Defense);
				var npcWeaponSkill = Attacker.Level * 5;

				// Crushing blow can only happen if the player's defense skill is 20 points smaller than the mob's weapon skill.
				if ((npcWeaponSkill - playerDefense) >= 20)
				{
					return (int)((npcWeaponSkill - playerDefense) * 200 - 1500);
				}
			}

			return chance;
		}

		/// <summary>
		/// After we already know that we did not crit, we want to check
		/// again against a bonus crit chance.
		/// 
		/// We use basic laws of probability:
		/// P(CritWithBonus | NoCrit) = 
		/// P(CritWithBonus) / P(NoCrit) =
		/// critBonus / (10000 - origCritChance)
		/// </summary>
		public void AddBonusCritChance(int critBonusPct)
		{
			if (IsCritical) return;

			var origCritChance = CalcCritChance();	// 0-10000
			var critChance = ((critBonusPct * 100) * 10000) / (10000 - origCritChance);

			IsCritical = Utility.Random(0, 10000) < critChance;
			if (IsCritical)
			{
				SetCriticalDamage();
			}
		}

		/// <summary>
		/// Calculates the crit chance between 0-10000
		/// </summary>
		/// <returns>The crit chance after taking into account defense, weapon skills, resilience etc</returns>
		public int CalcCritChance()
		{
			if (!CanCrit)
			{
				return 0;
			}

			var chance = (int)Attacker.GetBaseCritChance(UsedSchool, Spell, Weapon) * 100;

			if (Weapon != null)
			{
				if (Attacker is NPC && Victim is Character)
				{
					// NPC attacks Player
					var chr = Victim as Character;
					var weaponSkill = chr.Skills.GetValue(Weapon.Skill);
					var defSkill = chr.Skills.GetValue(SkillId.Defense);

					chance += (int)(4 * (weaponSkill - defSkill));
				}

				if (Attacker is Character && Victim is NPC)
				{
					// Player attacks NPC
					var chr = Attacker as Character;
					var weaponSkill = chr.Skills.GetValue(Weapon.Skill);
					var defSkill = Victim.Level * 5;

					if (defSkill > weaponSkill)
					{
						chance -= (int)(20 * (defSkill - weaponSkill));
					}
					// else: no change (mobs def is smaller than player's weapon skill)
				}
			}

			// attackerCritChance is not shown in the tooltip but affects the crit chance against the Victim
			var attackerCritChance = 100;
			if (UsedSchool == DamageSchool.Physical)
			{
				attackerCritChance += Victim.AttackerPhysicalCritChancePercentMod;
			}
			else
			{
				attackerCritChance += Victim.AttackerSpellCritChancePercentMod;
			}

			chance = (chance * attackerCritChance + 50) / 100;	// rounded
			chance -= (int)(Victim.GetResiliencePct() * 100); //resilience

			return MathUtil.ClampMinMax(chance, 0, 10000);
		}

		/// <summary>
		/// See: http://www.wowwiki.com/Formulas:Block
		/// Calculates the block chance between 1-10000
		/// </summary>
		public int CalcBlockChance()
		{
			if (!(Victim is Character))
			{
				// mobs can't block
				return 0;
			}

			var target = (Character)Victim;

			if (target.BlockChance < 1)
			{
				return 0;
			}

			var blockChance = target.BlockChance;
			var defValue = target.Skills.GetValue(SkillId.Defense);

			int weaponSkill;
			if (Attacker is Character)
			{
				weaponSkill = (int)((Character)Attacker).Skills.GetValue(Weapon.Skill);
				blockChance -= ((Character)Attacker).Expertise * 0.25f;
			}
			else
			{
				weaponSkill = Attacker.Level * 5;
			}
			blockChance += (defValue - weaponSkill) * 0.04f;

			blockChance *= 100;

			if (blockChance > 10000)
			{
				return 10000;
			}
			return (int)blockChance;
		}

		/// <summary>
		/// Calculates the parry chance taking into account the difference between 
		/// the weapon skill of the attacker and the defense skill of the victim.
		/// Also see <see cref="Unit.CalcParryChance"/>
		/// </summary>
		/// <returns>The chance between 1-10000</returns>
		public int CalcParryChance()
		{
			var defSkill = Victim.Level * 5;
			var weaponSkill = Attacker.Level * 5;

			if (Victim is Character)
			{
				defSkill = (int)((Character)Victim).Skills.GetValue(SkillId.Defense);
			}

			if (Attacker is Character)
			{
				weaponSkill = (int)((Character)Attacker).Skills.GetValue(Weapon.Skill);
			}
			var chance = Victim.CalcParryChance(Attacker);
			chance += (int)((defSkill - weaponSkill) * 0.04);
			return chance;
		}

		/// <summary>
		/// Calculates the dodge chance taking into account the difference between 
		/// the weapon skill of the attacker and the defense skill of the victim.
		/// Also see <see cref="Unit.CalcDodgeChance"/>
		/// </summary>
		/// <returns>The chance between 1-10000</returns>
		public int CalcDodgeChance()
		{
			var defSkill = Victim.Level * 5;
			var weaponSkill = Attacker.Level * 5;

			if (Victim is Character)
			{
				defSkill = (int)((Character)Victim).Skills.GetValue(SkillId.Defense);
			}

			if (Attacker is Character)
			{
				weaponSkill = (int)((Character)Attacker).Skills.GetValue(Weapon.Skill);
			}

			var chance = Victim.CalcDodgeChance(Attacker);
			chance += (int)((defSkill - weaponSkill) * 0.04);
			return chance;
		}

		#endregion

		#region Damages
		/// <summary>
		/// Adds all damage boni and mali
		/// </summary>
		internal void AddDamageMods()
		{
			if (Attacker != null)
			{
				if (!IsDot)
				{
					// does not add to dot
					Damage = Attacker.GetFinalDamage(UsedSchool, Damage, Spell);
				}
				else if (SpellEffect != null)
				{
					// periodic damage mod
					Damage = Attacker.Auras.GetModifiedInt(SpellModifierType.PeriodicEffectValue, Spell, Damage);
				}
			}
		}
		#endregion

		#region Absorb
		public int Absorb(int absorbAmount, DamageSchoolMask schools)
		{
			if (absorbAmount <= 0)
			{
				return 0;
			}

			if (SpellEffect != null && Spell.AttributesExD.HasFlag(SpellAttributesExD.CannotBeAbsorbed))
			{
				return 0;
			}

			if (schools.HasAnyFlag(UsedSchool))
			{
				var value = Math.Min(Damage, absorbAmount);
				absorbAmount -= value;
				Absorbed += value;
			}
			return absorbAmount;
		}
		#endregion

		internal void Reset(Unit attacker, Unit target, IWeapon weapon)
		{
			Attacker = attacker;
			Victim = target;
			Weapon = weapon;
			ProcHitFlags = ProcHitFlags.None;
		}

		internal void OnFinished()
		{
			if (Attacker != null && Victim is NPC)
			{
				((NPC)(Victim)).ThreatCollection.AddNewIfNotExisted(Attacker);
			}
			ReferenceCount--;
			SpellEffect = null;
		}

		public override string ToString()
		{
			return string.Format("Attacker: {0}, Target: {1}, Spell: {2}, Damage: {3}", Attacker, Victim,
				SpellEffect != null ? SpellEffect.Spell : null, Damage);
		}
	}

	public interface IAttackEventHandler
	{
		/// <summary>
		/// Called before hit chance, damage etc is determined.
		/// This is not used for Spell attacks, since those only have a single "stage".
		/// NOT CURRENTLY IMPLEMENTED
		/// </summary>
		void OnBeforeAttack(DamageAction action);

		/// <summary>
		/// Called on the attacker, right before resistance is subtracted and final damage is evaluated
		/// </summary>
		void OnAttack(DamageAction action);

		/// <summary>
		/// Called on the defender, right before resistance is subtracted and final damage is evaluated
		/// </summary>
		void OnDefend(DamageAction action);
	}
}