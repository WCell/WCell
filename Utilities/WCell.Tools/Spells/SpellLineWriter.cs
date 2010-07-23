using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Talents;
using WCell.Tools.Code;
using WCell.Util;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Tools.Domi;
using WCell.Util.Toolshed;
using NLog;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Content;
using WCell.RealmServer.Global;

namespace WCell.Tools.Spells
{
	public class SpellLineWriter
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private static CodeFileWriter writer;
		public static Dictionary<string, HashSet<Spell>>[] Maps
		{
			get;
			private set;
		}

		[Tool]
		public static void WriteSpellLines()
		{
			WriteSpellLines(ToolConfig.RealmServerRoot + "Spells/SpellLines.Def.cs");
		}

		public static void WriteSpellLines(string outputFileName)
		{
			CreateMaps();
			WCellEnumWriter.WriteSpellLinesEnum();

			using (writer = new CodeFileWriter(outputFileName, "WCell.RealmServer.Spells", "SpellLines", "static partial class", "",
				"WCell.Constants",
				"WCell.Constants.Spells"))
			{
				writer.WriteMethod("private", "static void", "SetupSpellLines", "", WriteSpellLinesMethod);
			}
		}

		private static void WriteSpellLinesMethod()
		{
			writer.WriteLine("SpellLine[] lines;");
			writer.WriteLine();

			for (var i = 0; i < Maps.Length; i++)
			{
				var map = Maps[i];
				var clss = (ClassId)i;

				if (map != null)
				{
					var listCount = map.Count;
					var s = 0;

					writer.WriteRegion((clss != 0 ? clss.ToString() : "Other") + " (" + listCount + ")");
					writer.WriteLine("lines = new SpellLine[]");
					writer.OpenBracket();

					foreach (var set in map.Values)
					{
						var spells = set.ToList();
						spells.Sort((a, b) => a.Rank.CompareTo(b.Rank));

						var lineName = SpellLine.GetSpellLineName(spells.First());
						writer.WriteLine("new SpellLine(SpellLineId." + lineName + ", ");
						writer.IndentLevel++;
						var j = 0;
						foreach (var spell in spells)
						{
							j++;
							writer.WriteIndent("SpellHandler.Get(SpellId." + spell.SpellId + ")");
							if (j < set.Count)
							{
								writer.WriteLine(",");
							}
						}
						writer.IndentLevel--;
						writer.Write(")");
						if (s < listCount - 1)
						{
							writer.WriteLine(",");
						}
						++s;
					}
					writer.CloseBracket(true);
					writer.WriteLine("AddSpellLines(ClassId.{0}, lines);", clss);
					writer.WriteEndRegion();
					writer.WriteLine();
				}
			}
		}

		public static void CreateMaps()
		{
			if (Maps != null)
			{
				return;
			}

			SpellHandler.LoadSpells();
			SpellHandler.Initialize2();

			World.InitializeWorld();
			ArchetypeMgr.EnsureInitialize();		// for default spells

			NPCMgr.LoadNPCDefs();					// for trainers

			Maps = new Dictionary<string, HashSet<Spell>>[(int)ClassId.End];

			FindTalents();
			FindAbilities();
		}

		/// <summary>
		/// Find all class abilities that Trainers have to offer
		/// </summary>
		private static void FindAbilities()
		{
			// Add initial abilities
			foreach (var arr in ArchetypeMgr.Archetypes)
			{
				if (arr != null)
				{
					foreach (var archetype in arr)
					{
						if (archetype == null)
						{
							continue;
						}

						foreach (var spell in archetype.Spells)
						{
							AddSpell(spell);
						}
					}
				}
			}

			// Add Trainer abilities
			foreach (var npc in NPCMgr.GetAllEntries())
			{
				if (npc.TrainerEntry != null)
				{
					foreach (var spellEntry in npc.TrainerEntry.Spells.Values)
					{
						var spell = spellEntry.Spell;
						if (spell.Id == 24866)
						{
							spell.ToString();
						}
						if (spell.Ability != null && spell.Skill.Category == SkillCategory.ClassSkill)
						{
							AddSpell(spell);
						}
					}
				}
			}
		}

		/// <summary>
		/// Add all Talents
		/// </summary>
		private static void FindTalents()
		{
			foreach (var spell in SpellHandler.ById)
			{
				if (spell == null ||
					((spell.Talent == null || spell.ClassId == 0) && (spell.Skill == null || spell.Rank == 0 || spell.Skill.Category != SkillCategory.Profession)) ||
					spell.IsTriggeredSpell ||
					spell.HasEffectWith(effect => effect.EffectType == SpellEffectType.LearnPetSpell || effect.EffectType == SpellEffectType.LearnSpell))
				{
					continue;
				}

				var clss = spell.ClassId;
				if (spell.Ability == null || spell.Ability.ClassMask == 0 || spell.Ability.ClassMask.HasAnyFlag(clss))
				{
					if (spell.SpellId.ToString().Contains("_"))
					{
						log.Warn("Duplicate: " + spell);
						continue;
					}
					if (string.IsNullOrEmpty(spell.Description))
					{
						continue;
					}

					AddSpell(spell);
				}
			}
		}

		private static void AddSpell(Spell spell)
		{
			var clss = spell.ClassId;
			if (clss == ClassId.PetTalents && spell.Talent != null)
			{
				clss = ClassId.Hunter;
			}

			var map = Maps[(int)clss];
			if (map == null)
			{
				Maps[(int)clss] = map = new Dictionary<string, HashSet<Spell>>(100);
			}

			var name = spell.Name;
			if (!string.IsNullOrEmpty(spell.RankDesc) && !spell.RankDesc.Contains("Rank"))
			{
				name += " " + spell.RankDesc;
			}
			var line = map.GetOrCreate(name);
			line.Add(spell);
		}
	}
}