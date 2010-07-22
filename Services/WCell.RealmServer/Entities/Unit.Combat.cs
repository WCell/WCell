using System;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core.Timers;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Variables;
using WCell.Util.NLog;

namespace WCell.RealmServer.Entities
{
	// TODO: Combat mode triggered when pet starts combat - Complete, needs testing
	public partial class Unit
	{
		#region Global Variables
		// <summary>
		// This is added to the CombatReach of all Units
		// </summary>
		//public static float BaseAttackReach = 1f;

		/// <summary>
		/// Default base-range in which a mob will aggro (in yards).
		/// Also see <see cref="AggroRangePerLevel"/>
		/// </summary>
		public static float AggroBaseRangeDefault = 20;

		/// <summary>
		/// Amount of yards to add to the <see cref="AggroBaseRangeDefault"/> per level difference.
		/// </summary>
		public static float AggroRangePerLevel = 1;

		/// <summary>
		/// Mobs with a distance >= this will not start aggressive actions
		/// </summary>
		public static float AggroMaxRangeDefault = 45;

		private static float aggroMinRangeDefault = 5;

		/// <summary>
		/// Mobs within this range will *definitely* aggro
		/// </summary>
		public static float AggroMinRangeDefault
		{
			get { return aggroMinRangeDefault; }
			set
			{
				aggroMinRangeDefault = value;
				AggroMinRangeSq = value * value;
			}
		}

		public static float AggroRangeNPCs = 10f;

		[NotVariable]
		public static float AggroMinRangeSq = aggroMinRangeDefault * aggroMinRangeDefault;

		/// <summary>
		/// Used to determine melee distance
		/// </summary>
		public static float DefaultMeleeDistance = 3f;

		/// <summary>
		/// Used to determine ranged attack distance
		/// </summary>
		public static float DefaultRangedDistance = 40f;
		#endregion

		/// <summary>
		/// Times our melee and ranged attacks
		/// </summary>
		protected TimerEntry m_attackTimer;
		/// <summary>
		/// whether this Unit is currently in Combat-mode (effects regeneration etc).
		/// </summary>
		protected bool m_isInCombat;

		/// <summary>
		/// whether this Unit is currently actively fighting.
		/// </summary>
		protected bool m_isFighting;

		protected int m_lastCombatTime;

		protected DamageAction m_DamageAction;

		protected int m_extraAttacks;

		/// <summary>
		/// The Environment.TickCount of your last strikes
		/// </summary>
		protected int m_lastStrike, m_lastOffhandStrike;

		protected Spell m_AutorepeatSpell;

		/// <summary>
		/// A pending SpellCast to Impact on next combat hit
		/// </summary>
		protected internal SpellCast m_pendingCombatAbility;

		/// <summary>
		/// Recycled AttackState (not actually relevant)
		/// </summary>
		internal DamageAction DamageAction
		{
			get
			{
				return m_DamageAction;
			}
			set
			{
				m_DamageAction = value;
			}
		}

		/// <summary>
		/// The last time when this Unit was still actively Fighting
		/// </summary>
		public int LastCombatTime
		{
			get { return m_lastCombatTime; }
			set { m_lastCombatTime = value; }
		}

		/// <summary>
		/// While in combat, this method will reset the current swing delay (swing timer is reset)
		/// </summary>
		public void ResetSwingDelay()
		{
			m_lastCombatTime = Environment.TickCount;
		}

		/// <summary>
		/// The spell that is currently being triggered automatically by the CombatTimer
		/// </summary>
		public Spell AutorepeatSpell
		{
			get { return m_AutorepeatSpell; }
			set
			{
				if (m_AutorepeatSpell != value)
				{
					m_AutorepeatSpell = value;
					if (value != null)
					{
						if (value.IsRangedAbility)
						{
							SheathType = SheathType.Ranged;
						}
					}
					else
					{
						SheathType = SheathType.Melee;
					}
				}
			}
		}

		/// <summary>
		/// A pending ability waiting to be executed upon next hit
		/// </summary>
		public SpellCast PendingCombatAbility
		{
			get { return m_pendingCombatAbility; }
		}

		/// <summary>
		/// Amount of extra attacks to hit on next thit
		/// </summary>
		public int ExtraAttacks
		{
			get { return m_extraAttacks; }
			set { m_extraAttacks = value; }
		}

