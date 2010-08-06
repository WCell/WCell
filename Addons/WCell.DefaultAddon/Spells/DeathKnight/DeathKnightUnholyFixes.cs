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

			// "Blood Strike or Pestilence" -> "the Blood Rune becomes a Death Rune"
			DeathKnightFixes.MakeRuneConversionProc(SpellLineId.DeathKnightUnholyReaping,
				SpellLineId.DeathKnightBloodStrike, SpellLineId.DeathKnightPestilence,
				RuneType.Death, RuneType.Blood);

			FixBloodCakedStrike();
			FixImpurity();

			// Dirge is only triggered by "Death Strike, Plague Strike and Scourge Strike"
			SpellLineId.DeathKnightUnholyDirge.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				var effect = spell.GetEffect(AuraType.ProcTriggerSpellWithOverride);
				effect.ClearAffectMask();
				effect.AddAffectingSpells(SpellLineId.DeathKnightDeathStrike, SpellLineId.DeathKnightPlagueStrike, SpellLineId.DeathKnightUnholyScourgeStrike);
			});

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
				spell.GetEffect(AuraType.None).AuraEffectHandlerCreator = () => new ModAllRuneCooldownsPercentHandler();
			});

			FixAntiMagicZone();
			FixNecrosis();
			FixAntiMagicShell();
		}

		#region Anti Magic Shell
		private static void FixAntiMagicShell()
		{
			SpellLineId.DeathKnightAntiMagicShell.Apply(spell =>
			{

			});
		}

		public class AMSAuraHandler : SchoolAbsorbHandler
		{
			protected override void Apply()
			{
				base.Apply();
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

				RemainingValue -= absorbed;
				action.Absorbed += absorbed;
			}
		}
		#endregion

		#region Necrosis
		private static void FixNecrosis()
		{
			// Necrosis deals shadow damage on top of every attack that was performed
			SpellLineId.DeathKnightUnholyNecrosis.Apply(spell =>
			{
				spell.SchoolMask = DamageSchoolMask.Shadow;		// "Shadow damage"

				var effect = spell.GetEffect(AuraType.Dummy);
				effect.AuraEffectHandlerCreator = () => new NecrosisDamageHandler();
			});
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
			SpellLineId.DeathKnightFrostTundraStalker.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.BasePoints = spell.Rank * 2;
				effect.DiceSides = 0;
				effect.AuraEffectHandlerCreator = () => new RageOfRivendareHandler();
			});
		}

		public class RageOfRivendareHandler : AttackEventEffectHandler
		{
			public override void OnBeforeAttack(DamageAction action)
			{ }

			public override void OnAttack(DamageAction action)
			{
				// "Your spells and abilities deal 4% more damage to targets infected with Blood Plague."
				if (action.SpellEffect != null && action.Victim.Auras.Contains(SpellLineId.DeathKnightBloodPlaguePassive))
				{
					action.ModDamagePercent(EffectValue);
				}
			}

			public override void OnDefend(DamageAction action)
			{
			}
		}
		#endregion

		private static void FixImpurity()
		{
			// TODO Impurity needs to increase AP damage bonus in %
			SpellLineId.DeathKnightUnholyImpurity.Apply(spell =>
			{
				var oldEffect = spell.RemoveEffect(SpellEffectType.Dummy);
				var effect = spell.AddAuraEffect(AuraType.ModDamageDonePercent);
				effect.MiscValue = (int)DamageSchoolMask.MagicSchools;
				oldEffect.CopyValuesTo(effect);				// copy values
			});
		}


		#region Blood-Caked Strike
		private static void FixBloodCakedStrike()
		{
			SpellLineId.DeathKnightUnholyBloodCakedBlade.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.IsProc = true;
				effect.AuraEffectHandlerCreator = () => new ProcTriggerSpellOnAutoAttackHandler();
			});

			SpellHandler.Apply(spell =>
			{
				// "hits for $50463s1% weapon damage plus ${$50463m1/2}.1% for each of your diseases on the target"
				spell.GetEffect(SpellEffectType.WeaponPercentDamage).SpellEffectHandlerCreator =
					(cast, effct) => new BloodCakedStrikeHandler(cast, effct);
			}, SpellId.EffectBloodCakedStrike);
		}

		class BloodCakedStrikeHandler : WeaponPercentDamageEffectHandler
		{
			public BloodCakedStrikeHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			public override void OnHit(DamageAction action)
			{
				var doubleBonus = CalcEffectValue() * action.Victim.Auras.GetVisibleAuraCount(DispelType.Disease);
				action.Damage += (action.Damage * doubleBonus + 100) / 200;	// + <1/2 of effect value> percent per disease
			}
		}
		#endregion

		#region Corpse Explosion
		private static void FixCorpseExplosion()
		{
			// Corpse explosion transforms a corpse and applies damage to enemies in the area
			SpellLineId.DeathKnightUnholyCorpseExplosion.Apply(spell =>
			{
				var effect1 = spell.Effects[0];
				effect1.TriggerSpellId = SpellId.ClassSkillCorpseExplosion;
				effect1.OverrideEffectValue = true;
				effect1.SpellEffectHandlerCreator = (cast, effct) => new CorpseExplosionHandler(cast, effct);

				spell.Effects[1].SpellEffectHandlerCreator = (cast, effct) => new VoidEffectHandler(cast, effct);
			});

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
				return corpse != null && !corpse.IsAlive && !corpse.Auras.Contains(SpellId.CorpseExploded);
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

					m_cast.Trigger(SpellId.CorpseExploded, corpse);	// "explode" & convert corpse

					base.Apply();
				}
			}
		}
		#endregion
	}
}
