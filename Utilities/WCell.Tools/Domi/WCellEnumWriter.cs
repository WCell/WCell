/*************************************************************************
 *
 *   file		: DBCEnumBuilder.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-09-16 03:10:51 +0800 (Tue, 16 Sep 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 628 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.DBC;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.Database;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Talents;
using WCell.Tools.Code;
using WCell.Tools.Spells;
using WCell.Util;
using WCell.Util.Toolshed;
using WCell.RealmServer.Content;
using WCell.RealmServer;
using WCell.Util.NLog;


namespace WCell.Tools.Domi
{
	[Tool]
	/// <summary>
	/// 
	/// </summary>
	public static class WCellEnumWriter
	{
		#region Delegates

		public delegate string GetField<T>(T entry);

		public delegate string GetField<T, S>(T entry, S str);

		public delegate bool Validator<T>(T entry);

		#endregion

		public static Dictionary<string, string> NumDict;

		private static bool initialized;

		/// <summary>
		/// Set this to your output-Dir.
		/// </summary>
		public static string Dir = ToolConfig.WCellConstantsRoot;

		public static Regex NumRegex = new Regex(@"^\d+");


		public static void WriteAllEnums()
		{
			RealmDBMgr.Initialize();
			ContentMgr.Initialize();
			World.InitializeWorld();
			SpellHandler.LoadSpells();
			FactionMgr.Initialize();
            WriteSpellMechanicEnum();
			SpellHandler.Initialize2();
			AreaTriggerMgr.Initialize();

			WriteZoneEnum();
			WriteMapEnum();
			WriteSkillEnums();
			WriteRangeEnum();
			WriteFactionEnums();
			WriteSpellFocusEnum();
			WriteSpellId();
			//WriteSpellMechanicEnum();
			WriteTalentEnums();
			WriteItemId();
			WriteItemSetId();
			WriteNpcId();
			WriteCreatureFamilyId();
			WriteGOEntryId();
			WriteRealmCategory();
			WriteTotemCategories();
			WriteAreaTriggers();

			//NPCMgr.ForceInitialize();
			//WriteRideEnum();
		}

		public static void WriteRealmCategory()
		{
			var categories = CfgCategories.ReadCategories();
			WriteEnum("RealmCategory", " : uint", "Realm", categories,
					  (item) => { return true; },
					  (item) => { return item.Value; },
					  (item) => { return item.Key.ToString(); });
		}

		public static void WriteItemId()
		{
			World.InitializeWorld();
			SpellHandler.LoadSpells();
			FactionMgr.Initialize();
			NPCMgr.LoadAll();
			ItemMgr.ForceInitialize();

			WriteEnum("ItemId", " : uint", "Items", ItemMgr.Templates, false,
					  (item) => { return item.DefaultName != null; },
					  (item) => { return item.DefaultName; },
					  template =>
					  {
						  var strs = new List<string>();
						  if (!string.IsNullOrEmpty(template.DefaultDescription))
						  {
							  strs.Add("Desc: " + template.DefaultDescription);
						  }
						  if (template.InventorySlotType != InventorySlotType.None)
						  {
							  strs.Add(template.InventorySlotType.ToString());
						  }
						  if (template.Level > 0)
						  {
							  strs.Add("Level: " + template.Level);
						  }
						  if (template.Set != null)
						  {
							  strs.Add("Set: " + template.Set);
						  }
						  if (template.Class != ItemClass.None)
						  {
							  strs.Add("Class: " + template.Class);
						  }
						  return strs.ToString("\n");
					  },
					  (item, name) => null,
					  (item) => { return item.Id.ToString(); });
		}

		public static void WriteItemSetId()
		{
			RealmDBMgr.Initialize();
			World.InitializeWorld();
			SpellHandler.LoadSpells();
			ItemMgr.ForceInitialize();

			WriteEnum("ItemSetId", " : uint", "Items", ItemMgr.Sets, false,
					  (item) => true,
					  (item) => item.Name,
					  set =>
					  {
						  var strs = new List<string>();
						  foreach (ItemTemplate templ in set.Templates)
						  {
							  strs.Add(templ.ToString());
						  }
						  strs.Add(" \nBoni:");
						  for (int i = 0; i < set.Boni.Length; i++)
						  {
							  Spell[] boni = set.Boni[i];
							  if (boni != null)
							  {
								  strs.Add((i + 1) + " Items: " + boni.ToString(", "));
							  }
						  }
						  if (set.RequiredSkillValue > 0)
						  {
							  strs.Add(" \nRequires: " + set.RequiredSkillValue + " " + set.RequiredSkill);
						  }
						  return strs.ToString("\n");
					  },
					  (item, name) => null,
					  (item) => item.Id.ToString());
		}

		public static void WriteNpcId()
		{
			RealmDBMgr.Initialize();
			SpellHandler.LoadSpells();
			FactionMgr.Initialize();
			NPCMgr.LoadAll();

			WriteEnum("NPCId", " : uint", "NPCs", NPCMgr.GetAllEntries(), false,
					  (item) => { return item != null; },
					  (item) => { return item.DefaultName; },
					  entry =>
					  {
						  var strs = new List<string>();
						  if (entry.DefaultTitle.Length > 0)
						  {
							  strs.Add("Title: " + entry.DefaultTitle);
						  }
						  if (entry.MinLevel > 0)
						  {
							  strs.Add("Level: " + entry.MinLevel +
									   (entry.MaxLevel != entry.MinLevel ? " - " + entry.MaxLevel : ""));
						  }
						  if (entry.Rank != CreatureRank.Normal)
						  {
							  strs.Add("Rank: " + entry.Rank + (entry.IsLeader ? " (Leader)" : ""));
						  }
						  else if (entry.IsLeader)
						  {
							  strs.Add("Leader");
						  }
						  return strs.ToString("\n");
					  },
					  (item, name) => null,
					  (item) => { return item.Id.ToString(); });
		}

		public static void WriteCreatureFamilyId()
		{
			NPCMgr.InitDefault();

			WriteEnum("CreatureFamilyId", "", "NPCs", NPCMgr.CreatureFamilies.Values,
					  family => true,
					  family => family.Name,
					  family => ((uint)family.Id).ToString());
		}

		public static void WriteGOEntryId()
		{
			GOMgr.LoadAll();

			WriteEnum("GOEntryId", " : uint", "GameObjects", GOMgr.Entries.Values, false,
					  (entry) => { return true; },
					  (entry) => { return entry.DefaultName.Trim().Length > 0 ? entry.DefaultName.Trim() : "Unknown"; },
					  (entry) =>
					  {
						  var strs = new List<string>();
						  strs.Add("Type: " + entry.Type + (entry.IsConsumable ? " (Consumable)" : ""));
						  if (entry.LinkedTrap != null)
						  {
							  strs.Add("Trap: " + entry.LinkedTrap);
						  }
						  if (entry.Lock != null)
						  {
							  strs.Add("Lock: " + entry.Lock);
						  }
						  //if (entry.Templates != null && entry.Templates.Count > 0)
						  //{
						  //    strs.Add("Templates: " + entry.Templates.ToString("; "));
						  //}
						  return strs.ToString("\n");
					  },
					  (entry, name) => null,
					  (entry) => { return ((int)entry.Id).ToString(); });
		}

		public static void WriteSpellFocusEnum()
		{
			Dictionary<uint, string> entries = SpellFocusObjectReader.Read();
			WriteEnum("SpellFocus", " : uint", "Spells", entries,
					  (item) => { return true; },
					  (item) => { return item.Value.Trim().Length > 0 ? item.Value : "Unknown"; },
					  (item) => { return item.Key.ToString(); });
		}

		public static void WriteRideEnum()
		{
			WriteEnum("MountId", " : uint", "NPCs", SpellHandler.ById,
					  (item) =>
					  {
						  return item.HasEffectWith((effect) =>
													  {
														  if (effect.AuraType == AuraType.Mounted &&
															  NPCMgr.Mounts.ContainsKey((MountId)item.Effects[0].MiscValue))
														  {
															  int mountId = item.Effects[0].MiscValue;
															  NPCEntry mount = NPCMgr.Mounts[(MountId)mountId];
															  return mount != null;
														  }
														  return false;
													  });
					  },
					  (item) => { return item.Name; },
					  (item) =>
					  {
						  var mountId = item.Effects[0].MiscValue;
						  var mount = NPCMgr.Mounts[(MountId)mountId];
						  uint[] displayIds = mount.DisplayIds;
						  return displayIds[0].ToString();
					  });
		}

		public static void WriteZoneEnum()
		{
			WriteEnum("ZoneId", " : uint", "World", World.ZoneTemplates,
					  (item) => { return true; },
					  (item) => { return item.Name.Trim().Length > 0 ? item.Name : "Unnamed"; },
					  (item) => { return ((uint)item.Id).ToString(); });
		}

		public static void WriteMapEnum()
		{
			WriteEnum("MapId", " : uint", "World", World.MapTemplates,
					  (map) => { return true; },
					  (map) => { return map.Name.Trim().Length > 0 ? map.Name : "Unnamed"; },
					  (map) => { return ((uint)map.Id).ToString(); });
		}

		public static void WriteTalentEnums()
		{
			WriteEnum("TalentId", " : uint", "Talents", TalentMgr.Entries,
					  (talent) => { return true; },
					  (talent) =>
					  {
						  try
						  {
							  if (talent.Spells.Length > 0)
								  return talent.Tree.Class + " " + talent.Tree.Name + " " +
										 Regex.Replace(talent.Spells[0].Name, @"_\d", "");
							  else
								  return talent.Tree.Class + " " + talent.Tree.Name;
						  }
						  catch (Exception e)
						  {
							  LogUtil.ErrorException(e, "Failed to write Talent: " + talent);
						  }
						  return null;
					  },
					  (talent) => { return ((uint)talent.Id).ToString(); });

			WriteEnum("TalentTreeId", " : uint", "Talents", TalentMgr.TalentTrees,
					  (tree) => { return true; },
					  (tree) => { return tree.Class + " " + tree.Name; },
					  (tree) => { return ((uint)tree.Id).ToString(); });
		}

		public static void WriteSkillEnums()
		{
			WriteEnum("SkillId", " : uint", "Skills", SkillHandler.ById,
					  (skill) => { return true; },
					  (skill) => { return skill.Name; },
					  (skill) => { return ((uint)skill.Id).ToString(); });
		}

		public static void WriteSpellMechanicEnum()
		{
			WriteEnum("SpellMechanic", " : uint", "Spells", Spell.mappeddbcMechanicReader.Entries,
					  mech => { return true; },
					  mech =>
					  {
						  string str = mech.Value;
						  if (str.Length < 1)
						  {
							  str = "Unknown";
						  }
						  return str;
					  },
					  mech =>
					  {
						  string str = mech.Key.ToString();
						  return str;
					  });
		}

		public static void WriteSpellLinesEnum()
		{
			SpellLineWriter.CreateMaps();

			var i = 0;
			var list = new List<string>(2000);
			for (var i1 = 0; i1 < SpellLineWriter.Maps.Length; i1++)
			{
				var map = SpellLineWriter.Maps[i1];
				if (map == null) continue;

				foreach (var spells in map.Values)
				{
					list.Add(SpellLineWriter.GetSpellLineName(spells.First()));
				}
			}

			WriteEnum("SpellLineId", " : uint", "Spells", list,
					  item => { return true; },
					  item =>
					  {
						  return item;
					  },
					  item => (++i).ToString());
		}

		public static void WriteSpellId()
		{
			RealmDBMgr.Initialize();
			ContentMgr.Initialize();
			World.InitializeWorld();
			SpellHandler.LoadSpells();
			FactionMgr.Initialize();
			SpellHandler.Initialize2();

			var noSpell = new Spell { Name = "None", Effects = new SpellEffect[0] };

			SpellHandler.ById[0] = noSpell;
			WriteEnum("SpellId", " : uint", "Spells", SpellHandler.ById, false,
					  (spell) => { return true; },
					  (spell) => { return spell.FullName + spell.RankDesc; },
					  (spell) =>
					  {
						  TalentEntry talent = spell.Talent;

						  var descs = new List<string>();

						  if (spell.IsPassive)
						  {
							  descs.Add("Passive");
						  }

						  if (talent != null)
						  {
							  descs.Add("Talent");
						  }
						  else if (spell.Ability != null &&
								   spell.Ability.Skill != null && spell.Ability.Skill.Category != SkillCategory.Invalid)
						  {
							  descs.Add(spell.Ability.Skill.Category.ToString());
						  }

						  if (spell.IsTeachSpell)
						  {
							  descs.Add("Teachspell");
						  }
						  if (spell.SpellLevels.Level > 0)
						  {
                              descs.Add("Level: " + spell.SpellLevels.Level);
						  }

						  string desc = string.Join(", ", descs.ToArray());

						  if (!string.IsNullOrEmpty(spell.Description))
						  {
							  if (desc.Length > 0)
							  {
								  desc += "\n";
							  }
							  desc += spell.Description;
						  }

						  return desc;
					  },
					  (spell, name) => { return null; },
					  (spell) => { return spell.Id.ToString(); });
		}

		public static void WriteSpellRankEnum()
		{
			var ranks = new HashSet<string>();
			foreach (Spell spell in SpellHandler.ById)
			{
				if (spell == null)
				{
					continue;
				}

				string rankDesc = spell.RankDesc;
				if (!ranks.Contains(rankDesc))
				{
					ranks.Add(rankDesc);
				}
			}

			int rankCount = 1;
			string[] ranksSorted = ranks.ToArray();
			Array.Sort(ranksSorted);
			WriteEnum("SpellRank", " : uint", "Spells", ranksSorted,
					  (desc) => { return true; },
					  (desc) => { return desc; },
					  (desc) => { return (rankCount++).ToString(); });
		}

		public static void WriteRangeEnum()
		{
			var ranges = new MappedDBCReader<DistanceEntry, DistanceConverter>(
                RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLRANGE));

			WriteEnum("Range", " : uint", "Spells", ranges.Entries.Values,
					  (entry) => { return entry.Distance == (int)entry.Distance; },
					  (entry) => { return entry.Name; },
					  (entry) => { return entry.Distance.ToString(); });
		}

		public static void WriteFactionEnums()
		{
			FactionMgr.Initialize();

			WriteEnum("FactionId", " : uint", "Factions", FactionMgr.ById,
					  (faction) => { return true; },
					  (faction) => { return faction.Entry.Name; },
					  (faction) => { return ((int)faction.Id).ToString(); });

			WriteEnum("FactionReputationIndex", "", "Factions", FactionMgr.ByReputationIndex,
					  (faction) => true,
					  (faction) => faction.Entry.Name,
					  (faction) => ((int)faction.ReputationIndex).ToString());

			WriteEnum("FactionTemplateId", " : uint", "Factions", FactionMgr.ByTemplateId,
					  (faction) => { return true; },
					  (faction) => { return faction.Entry.Name; },
					  (faction) => { return ((int)faction.Template.Id).ToString(); });
		}

		public static void WriteTotemCategories()
		{
			var entries = ItemMgr.ReadTotemCategories();
			WriteEnum("TotemCategory", " : uint", "Items", entries,
					  cat => true,
					  cat => cat.Value.Name,
					  cat => cat.Value.Id.ToString());
		}

		public static void WriteAreaTriggers()
		{
			var triggers = AreaTriggerMgr.AreaTriggers;
			WriteEnum("AreaTriggerId", " : uint", "AreaTriggers", triggers,
					  (entry) => true,
					  (entry) => entry.Template != null ? entry.Template.Name : "Unknown",
					  (entry) => entry.Id.ToString());
		}

		public static void WriteEnum<T>(string enumName, string enumSuffix, string group, IEnumerable<T> values,
										Validator<T> validator,
										GetField<T> getNameDelegate,
										GetField<T> getIdDelegate)
		{
			WriteEnum(enumName, enumSuffix, group, values, false,
					  validator,
					  getNameDelegate,
					  null,
					  null,
					  getIdDelegate);
		}

		public static void WriteEnum<T>(
			string enumName,
			string enumSuffix,
			string group,
			IEnumerable<T> values,
			bool hexadecimal,
			Validator<T> validator,
			GetField<T> getNameDelegate,
			GetField<T> getCommentDelegate,
			GetField<T, string> getDuplNameDelegate,
			GetField<T> getIdDelegate)
		{
			Init();
			var dir = Path.Combine(Dir, group);
			Directory.CreateDirectory(dir);

			var file = Path.Combine(dir, enumName + ".cs");
			Console.Write("Writing enum {0} to {1}...", enumName, new DirectoryInfo(file).FullName);

			var first = true;
			using (var writer = new CodeFileWriter(file,
												   "WCell.Constants." + group, enumName, "enum", enumSuffix))
			{
				try
				{
					var names = new Dictionary<string, int>(values.Count());

					foreach (T item in values)
					{
						if (item == null || item.Equals(default(T)) || !validator(item))
							continue;

						var name = getNameDelegate(item);

						if (name == null)
						{
							throw new Exception(string.Format("Name for Item {0} in {1}/{2} was null.", item, group, enumName));
						}

						name = BeautifyName(name);

						int count;
						if (!names.TryGetValue(name, out count))
						{
							names.Add(name, 1);
						}
						else
						{
							names.Remove(name);
							names.Add(name, ++count);

							string duplName = null;
							if (getDuplNameDelegate != null)
							{
								duplName = getDuplNameDelegate(item, name);
							}

							if (duplName != null)
							{
								name = duplName;
							}
							else
							{
								name = name + "_" + count;
							}
						}

						var val = getIdDelegate(item);
						if (hexadecimal)
						{
							int ival;
							if (int.TryParse(val, out ival))
							{
								val = string.Format("0x{0:x}", ival);
							}
						}

						if (first)
						{
							first = false;
							long id;
							if (!long.TryParse(val, out id))
							{
								throw new InvalidDataException("Invalid ID was not numeric: " + val);
							}

							if (id > 0)
							{
								writer.WriteLine("None = 0,");
							}
						}

						string comment;
						if (getCommentDelegate != null)
						{
							comment = getCommentDelegate(item);
							if (comment != null)
							{
								var commentLines = comment.Split(new[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
								if (commentLines.Length > 0)
								{
									writer.StartSummary();
									foreach (string line in commentLines)
									{
										writer.WriteXmlCommentLine(line);
									}
									writer.EndSummary();
								}
							}
						}
						writer.WriteLine(string.Format("{0} = {1},", name, val));
					}

					writer.WriteLine("End");

					writer.Finish();

					Console.WriteLine(" Done.");
				}
				catch (Exception ex)
				{
					writer.OnException(ex);
				}
			}
		}

		#region Misc

		private static void Init()
		{
			if (!initialized)
			{
				initialized = true;
				if (NumDict == null)
				{
					var numDict = new Dictionary<string, string>();

					numDict.Add("0", "Zero");
					numDict.Add("1", "One");
					numDict.Add("2", "Two");
					numDict.Add("4", "Four");
					numDict.Add("5", "Five");
					numDict.Add("7", "Seven");
					numDict.Add("8", "Eight");
					numDict.Add("9", "Seven");
					numDict.Add("10", "Ten");
					numDict.Add("12", "Twelf");
					numDict.Add("13", "Thirteen");
					numDict.Add("15", "Fifteen");
					numDict.Add("17", "Seventeen");
					numDict.Add("18", "Eighteen");
					numDict.Add("19", "Nineteen");
					numDict.Add("20", "Twenty");
					numDict.Add("21", "TwentyOne");
					numDict.Add("22", "TwentyTwo");
					numDict.Add("25", "TwentyFive");
					numDict.Add("26", "TwentySix");
					numDict.Add("28", "TwentyEight");
					numDict.Add("29", "TwentyNine");
					numDict.Add("30", "Thirty");
					numDict.Add("32", "ThirtyTwo");
					numDict.Add("33", "ThirtyThree");
					numDict.Add("34", "ThirtyFour");
					numDict.Add("37", "ThirtySeven");
					numDict.Add("40", "Fourty");
					numDict.Add("42", "FourtyTwo");
					numDict.Add("45", "FourtyFife");
					numDict.Add("47", "FourtySeven");
					numDict.Add("49", "FourtyNine");
					numDict.Add("50", "Fifty");
					numDict.Add("52", "FiftyTwo");
					numDict.Add("53", "FiftyThree");
					numDict.Add("55", "FiftyFife");
					numDict.Add("59", "FiftyNine");
					numDict.Add("60", "Sixty");
					numDict.Add("68", "SixtyEight");
					numDict.Add("70", "Seventy");
					numDict.Add("80", "Eighty");
					numDict.Add("85", "EightyFife");
					numDict.Add("92", "NinetyTwo");
					numDict.Add("99", "NinetyNine");
					numDict.Add("100", "Hundred");
					numDict.Add("103", "HundredThree");
					numDict.Add("130", "HundredThirty");
					numDict.Add("500", "FiveHundred");
					numDict.Add("5000", "FiveThousand");
					NumDict = (new Func<Dictionary<string, string>>(() => numDict))();
				}
			}
		}

		public static string GetNumString(string num)
		{
			if (!NumDict.ContainsKey(num))
			{
				string msg = "Number \"" + num + "\" is not defined - add it to the numDict!";
				//throw new ArgumentException();
				Console.WriteLine(msg);
				return "_" + num;
			}
			return NumDict[num];
		}

		#endregion

		#region DBC readers

		#region Nested type: DistanceConverter

		private class DistanceConverter : AdvancedDBCRecordConverter<DistanceEntry>
		{
			public override DistanceEntry ConvertTo(byte[] rawData, ref int id)
			{
				var entry = new DistanceEntry();
				id = (int)(entry.Id = GetUInt32(rawData, 0));
				entry.Distance = GetFloat(rawData, 3);
				entry.Name = GetString(rawData, 6);
				return entry;
			}
		}

		#endregion

		#region Nested type: DistanceEntry

		private struct DistanceEntry
		{
			public float Distance;
			public uint Id;
			public string Name;
		}

		#endregion

		#endregion

		public static string BeautifyName(string name)
		{
			name = name.Replace("'s", "s").
						Replace("%", "Percent");

			var parts = Regex.Split(name, @"\s+|[^\w\d_]+", RegexOptions.None);

			for (int i = 0; i < parts.Length; i++)
			{
				string part = parts[i];
				if (part.Length == 0)
				{
					continue;
				}
				//if (part.Length > 1) {
				//    part = part.ToLower();
				string firstChar = part[0] + "";
				part = firstChar.ToUpper() + part.Substring(1);
				//}
				//else {
				//    part = part.ToUpper();
				//}

				parts[i] = part;
			}

			name = string.Join("", parts);

			// don't allow digits at the start
			Match numMatch = NumRegex.Match(name);
			if (numMatch.Success)
			{
				string num = GetNumString(numMatch.Value);
				if (name.Length > num.Length)
				{
					name = num + name.Substring(numMatch.Value.Length, 1).ToUpper() + name.Substring(numMatch.Value.Length + 1);
				}
				else
				{
					name = num;
				}
			}
			return name;
		}
	}
}