		/// <summary>
		/// Adds damage mods to the given AttackAction
		/// </summary>
		public virtual void AddAttackMods(DamageAction action)
		{
			if (action.Victim is NPC && m_dmgBonusVsCreatureTypePct != null)
			{
				var bonus = m_dmgBonusVsCreatureTypePct[(int) ((NPC) this).Entry.Type];
				action.Damage += (bonus*action.Damage + 50)/100;
			}
			foreach (var mod in AttackEventHandlers)
			{
				mod.OnAttack(action);
			}
		}

		/// <summary>
		/// Adds damage mods to the given AttackAction
		/// </summary>
		public virtual void AddDefenseMods(DamageAction action)
		{
			foreach (var mod in AttackEventHandlers)
			{
				mod.OnDefend(action);
			}
		}

		/// <summary>
		/// Adds damage mods to the given AttackAction
		/// </summary>
		public virtual int AddHealingMods(int healValue, SpellEffect effect, DamageSchool school)
		{
			return healValue;
		}

		/// <summary>
		/// Pending combat abilities are triggered as SpellCast but will impact
		/// upon next Strike in combat (eg Heroic Strike).
		/// </summary>
		public void CancelPendingAbility()
		{
			if (m_pendingCombatAbility != null)
			{
				m_pendingCombatAbility.Cancel();
				m_pendingCombatAbility = null;
			}
		}

		internal DamageAction GetUnusedAction()
		{
			if (m_DamageAction == null || m_DamageAction.IsInUse)
			{
				return new DamageAction(this);
			}
			return m_DamageAction;
		}

		#region Standard Attack

		/// <summary>
		/// Use the given weapon to strike
		/// </summary>
		/// <param name="weapon"></param>
		public void Strike()
		{
			Strike(MainWeapon);
		}

		/// <summary>
		/// Use the given weapon to strike
		/// </summary>
		/// <param name="weapon"></param>
		public void Strike(IWeapon weapon)
		{
			var action = GetUnusedAction();

			Unit target;
			if (m_pendingCombatAbility != null)
			{
				target = m_pendingCombatAbility.Selected as Unit;
				if (target == null)
				{
					log.Warn("{0} tried to use Combat ability {1} without having a target selected.", this, m_pendingCombatAbility);
					m_pendingCombatAbility.Cancel();
					m_pendingCombatAbility = null;
					return;
				}
			}
			else
			{
				target = m_target;
			}

			Strike(weapon, action, target);
		}

		/// <summary>
		/// Do a single attack using the given weapon on the given target.
		/// </summary>
		/// <param name="weapon"></param>
		/// <param name="action"></param>
		public void Strike(Unit target)
		{
			Strike(MainWeapon, target);
		}

		/// <summary>
		/// Do a single attack using the given weapon on the given target.
		/// </summary>
		/// <param name="weapon"></param>
		/// <param name="action"></param>
		public void Strike(IWeapon weapon, Unit target)
		{
			Strike(weapon, GetUnusedAction(), target);
		}

		/// <summary>
		/// Do a single attack using the given Weapon and AttackAction.
		/// </summary>
		/// <param name="weapon"></param>
		/// <param name="action"></param>
		public void Strike(DamageAction action, Unit target)
		{
			Strike(MainWeapon, action, target);
		}

		/// <summary>
		/// Do a single attack using the given Weapon and AttackAction.
		/// </summary>
		/// <param name="weapon"></param>
		/// <param name="action"></param>
		public void Strike(IWeapon weapon, DamageAction action, Unit target)
		{
			if (!target.IsInWorld)
			{
				return;
			}

			if (weapon == null)
			{
				log.Error("Trying to strike without weapon: " + this);
				return;
			}

			//if (IsMovementConrolled)
			//{
			//    // stop running when landing a hit
			//    m_Movement.Stop();
			//}

			target.IsInCombat = true;

			action.Victim = target;
			action.Attacker = this;
			action.Weapon = weapon;

			if (m_pendingCombatAbility != null && m_pendingCombatAbility.IsCasting)
			{
				// Pending combat ability
				var ability = m_pendingCombatAbility;

				// send pending animation
				if (ability.IsInstant)
				{
					m_pendingCombatAbility.SendCastStart();
				}

				if (!ability.Spell.IsRangedAbility)
				{
					m_pendingCombatAbility.CheckHitAndSendSpellGo(true);
				}

				// prevent the ability from cancelling itself
				m_pendingCombatAbility = null;

				action.Schools = ability.Spell.SchoolMask;
				action.SpellEffect = ability.Spell.Effects[0];

				if (ability.Targets.Count > 0)
				{
					action.IsDot = false;

					// AoE spell
					foreach (var targ in ability.Targets)
					{
						var dmg = GetWeaponDamage(weapon, ability);
						action.Reset(this, (Unit)targ, weapon, dmg);
						action.DoAttack();
						if (ability.Spell.IsDualWieldAbility)
						{
							action.Reset(this, (Unit)targ, weapon, Utility.Random((int)MinOffHandDamage, (int)MaxOffHandDamage + 1));
							action.DoAttack();
						}
					}
				}
				else
				{
					// single target

					// calc damage
					action.Damage = GetWeaponDamage(weapon, m_pendingCombatAbility);
					if (!action.DoAttack() &&
						ability.Spell.AttributesExC.HasFlag(SpellAttributesExC.RequiresTwoWeapons))
					{
						// missed and is not attacking with both weapons -> don't trigger spell
						CancelPendingAbility();
						return;
					}
				}

				// Impact and trigger remaining effects (if not cancelled)
				if (!ability.Spell.IsRangedAbility)
				{
					ability.Impact(ability.Spell.IsOnNextStrike);
				}
			}
			else
			{
				// no combat ability
				m_extraAttacks += 1;
				do
				{
					// calc damage
					action.Damage = GetWeaponDamage(weapon, m_pendingCombatAbility);

					action.Schools = weapon.Damages.AllSchools();
					if (action.Schools == DamageSchoolMask.None)
					{
						action.Schools = DamageSchoolMask.Physical;
					}

					// normal attack
					action.DoAttack();
				} while (--m_extraAttacks > 0);
			}
			action.OnFinished();
		}

