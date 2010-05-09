using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.Util.Data;
using System;

namespace WCell.RealmServer.NPCs.Trainers
{
	/// <summary>
	/// Represents everything an NPC trainer has to offer
	/// </summary>
	//[DataHolder]
	public class TrainerEntry //: IDataHolder
	{
		/// <summary>
		/// The major type of trainer this is (Class, Profession, Secondary Skill, etc.)
		/// </summary>
		public TrainerType TrainerType = TrainerType.NotATrainer;

		///// <summary>
		///// The minor type of trainer this is (Druid, Alchemy, Skinning, etc.)
		///// </summary>
		//public TrainerSubType TrainerSubType = TrainerSubType.NotATrainer;

		[NotPersistent]
		/// <summary>
		/// A Map of Spells this trainer has to sell, indexed by SpellId.
		/// </summary>
		public readonly IDictionary<SpellId, TrainerSpellEntry> Spells =
			new Dictionary<SpellId, TrainerSpellEntry>(20);

		/// <summary>
		/// Text dislayed in the upper panel of the client's trainer list menu.
		/// </summary>
		public string Message = "Hello!";

		//// Requirements:

		[NotPersistent]
		/// <summary>
		/// Characters are required to have this Class to learn from this Trainer
		/// </summary>
		public ClassMask ClassMask;

		[NotPersistent]
		/// <summary>
		/// Characters are required to have this Race to learn from this Trainer
		/// </summary>
		public RaceMask RaceMask;

		/// <summary>
		/// The required profession or secondary skill that the character must have to learn from this Trainer
		/// </summary>
		public SpellId RequiredSpellId;

		//[NotPersistent]
		//public Spell RequiredSpell;

		///// <summary>
		///// The required profession or secondary skill that the character must have to learn from this Trainer
		///// </summary>
		//public SkillId RequiredSkillId;

		///// <summary>
		///// The required level of skill that a character must have obtained in the RequiredSkill in order to learn from this Trainer.
		///// </summary>
		//public int RequiredSkillAmount;

		public void Sort(IComparer<TrainerSpellEntry> comparer)
		{
			// TODO: Provide custom sorting for SpellEntries
		}

		private int lastIndex;
		public void AddSpell(TrainerSpellEntry entry)
		{
			if (Spells.Count == 0)
			{
				SetupTrainer(entry.Spell);
			}
			if (!Spells.ContainsKey(entry.SpellId))
			{
				Spells.Add(entry.SpellId, entry);
				entry.Index = lastIndex++;
			}
		}

		/// <summary>
		/// Determine Trainer-information, based on first Spell
		/// </summary>
		/// <param name="spell"></param>
		private void SetupTrainer(Spell spell)
		{
			// determine Classes & Races
			if (spell.Ability != null)
			{
				RaceMask = spell.Ability.RaceMask;
				ClassMask = spell.Ability.ClassMask;
			}
		}

		/// <summary>
		/// Whether this NPC can train the character in their specialty.
		/// </summary>
		/// <returns>True if able to train.</returns>
		public bool CanTrain(Character chr)
		{
			return (RequiredSpellId == 0 || chr.Spells.Contains(RequiredSpellId)) &&
				(RaceMask == 0 || RaceMask.Has(chr.RaceMask)) &&
				(ClassMask == 0 || ClassMask.Has(chr.ClassMask));
		}

		/// <summary>
		/// Returns the TrainerSpellEntry from SpellsForSale with the given spellId, else null.
		/// </summary>
		public TrainerSpellEntry GetSpellEntry(SpellId spellId)
		{
			TrainerSpellEntry entry;
			Spells.TryGetValue(spellId, out entry);
			return entry;
		}
	}
}
