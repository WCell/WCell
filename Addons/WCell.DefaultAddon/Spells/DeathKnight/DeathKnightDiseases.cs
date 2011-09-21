using System.Linq;
using NLog;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;
using WCell.Util;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	/// <summary>
	/// Fixes the disease talents and debuffs
	/// </summary>
	public static class DeathKnightDiseases
	{
		[Initialization(InitializationPass.Second)]
		public static void FixDiseases()
		{
			FixPassiveDiseaseTalent(SpellLineId.DeathKnightBloodPlague, SpellId.EffectBloodPlague);
			FixPassiveDiseaseTalent(SpellLineId.DeathKnightFrostFever, SpellId.EffectFrostFever);

			FixUnholyFeverAndEbonPlague();
			FixUnholyBlight();
			FixWanderingPlague();
			FixPestilence();
		}

		#region Pestilence
		private static void FixPestilence()
		{
			// Pestilence uses the first target as starter for a disease
			SpellLineId.DeathKnightPestilence.Apply(spell =>
			{
				var spreadEffect = spell.GetEffect(SpellEffectType.ScriptEffect);
				spreadEffect.SpellEffectHandlerCreator = (cast, effct) => new SpreadPestilenceHandler(cast, effct);

				// make sure we only have one Dummy effect, so we can access it unambiguously in the effect handler
				spell.GetFirstEffectWith(effect => effect.ImplicitTargetA == ImplicitSpellTargetType.DynamicObject).EffectType = SpellEffectType.None;

				// make the dummy handler collect the single enemy target
				spell.GetEffect(SpellEffectType.Dummy).SpellEffectHandlerCreator =
					(cast, effct) => new VoidWithTargetsEffectHandler(cast, effct);
			});
		}

		internal class SpreadPestilenceHandler : SpellEffectHandler
		{
			private Spell infectionSpell;

			public SpreadPestilenceHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			public override ObjectTypes TargetType
			{
				get { return ObjectTypes.Unit; }
			}

			/// <summary>
			/// Find the source of the pestilence infection, determine the disease to be spread
			/// and infect all targets.
			/// </summary>
			public override void Apply()
			{
				// get the initial target
				var startHandler = m_cast.GetHandler(SpellEffectType.Dummy);
				if (startHandler == null)
				{
					LogManager.GetCurrentClassLogger().Warn("Spell {0} does not have a Dummy handler anymore.", Effect.Spell);
					return;
				}

				var source = startHandler.Targets.FirstOrDefault() as Unit;
				if (source != null)
				{
					var aura = source.Auras[SpellId.EffectBloodPlague];
					if (aura == null)
					{
						aura = source.Auras[SpellId.EffectFrostFever];
					}
					if (aura != null)
					{
						infectionSpell = aura.Spell;
					}
				}

				if (infectionSpell != null)
				{
					base.Apply();
				}
				else
				{
					m_cast.Cancel(SpellFailedReason.TargetAurastate);
				}
			}

			protected override void Apply(WorldObject target)
			{
				m_cast.Trigger(infectionSpell, Effect, target);
			}
		}
		#endregion

		#region Blood Plague & Frost Fever
		private static void FixPassiveDiseaseTalent(SpellLineId passiveSpell, SpellId effectId)
		{
			//  the debuff needs to use the disease talent to apply damage
			SpellEffect passiveEffect = null;
			passiveSpell.Apply(spell =>
			{
				passiveEffect = spell.GetEffect(AuraType.Dummy);
				passiveEffect.APValueFactor = 0.055f * 1.15f;
				passiveEffect.RealPointsPerLevel = 0;
			});

			SpellHandler.Apply(spell =>
			{
				var dmgEffect = spell.GetEffect(AuraType.PeriodicDamage);
				dmgEffect.EffectValueOverrideEffect = passiveEffect;		// calculate damage based on the passive spell
			}, effectId);
		}
		#endregion

		#region Wandering Plague
		private static void FixWanderingPlague()
		{
			// Wandering Plague has a proc and an AoE damage component
			SpellLineId.DeathKnightUnholyWanderingPlague.Apply(spell =>
			{
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
				var unit = (Unit)target;
				if (unit.Auras.Contains(aura => aura.Spell.InterruptFlags.HasFlag(AuraInterruptFlags.OnDamage)))
				{
					// "Ignores any target under the effect of a spell that is cancelled by taking damage."
					return;
				}

				var daction = m_cast.TriggerAction as DamageAction;
				if (daction != null && m_cast.TriggerEffect != null)
				{
					// "will cause $s1% additional damage to the target and all enemies within 8 yards"
					var damage = daction.GetDamagePercent(m_cast.TriggerEffect.CalcEffectValue());
					unit.DealSpellDamage(m_cast.CasterUnit, Effect, damage, false);
				}
			}

			public override ObjectTypes TargetType
			{
				get { return ObjectTypes.Unit; }
			}
		}
		#endregion

		#region Unholy Fever & Ebon Plague
		static readonly SpellId[] CryptFeverRanks = new[]
		{
			SpellId.CryptFeverRank1,
			SpellId.CryptFever,
			SpellId.CryptFever_2,
		};
		static readonly SpellId[] EbonPlagueRanks = new[]
		{
			SpellId.EbonPlague,
			SpellId.EbonPlague_2,
			SpellId.EbonPlague_3,
		};

		private static void FixUnholyFeverAndEbonPlague()
		{
			// Both diseases toggle this spell to increase disease damage taken percent
			var dmgAmplifier = SpellHandler.AddCustomSpell(65142, "Overridden Disease Damage Percent");
			var dmgAmplifierEffect = dmgAmplifier.AddAuraEffect(() => new ModDiseaseDamagePercentHandler());
			dmgAmplifierEffect.OverrideEffectValue = true;

			// Crypt Fever does not proc correctly
			SpellLineId.DeathKnightUnholyCryptFever.Apply(spell =>
			{
				// proc when a new Aura is applied to a target
                spell.ProcTriggerFlags = ProcTriggerFlags.DoneHarmfulMagicSpell;

				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.IsProc = true;
				effect.TriggerSpellId = CryptFeverRanks[spell.Rank - 1];
				effect.AuraEffectHandlerCreator = () => new CryptFeverHandler();
				effect.ClearAffectMask();
			});

			// The Ebon Plague effect also "increases magic damage taken by $s2%"
			SpellHandler.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.AuraType = AuraType.ModDamageTakenPercent;
			}, EbonPlagueRanks);
		}

		public class CryptFeverHandler : ProcOnDiseaseTriggerSpellHandler	// only proc on disease
		{
			public override void OnProc(Unit triggerer, IUnitAction action)
			{
				// Crypt Fever is triggered by default but changes to Ebon Plague, if
				// the caster (which by default is also the owner of this Aura) has the Ebon Plaguebringer talent.
				var ebonPlague = m_aura.Owner.Spells.GetHighestRankOf(SpellLineId.DeathKnightUnholyEbonPlaguebringer);
				SpellId spell;

				// select spell to be triggered, based on talent rank
				if (ebonPlague != null)
				{
					spell = EbonPlagueRanks[ebonPlague.Rank - 1];				// Ebon Plaguebringer
				}
				else
				{
					spell = CryptFeverRanks[m_spellEffect.Spell.Rank - 1];		// Crypt Fever
				}

				SpellCast.ValidateAndTriggerNew(SpellHandler.Get(spell), m_aura.CasterReference, Owner, triggerer,
												m_aura.Controller as SpellChannel, m_aura.UsedItem, action, m_spellEffect);
			}
		}

		public class ModDiseaseDamagePercentHandler : AttackEventEffectHandler
		{
			public override void OnDefend(DamageAction action)
			{
				var spell = action.Spell;
				if (spell != null && spell.DispelType == DispelType.Disease)
				{
					action.ModDamagePercent(EffectValue);
				}
			}
		}
		#endregion

		#region Unholy Blight
		static void FixUnholyBlight()
		{
			// Unholy Blight needs to proc an Aura when Death Coil is casted
			SpellLineId.DeathKnightUnholyUnholyBlight.Apply(spell =>
			{
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

	/// <summary>
	/// Increases weapon damage by EffectValue% "for each of your diseases on the target"
	/// </summary>
	public class WeaponDiseaseDamagePercentHandler : WeaponDamageEffectHandler
	{
		public WeaponDiseaseDamagePercentHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void OnHit(DamageAction action)
		{
			var bonusPercent = CalcEffectValue() * action.Victim.Auras.GetVisibleAuraCount(m_cast.CasterReference, DispelType.Disease);
			action.Damage += (action.Damage * bonusPercent + 50) / 100;	// rounded
		}
	}

	/// <summary>
	/// Increases weapon damage by "${$...mX/2}.1% for each of your diseases on the target"
	/// </summary>
	public class WeaponDiseaseDamageHalfPercentHandler : WeaponDamageEffectHandler
	{
		public WeaponDiseaseDamageHalfPercentHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void OnHit(DamageAction action)
		{
			var doubleBonus = CalcEffectValue() * action.Victim.Auras.GetVisibleAuraCount(m_cast.CasterReference, DispelType.Disease);
			action.Damage += (action.Damage * doubleBonus + 100) / 200;	// + <1/2 of effect value> percent per disease
		}
	}
}