		/// <summary>
		/// Returns random damage for the given weapon
		/// </summary>
		public int GetWeaponDamage(IWeapon weapon, SpellCast pendingAbility)
		{
			int damage;
			if (weapon == m_offhandWeapon)
			{
				damage = Utility.Random((int)MinOffHandDamage, (int)MaxOffHandDamage + 1);
			}
			else
			{
				if (weapon.IsRanged)
				{
					damage = Utility.Random((int)MinRangedDamage, (int)MaxRangedDamage + 1);
				}
				else
				{
					damage = Utility.Random((int)MinDamage, (int)MaxDamage + 1);
				}
			}

			if (pendingAbility != null && pendingAbility.IsCasting)
			{
				// get boni, damage and let the Spell impact
				var multiplier = 100;

				foreach (var effectHandler in pendingAbility.Handlers)
				{
					if (effectHandler.Effect.IsStrikeEffectFlat)
					{
						damage += effectHandler.CalcEffectValue();
					}
					else if (effectHandler.Effect.IsStrikeEffectPct)
					{
						multiplier += effectHandler.CalcEffectValue();
					}
				}
				damage = (damage * multiplier + 50) / 100;
			}
			return damage;
		}
		#endregion

		#region Spells
		/// <summary>
		/// Does spell-damage to this Unit
		/// </summary>
		public void DoSpellDamage(Unit attacker, SpellEffect effect, int dmg)
		{
			DamageSchool school;
			if (effect != null)
			{
				school = GetLeastResistant(effect.Spell);
				if (effect.Spell.DamageIncreasedByAP)
				{
					int ap;
					if (effect.Spell.IsRangedAbility)
					{
						ap = TotalRangedAP;
					}
					else
					{
						ap = TotalMeleeAP;
					}

					dmg += (ap + 7) / 14;	// round
				}
			}
			else
			{
				school = DamageSchool.Physical;
			}

			if (IsEvading || IsImmune(school) || IsInvulnerable || !IsAlive)
			{
				return;
			}

			var action = attacker != null ? attacker.m_DamageAction : m_DamageAction;
			if (action == null || action.IsInUse)
			{
				// currently in use
				action = new DamageAction(attacker);
			}
			else
			{
				action.Attacker = attacker;
				action.HitFlags = 0;
				action.VictimState = 0;
				action.Weapon = null;
			}

			if (effect != null)
			{
				action.UsedSchool = school;
				action.Schools = effect.Spell.SchoolMask;
				action.IsDot = effect.IsPeriodic;
			}
			else
			{
				action.UsedSchool = DamageSchool.Physical;
				action.Schools = DamageSchoolMask.Physical;
				action.IsDot = false;
			}

			action.Damage = dmg;
			action.ResistPct = GetResistChancePct(this, action.UsedSchool);
			action.Absorbed = 0;

			action.Victim = this;

			if (attacker != null)
			{
				if (effect != null && !action.IsDot && !effect.Spell.AttributesExB.HasFlag(SpellAttributesExB.CannotCrit) &&
					attacker.CalcSpellCritChance(this, action.UsedSchool, action.ResistPct, effect.Spell) > Utility.Random(0f, 100f))
				{
					action.IsCritical = true;
					action.SetCriticalDamage();
				}
				else
				{
					action.IsCritical = false;
				}

				AddDefenseMods(action);
				attacker.AddAttackMods(action);
			}


			action.Absorbed += Absorb(action.UsedSchool, action.Damage);
			action.Resisted = (int)Math.Round(action.Damage * action.ResistPct / 100);
			action.Blocked = 0; // TODO: Deflect
			action.SpellEffect = effect;

			//TODO: figure this out: However, when spells do only damage, it's not just a full hit or full miss situation. Pure damage spells can be resisted for 0%, 25%, 50%, 75%, or 100% of their regular damage. 

			DoRawDamage(action);
			CombatLogHandler.SendMagicDamage(action);
			action.OnFinished();
			//Your average resistance can still be anywhere betweeen 0% and 75%. If your average resistance is maxed out, then there's a really good chance of having 75% of the spell's damage be resisted. 
			//There's also a fairly good chance of having 100% of the spell's damage be resisted, a slightly lower chance of 50% of its damage being resisted, a small chances of only 25%, or even 0% of the damage being resisted. 
			//It's a weighted average. Visualize it as a bell curve around your average resistance.
		}

