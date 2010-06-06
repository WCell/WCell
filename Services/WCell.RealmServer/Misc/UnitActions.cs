using System;
using NLog;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.Misc
{
	#region Action Interfaces
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

		Spell Spell
		{
			get;
		}
	}

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

		bool IsCritical
		{
			get;
		}

		DamageSchool UsedSchool
		{
			get;
		}

		ProcTriggerFlags TargetProcTriggerFlags
		{
			get;
		}

		ProcTriggerFlags AttackerProcTriggerFlags
		{
			get;
		}

		IWeapon Weapon
		{
			get;
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

		public ProcTriggerFlags TargetProcTriggerFlags
		{
			get { return ProcTriggerFlags.None; }
		}

		public ProcTriggerFlags AttackerProcTriggerFlags
		{
			get { return ProcTriggerFlags.None; }
		}

		public IWeapon Weapon
		{
			get { return null; }
		}

		public Spell Spell
		{
			get { return null; }
		}
	}
	#endregion

	/// <summary>
	/// Contains all information related to any direct attack that deals positive damage
	/// </summary>
	public class DamageAction : IDamageAction
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

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

		public int Damage
		{
			get;
			set;
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

		#region Situational Properties
		public DamageSchool UsedSchool
		{
			get;
			set;
		}

		public bool IsInUse
		{
			get { return Victim != null; }
		}

		public bool IsWeaponAttack
		{
			get
			{
				return Weapon != null &&
					   (SpellEffect == null || SpellEffect.Spell.IsWeaponAbility);
			}
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
					SpellEffect.Spell.AttributesExB.HasFlag(SpellAttributesExB.AutoRepeat);
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

		public bool CanCrit
		{
			get { return (SpellEffect == null || !Spell.AttributesExB.HasFlag(SpellAttributesExB.CannotCrit)); }
		}

		public int ActualDamage
		{
			get
			{
				return Damage - Absorbed - Resisted - Blocked;
			}
		}
		#endregion

		#region Proc Flags
		public ProcTriggerFlags TargetProcTriggerFlags
		{
			get
			{
				if (SpellEffect != null && SpellEffect.IsProc)
				{
					return ProcTriggerFlags.None;
				}

				var flags = ProcTriggerFlags.AnyHostileAction;
				if (IsRangedAttack)
				{
					flags |= ProcTriggerFlags.RangedAttack | ProcTriggerFlags.PhysicalAttack;
					if (IsCritical)
					{
						flags |= ProcTriggerFlags.RangedCriticalHit;
					}
				}
				else if (IsMeleeAttack)
				{
					flags |= ProcTriggerFlags.MeleeAttack | ProcTriggerFlags.PhysicalAttack;
					if (IsCritical)
					{
						flags |= ProcTriggerFlags.MeleeCriticalHit;
					}
				}

				if (SpellEffect != null)
				{
					flags |= ProcTriggerFlags.SpellHit;
					if (IsCritical)
					{
						flags |= ProcTriggerFlags.SpellCastSpecific2;
					}
				}
				return flags;
			}
		}

		public ProcTriggerFlags AttackerProcTriggerFlags
		{
			get
			{
				if (SpellEffect != null && SpellEffect.IsProc)
				{
					return ProcTriggerFlags.None;
				}

				var flags = ProcTriggerFlags.ActionSelf;
				if (IsRangedAttack)
				{
					flags |= ProcTriggerFlags.RangedAttackSelf;
					if (IsCritical)
					{
						//flags |= ProcTriggerFlags.RangedCriticalHit;
					}
				}
				else if (IsMeleeAttack)
				{
					flags |= ProcTriggerFlags.MeleeAttackSelf;
					if (IsCritical)
					{
						flags |= ProcTriggerFlags.MeleeCriticalHitSelf;
					}
				}
				if (SpellEffect != null)
				{
					flags |= ProcTriggerFlags.SpellCast;
				}
				return flags;
			}
		}
		#endregion

		#region Attack and Damage
		/// <summary>
		/// Does a melee/ranged/wand physical attack. (Not spells)
		/// Calculates resistances/attributes (resilience, hit chance) and takes them into account.
		/// </summary>
		/// <returns>Whether the attack hit</returns>
		public bool DoAttack()
		{
			if (Victim == null)
			{
				log.Error("{0} tried to attack with no Target selected.", Attacker);
				return false;
			}

			if (Victim.IsEvading)
			{
				Evade();
				return false;
			}
			else if (Victim.IsImmune(DamageSchool.Physical) || Victim.IsInvulnerable)
			{
				MissImmune();
				return false;
			}

			//foreach (var mod in Attacker.AttackModifiers)
			//{
			//    mod.ModPreAttack(this);
			//}

			if (CanCrit && Victim.StandState != StandState.Stand)
			{
				StrikeCritical();
				return true;
			}

			//hitinfo declarations
			var hitChance = CalcHitChance();

			var random = Utility.Random(1, 10000);
			if (random > hitChance)
			{
				// missed the target
				Miss();
				return false;
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
						return false;
					}
					else if (random > (hitChance - dodge - parry))
					{
						// parry
						Parry();
						return false;
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
					return true;
				}
				else
				{
					var crushingblow = CalcCrushingBlowChance();
					var critChance = CalcCritChance();

					critChance -= (int)(Victim.GetResiliencePct() * 100); //resilience
					int critical;

					if (critChance < 0)
					{
						critical = 0;
					}
					else
					{
						critical = critChance;
					}
					if (random > (hitChance - dodgeParry - glancingblow - crushingblow))
					{
						//crushing blow
						StrikeCrushing();
						return true;
					}
					else if (CanCrit && random > (hitChance - dodgeParry - glancingblow - crushingblow - critical))
					{
						// critical hit
						StrikeCritical();
						return true;
					}
					else
					{
						var blockchance = CalcBlockChance();
						if (CanBlockParry && random > (hitChance - dodgeParry - glancingblow - critical - crushingblow - blockchance))
						{
							// block
							Block();
							return false;
						}
						else
						{
							// normal attack
							StrikeNormal();
							return true;
						}
					}
				}
			}
		}

		public void MissImmune()
		{
			Damage = 0;
			VictimState = VictimState.Immune;
			DoStrike();
		}

		public void Miss()
		{
			Damage = 0;
			DoStrike();
		}

		public void Dodge()
		{
			Damage = 0;
			VictimState = VictimState.Dodge;
			HitFlags = HitFlags.Miss;
			Blocked = 0;
			IsCritical = false;
			DoStrike();
		}

		public void Block()
		{
			HitFlags = HitFlags.NormalSwingAnim | HitFlags.Block;
			VictimState = VictimState.Block;
			Blocked = CalcBlockDamage();
			IsCritical = false;
			DoStrike();
		}

		public void Evade()
		{
			Damage = 0;
			VictimState = VictimState.Evade;
			DoStrike();
		}

		public void Parry()
		{
			Damage = 0;
			VictimState = VictimState.Parry;
			HitFlags = HitFlags.Miss;
			Blocked = 0;
			IsCritical = false;
			DoStrike();
		}

		public void StrikeCrushing()
		{
			Damage = (Damage * 1.5f).RoundInt();
			HitFlags = HitFlags.NormalSwingAnim | HitFlags.Crushing;
			VictimState = VictimState.Wound;
			Blocked = 0;
			IsCritical = false;
			DoStrike();
		}

		public void StrikeCritical()
		{
			Damage = Attacker.CalcCritDamage(Damage, Victim, SpellEffect).RoundInt();
			HitFlags = HitFlags.NormalSwingAnim | HitFlags.Resist_1 | HitFlags.Resist_2 | HitFlags.CriticalStrike;
			VictimState = VictimState.Wound;
			Blocked = 0;
			// Automatic double damage against sitting target - but doesn't proc crit abilities
			IsCritical = Victim.StandState == StandState.Stand;
			DoStrike();
		}

		public void StrikeGlancing()
		{
			Damage = (int)(Damage * CalcGlancingBlowDamageFactor());
			VictimState = VictimState.Wound;
			HitFlags = HitFlags.NormalSwingAnim | HitFlags.Glancing;
			Blocked = 0;
			IsCritical = false;
			DoStrike();
		}

		public void StrikeNormal()
		{
			HitFlags = HitFlags.NormalSwingAnim;
			VictimState = VictimState.Wound;
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

				Attacker.AddDamageMods(this);

				Resisted = (ResistPct * Damage / 100f).RoundInt();
				Absorbed = Victim.Absorb(UsedSchool, Damage);
				if (Absorbed > 0)
				{
					HitFlags |= HitFlags.Absorb_1 | HitFlags.Absorb_2;
				}
			}
			else
			{
				Absorbed = Resisted = 0;
			}

			if (Weapon == Attacker.OffHandWeapon)
			{
				HitFlags |= HitFlags.LeftSwing;
			}

			Victim.DoRawDamage(this);

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

			// reset Target
			Victim = null;
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
			var hitchance = 0;
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

			if (Attacker is Character)
			{
				var atk = Attacker as Character;

				var hitrating = atk.GetCombatRatingMod(CombatRating.MeleeHitChance);

				if (!IsRangedAttack)
				{
					hitchance = (int)(100 * (hitrating / GameTables.GetCRTable(CombatRating.MeleeHitChance)[Attacker.Level - 1]));
				}
				else
				{
					hitchance = (int)(100 * (hitrating / GameTables.GetCRTable(CombatRating.RangedHitChance)[Attacker.Level - 1]));
				}
				skillBonus -= (int)atk.Skills.GetValue(Weapon.Skill);
				hitchance += atk.HitChanceMod;
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
		/// Should be between 1 - 10000, 
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
				if (chance > 10000)
					return 10000;

				return chance;
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
		/// Calculates the crit chance between 1-10000
		/// </summary>
		/// <returns>The crit chance after taking into account the defense/weapon skill</returns>
		public int CalcCritChance()
		{
			var chance = Attacker.CalcCritChanceBase(Victim, SpellEffect, Weapon);

			if (Attacker is NPC && Victim is Character)
			{
				var weaponSkill = Attacker.Level * 5;
				var chr = Victim as Character;
				var defSkill = chr.Skills.GetValue(SkillId.Defense);

				chance += 0.04f * (weaponSkill - defSkill);
			}

			if (Attacker is Character && Victim is NPC)
			{
				var chr = Attacker as Character;
				var weaponSkill = chr.Skills.GetValue(Weapon.Skill);
				var defSkill = Victim.Level * 5;

				if (defSkill > weaponSkill)
				{
					chance -= 0.2f * (defSkill - weaponSkill);
				}
				// else: no change (mobs def is smaller than player's weapon skill)
			}
			if (chance > 10000)
			{
				return 10000;
			}
			return (int)chance * 100;
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
		/// TODO: Merge with Unit.CalcParryChance
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
		/// TODO: Merge with Unit.CalcDodgeChance
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

		internal void Reset(Unit attacker, Unit target, IWeapon weapon, int totalDamage)
		{
			Attacker = attacker;
			Victim = target;
			Weapon = weapon;
			Damage = totalDamage;
		}

		internal void OnFinished()
		{
			if (Attacker != null && Victim is NPC)
			{
				((NPC)(Victim)).ThreatCollection.AddNew(Attacker);
			}
			Victim = null;
			SpellEffect = null;
		}

		public override string ToString()
		{
			return string.Format("Attacker: {0}, Target: {1}, Spell: {2}, Damage: {3}", Attacker, Victim,
				SpellEffect != null ? SpellEffect.Spell : null, Damage);
		}
	}

	public interface IAttackModifier
	{
		/// <summary>
		/// Called before hit chance, damage etc is determined.
		/// This is not used for Spell attacks, since those only have a single "stage".
		/// NOT CURRENTLY IMPLEMENTED
		/// </summary>
		void ModPreAttack(DamageAction action);

		/// <summary>
		/// Called when the strike only depends on whether it can be resisted
		/// </summary>
		void ModAttack(DamageAction action);
	}
}
