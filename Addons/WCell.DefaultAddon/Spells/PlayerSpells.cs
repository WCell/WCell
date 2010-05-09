using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.Constants.Spells;
using WCell.RealmServer.Skills;
using WCell.Util;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Entities;
using WCell.Constants.Misc;
using WCell.Util.Graphics;
using WCell.Constants.Skills;

namespace WCell.Addons.Default.Spells
{
	/// <summary>
	/// This file contains some hardcoded Spell fixes.
	/// It should be replaced with by values that are read from the DB ASAP.
	/// </summary>
	public static class PlayerSpells
	{
		/// <summary>
		/// After Spells were loaded and before overload info is applied
		/// </summary>
		[Initialization(InitializationPass.Second)]
		public static void FixPlayerSpells()
		{
			FixRacials();
			FixSkills();
		}

		#region Racials
		static void FixRacials()
		{
			// Cannibalize' info is a different spell
			var cannibalize = SpellHandler.Get(SpellId.SecondarySkillCannibalizeRacial);
			var cannibalize2 = SpellHandler.Get(SpellId.Cannibalize);
			cannibalize.TargetFlags = SpellTargetFlags.UnitCorpse;
			cannibalize.Durations = cannibalize2.Durations;
			cannibalize.AttributesEx |= cannibalize2.AttributesEx;
			cannibalize.InterruptFlags |= InterruptFlags.OnMovement | InterruptFlags.OnTakeDamage;
			cannibalize.ChannelInterruptFlags |= cannibalize2.ChannelInterruptFlags;
			cannibalize.Visual = cannibalize2.Visual;
			// copy effects
			var effects = cannibalize.Effects.ToList();
			effects.AddRange(cannibalize2.Effects);
			cannibalize.Effects = effects.ToArray();

			// Stoneform is missing a trigger spell (because it already has 3 other effects)
			SpellHandler.Apply(spell =>
			{
				spell.AddTriggerSpellEffect(SpellId.StoneformRacial, ImplicitTargetType.Self);
			},
			SpellId.SecondarySkillStoneformRacial);
		}
		#endregion

		#region Skills
		private static void FixSkillSpells()
		{
			// Throw does not require to be behind the target
			SpellHandler.Apply(spell =>
			{
				spell.AttributesExB &= ~SpellAttributesExB.RequiresBehindTarget;
			},
			SpellId.WeaponProficiencyThrow);

		}

		[Initialization(InitializationPass.Fifth)]
		public static void FixSkills()
		{
			// Jewelcrafting also gives Prospecting
			SpellHandler.Apply(spell => spell.AdditionallyTaughtSpells.Add(SpellHandler.Get(SpellId.ProfessionProspecting)),
				SpellId.ProfessionJewelcraftingApprentice);
		}
		#endregion
	}
}