		/// <summary>
		/// Calculates the chance to hit the given defender with a spell of the given school
		/// </summary>
		/// <returns>The effective hitchance</returns>
		public float CalcSpellHitChance(Unit defender, DamageSchool dmgSchool, float resistChance)
		{
			float res = GetResistance(dmgSchool);

			if (this is Character && defender is Character) //pvp has diffrent values compared to pve
			{
				res += (float)(99.24f / (1 + 0.0281f * Math.Log((0.516f * (defender.Level - Level)), MathUtil.E)));
			}
			else
			{
				res += (float)(98.77f / (1 + 0.0215f * Math.Log((0.6862f * (defender.Level - Level)), MathUtil.E)));
			}

			res *= resistChance;

			res -= defender.GetAttackerSpellHitChanceMod(dmgSchool);

			if (res < 1)
			{
				res = 1;
			}

			return res;
		}

		public float CalcSpellCritChance(Unit defender, DamageSchool dmgSchool, float resistPct, Spell spell)
		{
			var chance = GetSpellCritChance(dmgSchool);
			if (this is Character)
			{
				var chr = (Character)this;
				chance += chr.PlayerSpells.GetModifierFlat(SpellModifierType.CritChance, spell);
			}
			chance -= defender.GetResiliencePct();
			return chance;
		}

		/// <summary>
		/// Calculates this Unit's chance to resist the given school.
		/// Value is between 0 and 100
		/// </summary>
		public float GetResistChancePct(Unit attacker, DamageSchool school)
		{
			int attackerLevel;
			var res = GetResistance(school);
			if (attacker != null)
			{
				attackerLevel = Math.Max(1, attacker.Level);
				res -= attacker.GetTargetResistanceMod(school);
			}
			else
			{
				attackerLevel = 1;
			}

			res = Math.Max(0, res);

			var resist = (res / (attackerLevel * 5f)) * 0.75f;

			if (resist > 75)
			{
				resist = 75f;
			}

			if (resist < 0)
			{
				resist = 0;
			}

			return resist;
		}

		/// <summary>
		/// Returns percent * 100 of chance to dodge
		/// Modified by expertise.
		/// </summary>
		public int CalcDodgeChance(WorldObject attacker)
		{
			float dodgeChance;

			if (this is Character)
			{
				var def = (Character)this;
				dodgeChance = def.DodgeChance;
			}
			else
			{
				dodgeChance = 5;
			}

			if (attacker is Character)
			{
				var atk = (Character)attacker;
				dodgeChance -= atk.Expertise * 0.25f;
			}

			dodgeChance *= 100;
			return (int)dodgeChance;
		}

		/// <summary>
		/// Returns percent * 100 of chance to parry.
		/// Modified by expertise.
		/// </summary>
		/// <returns></returns>
		public int CalcParryChance(Unit attacker)
		{
			float parryChance = 0;

			if (this is Character)
			{
				var def = (Character)this;
				parryChance += def.ParryChance;
			}
			else
			{
				return 5;
			}

			if (attacker is Character)
			{
				var atk = (Character)attacker;
				parryChance -= atk.Expertise * 0.25f;
			}

			parryChance *= 100;
			return (int)parryChance;
		}

