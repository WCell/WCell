/*************************************************************************
 *
 *   file		: SkillOutput.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-24 01:12:44 +0100 (sø, 24 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1213 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WCell.Core;
using WCell.Constants;
using WCell.RealmServer;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.Tools.Domi;
using WCell.Util.Toolshed;

namespace WCell.Tools.Domi.Output
{
	public class SkillOutput
	{
		[Tool]
		public static void WriteSkillInfos()
		{
			SpellOutput.Init();

			WriteAllAbilities();
			//WriteSRCInfos();
			//WriteSkillHierarchy();
		}

		public static void WriteSRCInfos()
		{
			//DBCEnumBuilder.WriteSpellEnums();
			//SpellEffectCreator.CreateAll();
			//WCell.RealmServer.Spells.Spells.Init();

			// Ralek.Program.RalekMain();

			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "/SkillsRaceClassInfos.txt", false))
			{
				for (int race = 0; race < SkillHandler.RaceClassInfos.Length; race++)
				{
					var byClass = SkillHandler.RaceClassInfos[race];
					if (byClass != null)
					{
						string indent = "";
						writer.WriteLine(indent + "Race: " + (RaceId) race);
						for (int clss = 0; clss < byClass.Length; clss++)
						{
							var infos = byClass[clss];
							if (infos != null)
							{
								indent = "\t";
								writer.WriteLine(indent + "Class: " + (ClassId) clss);
								foreach (SkillRaceClassInfo info in infos.Values)
								{
									writer.WriteLine(info.ToString(indent + "\t"));
								}
							}
						}
					}
				}
			}
		}

		public static void WriteAllAbilities()
		{
			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "/SkillAbilities.txt", false))
			{
				foreach (var abilities in SkillHandler.AbilitiesBySkill)
				{
					if (abilities != null)
					{
						writer.WriteLine("Skill: " + abilities.First().Skill);
						foreach (SkillAbility ability in abilities)
						{
							writer.WriteLine("\t" + ability);
						}
						writer.WriteLine();
						writer.WriteLine();
						writer.WriteLine();
					}
				}
			}
		}


		public static void WriteAbilitiesWhere(string name, Predicate<SkillAbility> predicate)
		{
			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "/" + name + ".txt", false))
			{
				var toWrite = new List<SkillAbility>();
				foreach (var abilities in SkillHandler.AbilitiesBySkill)
				{
					if (abilities != null)
					{
						toWrite.Clear();
						foreach (var ability in abilities)
						{
							if (predicate(ability))
							{
								toWrite.Add(ability);
							}
						}

						if (toWrite.Count > 0)
						{
							writer.WriteLine("Skill: " + abilities.First().Skill);
							foreach (var ability in toWrite)
							{
								writer.WriteLine("\t" + ability);
							}
							writer.WriteLine();
							writer.WriteLine();
						}
					}
				}
			}
		}

		public static void WriteSkillHierarchy()
		{
			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "/SkillHierarchy.txt", false))
			{
				foreach (var abilities in SkillHandler.AbilitiesBySkill)
				{
					if (abilities != null)
					{
						writer.WriteLine("Skill: " + abilities.First().Skill);
						foreach (SkillAbility ability in abilities)
						{
							if (ability.PreviousAbility == null)
							{
								writer.WriteLine("\t" + ability);
							}
						}
						writer.WriteLine();
						writer.WriteLine();
						writer.WriteLine();
					}
				}
			}
		}
	}
}