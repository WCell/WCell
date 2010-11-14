using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Quests
{
	public static class QuestConstants
	{
		/// <summary>
		/// Needed for certain Packets
		/// </summary>
		public static readonly uint[] MenuStatusLookup = new uint[(int)QuestStatus.Count];

		static QuestConstants()
		{
			MenuStatusLookup[(int)QuestStatus.Completable] = 4;
			MenuStatusLookup[(int)QuestStatus.RepeateableCompletable] = 4;
			MenuStatusLookup[(int)QuestStatus.Obsolete] = 6;
			MenuStatusLookup[(int)QuestStatus.NotCompleted] = 1;
		}

		public const int MaxReputations = 5;
		public const int MaxRewardItems = 4;
		public const int MaxRewardChoiceItems = 6;
		public const int MaxObjectInteractions = 4;
		public const int MaxReceivedItems = 4;
        public const int MaxObjectiveTexts = 4;
		public const int MaxRequirements = 4;
		public const int MaxEmotes = 4;
		public const int MaxQuestsPerQuestGiver = 20;

		/// <summary>
		/// Used in certain Packets
		/// </summary>
		public const uint GOIndicator = 0x80000000;

		public static ClassId GetClassId(this QuestSort sort)
		{
			switch (sort)
			{
				case QuestSort.Warlock: return ClassId.Warlock;
				case QuestSort.Warrior: return ClassId.Warrior;
				case QuestSort.Shaman: return ClassId.Shaman;
				case QuestSort.Paladin: return ClassId.Paladin;
				case QuestSort.Mage: return ClassId.Mage;
				case QuestSort.Rogue: return ClassId.Rogue;
				case QuestSort.Hunter: return ClassId.Hunter;
				case QuestSort.Priest: return ClassId.Priest;
				case QuestSort.Druid: return ClassId.Druid;
				case QuestSort.DeathKnight: return ClassId.DeathKnight;
			}
			return ClassId.End;
		}
	}
}