		public float GetResiliencePct()
		{
			float resiliencePercentage = 0;

			if (this is Character)
			{
				var def = this as Character;
				float resilience = def.GetCombatRatingMod(CombatRating.MeleeResilience);
				resiliencePercentage += resilience / /*GameTables.ResilienceRating[def.Level]; */
										GameTables.GetCRTable(CombatRating.MeleeResilience).GetMax((uint)def.Level - 1);
			}
			else
			{
				resiliencePercentage = 0;
			}

			return resiliencePercentage;
		}

		public virtual float CalcCritDamage(float dmg, Unit victim, SpellEffect effect)
		{
			return dmg * 2;
		}

		/// <summary>
		/// Crit chance from 0 to 100
		/// </summary>
		/// <returns></returns>
		public virtual float CalcCritChanceBase(Unit victim, SpellEffect effect, IWeapon weapon)
		{
			return 5;		// mobs got 5% by default
		}

		/// <summary>
		/// whether this Unit completely resists a spell
		/// </summary>
		public bool CheckResist(Unit attacker, DamageSchool school, SpellMechanic mechanic)
		{
			if (Utility.Random(0f, 100f) <
				GetMechanicResistance(mechanic) -
				GetAttackerSpellHitChanceMod(school) +
				GetResistChancePct(attacker, school))
				return true;

			return false;
		}

		/// <summary>
		/// whether this Unit resists a debuff (independent on resistances)
		/// </summary>
		public bool CheckDebuffResist(int attackerLevel, DamageSchool school)
		{
			if (Utility.Random(0, 100) < 
				GetDebuffResistance(school) -
				GetAttackerSpellHitChanceMod(school))
				return true;

			return false;
		}
		#endregion

		#region Combat
		/// <summary>
		/// whether this Unit is currently in Combat.
		/// If it is actively fighting (rather than being forced into CombatMode),
		/// IsFighting must be true.
		/// </summary>
		public bool IsInCombat
		{
			get { return m_isInCombat; }
			set
			{
				if (m_isInCombat == value) return;

				if (m_isInCombat = value)
				{
					UnitFlags |= UnitFlags.Combat;

					StandState = StandState.Stand;

					// remove non-combat Auras
					m_auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);

					// cancel non-combat spellcasts
					if (m_spellCast != null)
					{
						var spell = m_spellCast.Spell;
						if (spell != null &&
							spell.RequiresCasterOutOfCombat)
						{
							m_spellCast.Cancel();
						}
					}

					// Pet entered combat, so will it's master.
					if (Master != this)
					{
						Master.IsInCombat = true;
					}

					OnEnterCombat();
					m_attackTimer.Start(1);
				}
				else
				{
					if (this is NPC)
					{
						// NPCs cannot remain in combat-mode without fighting
						IsFighting = false;
					}
					CancelPendingAbility();
					UnitFlags &= ~UnitFlags.Combat;
					m_attackTimer.Stop();

					OnLeaveCombat();
				}

				this.UpdatePowerRegen();
			}
		}


		/// <summary>
		/// Indicates whether this Unit is currently trying to swing at its target.
		/// If <c>IsInCombat</c> is set but Unit is not fighting,
		/// it will leave Combat mode after <c>CombatDeactivationDelay</c> without combat
		/// activity, if not under the influence of a debuff.
		/// </summary>
		public bool IsFighting
		{
			get { return m_isFighting; }
			set
			{
				if (m_isFighting != value)
				{
#pragma warning disable 665
					if (m_isFighting = value)
#pragma warning restore 665
					{
						if (m_target == null)
						{
							LogUtil.ErrorException(new Exception(string.Format(
								"{0} ({1}) is trying to fight without valid Target.",
								this, m_brain != null ? m_brain.State.ToString() : "")));
							m_isFighting = false;
							return;
						}

						Dismount();

						if (this is NPC)
						{
							// NPCs are always in combat mode when starting to attack
							IsInCombat = true;
						}
						else
						{
							m_attackTimer.Start(1);
						}
						//m_lastFightTime = Environment.TickCount;

						//CombatTick(0);
					}
					else
					{
						CancelPendingAbility();

						CombatHandler.SendCombatStop(this, m_target, 0);
					}
				}
			}
		}

