using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
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
				if (action.SpellEffect != null && action.Victim.AuraState == AuraStateMask.Frozen)
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
	}
}
