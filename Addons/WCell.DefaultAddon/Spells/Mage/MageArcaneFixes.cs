using System;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.Addons.Default.Spells.Mage
{
	public static class MageArcaneFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixMe()
		{
			// conjure water and food don't have any per level bonus
			SpellLineId.MageConjureRefreshment.Apply(spell => spell.ForeachEffect(effect => effect.RealPointsPerLevel = 0));

			SpellLineId.MageArcaneArcanePotency.Apply(spell =>
			{
				spell.GetEffect(AuraType.Dummy).AuraEffectHandlerCreator = () => new AddProcHandler(new ArcanePotencyProcHandler(
					ProcTriggerFlags.DoneBeneficialMagicSpell,
					spell.GetEffect(AuraType.Dummy).CalcEffectValue()));

			});

			SpellHandler.Apply(spell =>
			{
				spell.AddEffect((cast, effect) => new ClearCastingAndPresenceOfMindHandler(cast, effect), ImplicitSpellTargetType.Self);
			}, SpellId.EffectClearcasting, SpellId.MageArcanePresenceOfMind);


			// "When your Mana Shield, Frost Ward, Fire Ward, or Ice Barrier absorbs damage
			//  your spell damage is increased by $s1% of the amount absorbed for $44413d."
			SpellLineId.MageArcaneIncantersAbsorption.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.AuraEffectHandlerCreator = () => new IncantersAbsorptionHandler();
			});
			//SpellHandler.Apply(spell => {
			//    var effect = spell.GetEffect(AuraType.ModDamageDone);
			//    if (effect != null)
			//    {
			//        effect.AuraEffectHandlerCreator = () => new IncantersAbsorption2Handler();
			//    }
			//},
			//SpellId.EffectClassSkillIncantersAbsorption);
		}

		#region Incanter's Absorption
		public class IncantersAbsorptionHandler : AttackEventEffectHandler
		{
			/// <summary>
			/// Register with OnAttack, since it's executed right after OnDefend, which is where
			/// Absorption handlers are handled.
			/// "When your Mana Shield, Frost Ward, Fire Ward, or Ice Barrier absorbs damage
			///  your spell damage is increased by $s1% of the amount absorbed for $44413d."
			/// </summary>
			public override void OnAttack(DamageAction action)
			{
				if (action.Absorbed > 0)
				{
					// apply aura
					Owner.SpellCast.TriggerSelf(SpellId.EffectClassSkillIncantersAbsorption);

					// retreive aura & handler
					var aura = Owner.Auras[SpellId.EffectClassSkillIncantersAbsorption];
					if (aura != null)
					{
						var handler = aura.GetHandler(AuraType.ModDamageDone) as ModDamageDoneHandler;
						if (handler != null)
						{
							// override effect value
							handler.BaseEffectValue = EffectValue;
						}
					}
				}
			}
		}
		#endregion
		
		#region Clear Casting & Arcane Potency
		public class ClearCastingAndPresenceOfMindHandler : SpellEffectHandler
		{
			public ClearCastingAndPresenceOfMindHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect) { }

			protected override void Apply(WorldObject target)
			{
				var caster = m_cast.CasterChar;
				if (caster == null) return;
				//var aura = caster.Auras[SpellLineId.MageArcaneArcanePotency];
				//if (aura != null)

				var handler = caster.GetProcHandler<ArcanePotencyProcHandler>();
				if (handler != null)
				{
					handler.trigger = true;
				}
			}
		}

		public class ArcanePotencyProcHandler : IProcHandler
		{
			public bool trigger;
			private int modPercentage;

			public ArcanePotencyProcHandler(ProcTriggerFlags flags, int valPercentage)
			{
				modPercentage = valPercentage;
				trigger = false;
			}
			public Unit Owner
			{
				get;
				private set;
			}
			public Spell ProcSpell
			{
				get { return null; }
			}
			public uint ProcChance
			{
				get { return 100; }
			}
			public DateTime NextProcTime
			{
				get;
				set;
			}

			public int MinProcDelay
			{
				get { return 0; }
			}
			public int StackCount
			{
				get { return 0; }
			}
			public ProcTriggerFlags ProcTriggerFlags
			{
				get { return ProcTriggerFlags.DoneBeneficialMagicSpell; }
			}

            public ProcHitFlags ProcHitFlags
            {
                get { return ProcHitFlags.None; }
            }

			public void Dispose()
			{
				Owner.RemoveProcHandler(this);
			}

			public bool CanBeTriggeredBy(Unit triggerer, IUnitAction action, bool active)
			{
				var dAction = action as DamageAction;
				if (dAction != null)
					return true;
				return false;
			}
			public void TriggerProc(Unit triggerer, IUnitAction action)
			{
				if (trigger)
				{
					var dAction = action as DamageAction;
					if (dAction.CanCrit)
					{
						dAction.AddBonusCritChance(modPercentage);
						trigger = false;
					}
				}
			}
		}
		#endregion
	}
}
