using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Skills;
using WCell.RealmServer.Database;
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

		private static HashSet<SkillId> LineSkills = new HashSet<SkillId>();

		[Tool]
		public static void WriteSpellLines()
		{
			WriteSpellLines(ToolConfig.RealmServerRoot + "Spells/SpellLines.Def.cs");
		}

		public static void WriteSpellLines(string outputFileName)
		{
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

					writer.WriteMap((clss != 0 ? clss.ToString() : "Other") + " (" + listCount + ")");
					writer.WriteLine("lines = new SpellLine[]");
					writer.OpenBracket();

					foreach (var set in map.Values)
					{
						var spells = set.ToList();
						var lineName = GetSpellLineName(spells.First());

						spells.Sort((a, b) => a.Id.CompareTo(b.Id));		// first sort by rank and then by id
						spells.Sort((a, b) => a.Rank.CompareTo(b.Rank));

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
					writer.WriteEndMap();
					writer.WriteLine();
				}
			}
		}

		public static string GetSpellLineName(Spell spell)
		{
			var name = spell.Name;
			
			if (spell.Talent != null)
			{
				name = spell.Ability.Skill.Name + name;
			}

			var clss = spell.ClassId;
			if (clss == 0)
			{
				var ids = spell.Ability.ClassMask.GetIds();
				if (ids.Length == 1)
				{
					clss = ids.First();
				}
			}
			if (clss == ClassId.PetTalents && spell.Talent != null)
			{
				name = "HunterPet" + name;
			}
			else
			{
				name = spell.ClassId + name;
			}

			return WCellEnumWriter.BeautifyName(name);
		}

		public static void CreateMaps()
		{
			if (Maps != null)
			{
				return;
			}

			RealmDBMgr.Initialize();
			ContentMgr.Initialize();
			SpellHandler.LoadSpells();
			SpellHandler.Initialize2();

			World.InitializeWorld();
			World.LoadDefaultMaps();
			ArchetypeMgr.EnsureInitialize();		// for default spells

			NPCMgr.LoadNPCDefs();					// for trainers

			Maps = new Dictionary<string, HashSet<Spell>>[(int)ClassId.End];

			FindTalents();
			FindAbilities();

			foreach (var spell in SpellHandler.ById)
			{
				if (spell != null && spell.Ability != null && LineSkills.Contains(spell.Ability.Skill.Id))
				{
					AddSpell(spell, true);
				}
			}

			// remove empty lines
			foreach (var dict in Maps)
			{
				if (dict != null)
				{
					foreach (var pair in dict.ToArray())
					{
						if (pair.Value.Count == 0)
						{
							dict.Remove(pair.Key);
						}
					}
				}
			}
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

			// Add trainable abilities
			foreach (var npc in NPCMgr.GetAllEntries())
			{
				if (npc.TrainerEntry != null)
				{
					foreach (var spellEntry in npc.TrainerEntry.Spells.Values)
					{
						var spell = spellEntry.Spell;
						if (spell.Ability != null && spell.Ability.Skill.Category == SkillCategory.ClassSkill)
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
					((spell.Talent == null || spell.ClassId == 0) && (spell.Ability == null || spell.Rank == 0 || spell.Ability.Skill.Category != SkillCategory.Profession)) ||
					spell.IsTriggeredSpell ||
					spell.HasEffectWith(effect => effect.EffectType == SpellEffectType.LearnPetSpell || effect.EffectType == SpellEffectType.LearnSpell))
				{
					continue;
				}

				var clss = spell.ClassId;
				if (spell.Ability == null || spell.Ability.ClassMask == 0 || spell.Ability.ClassMask.HasAnyFlag(clss))
				{
					//if (spell.SpellId.ToString().Contains("_"))
					//{
					//    log.Warn("Duplicate: " + spell);
					//    continue;
					//}
					if (string.IsNullOrEmpty(spell.Description))
					{
						continue;
					}

					AddSpell(spell);
				}
			}
		}

		private static void AddSpell(Spell spell, bool force = false)
		{
			if (!force)
			{
				if (spell.Ability.Skill == null ||
					(spell.Ability.Skill.Category != SkillCategory.ClassSkill && spell.Ability.Skill.Category != SkillCategory.Invalid))
				{
					return;
				}

				LineSkills.Add(spell.Ability.Skill.Id);
			}

			var clss = spell.ClassId;
			if (clss == 0)
			{
				var ids = spell.Ability.ClassMask.GetIds();
				if (ids.Length == 1)
				{
					clss = ids.First();
				}
				else
				{
					// not a single class spell
					return;
				}
			}
			if (clss == ClassId.PetTalents && spell.Talent != null)
			{
				clss = ClassId.Hunter;
			}

			//var name = GetSpellLineName(spell);
			var name = spell.Name;
			var map = Maps[(int)clss];
			if (map == null)
			{
				Maps[(int)clss] = map = new Dictionary<string, HashSet<Spell>>(100);
			}
			var line = map.GetOrCreate(name);

			// don't add triggered spells
			if (spell.IsTriggeredSpell)
			{
				return;
			}

			if (line.Count > 0)
			{
				if (line.Contains(spell))
				{	
					// already added
					return;
				}

				// must have a rank
				if (spell.Rank == 0 && line.Any(spll => spll.Rank != 0))
				{
					return;
				}
				if (spell.Rank > 0)
				{
					if (line.Any(spll => spll.Rank == spell.Rank && spll.Talent != null))
					{
						// already has one with the same rank and a talent
						return;
					}
					line.RemoveWhere(spll => spll.Rank == 0 && spll.Talent == null);
				}

				// must not have a rank
				if (line.Any(spll => spll.Rank == 0 && spll.Talent != null))
				{
					// there is only one
					return;
				}

				 // don't add weird copies or unknown anonymous triggered effects
				if (line.Any(spll => spell.Rank == spll.Rank &&
					(spll.Description.Contains(spell.Id.ToString()) || spll.CategoryCooldownTime > 0)))
				{
					return;
				}
				line.RemoveWhere(spll => spell.Rank == spll.Rank && 
					(spell.Description.Contains(spll.Id.ToString()) || spll.CategoryCooldownTime == 0));
			}

			line.Add(spell);
		}
	}
}