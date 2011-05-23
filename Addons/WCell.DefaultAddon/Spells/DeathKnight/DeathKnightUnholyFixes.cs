using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;
using WCell.Util;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	public class DeathKnightUnholyFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Unholy Virulence has some incorrect restrictions
			SpellLineId.DeathKnightUnholyVirulence.Apply(spell =>
			{
				// Improves all Spells
				spell.GetEffect(AuraType.ModSpellHitChance).MiscValue = 0;
			});

			FixCorpseExplosion();

			FixBloodCakedStrike();
			FixImpurity();

            //// Dirge is only triggered by "Death Strike, Plague Strike and Scourge Strike"
            //SpellLineId.DeathKnightUnholyDirge.Apply(spell =>
            //{
            //    spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

            //    var effect = spell.GetEffect(AuraType.ProcTriggerSpellWithOverride);
            //    effect.ClearAffectMask();
            //    effect.AddAffectingSpells(SpellLineId.DeathKnightDeathStrike, SpellLineId.DeathKnightPlagueStrike, SpellLineId.DeathKnightUnholyScourgeStrike);
            //});

			SpellLineId.DeathKnightUnholyMagicSuppression.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.AddModifierFlat);
				effect.ClearAffectMask();
				effect.AddAffectingSpells(SpellLineId.DeathKnightAntiMagicShell);		// only affects Anti Magic Shell
			});

			FixRageOfRivendare();
			FixUnholyPresence();

			// Improved Unholy Presence also applies the s1% movement speed to the other two presences and increases rune cooldown
			SpellLineId.DeathKnightUnholyImprovedUnholyPresence.Apply(spell =>
			{
				var retainEffect = spell.GetEffect(AuraType.Dummy);
				retainEffect.AuraType = AuraType.ModIncreaseSpeed;
				retainEffect.AddRequiredActivationAuras(SpellLineId.DeathKnightBloodPresence, SpellLineId.DeathKnightFrostPresence);

				// "your runes finish their cooldowns $s2% faster in Unholy Presence"
				//spell.GetEffect(AuraType.None).AuraEffectHandlerCreator = () => new ModAllRuneCooldownsPercentHandler();
			});

			FixAntiMagicZone();
			FixNecrosis();
			FixAntiMagicShell();

            //// Scourge Strike adds damage per disease on target
            //SpellLineId.DeathKnightUnholyScourgeStrike.Apply(spell =>
            //{
            //    var effect = spell.GetEffect(SpellEffectType.Dummy);
            //    effect.SpellEffectHandlerCreator = (cast, effct) => new WeaponDiseaseDamagePercentHandler(cast, effct);
            //});

			// Desecration has no affect mask
			SpellLineId.DeathKnightUnholyDesecration.Apply(spell =>
			{
				// "Plague Strikes and Scourge Strikes cause the Desecrated Ground effect"
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				effect.SetAffectMask(SpellLineId.DeathKnightPlagueStrike);
			});

			FixDeathStrike();

            //// Desolation has no AffectMask
            //SpellLineId.DeathKnightUnholyDesolation.Apply(spell =>
            //    spell.GetEffect(AuraType.ProcTriggerSpell).SetAffectMask(SpellLineId.DeathKnightBloodStrike));

			FixDeathCoil();

			// Death & Decay simply does periodic damage
			// TODO: "This ability produces a high amount of threat."
			SpellLineId.DeathKnightDeathAndDecay.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy2);
				effect.AuraType = AuraType.PeriodicDamage;
			});

			SpellLineId.DeathKnightRaiseAlly.Apply(spell =>
			{
				// TODO: Find corpse
				// TODO: Mark corpse unusable
				var effect = spell.GetEffect(SpellEffectType.Dummy);
				effect.EffectType = SpellEffectType.TriggerSpell;
				effect.TriggerSpellId = (SpellId)effect.CalcEffectValue();
			});
		}

		#region Death Coil
		private static void FixDeathCoil()
		{
			// Death Coil needs to heal or harm, depending on the target
			SpellLineId.DeathKnightDeathCoil.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.Dummy);
				effect.SpellEffectHandlerCreator = (cast, effct) => new DeathCoilHandler(cast, effct);
			});
		}

		internal class DeathCoilHandler : SpellEffectHandler
		{
			private bool heal;

			public DeathCoilHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			public override SpellFailedReason InitializeTarget(WorldObject target)
			{
				var caster = m_cast.CasterObject;
				if (caster == null) return SpellFailedReason.CasterDead;	// message doesn't matter, if the caster isn't there, since no one will see it

				if (target is NPC && ((NPC)target).Entry.Type == CreatureType.Undead && caster.IsInSameDivision(target))
				{
					heal = true;
				}
				else if (!caster.MayAttack(target))
				{
					return SpellFailedReason.BadTargets;
				}
				return base.InitializeTarget(target);
			}

			protected override void Apply(WorldObject target)
			{
				var caster = m_cast.CasterUnit;

				var unit = (Unit)target;
				if (heal)
				{
					// "healing ${$m1*1.5} damage from a friendly Undead target"
					unit.Heal((CalcEffectValue() * 15 + 5) / 10, caster, Effect);
				}
				else
				{
					// "causing $s1 Shadow damage to an enemy target"
					unit.DealSpellDamage(caster, Effect, CalcEffectValue());
				}
			}

			public override ObjectTypes TargetType
			{
				get { return ObjectTypes.Unit; }
			}
		}
		#endregion

		#region Death Strike
		private static void FixDeathStrike()
		{
			// Death Strike heals for each disease on the target
			SpellLineId.DeathKnightDeathStrike.Apply(spell =>
			{
				spell.AddEffect((cast, effct) => new DeathStrikeHealHandler(cast, effct), ImplicitSpellTargetType.SingleEnemy);
			});
		}

		internal class DeathStrikeHealHandler : SpellEffectHandler
		{
			public DeathStrikeHealHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			protected override void Apply(WorldObject target)
			{
				var caster = m_cast.CasterUnit;
				if (caster != null)
				{
					var unit = (Unit)target;
					// see http://www.wowwiki.com/Death_Strike
					// "heals the Death Knight for up to 5% of maximum health, plus 5% for each disease on the target for a maximum of 15% for two or more diseases."
					var healPctPerDisease = m_cast.Spell.Effects[0].ChainAmplitude;
					var mult = Math.Min(3, 1 + unit.Auras.GetVisibleAuraCount(m_cast.CasterReference, DispelType.Disease));
					var percent = MathUtil.RoundInt(mult * healPctPerDisease);

					// add bonus damage from the Improved Death Strike talent
					var talent = caster.Auras[SpellLineId.DeathKnightBloodImprovedDeathStrike];
					if (talent != null)
					{
						var handler = talent.GetHandler(AuraType.None);
						if (handler != null)
						{
							// Improved Death Strike "increases the healing granted by $s3%."
							percent += (percent * handler.EffectValue) / 100;
						}
					}
					caster.HealPercent(percent, caster, Effect);
				}
			}

			public override ObjectTypes TargetType
			{
				get { return ObjectTypes.Unit; }
			}
		}
		#endregion

		#region Anti Magic Shell
		private static void FixAntiMagicShell()
		{
			SpellLineId.DeathKnightAntiMagicShell.Apply(spell =>
			{
				spell.GetEffect(AuraType.SchoolAbsorb).AuraEffectHandlerCreator = () => new AMSAuraHandler();
			});
		}

		public class AMSAuraHandler : SchoolAbsorbHandler
		{
			protected override void Apply()
			{
				base.Apply();

				// the amount to be absorbed is determined by another effect
				var handler = m_aura.GetHandler(AuraType.LimitAbsorbToCasterMaxHealthPercent);
				var healthPct = handler != null ? handler.EffectValue : 1;

				// "up to a maximum of $s2% of the Death Knight's health"
				RemainingValue = (Owner.Health * healthPct + 50) / 100;
			}
		}
		#endregion

		#region Necrosis
		private static void FixNecrosis()
		{
			// Necrosis deals shadow damage on top of every attack that was performed
            //SpellLineId.DeathKnightUnholyNecrosis.Apply(spell =>
            //{
            //    spell.SchoolMask = DamageSchoolMask.Shadow;		// "Shadow damage"

            //    var effect = spell.GetEffect(AuraType.Dummy);
            //    effect.AuraEffectHandlerCreator = () => new NecrosisDamageHandler();
            //});
		}

		/// <summary>
		/// TODO: The applied damage should not be dependent on the order of 
		/// AttackEventEffectHandler execution (which it currently is).
		/// </summary>
		internal class NecrosisDamageHandler : AttackEventEffectHandler
		{
			public override void OnAttack(DamageAction action)
			{
				var attacker = action.Attacker;		// same as Owner
				var victim = action.Victim;
				var dmg = action.GetDamagePercent(EffectValue);
				victim.AddMessage(() => victim.DealSpellDamage(attacker, m_spellEffect, dmg, false, false));	// may not crit
			}
		}
		#endregion

		#region Anti Magic Zone
		private static void FixAntiMagicZone()
		{
			SpellLineId.DeathKnightUnholyAntiMagicZone.Apply(spell =>
			{
				// "The Anti-Magic Zone lasts for $51052d or until it absorbs ${$51052m1+2*$AP} spell damage."
				spell.SetDuration(0);									// this spell has no duration (the area aura decides that)
				var effect = spell.GetEffect(SpellEffectType.Summon);
				effect.APValueFactor = 2;								// value is used for NPC health
			});

			// The AreaAura of the AMZ must reduce the NPC's health as damage gets absorbed
			SpellHandler.Apply(spell =>
			{
				spell.AttributesExC ^= SpellAttributesExC.PersistsThroughDeath;				// remove when the totem's health is 0
				spell.GetEffect(AuraType.SchoolAbsorb).AuraEffectHandlerCreator = () => new AMZAuraHandler();
			}, SpellId.AntiMagicZone);
		}

		/// <summary>
		/// Make AMZ non-attackable
		/// </summary>
		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void FixAntiMagicZoneNPC()
		{
			var amz = NPCMgr.GetEntry(NPCId.AntiMagicZone);
			amz.UnitFlags |= UnitFlags.NotAttackable | UnitFlags.NotSelectable;
		}

		public class AMZAuraHandler : AttackEventEffectHandler
		{
			/// <summary>
			/// The caster's health is used as value by everony within the zone, where
			/// caster = totem. When it's dead, Aura will get removed.
			/// </summary>
			public int RemainingValue
			{
				get
				{
					var caster = m_aura.CasterUnit;
					if (caster != null)
					{
						return caster.Health;
					}
					return 0;
				}
				set
				{
					var caster = m_aura.CasterUnit;
					if (caster != null)
					{
						caster.Health = value;
					}
				}
			}

			protected override void Remove(bool cancelled)
			{
				var owner = Owner;
				if (owner == m_aura.CasterUnit && !owner.IsPlayer)
				{
					Owner.Delete(); // delete totem when removed
				}
				base.Remove(cancelled);
			}

			public override void OnBeforeAttack(DamageAction action)
			{
			}

			public override void OnAttack(DamageAction action)
			{
			}

			public override void OnDefend(DamageAction action)
			{
				// absorb EffectValue % from the damage
				var absorbed = Math.Min(action.GetDamagePercent(EffectValue), RemainingValue);

				// RemainingValue corresponds to AMZ's health, when it reaches 0, AMZ will be destroyed
				RemainingValue = action.Absorb(absorbed, (DamageSchoolMask)m_spellEffect.MiscValue);
			}
		}
		#endregion

		#region Unholy Presence
		private static void FixUnholyPresence()
		{
			// Unholy Presence toggles a second aura
			SpellLineId.DeathKnightUnholyPresence.Apply(spell =>
			{
				spell.AddAuraEffect(() => new ToggleAuraHandler(SpellId.UnholyPresence));
			});
		}
		#endregion

		#region Rage of Rivendare
		private static void FixRageOfRivendare()
		{
			// Tundra Stalker needs a custom Attack event aura handler & correct effect value
            //SpellLineId.DeathKnightFrostTundraStalker.Apply(spell =>
            //{
            //    var effect = spell.GetEffect(AuraType.OverrideClassScripts);
            //    effect.BasePoints = spell.Rank * 2;
            //    effect.DiceSides = 0;
            //    effect.AuraEffectHandlerCreator = () => new RageOfRivendareHandler();
            //});
		}

		public class RageOfRivendareHandler : AttackEventEffectHandler
		{
			public override void OnAttack(DamageAction action)
			{
				// "Your spells and abilities deal 4% more damage to targets infected with Blood Plague."
				if (action.SpellEffect != null && action.Victim.Auras.Contains(SpellId.EffectBloodPlague))
				{
					action.ModDamagePercent(EffectValue);
				}
			}
		}
		#endregion

		private static void FixImpurity()
		{
			// TODO Impurity needs to increase AP damage bonus in %
            //SpellLineId.DeathKnightUnholyImpurity.Apply(spell =>
            //{
            //    var oldEffect = spell.RemoveEffect(SpellEffectType.Dummy);
            //    var effect = spell.AddAuraEffect(AuraType.ModDamageDonePercent);
            //    effect.MiscValue = (int)DamageSchoolMask.MagicSchools;
            //    oldEffect.CopyValuesTo(effect);				// copy values
            //});
		}


		#region Blood-Caked Strike
		private static void FixBloodCakedStrike()
		{
            //SpellLineId.DeathKnightUnholyBloodCakedBlade.Apply(spell =>
            //{
            //    var effect = spell.GetEffect(AuraType.Dummy);
            //    effect.IsProc = true;
            //    effect.AuraEffectHandlerCreator = () => new ProcTriggerSpellOnAutoAttackHandler();
            //});

			SpellHandler.Apply(spell =>
			{
				// "hits for $50463s1% weapon damage plus ${$50463m1/2}.1% for each of your diseases on the target"
				spell.GetEffect(SpellEffectType.WeaponPercentDamage).SpellEffectHandlerCreator =
					(cast, effct) => new WeaponDiseaseDamageHalfPercentHandler(cast, effct);
			}, SpellId.EffectBloodCakedStrike);
		}
		#endregion

		#region Corpse Explosion
		private static void FixCorpseExplosion()
		{
			// Corpse explosion transforms a corpse and applies damage to enemies in the area
            //SpellLineId.DeathKnightUnholyCorpseExplosion.Apply(spell =>
            //{
            //    var effect1 = spell.Effects[0];
            //    effect1.TriggerSpellId = SpellId.ClassSkillCorpseExplosion;
            //    effect1.OverrideEffectValue = true;
            //    effect1.SpellEffectHandlerCreator = (cast, effct) => new CorpseExplosionHandler(cast, effct);

            //    spell.Effects[1].SpellEffectHandlerCreator = (cast, effct) => new VoidNoTargetsEffectHandler(cast, effct);
            //});

			// needs to override effect value with the value of the effect that triggered it
			// SpellHandler.Apply(spell => {}, SpellId.ClassSkillCorpseExplosion);

		}

		/// <summary>
		/// TODO: Explosion animation
		/// </summary>
		public class CorpseExplosionHandler : TriggerSpellEffectHandler
		{
			public const float CorpseSearchRadius = 10;

			public CorpseExplosionHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			public override bool HasOwnTargets
			{
				get { return false; }
			}

			static bool IsValidCorpse(NPC corpse)
			{
				// this check won't let us use the spell on corpses of NPCs that by default have the CorpseDisplayId
				return corpse != null && !corpse.IsAlive && !corpse.Auras.Contains(SpellId.CorpseExplode);
			}

			public override void Apply()
			{
				// find corpse
				var corpse = m_cast.Selected as NPC;
				if (!IsValidCorpse(corpse))
				{
					corpse = null;

					// find corpse nearby
					var target = m_cast.Selected ?? m_cast.CasterUnit;
					if (target == null)
					{
						return;		// should not happen
					}
					target.IterateEnvironment(CorpseSearchRadius, obj =>
					{
						if (IsValidCorpse(obj as NPC))
						{
							corpse = (NPC)obj;
							return false;
						}
						return true;
					});
				}

				if (corpse != null)
				{
					m_cast.Selected = corpse;
					m_cast.TargetLoc = corpse.Position;

					m_cast.Trigger(SpellId.CorpseExplode, corpse);	// "explode" & convert corpse

					base.Apply();
				}
			}
		}
		#endregion
	}
}