		/// <summary>
		/// Tries to land a mainhand hit + maybe offhand hit on the current Target
		/// </summary>
		protected virtual void CombatTick(float timeElapsed)
		{
			// if currently casting a spell, skip this
			if (IsUsingSpell)
			{
				m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
				return;
			}

			if (!CheckCombatState())
			{
				if (m_isInCombat)
				{
					// if still in combat - check soon again
					m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
				}
				return;
			}

			if (!CanDoHarm || !CanMelee)
			{
				m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
				return;
			}

			var target = m_target;

			var now = Environment.TickCount;
			var usesOffHand = UsesDualWield;

			var isRanged = m_AutorepeatSpell != null && m_AutorepeatSpell.IsRangedAbility;
			var mainHandDelay = m_lastStrike + (isRanged ? RangedAttackTime : MainHandAttackTime) - now;
			int offhandDelay;

			var strikeReady = mainHandDelay <= 0;
			bool offHandReady;
			if (usesOffHand)
			{
				offhandDelay = m_lastOffhandStrike + OffHandAttackTime - now;
				offHandReady = true;
			}
			else
			{
				offhandDelay = int.MaxValue;
				offHandReady = false;
			}

			// try to strike
			if (strikeReady || offHandReady)
			{
				if (this is Character && !target.IsInFrontOf(this))
				{
					CombatHandler.SendAttackSwingBadFacing(this as Character);
				}
				else
				{
					var distanceSq = GetDistanceSq(target);
					if (strikeReady)
					{
						var weapon = isRanged ? m_RangedWeapon : m_mainWeapon;
						if (weapon != null)
						{
							if (IsInAttackRangeSq(weapon, target, distanceSq))
							{
								if (m_AutorepeatSpell != null)
								{
									// Auto-shot (only when not running)
									if (!IsMoving)
									{
										SpellCast.TargetFlags = SpellTargetFlags.Unit;
										SpellCast.Selected = target;
										SpellCast.Start(m_AutorepeatSpell, false);
										m_lastStrike = now;
										mainHandDelay += OffHandAttackTime;
									}
								}
								else if (!isRanged)
								{
									Strike(weapon);
									m_lastStrike = now;
									mainHandDelay += MainHandAttackTime;
								}
							}
							else
							{
								// too far away
								if (this is Character)
								{
									CombatHandler.SendAttackSwingNotInRange(this as Character);
								}
								else if (this is NPC)
								{
									Brain.OnCombatTargetOutOfRange();
								}
							}
						}
					}

					if (offHandReady)
					{
						if (IsInAttackRangeSq(m_offhandWeapon, target, distanceSq))
						{
							Strike(m_offhandWeapon);
							m_lastOffhandStrike = now;
							offhandDelay = OffHandAttackTime;
						}
					}
				}
			}

			// determine the next strike
			int delay;

			strikeReady = mainHandDelay <= 0;
			offHandReady = usesOffHand && offhandDelay <= 0;

			if (strikeReady)
			{
				// mainhand is ready but not in reach
				if (offHandReady || !usesOffHand)
				{
					// mainhand is ready and offhand is either also ready or not present
					delay = DamageAction.DefaultCombatTickDelay;
				}
				else
				{
					// mainhand is ready and offhand is still waiting
					delay = Math.Min(DamageAction.DefaultCombatTickDelay, offhandDelay);
				}
			}
			else
			{
				// mainhand is not ready
				if (offHandReady)
				{
					// mainhand is not ready but offhand is ready
					delay = Math.Min(DamageAction.DefaultCombatTickDelay, mainHandDelay);
				}
				else
				{
					if (usesOffHand)
					{
						// mainhand and offhand are both not ready
						delay = Math.Min(offhandDelay, mainHandDelay);
					}
					else
					{
						// mainhand is not ready and there is no offhand
						delay = mainHandDelay;
					}
				}
			}

			m_attackTimer.Start(delay / 1000f);
		}

		/// <summary>
		/// Checks whether the Unit can attack.
		/// Also deactivates combat mode, if unit has left combat for long enough.
		/// TODO: Cannot leave combat state if Pet is attacking
		/// </summary>
		protected virtual bool CheckCombatState()
		{
			if (m_comboTarget != null && !m_comboTarget.IsAlive)
			{
				ResetComboPoints();
			}

			if (m_target == null || !CanHarm(m_target))
			{
				IsFighting = false;
			}
			else if (!CanSee(m_target))
			{
				if (this is Character)
				{
					CharacterHandler.SendClearTarget(((Character)this), m_target);
				}
				Target = null;
				IsFighting = false;
			}
			else if (!CanDoHarm)
			{
				return false;
			}

			return m_isFighting;
		}

