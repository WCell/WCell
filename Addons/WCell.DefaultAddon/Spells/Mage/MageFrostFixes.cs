using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Mage
{
	public class MageFrostFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixSpells()
		{
			//ColdSnap resets the cooldown of all Frost spells, except his own cooldown
			SpellLineId.MageFrostColdSnap.Apply(spell =>
			{
				spell.Effects[0].SpellEffectHandlerCreator =
				(cast, effect) => new ColdSnapHandler(cast, effect);
			});

			SpellLineId.MageFrostSummonWaterElemental.Apply(spell =>
			{
				spell.Effects[0].SpellEffectHandlerCreator =
				(cast, effect) => new SummonWaterElementalHandler(cast, effect);
			});

			SpellLineId.MageFrostShatter.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.OverrideClassScripts);
				effect.AuraEffectHandlerCreator = () => new MageFrostShatterHandler();

			});

			// Frost Channeling has reversed affect masks for the 2 effects
			SpellLineId.MageFrostFrostChanneling.Apply(spell =>
			{
				var powerCostEffect = spell.GetEffect(AuraType.ModPowerCost);
				var threatEffect = spell.GetEffect(AuraType.ModThreat);

				powerCostEffect.CopyAffectMaskTo(threatEffect.AffectMask);	// mods threat of frost spells
				powerCostEffect.ClearAffectMask();							// reduces mana cost of all spells
			});

			// Blizzard adds a chill effect if caster has Improved Blizzard
			SpellLineId.MageBlizzard.Apply(spell =>
			{
				var triggerEffect = spell.GetEffect(AuraType.PeriodicTriggerSpell);
				var triggerSpell = triggerEffect.GetTriggerSpell();
				if (triggerSpell == null)
				{
					LogManager.GetCurrentClassLogger().Warn("Blizzard spell has no Trigger effect: " + spell);
					return;
				}

				// Blizzard triggers a damage spell on all targets periodically
				// That effect should also trigger the chill effect, if caster has Improved Blizzard
				var dmgEffect = triggerSpell.GetEffect(SpellEffectType.SchoolDamage);
				var chillEffect = triggerSpell.AddEffect(SpellEffectType.Dummy, dmgEffect.ImplicitTargetA);
				chillEffect.ImplicitTargetB = dmgEffect.ImplicitTargetB;
				chillEffect.SpellEffectHandlerCreator = (cast, effect) => new ImprovedBlizzardHandler(cast, effect);
			});

			//IceLance deals tripple dmg to frozen targets
			SpellLineId.MageIceLance.Apply(spell =>
			{
				spell.Effects[0].SpellEffectHandlerCreator = (cast, effect) => new IceLanceHandler(cast, effect);
			});
		}

		#region ColdSnap
		public class ColdSnapHandler : DummyEffectHandler
		{
			public ColdSnapHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			protected override void Apply(WorldObject target)
			{
				if (target is Character)
				{
					var charSpells = ((Character)target).PlayerSpells;
					foreach (Spell spell in charSpells)
					{
						if (spell.SchoolMask == DamageSchoolMask.Frost && spell.SpellId != SpellId.MageFrostColdSnap)
						{
							charSpells.ClearCooldown(spell, false);
						}
					}
				}
			}
		}
		#endregion

		#region SummonWaterElemental
		public class SummonWaterElementalHandler : DummyEffectHandler
		{
			public SummonWaterElementalHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			public override void Apply()
			{
				var chr = m_cast.CasterUnit as Character;
				if (chr != null)
				{
					var charSpells = chr.PlayerSpells;
					if (charSpells.Contains(SpellId.GlyphOfEternalWater))
						chr.SpellCast.Trigger(SpellId.SummonWaterElemental_7, chr);
					else
						chr.SpellCast.Trigger(SpellId.SummonWaterElemental_6, chr);
				}
			}
		}
		#endregion

		#region MageFrostShatter
		public class MageFrostShatterHandler : AttackEventEffectHandler
		{
			public override void OnAttack(DamageAction action)
			{
				if (action.SpellEffect != null && action.Victim.AuraState.HasAnyFlag(AuraStateMask.Frozen))
				{
					switch (m_aura.Spell.SpellId)
					{
						case SpellId.MageFrostShatterRank1:
							action.AddBonusCritChance(17);
							break;
						case SpellId.MageFrostShatterRank2:
							action.AddBonusCritChance(34);
							break;
						case SpellId.MageFrostShatterRank3:
							action.AddBonusCritChance(50);
							break;
					}
				}
			}
		}
		#endregion

		#region Blizzard
		public class ImprovedBlizzardHandler : SpellEffectHandler
		{
			public ImprovedBlizzardHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
			{
			}

			protected override void Apply(WorldObject target)
			{
				var caster = m_cast.CasterUnit;
				if (caster != null)
				{
					var improvedBlizzard = caster.Auras[SpellLineId.MageFrostImprovedBlizzard];
					if (improvedBlizzard != null)
					{
						// Caster has Improved Blizzard
						var rank = improvedBlizzard.Spell.Rank;
						var chilledLine = SpellLineId.MageChilled.GetLine();
						var chilledSpell = chilledLine.GetRank(rank);
						if (chilledSpell != null)
						{
							m_cast.Trigger(chilledSpell, Effect, target);
						}
					}
				}
			}
		}
		#endregion

		#region IceLanceHandler
		public class IceLanceHandler : SpellEffectHandler
		{
			public IceLanceHandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			protected override void Apply(WorldObject target)
			{
				var unit = target as Unit;

				if (unit != null && unit.AuraState.HasAnyFlag(AuraStateMask.Frozen))
				{
					((Unit)target).DealSpellDamage(m_cast.CasterUnit, Effect, CalcEffectValue() * 3);
				}
				else
					((Unit)target).DealSpellDamage(m_cast.CasterUnit, Effect, CalcEffectValue());
			}
		}
		#endregion
	}
}
