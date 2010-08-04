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
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
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

			FixUnholyFever();
			FixCorpseExplosion();
			FixUnholyBlight();
			// "Blood Strike or Pestilence" "the Blood Rune becomes a Death Rune"
			DeathKnightFixes.MakeRuneConversionProc(SpellLineId.DeathKnightUnholyReaping, 
				SpellLineId.DeathKnightBloodStrike, SpellLineId.DeathKnightPestilence,
				RuneType.Death, RuneType.Blood);
			FixWanderingPlague();
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
		}

		private static void FixImpurity()
		{
			// TODO Impurity needs to increase AP bonus in %
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
				effect.AuraType = AuraType.ProcTriggerSpell;
			});

			SpellHandler.Apply(spell =>
			{
				// "hits for $50463s1% weapon damage plus ${$50463m1/2}.1% for each of your diseases on the target"
				spell.GetEffect(SpellEffectType.WeaponDamage).SpellEffectHandlerCreator =
					(cast, effct) => new BloodCakedStrikeHandler(cast, effct);
			}, SpellId.EffectBloodCakedStrike);
		}

		class BloodCakedStrikeHandler : WeaponDamageEffectHandler
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

		#region Wandering Plague
		private static void FixWanderingPlague()
		{
			SpellLineId.DeathKnightUnholyWanderingPlague.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				var effect = spell.GetEffect(AuraType.Dummy);
				effect.IsProc = true;
				effect.AuraEffectHandlerCreator = () => new WanderingPlagueProcHandler();
			});
			SpellHandler.Apply(spell =>
			{
				spell.GetEffect(SpellEffectType.SchoolDamage).SpellEffectHandlerCreator =
					(cast, effct) => new WanderingPlagueDamageHandler(cast, effct);
			}, SpellId.WanderingPlague_2);
		}

		/// <summary>
		/// WP only procs on disease
		/// </summary>
		internal class WanderingPlagueProcHandler : ProcOnDiseaseTriggerSpellHandler
		{
			public override bool CanProcBeTriggeredBy(IUnitAction action)
			{
				if (base.CanProcBeTriggeredBy(action))
				{
					// "there is a chance equal to your melee critical strike chance" 
					var chr = action.Attacker as Character;
					if (chr != null)
					{
						return Utility.RandomFloat() < chr.CritChanceMeleePct;
					}
				}
				return false;
			}

			public override void OnProc(Unit triggerer, IUnitAction action)
			{
				// gives the proc action to the actual damage spell
				Owner.SpellCast.Trigger(SpellHandler.Get(SpellId.WanderingPlague_2), m_spellEffect, action);
			}
		}

		internal class WanderingPlagueDamageHandler : SpellEffectHandler
		{
			public WanderingPlagueDamageHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			protected override void Apply(WorldObject target)
			{
				// TODO: "Ignores any target under the effect of a spell that is cancelled by taking damage."
				var daction = m_cast.TriggerAction as DamageAction;
				if (daction != null && m_cast.TriggerEffect != null)
				{
					// "will cause $s1% additional damage to the target and all enemies within 8 yards"
					var damage = daction.GetDamagePercent(m_cast.TriggerEffect.CalcEffectValue());
					((Unit)target).DoSpellDamage(m_cast.CasterUnit, Effect, damage, false);
				}
			}

			public override ObjectTypes TargetType
			{
				get { return ObjectTypes.Unit; }
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
				effect1.SpellEffectHandlerCreator = (cast, effct) => new CorpseExplosionHandler(cast, effct);

				spell.Effects[1].SpellEffectHandlerCreator = (cast, effct) => new VoidEffectHandler(cast, effct);
			});

			// needs to override effect value with the value of the effect that triggered it
			SpellHandler.Apply(spell => spell.GetEffect(SpellEffectType.SchoolDamage).OverrideEffectValue = true,
							   SpellId.ClassSkillCorpseExplosion);

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

		#region Unholy Fever
		static readonly SpellId[] CryptFeverRanks = new[]
		{
			SpellId.CryptFeverRank1,
			SpellId.CryptFever,
			SpellId.CryptFever_2,
		};

		private static void FixUnholyFever()
		{
			// Crypt Fever does not proc correctly
			SpellLineId.DeathKnightUnholyCryptFever.Apply(spell =>
			{
				// proc when a new Aura is applied to a target
				spell.ProcTriggerFlags = ProcTriggerFlags.AuraStarted;

				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.AuraType = AuraType.ProcTriggerSpell;
				effect.TriggerSpellId = CryptFeverRanks[spell.Rank - 1];
				effect.AuraEffectHandlerCreator = () => new ProcOnDiseaseTriggerSpellHandler();
				effect.ClearAffectMask();
			});

			// The Crypt fever effect should increase damage taken %
			SpellHandler.Apply(spell =>
			{
				var effect = spell.Effects[0];
				effect.AuraType = AuraType.ModDamageTakenPercent;
			}, CryptFeverRanks);
		}
		#endregion

		#region Unholy Blight
		static void FixUnholyBlight()
		{
			// Unholy Blight needs to proc an Aura when Death Coil is casted
			SpellLineId.DeathKnightUnholyUnholyBlight.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

				var effect = spell.GetEffect(AuraType.Dummy);
				effect.AuraType = AuraType.ProcTriggerSpell;
				effect.AddToAffectMask(SpellLineId.DeathKnightDeathCoil);
				effect.TriggerSpellId = SpellId.ClassSkillUnholyBlight;
				//effect.AuraEffectHandlerCreator = () => new UnholyBlightHandler();
			});
			SpellHandler.Apply(spell =>
			{
				// add periodic damage
				spell.GetEffect(AuraType.PeriodicDamage).AuraEffectHandlerCreator = () => new UnholyBlightDamageHandler();

				// "preventing any diseases on the victim from being dispelled"
				var dispelImmEffect = spell.AddAuraEffect(AuraType.DispelImmunity);
				dispelImmEffect.MiscValue = (int)DispelType.Disease;
			}, SpellId.ClassSkillUnholyBlight);
		}

		public class UnholyBlightDamageHandler : ParameterizedPeriodicDamageHandler
		{
			protected override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
			{
				if (creatingCast == null || !(creatingCast.TriggerAction is DamageAction))
				{
					failReason = SpellFailedReason.Error;
					return;
				}

				var daction = ((DamageAction)creatingCast.TriggerAction);

				// "taking $49194s1% of the damage done by the Death Coil over $50536d"
				TotalDamage = daction.ActualDamage;
			}
		}
		#endregion
	}
}