		/// <summary>
		/// Resets the attack timer to delay the next strike by the current weapon delay,
		/// if Unit is fighting.
		/// </summary>
		public void ResetAttackTimer()
		{
			if (m_isFighting)
			{
				// figure out delay
				int delay;
				if (m_offhandWeapon != null)
				{
					delay = Math.Min(MainHandAttackTime, OffHandAttackTime);
				}
				else
				{
					delay = MainHandAttackTime;
				}

				if (m_RangedWeapon != null && m_RangedWeapon.IsRanged)
				{
					delay = Math.Min(delay, RangedAttackTime);
				}

				// start
				m_attackTimer.Start(delay / 1000f);
			}
			else
			{
				m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
			}
		}

		/// <summary>
		/// Is called whenever this Unit enters Combat mode
		/// </summary>
		protected virtual void OnEnterCombat()
		{
			StandState = StandState.Stand;
			m_lastCombatTime = Environment.TickCount;
			if (m_brain != null)
			{
				m_brain.OnEnterCombat();
			}
		}

		/// <summary>
		/// Is called whenever this Unit leaves Combat mode
		/// </summary>
		protected virtual void OnLeaveCombat()
		{
			ResetComboPoints();
			if (m_brain != null)
			{
				m_brain.OnLeaveCombat();
			}
		}

		/// <summary>
		/// Is called whenever this Unit receives any kind of damage
		/// 
		/// TODO: There is a small chance with each hit by your weapon that it will lose 1 durability point.
		/// TODO: There is a small chance with each spell cast that you will lose 1 durability point to your weapon. 
		/// TODO: There is a small chance with each hit absorbed by your armor that it will lose 1 durability point.
		/// </summary>
		protected internal virtual void OnDamageAction(IDamageAction action)
		{
			if (action is DamageAction && action.Attacker != null)
			{
				var aaction = (DamageAction)action;

				// Get the flags now, so they won't be changed by anything that happens afterwards
				var attackerProcTriggerFlags = action.AttackerProcTriggerFlags;
				var targetProcTriggerFlags = action.TargetProcTriggerFlags;

				if (IsAlive)
				{
					// aura states
					if (aaction.VictimState == VictimState.Parry)
					{
						AuraState |= AuraStateMask.Parry | AuraStateMask.DodgeOrBlockOrParry;
						if (aaction.Victim.PowerType == PowerType.Rage)
						{
							RageGenerator.GenerateTargetRage(aaction);
						}
					}
					else if (aaction.VictimState == VictimState.Block ||
							 aaction.VictimState == VictimState.Dodge)
					{
						AuraState |= AuraStateMask.DodgeOrBlockOrParry;
					}
					else
					{
						// Unset
						AuraState &= ~AuraStateMask.Parry | AuraStateMask.DodgeOrBlockOrParry;
					}
				}
				else
				{
					AuraState = AuraStateMask.None;
				}

				if (action.ActualDamage > 0)
				{
					if (!action.IsDot)
					{
						var attacker = action.Attacker;
						var weapon = action.Weapon;
						var weaponAttack = weapon != null && !attacker.IsPvPing;

						// Remove damage-sensitive Auras
						m_auras.RemoveByFlag(AuraInterruptFlags.OnDamage);
						attacker.m_lastCombatTime = Environment.TickCount;

						if (attacker is Character && weaponAttack)
						{
							((Character)attacker).Skills.GainWeaponSkill(action.Victim.Level, weapon);
						}

						// stand up when hit
						StandState = StandState.Stand;

						// Generate Rage
						if (attacker.IsAlive && attacker.PowerType == PowerType.Rage)
						{
							RageGenerator.GenerateAttackerRage(aaction);
						}

						if (IsAlive)
						{
							if (PowerType == PowerType.Rage)
							{
								RageGenerator.GenerateTargetRage(aaction);
							}

							// aggro'd -> Enter combat mode and update combat-time
							IsInCombat = true;
							m_lastCombatTime = Environment.TickCount;

							// during pvp one does not gain any weapon skill
							if (this is Character)
							{
								if (weaponAttack)
								{
									((Character)this).Skills.GainDefenseSkill(action.Attacker.Level);
								}
							}
						}

						// Pushback SpellCast
						if (IsUsingSpell)
						{
							if (SpellCast.Spell.InterruptFlags.HasFlag(InterruptFlags.OnTakeDamage))
							{
								SpellCast.Cancel();
							}
							else
							{
								SpellCast.Pushback();
							}
						}
					}

					if (action.Weapon != OffHandWeapon)
					{
						// proc (if not offhand)
						action.Attacker.Proc(attackerProcTriggerFlags, this, action, true);
						Proc(targetProcTriggerFlags, action.Attacker, action, false);	
					}
				}
			}
		}
		#endregion

