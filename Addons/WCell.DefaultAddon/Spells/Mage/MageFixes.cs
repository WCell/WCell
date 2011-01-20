using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Effects;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Spells.Mage
{
	public static class MageFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixMage()
		{
			// The improved counterspell aura should apply to Counterspell only
			SpellLineId.MageArcaneImprovedCounterspell.Apply(spell =>
			{
				spell.AddCasterProcSpells(SpellLineId.MageCounterspell);
			});

			// Cone of cold is missing Range
			SpellLineId.MageConeOfCold.Apply(spell =>
			{
				spell.Range.MaxDist = 10;
			});

			//molten armor buffs Crit rating based on your Spirit
			SpellLineId.MageMoltenArmor.Apply(spell =>
			{
				spell.Effects[2].MiscValue = (int)StatType.Spirit;
				spell.Effects[2].MiscValueB = (int)CombatRating.SpellCritChance;
			});

			// MageIceBlock applies Hypothermia on every cast
			SpellLineId.MageIceBlock.Apply(spell => spell.AddCasterTriggerSpells(SpellId.Hypothermia));
			SpellHandler.Apply(spell =>
			{
				spell.IsPreventionDebuff = true;
			}, SpellId.Hypothermia);


			// Mage living bomb persists through death and should make boom when removed
			SpellLineId.MageFireLivingBomb.Apply(spell =>
			{
				spell.AttributesExC = SpellAttributesExC.PersistsThroughDeath;
				spell.CanOverrideEqualAuraRank = false;
			});
			SpellHandler.Apply(spell =>
			{
				spell.Effects[1].TriggerSpellId = SpellId.ClassSkillLivingBombRank1;
				spell.Effects[1].AuraEffectHandlerCreator =
					() => new TriggerSpellAfterAuraRemovedHandler();
			}, SpellId.MageFireLivingBombRank1);
			SpellHandler.Apply(spell =>
			{
				spell.Effects[1].TriggerSpellId = SpellId.ClassSkillLivingBombRank2_2;
				spell.Effects[1].AuraEffectHandlerCreator =
					() => new TriggerSpellAfterAuraRemovedHandler();
			}, SpellId.ClassSkillLivingBombRank2);
			SpellHandler.Apply(spell =>
			{
				spell.Effects[1].TriggerSpellId = SpellId.ClassSkillLivingBombRank3_2;
				spell.Effects[1].AuraEffectHandlerCreator =
					() => new TriggerSpellAfterAuraRemovedHandler();
			}, SpellId.ClassSkillLivingBombRank3);

			// These spells cancel eachother
			AuraHandler.AddAuraGroup(SpellLineId.MageFrostArmor, SpellLineId.MageIceArmor, SpellLineId.MageMageArmor, SpellLineId.MageMoltenArmor);

			// Mana gems don't have a limit
			SpellHandler.Apply(spell =>
			{
				spell.RemoveEffect(SpellEffectType.CreateItem);
				var efct = spell.GetEffect(SpellEffectType.Dummy);
				efct.EffectType = SpellEffectType.TriggerSpell;
				efct.TriggerSpellId = (SpellId)efct.CalcEffectValue();
			}, SpellLineId.MageConjureManaGem);

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

		}

		public class TriggerSpellAfterAuraRemovedHandler : AuraEffectHandler
		{
			protected override void Remove(bool cancelled)
			{
				if (!cancelled)
				{
					var triggerSpell = m_spellEffect.TriggerSpell;

					var caster = m_aura.CasterUnit;
					if (caster != null)
					{
						var loc = m_aura.Auras.Owner.Position;
						SpellCast.Trigger(caster, triggerSpell, ref loc);
					}
				}

				base.Remove(cancelled);
			}
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
	}
}