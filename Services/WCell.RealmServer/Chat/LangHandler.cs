/*************************************************************************
 *
 *   file			: LangHandler.cs
 *   copyright		: (C) The WCell Team
 *   email			: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-20 06:16:32 +0100 (lø, 20 feb 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1257 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Util;

namespace WCell.RealmServer.Chat
{
	/// <summary>
	/// The Language description class
	/// </summary>
	public class LanguageDescription
	{
		public LanguageDescription(ChatLanguage lang, SpellId spell, SkillId skill)
		{
			Language = lang;
			SpellId = spell;
			SkillId = skill;
		}

		#region Fields

		public ChatLanguage Language { get; set; }
		public SpellId SpellId { get; set; }
		public SkillId SkillId { get; set; }

		#endregion
	}

	/// <summary>
	/// Chat System class
	/// </summary>
	public static class LanguageHandler
	{
		private static Logger s_log = LogManager.GetCurrentClassLogger();

		public static readonly LanguageDescription[] ByLang = new LanguageDescription[(int)ChatLanguage.End];
		public static readonly LanguageDescription[] ByRace = new LanguageDescription[(int)RaceId.End];

		/// <summary>
		/// Constructor
		/// </summary>
		static LanguageHandler()
		{
			// Adding languages avaliable to list
			ByLang[(int)ChatLanguage.Orcish] = new LanguageDescription(ChatLanguage.Orcish, SpellId.LanguageOrcish, SkillId.LanguageOrcish);
			ByLang[(int)ChatLanguage.Darnassian] = new LanguageDescription(ChatLanguage.Darnassian, SpellId.LanguageDarnassian, SkillId.LanguageDarnassian);
			ByLang[(int)ChatLanguage.Taurahe] = new LanguageDescription(ChatLanguage.Taurahe, SpellId.LanguageTaurahe, SkillId.LanguageTaurahe);
			ByLang[(int)ChatLanguage.Dwarvish] = new LanguageDescription(ChatLanguage.Dwarvish, SpellId.LanguageDwarven, SkillId.LanguageDwarven);
			ByLang[(int)ChatLanguage.Common] = new LanguageDescription(ChatLanguage.Common, SpellId.LanguageCommon, SkillId.LanguageCommon);
			ByLang[(int)ChatLanguage.DemonTongue] = new LanguageDescription(ChatLanguage.DemonTongue, SpellId.LanguageDemonTongue, SkillId.LanguageDemonTongue);
			ByLang[(int)ChatLanguage.Titan] = new LanguageDescription(ChatLanguage.Titan, SpellId.LanguageTitan, SkillId.LanguageTitan);
			ByLang[(int)ChatLanguage.Thalassian] = new LanguageDescription(ChatLanguage.Thalassian, SpellId.LanguageThalassian, SkillId.LanguageThalassian);
			ByLang[(int)ChatLanguage.Draconic] = new LanguageDescription(ChatLanguage.Draconic, SpellId.LanguageDraconic, SkillId.LanguageDraconic);
			ByLang[(int)ChatLanguage.OldTongue] = new LanguageDescription(ChatLanguage.OldTongue, SpellId.LanguageOldTongueNYI, SkillId.LanguageOldTongue);
			ByLang[(int)ChatLanguage.Gnomish] = new LanguageDescription(ChatLanguage.Gnomish, SpellId.LanguageGnomish, SkillId.LanguageGnomish);
			ByLang[(int)ChatLanguage.Troll] = new LanguageDescription(ChatLanguage.Troll, SpellId.LanguageTroll, SkillId.LanguageTroll);
			ByLang[(int)ChatLanguage.Forsaken] = new LanguageDescription(ChatLanguage.Forsaken, SpellId.LanguageGutterspeak, SkillId.LanguageForsaken);
			ByLang[(int)ChatLanguage.Draenei] = new LanguageDescription(ChatLanguage.Draenei, SpellId.LanguageDraenei, SkillId.LanguageDraenei);
            ByLang[(int)ChatLanguage.Zombie] = new LanguageDescription(ChatLanguage.Zombie, SpellId.LanguageDraenei, SkillId.LanguageDraenei);
            ByLang[(int)ChatLanguage.Gilnean] = new LanguageDescription(ChatLanguage.Gilnean, SpellId.LanguageGilnean, SkillId.LanguageGilnean);
            ByLang[(int)ChatLanguage.Goblin] = new LanguageDescription(ChatLanguage.Goblin, SpellId.LanguageGoblin, SkillId.LanguageGoblin);

			ByRace[(int)RaceId.Orc] = ByLang[(int)ChatLanguage.Orcish];
			ByRace[(int)RaceId.Human] = ByLang[(int)ChatLanguage.Common];
			ByRace[(int)RaceId.Dwarf] = ByLang[(int)ChatLanguage.Common];
			ByRace[(int)RaceId.NightElf] = ByLang[(int)ChatLanguage.Darnassian];
			ByRace[(int)RaceId.Undead] = ByLang[(int)ChatLanguage.Forsaken];
			ByRace[(int)RaceId.Tauren] = ByLang[(int)ChatLanguage.Taurahe];
			ByRace[(int)RaceId.Gnome] = ByLang[(int)ChatLanguage.Gnomish];
			ByRace[(int)RaceId.Troll] = ByLang[(int)ChatLanguage.Troll];
			ByRace[(int)RaceId.Goblin] = ByLang[(int)ChatLanguage.Goblin];
			ByRace[(int)RaceId.BloodElf] = ByLang[(int)ChatLanguage.Thalassian];
			ByRace[(int)RaceId.Draenei] = ByLang[(int)ChatLanguage.Draenei];
			ByRace[(int)RaceId.FelOrc] = ByLang[(int)ChatLanguage.Orcish];
			ByRace[(int)RaceId.Naga] = ByLang[(int)ChatLanguage.Orcish];
			ByRace[(int)RaceId.Broken] = ByLang[(int)ChatLanguage.DemonTongue];
			ByRace[(int)RaceId.Skeleton] = ByLang[(int)ChatLanguage.Forsaken];
            ByRace[(int)RaceId.Worgen] = ByLang[(int)ChatLanguage.Gilnean];
            ByRace[(int)RaceId.Gilnean] = ByLang[(int)ChatLanguage.Gilnean];
            ByRace[(int)RaceId.Vrykul] = ByLang[(int)ChatLanguage.Orcish];
            ByRace[(int)RaceId.Tuskarr] = ByLang[(int)ChatLanguage.Orcish];
            ByRace[(int)RaceId.ForestTroll] = ByLang[(int)ChatLanguage.Orcish];
            ByRace[(int)RaceId.Taunka] = ByLang[(int)ChatLanguage.Orcish];
            ByRace[(int)RaceId.NorthrendSkeleton] = ByLang[(int)ChatLanguage.Orcish];
            ByRace[(int)RaceId.IceTroll] = ByLang[(int)ChatLanguage.Orcish];
		}

		#region Get language description Methods

		/// <summary>
		/// Get language description by Type
		/// </summary>
		/// <param name="language">the Language type</param>
		/// <returns></returns>
		public static LanguageDescription GetLanguageDescByType(ChatLanguage language)
		{
			return ByLang.Get((uint)language);
		}

		/// <summary>
		/// Get language description by Spell Id
		/// </summary>
		/// <param name="spell">spell type</param>
		/// <returns></returns>
		public static LanguageDescription GetLanguageDescBySpellId(SpellId spell)
		{
			for (int i = 0; i < ByLang.Length; i++)
			{
				if (ByLang[i] != null && ByLang[i].SpellId == spell)
				{
					return ByLang[i];
				}
			}
			return null;
		}

		/// <summary>
		/// Get language description by Spell Id
		/// </summary>
		/// <returns></returns>
		public static LanguageDescription GetLanguageDescByRace(RaceId race)
		{
			return ByRace.Get((uint)race);
		}

		/// <summary>
		/// Get language description by Skill Type
		/// </summary>
		/// <param name="skillLanguage">Skill type</param>
		/// <returns></returns>
		public static LanguageDescription GetLanguageDescBySkillType(SkillId skillLanguage)
		{
			for (int i = 0; i < ByLang.Length; i++)
			{
				if (ByLang[i] != null && ByLang[i].SkillId == skillLanguage)
				{
					return ByLang[i];
				}
			}

			return null;
		}

		#endregion
	}
}