using WCell.Constants.NPCs;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs.Trainers
{
	/// <summary>
	/// Represents something that a trainer can teach
	/// </summary>
	[DataHolder]
	public class TrainerSpellEntry : IDataHolder
	{
		public NPCId TrainerId;

		public SpellId SpellId;

		///// <summary>
		///// The Spell to be casted when training <see cref="SpellId"/>.
		///// </summary>
		//public SpellId CastSpellId;

		/// <summary>
		/// The base cost of the spell at the trainer.
		/// </summary>
		public uint Cost;

		///// <summary>
		///// The Id of a spell that is to be deleted when learning this one.
		///// </summary>
		//public SpellId DeleteSpellId;

		/// <summary>
		/// 
		/// </summary>
		public SpellId RequiredSpellId;

		/// <summary>
		/// The minimum level a character must have acheived in order to purchase this spell.
		/// </summary>
		public int RequiredLevel;

		/// <summary>
		/// The required profession or secondary skill that this character must be trained in in order to purchase this spell.
		/// </summary>
		public SkillId RequiredSkillId;

		/// <summary>
		/// The required level of skill that a character must have obtained in the RequiredSkill in order to purchase this spell.
		/// </summary>
		public uint RequiredSkillAmount;

		/// <summary>
		/// The spell that the character will learn upon purchase of this TrainerSpellEntry from a trainer.
		/// </summary>
		[NotPersistent]
		public Spell Spell;

		/// <summary>
		/// The index of this Entry within the Trainer list
		/// </summary>
		[NotPersistent]
		public int Index;

		/// <summary>
		/// The price of the spell after Reputation discounts are applied.
		/// </summary>
		public uint GetDiscountedCost(Character character, NPC trainer)
		{
			if (character == null || trainer == null)
				return Cost;
			return character.Reputations.GetDiscountedCost(trainer.Faction.ReputationIndex, Cost);
		}

		/// <summary>
		/// The availability of the spell for the spell list filter.
		/// </summary>
		/// <returns>Available, Unavailable, AlreadyKnown</returns>
		public TrainerSpellState GetTrainerSpellState(Character character)
		{
			var spell = Spell;
			if (spell.IsTeachSpell)
			{
				spell = spell.LearnSpell;
			}

			if (character.Spells.Contains(spell.Id))
			{
				return TrainerSpellState.AlreadyLearned;
			}

			if (spell.PreviousRank != null && !character.Spells.Contains(spell.PreviousRank.Id))
			{
				return TrainerSpellState.Unavailable;
			}

			if (spell.Ability == null || RequiredLevel > 0 && character.Level < RequiredLevel)
			{
				return TrainerSpellState.Unavailable;
			}

			if (RequiredSpellId != 0)
				return TrainerSpellState.Unavailable;

			if (RequiredSkillId != 0 && !character.Skills.CheckSkill(RequiredSkillId, (int)RequiredSkillAmount))
				return TrainerSpellState.Unavailable;

			if (Spell.IsProfession && Spell.TeachesApprenticeAbility && character.Skills.FreeProfessions == 0)
				return TrainerSpellState.Unavailable;

			return TrainerSpellState.Available;
		}

		#region IDataHolder Members

		public void FinalizeDataHolder()
		{
			if ((Spell = SpellHandler.Get(SpellId)) == null)
			{
				ContentMgr.OnInvalidDBData("SpellId is invalid in " + this);
			}
			else if (RequiredSpellId != SpellId.None && SpellHandler.Get(RequiredSpellId) == null)
			{
				ContentMgr.OnInvalidDBData("RequiredSpellId is invalid in " + this);
			}
			else if (RequiredSkillId != SkillId.None && SkillHandler.Get(RequiredSkillId) == null)
			{
				ContentMgr.OnInvalidDBData("RequiredSkillId is invalid in " + this);
			}
			else
			{
				var trainer = NPCMgr.GetEntry(TrainerId);
				if (trainer == null)
				{
					ContentMgr.OnInvalidDBData("TrainerId is invalid in " + this);
				}
				else
				{
					if (RequiredLevel == 0 && Spell.SpellLevels != null)
					{
						RequiredLevel = Spell.SpellLevels.Level;
					}

					if (trainer.TrainerEntry == null)
					{
						trainer.TrainerEntry = new TrainerEntry();
					}
					trainer.TrainerEntry.AddSpell(this);
				}
			}
		}

		#endregion

		public override string ToString()
		{
			return string.Format("TrainerSpellEntry (Trainer: {0}, Spell: {1}, RequiredSpell: {2}, Required Skill: {3} ({4}))",
				TrainerId, SpellId, RequiredSpellId, RequiredSkillId, RequiredSkillAmount);
		}
	}
}