		#region Range Checks
		public float GetAttackRange(IWeapon weapon, Unit target)
		{
			return weapon.MaxRange + CombatReach + target.CombatReach;
		}

		public float GetBaseAttackRange(Unit target)
		{
			return MaxAttackRange + target.CombatReach;
		}

		public bool IsInBaseAttackRange(Unit target)
		{
			var distSq = GetDistanceSq(target);
			var range = GetBaseAttackRange(target);
			return distSq <= range * range;
		}

		public bool IsInBaseAttackRangeSq(Unit target, float distSq)
		{
			var range = GetBaseAttackRange(target);
			return distSq <= range * range;
		}

		public float GetMinAttackRange(IWeapon weapon, Unit target)
		{
			return weapon.MinRange;
		}

		/// <summary>
		/// Returns whether the given Object is in range of Main- or Extra (Ranged)- weapon
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool IsInAttackRange(Unit target)
		{
			var distSq = GetDistanceSq(target);
			return IsInAttackRangeSq(target, distSq);
		}

		/// <summary>
		/// Whether the target is in reach to be attacked
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool CanReachForCombat(Unit target)
		{
			return MayMove || IsInAttackRange(target);
		}

		public bool IsInAttackRangeSq(Unit target, float distSq)
		{
			if (!CanMelee)
			{
				if (this is NPC)
				{
					return IsInBaseAttackRangeSq(target, distSq);
				}
				return false;
			}
			if (UsesRangedWeapon)
			{
				if (IsInAttackRangeSq(m_RangedWeapon, target, distSq))
				{
					return true;
				}
			}

			return IsInMeleeAttackRangeSq(m_mainWeapon, target, distSq);
		}

		public bool IsInMaxRange(Spell spell, WorldObject target)
		{
			var max = GetSpellMaxRange(spell, target);
			var distSq = GetDistanceSq(target);
			return distSq <= max * max;
		}

		public bool IsInSpellRange(Spell spell, WorldObject target)
		{
			var max = GetSpellMaxRange(spell, target);
			var distSq = GetDistanceSq(target);
			if (spell.Range.MinDist > 0)
			{
				var min = spell.Range.MinDist; //GetSpellMinRange(spell.Range.MinDist, target);
				if (distSq < min * min)
				{
					return false;
				}
			}

			return distSq <= max * max;
		}

		/// <summary>
		/// Melee has no Min range
		/// </summary>
		public bool IsInMeleeAttackRangeSq(IWeapon weapon, Unit target, float distSq)
		{
			var max = GetAttackRange(weapon, target);
			return distSq <= max * max;
		}

		public bool IsInAttackRangeSq(IWeapon weapon, Unit target, float distSq)
		{
			var max = GetAttackRange(weapon, target);
			if (weapon.IsRanged)
			{
				var min = GetMinAttackRange(weapon, target);
				if (distSq < min * min)
				{
					return false;
				}
			}
			return distSq <= max * max;
		}

		public bool IsInRange(SimpleRange range, WorldObject obj)
		{
			var distSq = GetDistanceSq(obj);
			return (distSq <= range.MaxDist * range.MaxDist &&
					 (range.MinDist < 1 || distSq >= range.MinDist * range.MinDist));
		}

		public float AggroBaseRange
		{
			get { return AggroBaseRangeDefault /*+ CombatReach*/ + BoundingRadius; }
		}

		public float GetAggroRange(Unit victim)
		{

			return Math.Max(AggroBaseRange + ((Level - victim.Level) * AggroRangePerLevel), AggroMinRangeDefault);
		}

		public float GetAggroRangeSq(Unit victim)
		{
			//if (unit.BelongsToPlayer)
			//{
			var range = GetAggroRange(victim);
			return range * range;
			//}
			//return AggroRangeNPCs * AggroRangeNPCs;
		}
		#endregion

		#region Combat and Hostility Checks
		/// <summary>
		/// Checks for hostility etc
		/// 
		/// TODO: Restrict interference in Duels
		/// </summary>
		public SpellFailedReason CanCastSpellOn(Unit target, Spell spell)
		{
			//return spell.IsHarmful == IsHostileWith(target) && !target.IsImmune(spell.Mechanic)
			//    && !target.IsImmune(spell.School);
			var canHarm = CanHarm(target);
			if ((canHarm && !spell.HasHarmfulEffects) || (!canHarm && !spell.HasBeneficialEffects))
			{
				return canHarm ? SpellFailedReason.TargetFriendly : SpellFailedReason.TargetEnemy;
			}
			return SpellFailedReason.Ok;
		}

		#endregion
	}
}