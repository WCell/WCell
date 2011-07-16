/*************************************************************************
 *
 *   file		: TalentOutput.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-06-06 18:09:15 +0200 (lø, 06 jun 2009) $
 
 *   revision		: $Rev: 950 $
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
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Talents;

namespace WCell.Tools.Domi.Output
{
	public class TalentOutput
	{
		public static void WriteTalentInfos()
		{
			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "/Talents.txt", false))
			{
				foreach (var clssType in WCellConstants.AllClassIds)
				{
					if (clssType == ClassId.End)
					{
						break;
					}
					var trees = TalentMgr.TreesByClass[(int) clssType];
					writer.WriteLine("###########################################################");
					writer.WriteLine(clssType);
					foreach (TalentTree tree in trees)
					{
						writer.WriteLine("\t" + tree);
						foreach (TalentEntry talent in tree.Talents)
						{
							writer.WriteLine("\t\t" + talent);
						}
						writer.WriteLine();
					}
					writer.WriteLine();
					writer.WriteLine();
				}
			}
		}
	}
}