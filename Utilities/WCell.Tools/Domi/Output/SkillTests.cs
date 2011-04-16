/*************************************************************************
 *
 *   file		: SkillTests.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 12:35:36 +0100 (to, 31 jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
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
using WCell.RealmServer;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;

using WCell.Tools.Domi;

namespace WCell.Tools.Domi.Test
{
	public class SkillTesting
	{
		public static void Go()
		{
			Spells.Init();
			Skills.Init();
			//DBCEnumBuilder.WriteSpellEnums();
			//SpellEffectCreator.CreateAll();
			//WCell.RealmServer.Spells.Spells.Init();

           // Ralek.Program.RalekMain();

			using (StreamWriter writer = new StreamWriter("WCell.Tools/output/SkillsRaceClassInfos.txt", false)) {
				foreach (KeyValuePair<RaceType, Dictionary<ClassType, Dictionary<SkillId, SkillRaceClassInfo>>> byClass in Skills.RaceClassInfos) {
					string indent = "";
					writer.WriteLine(indent + "Race: " + byClass.Key);
					foreach (KeyValuePair<ClassType, Dictionary<SkillId, SkillRaceClassInfo>> infos in byClass.Value) {
						indent = "\t";
						writer.WriteLine(indent + "Class: " + infos.Key);
						foreach (SkillRaceClassInfo info in infos.Value.Values) {
							writer.WriteLine(info.ToString("\t\t"));
						}
					}
				}
			}
		}
	}
}