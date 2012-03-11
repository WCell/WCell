/*************************************************************************
 *
 *   file		: SpellEffectCreator.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-10-21 23:23:09 +0800 (Tue, 21 Oct 2008) $

 *   revision		: $Rev: 635 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.IO;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;

namespace WCell.Tools.Spells
{
    public class SpellEffectCreator
    {
        private static DirectoryInfo spellsDir = new DirectoryInfo("c:/Spells/");
        private static DirectoryInfo effectsDir = new DirectoryInfo(spellsDir.FullName + "Effects/");

        public static void WriteSpellEffects()
        {
            SpellHandler.LoadSpells();

            spellsDir.Create();
            effectsDir.Create();

            using (var writer = new StreamWriter(effectsDir.FullName + "_Effects.cs", false))
            {
                foreach (SpellEffectType type in Enum.GetValues(typeof(SpellEffectType)))
                {
                    CreateEffectFile(type);
                    writer.WriteLine("ByType.Add(SpellEffectType.{0}, (effect) => new {0}Effect(effect));", type);
                }
            }
        }

        public static void CreateEffectFile(SpellEffectType type)
        {
            using (var writer = new StreamWriter(effectsDir.FullName + type + ".cs", false))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using System.Linq;");
                writer.WriteLine("using System.Text;");

                writer.WriteLine("using WCell.RealmServer.Entities;");

                writer.WriteLine("namespace WCell.RealmServer.Spells.Effects");
                writer.WriteLine("{");
                writer.WriteLine("	public class {0}Effect : EffectHandler", type);
                writer.WriteLine("	{");
                writer.WriteLine("		public {0}Effect(SpellEffect effect)", type);
                writer.WriteLine("			: base(effect)");
                writer.WriteLine("		{");
                writer.WriteLine("		}");

                writer.WriteLine("		public override void Handle(SpellCast cast)");
                writer.WriteLine("		{");
                writer.WriteLine("			// TODO: {0}EffectHandler", type);
                writer.WriteLine("		}");
                writer.WriteLine("	}");
                writer.WriteLine("});");
            }
        }
    }
}