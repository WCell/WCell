using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Warlock
{
	public static class WarlockFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixWarlock()
		{
			// Curse of Doom cannot be casted on Players and spawns a Demon on target death
			SpellLineId.WarlockCurseOfDoom.Apply(spell =>
			{
				spell.CanCastOnPlayer = false;
				spell.Effects[0].AuraEffectHandlerCreator = () => new SummonDoomguardOnDeathHandler();
			});

            // Armors are mutually exclusive
            AuraHandler.AddAuraGroup(SpellLineId.WarlockFelArmor, SpellLineId.WarlockDemonArmor, SpellLineId.WarlockDemonSkin);

            // can't have more than one of these per caster
            AuraHandler.AddAuraCasterGroup(
                SpellLineId.WarlockCurseOfTongues, SpellLineId.WarlockCurseOfTheElements,
                SpellLineId.WarlockCurseOfDoom, SpellLineId.WarlockCurseOfAgony,
                SpellLineId.WarlockCurseOfWeakness, SpellLineId.WarlockAfflictionCurseOfExhaustion);

            // Shadowflame DoT
            SpellHandler.Apply(spell => spell.AddTargetTriggerSpells(SpellId.Shadowflame_3), SpellId.ClassSkillShadowflameRank1);
            SpellHandler.Apply(spell => spell.AddTargetTriggerSpells(SpellId.Shadowflame_5), SpellId.ClassSkillShadowflameRank2);
            SpellHandler.Apply(spell => spell.Effects[0].ImplicitTargetA = ImplicitSpellTargetType.ConeInFrontOfCaster, SpellId.Shadowflame_3);
            SpellHandler.Apply(spell => spell.Effects[0].ImplicitTargetA = ImplicitSpellTargetType.ConeInFrontOfCaster, SpellId.Shadowflame_5);

            // Incinerate has extra damage if target has Immolate
            SpellLineId.WarlockIncinerate.Apply(spell =>
				spell.Effects[0].SpellEffectHandlerCreator = (cast, effect) => new IncreaseDamageIfAuraPresentHandler(cast, effect));

            // Demonic Circle Teleport
            var teleReqSpell = SpellHandler.AddCustomSpell(62388, "DemonicCircleTeleportRequirement");
            teleReqSpell.IsPreventionDebuff = false;
            teleReqSpell.AddAuraEffect(AuraType.Dummy);
            teleReqSpell.Attributes |= SpellAttributes.InvisibleAura;
            teleReqSpell.Durations = new Spell.DurationEntry { Min = 360000, Max = 360000 };
            SpellHandler.Apply(spell =>
            {
				var efct = spell.AddEffect(SpellEffectType.Dummy, ImplicitSpellTargetType.None);
                efct.MiscValue = (int)GOEntryId.DemonicCircleSummon;
                efct.SpellEffectHandlerCreator = (cast, effect) => new RecallToGOHandler(cast, effect);
                spell.AddCasterTriggerSpells(teleReqSpell);
            }, SpellId.ClassSkillDemonicCircleTeleport);

            // Demonic Circle Summon
            SpellHandler.Apply(spell => spell.AddCasterTriggerSpells(teleReqSpell.SpellId), SpellLineId.WarlockDemonicCircleSummon);

            //life tap
            SpellHandler.Apply(spell =>
            {
                var spellEffect = spell.GetEffect(SpellEffectType.Dummy);
                spellEffect.SpellEffectHandlerCreator = (cast, effect) => new LifeTapHandler(cast, effect);
            }, SpellLineId.WarlockLifeTap);

			SpellLineId.WarlockImmolate.Apply(spell =>
			{
				spell.AddAuraEffect(() => new ApplyImmolateStateHandler(),ImplicitSpellTargetType.SingleEnemy);
			});

			SpellLineId.WarlockDestructionConflagrate.Apply(spell =>
			{
				var dmgeff = spell.GetEffect(SpellEffectType.SchoolDamage);
				var periodicdmgeff = spell.GetEffect(AuraType.PeriodicDamage);
				dmgeff.SpellEffectHandlerCreator = (cast, effect) => new ConflagrateHandler(cast, effect);
				periodicdmgeff.AuraEffectHandlerCreator = () => new ConflagratePeriodicHandler();
			});
        }

        public class IncreaseDamageIfAuraPresentHandler : SpellEffectHandler
        {
            public IncreaseDamageIfAuraPresentHandler(SpellCast cast, SpellEffect effect)
                : base(cast, effect)
            {
            }

            protected override void Apply(WorldObject target)
            {
                var unit = target as Unit;
                var oldVal = Effect.BasePoints;

                if (unit != null && unit.Auras[SpellLineId.WarlockImmolate] != null)
                {
                    Effect.BasePoints += Effect.BasePoints / 4;
                }
                ((Unit)target).DealSpellDamage(m_cast.CasterUnit, Effect, CalcEffectValue());
                Effect.BasePoints = oldVal;
            }

            public override ObjectTypes TargetType
            {
                get { return ObjectTypes.Unit; }
            }
        }

        public class SummonDoomguardOnDeathHandler : PeriodicDamageHandler
        {
            protected override void Apply()
            {
                base.Apply();
                if (m_aura.Auras.Owner.YieldsXpOrHonor && !m_aura.Auras.Owner.IsAlive)
                {
                    m_aura.Auras.Owner.SpellCast.TriggerSelf(SpellId.ClassSkillCurseOfDoomEffect);
                }
            }
        }

        public class LifeTapHandler : SpellEffectHandler
        {
            public LifeTapHandler(SpellCast cast, SpellEffect effect): base(cast, effect)
            {}

            public override void Apply()
            {
                var chr = Cast.CasterUnit as Character;

                if(chr != null)
                {
                    int effectValue = CalcEffectValue();
                    int removeHealth = (int) (chr.Spirit * 1.5f) + effectValue;
                    int addMana = (int)(chr.GetDamageDoneMod(Effect.Spell.Schools[0]) * 0.5f) + effectValue;
                    if((chr.Health - removeHealth) >= 1)
                    {
                        chr.Health -= removeHealth;
                        chr.Power += addMana;
                    }
                }
            }

            public override SpellFailedReason InitializeTarget(WorldObject target)
            {
                var chr = target as Character;
                if (chr != null)
                {
                    int effectValue = CalcEffectValue();
                    int removeHealth = (int)(chr.Spirit * 1.5f) + effectValue;
                    if ((chr.Health - removeHealth) <= 1)
                    {
                        return SpellFailedReason.CantDoThatRightNow;
                    }
                }
                return base.InitializeTarget(target);
            }
        }
		#region ApplyImmolateStateHandler
		public class ApplyImmolateStateHandler : DummyHandler
		{
			protected override void Apply()
			{
				Owner.IncMechanicCount(SpellMechanic.Custom_Immolate, true);
			}
			protected override void Remove(bool cancelled)
			{
				Owner.DecMechanicCount(SpellMechanic.Custom_Immolate, true);
			}
		}
		#endregion

		#region Conflagrate
		public class ConflagrateHandler : SchoolDamageEffectHandler
		{
			public ConflagrateHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
            {
            }

			protected override void Apply(WorldObject target)
			{
				var unit = (Unit)target;
				if (unit != null)
				{
					var aura = unit.Auras[unit.GetStrongestImmolate()];
					var modbasepoints = aura.Spell.Effects[0].GetModifiedDamage(m_cast.CasterUnit); //basepoints after applying all damage modifiers
					var totalbasepoints = modbasepoints * aura.MaxTicks;//total damage of immolate aura with all mods
					m_cast.Spell.Effects[0].BasePoints = 60 * totalbasepoints / 100; //60% of total damage
				}
				base.Apply(target);
			}
		}
		public class ConflagratePeriodicHandler : PeriodicDamageHandler
		{
			protected override void Apply()
			{
				var aura = Owner.Auras[Owner.GetStrongestImmolate()];
				if (aura != null)
				{
					var modbasepoints = aura.Spell.Effects[0].GetModifiedDamage(m_aura.CasterUnit);
					var totalbasepoints = modbasepoints * aura.MaxTicks;
					var finalvalue = m_aura.Spell.Effects[2].CalcEffectValue() * totalbasepoints / (100 * m_aura.MaxTicks);//40% of total damage
					m_aura.Spell.Effects[1].BasePoints = finalvalue;
					UpdateEffectValue();
					Owner.Auras.Remove(aura.Id);
				}
				base.Apply();
			}
		}
		
		#endregion
	}
}