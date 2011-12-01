using WCell.Constants.Items;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells
{
	public static class MiscSpells
	{
		/// <summary>
		/// Some Spells need special handling
		/// </summary>
		[Initialization(InitializationPass.Second)]
		public static void Fix()
		{
            // Mark Of the chosen effect has wrong Misc Value
            SpellHandler.Apply(spell =>
            {
                spell.Effects[0].MiscValue = -1;
            }, SpellId.EffectMarkOfTheChosenRank1);

            // Mark Of the chosen effect has wrong Misc Value
            SpellHandler.Apply(spell =>
            {
                spell.Effects[0].MiscValue = (int)StatType.Agility;
            }, SpellId.ClassSkillIntoTheWildernessPassive);

			// Vehicle Spells need to send the vehicle as target and apply the vehicle aura to the caster
			SpellHandler.Apply(spell =>
			{
                spell.SpellTargetRestrictions = new SpellTargetRestrictions { TargetFlags = SpellTargetFlags.Unit }; 
				var eff0 = spell.Effects[0];
				spell.Effects[0] = spell.Effects[1];
				spell.Effects[1] = eff0;
			}, SpellId.EnragedMammoth);

			// Deserter needs a custom AuraEffectHandler
			SpellHandler.Apply(spell =>
			{
				spell.Durations.Min = spell.Durations.Max = 15 * 60 * 1000;
				spell.Effects[0].AuraEffectHandlerCreator = () => new AuraDeserterHandler();
			}, SpellId.Deserter);

            //SpellHandler.Apply(spell =>
            //                       {
            //                           spell.Effects[0].
            //                       });

			FixDefaultInterrupt();

		    FixMounts();

			SpellHandler.Apply(spell =>
			{
				var eff = spell.Effects[0];
				eff.AuraEffectHandlerCreator = () => new ResSicknessHandler();

			}, SpellId.ResurrectionSickness);

			SpellHandler.Apply(spell =>
			{
				spell.Effects[1].SpellEffectHandlerCreator = (cast, effect) => new CopyWeapon(cast, effect);
				spell.Effects[2].SpellEffectHandlerCreator = (cast, effect) => new CopyOffHandWeapon(cast, effect);
			}, SpellId.CloneMe);

		}

		#region Interrupt
		/// <summary>
		/// The default interrupt spell is used by a multitude of spells
		/// </summary>
		private static void FixDefaultInterrupt()
		{
			// Only for "Non-player victim"
			SpellHandler.Apply(spell => 
				spell.GetEffect(SpellEffectType.InterruptCast).SpellEffectHandlerCreator = (cast, effct) => new NonPlayerVictimInterruptHandler(cast, effct),
				SpellId.InterruptRank1);
		}

		internal class NonPlayerVictimInterruptHandler : SpellEffectHandler
		{
			public NonPlayerVictimInterruptHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
			{
			}

			protected override void Apply(WorldObject target)
			{
				if (!target.IsPlayer)
				{
					base.Apply(target);
				}
			}
		}
		#endregion

		#region Special Mounts
        private static void FixMounts()
        {
            // Invincible
            //SpellHandler.Apply(spell =>
            //{
            //    spell.Effects[2].SpellEffectHandlerCreator =
            //        (cast, effect) =>
            //        new MountSpellHandler(cast, effect, SpellId.Invincible, SpellId.Invincible_2,
            //                              SpellId.Invincible_3, SpellId.Invincible_4);
            //},
            //                   SpellId.ClassSkillInvincible);

            // Celestial Steed
            //SpellHandler.Apply(spell =>
            //{
            //    spell.Effects[2].SpellEffectHandlerCreator =
            //        (cast, effect) =>
            //        new MountSpellHandler(cast, effect, SpellId.CelestialSteed_3, SpellId.CelestialSteed_4,
            //                              SpellId.CelestialSteed, SpellId.CelestialSteed_2);
            //},
            //                   SpellId.ClassSkillCelestialSteed);

            // Big Love Rocket
            //SpellHandler.Apply(spell =>
            //{
            //    spell.Effects[2].SpellEffectHandlerCreator =
            //        (cast, effect) =>
            //        new MountSpellHandler(cast, effect, SpellId.BigLoveRocket_2, SpellId.BigLoveRocket_3,
            //                              SpellId.BigLoveRocket_4, SpellId.BigLoveRocket_5);
            //},
            //                   SpellId.ClassSkillBigLoveRocket);

            // Winged Steed of the Ebon Blade
            SpellHandler.Apply(spell =>
                spell.AddEffect((cast, effect) =>
                    new MountSpellHandler(cast, effect, SpellId.None, SpellId.None,
                        SpellId.WingedSteedOfTheEbonBlade, SpellId.WingedSteedOfTheEbonBlade_2),
                        ImplicitSpellTargetType.Self),
                               SpellId.ClassSkillWingedSteedOfTheEbonBlade);

            // X-53 Touring Rocket
            SpellHandler.Apply(spell =>
                spell.AddEffect((cast, effect) =>
                    new MountSpellHandler(cast, effect, SpellId.None, SpellId.None,
                        SpellId.X53TouringRocket, SpellId.X53TouringRocket),
                        ImplicitSpellTargetType.Self),
                               SpellId.ClassSkillX53TouringRocket);

            // Blazing Hippogryph
            SpellHandler.Apply(spell =>
                spell.AddEffect((cast, effect) =>
                    new MountSpellHandler(cast, effect, SpellId.None, SpellId.None,
                        SpellId.BlazingHippogryph, SpellId.BlazingHippogryph_2), ImplicitSpellTargetType.Self),
                               SpellId.ClassSkillBlazingHippogryph);
        }

		public class AuraDeserterHandler : AuraEffectHandler
		{
			protected override void Apply()
			{
				var chr = m_aura.Auras.Owner as Character;
				if (chr != null)
				{
					chr.Battlegrounds.IsDeserter = true;
				}
			}

			protected override void Remove(bool cancelled)
			{
				var chr = m_aura.Auras.Owner as Character;
				if (chr != null)
				{
					chr.Battlegrounds.IsDeserter = false;
				}
			}
		}

        public class MountSpellHandler : SpellEffectHandler
        {
            private readonly SpellId _apprenticeRidingSpell;
            private readonly SpellId _journeymanRidingSpell;
            private readonly SpellId _expertRidingSpell;
            private readonly SpellId _artisanRidingSpell;

            public MountSpellHandler(SpellCast cast, SpellEffect effect, SpellId apprenticeRidingSkill, SpellId journeymanRidingSkill, SpellId expertRidingSkill, SpellId artisanRidingSkill)
                : base(cast, effect)
            {
                _apprenticeRidingSpell = apprenticeRidingSkill;
                _journeymanRidingSpell = journeymanRidingSkill;
                _expertRidingSpell = expertRidingSkill;
                _artisanRidingSpell = artisanRidingSkill;
            }

            public override void Apply()
            {
                var caster = m_cast.CasterUnit as Character;
                if (caster == null) return;

                if (_artisanRidingSpell != SpellId.None &&
                    caster.Spells.Contains(SpellId.SecondarySkillArtisanRidingArtisan) && caster.Map.CanFly)
                {
                    m_cast.Trigger(_artisanRidingSpell, caster);
                }
                else if (_expertRidingSpell != SpellId.None &&
                         caster.Spells.Contains(SpellId.SecondarySkillExpertRidingExpert) && caster.Map.CanFly)
                {
                    m_cast.Trigger(_expertRidingSpell, caster);
                }
                else if (_journeymanRidingSpell != SpellId.None &&
                         caster.Spells.Contains(SpellId.SecondarySkillJourneymanRidingJourneyman))
                {
                    m_cast.Trigger(_journeymanRidingSpell, caster);
                }
                else if (_apprenticeRidingSpell != SpellId.None &&
                         caster.Spells.Contains(SpellId.SecondarySkillApprenticeRidingApprentice))
                {
                    m_cast.Trigger(_apprenticeRidingSpell, caster);
                }
            }
        }
		#endregion

		#region ResSicknessHandler
		public class ResSicknessHandler : ModStatPercentHandler
		{
			protected override void Apply()
			{
				base.Apply();
				var chr = (Character)m_aura.Owner;
				if (chr != null)
				{
					var aura = chr.Auras[SpellId.ResurrectionSickness];
					if (aura != null)
					{
						if (chr.Level < 20)
						{
							aura.Duration = (chr.Level - 10) * 60000;
							AuraHandler.SendAuraUpdate(aura.Owner, aura);
						}
						else
						{
							aura.Duration = 600000;
							AuraHandler.SendAuraUpdate(aura.Owner, aura);
						}
					}
				}

			}
		}
		#endregion

		#region CopyWeaponHandler
		public class CopyWeapon : ScriptEffectHandler
		{
			public CopyWeapon(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}
			protected override void Apply(WorldObject target)
			{
				var image = (Unit)target;
				var chr = m_cast.CasterChar;

				if (image != null && chr != null)
				{
					var item = (Item)chr.GetWeapon(EquipmentSlot.MainHand);
					if (item != null)
					{
						image.VirtualItem1 = item.Template.ItemId;
						chr.SpellCast.Trigger(SpellHandler.Get((uint)m_cast.Spell.Effects[1].BasePoints), image);
					}

				}
			}
		};
		#endregion
		#region CopyOffHandWeaponHandler
		public class CopyOffHandWeapon : ScriptEffectHandler
		{
			public CopyOffHandWeapon(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}
			protected override void Apply(WorldObject target)
			{
				var image = (Unit)target;
				var chr = m_cast.CasterChar;

				if (image != null && chr != null)
				{
					var item = (Item)chr.GetWeapon(EquipmentSlot.OffHand);
					if (item != null)
					{
						image.VirtualItem2 = item.Template.ItemId;
						chr.SpellCast.Trigger(SpellHandler.Get((uint)m_cast.Spell.Effects[2].BasePoints), image);
					}
				}
			}
		};
		#endregion
	}
}