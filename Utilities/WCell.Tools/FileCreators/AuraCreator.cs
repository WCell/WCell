/*************************************************************************
 *
 *   file		: AuraCreator.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 19:35:36 +0800 (Thu, 31 Jan 2008) $
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
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Effects;
using System.Diagnostics;
using WCell.Constants.Spells;

namespace WCell.Tools.FileCreators
{
	public class AuraCreator
	{
		private static DirectoryInfo auraDir = new DirectoryInfo("WCell.RealmServer/Spells/Auras/");

		public static void WriteAuraEffects()
		{
			using (StreamWriter writer = new StreamWriter(auraDir.FullName + "_Auras.cs", false))
			{
				foreach (AuraType type in Enum.GetValues(typeof (AuraType)))
				{
					CreateAuraFile(type);
					writer.WriteLine("EffectHandlers.Add(AuraType.None, () => new {0}Handler());", type);
				}
			}
		}

		public static void CreateAuraFile(AuraType type)
		{
			string fname = type + ".cs";
			FileInfo[] files = auraDir.GetFiles(fname, SearchOption.AllDirectories);

			if (files.Length == 0)
			{
				throw new Exception("AuraFile not found: " + fname);
			}

			using (var writer = new StreamWriter(files[0].FullName, false))
			{
				writer.WriteLine("using System;");
				writer.WriteLine("using System.Collections.Generic;");
				writer.WriteLine("using System.Linq;");
				writer.WriteLine("using System.Text;");
				writer.WriteLine();
				writer.WriteLine("using WCell.RealmServer.Entities;");
				writer.WriteLine("using WCell.RealmServer.Spells;");
				writer.WriteLine("using WCell.RealmServer.Spells.Auras;");
				writer.WriteLine();
				writer.WriteLine("namespace WCell.RealmServer.Spells.Auras.Handlers");
				writer.WriteLine("{");
				writer.WriteLine("	public class {0}Handler : AuraEffectHandler", type);
				writer.WriteLine("	{");
				writer.WriteLine();
				writer.WriteLine("		protected internal override void Apply()");
				writer.WriteLine("		{");
				writer.WriteLine();
				writer.WriteLine("		}");
				writer.WriteLine();
				writer.WriteLine("	}");
				writer.WriteLine("};");
			}
		}
	}
}