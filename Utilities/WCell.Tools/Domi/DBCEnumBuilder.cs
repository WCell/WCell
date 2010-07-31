/*************************************************************************
 *
 *   file		: DBCEnumBuilder.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-09-15 21:10:51 +0200 (ma, 15 sep 2008) $
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.IO;
using WCell.Constants.Talents;
using WCell.Constants;
using WCell.Core.DBC;
using WCell.Util;

using WCell.RealmServer;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Talents;
using WCell.RealmServer.Global;
using WCell.Tools.Domi;
using WCell.RealmServer.NPCs;
using WCell.Tools.Code;
using WCell.RealmServer.Handlers;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.RealmServer.Items;
using WCell.Constants.Skills;
using WCell.Constants.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Database;

namespace WCell.Tools
{
	/// <summary>
	/// TODO: First build to another folder and only if it worked, move file
	/// </summary>
	public class DBCEnumBuilder
	{
		/// <summary>
		/// Set this to your output-Dir.
		/// </summary>
		public static string Dir = Tools.WCellConstantsRoot;


		public static void WriteAll()
		{
			DBSetup.Initialize();
			World.EnsureMapDataLoaded();
			SpellHandler.Initialize();
			FactionMgr.Initialize();
			SkillHandler.Initialize();
			TalentMgr.Initialize();

			WriteZoneEnum();
			WriteMapEnum();
			WriteSkillEnums();
			WriteRangeEnum();
			WriteFactionEnums();
			WriteSpellFocusEnum();
			WriteSpellId();
			WriteSpellMechanicEnum();
			WriteTalentEnums();
			WriteItemId();
			WriteItemSetId();
			WriteNpcId();
			WriteGOEntryId();
			WriteRealmCategory();

			NPCMgr.ForceInitialize();
			WriteRideEnum();
		}

		public static void WriteRealmCategory()
		{
			var categories = CfgCategories.ReadCategories();
			WriteEnum("RealmCategory", "Realm", categories,
				(item) => { return true; },
				(item) => {
					return item.Value;
				},
				(item) => { return item.Key.ToString(); });
		}

		public static void WriteItemId()
		{
			ItemMgr.ForceInitialize();
			WriteEnum("ItemId", "Items", ItemMgr.Templates, false,
				(item) => { return true; },
				(item) => {
					return item.Name;
				},
				template => {
					var strs = new List<string>();
					if (template.Description.Length > 0)
					{
						strs.Add("Desc: " + template.Description);
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
					if (template.ItemProfession != SkillId.None)
					{
						strs.Add("Skill: " + template.ItemProfession);
					}
					if (template.MaxAmount > 1)
					{
						strs.Add("Max Stack: " + template.MaxAmount);
					}
					if (template.Lock != null)
					{
						strs.Add("Lock: " + template.Lock);
					}
					if (template.TeachSpell != null)
					{
						strs.Add("Teaches: " + template.TeachSpell);
					}
					if (template.UseSpell != null)
					{
						strs.Add("Use: " + template.UseSpell.Spell +
							(template.UseSpell.Charges > 0 && template.UseSpell.Charges < int.MaxValue ? " (" + template.UseSpell.Charges + ")" : ""));
					}
					if (template.EquipSpell != null)
					{
						strs.Add("OnEquip: " + template.EquipSpell);
					}
					return strs.ToString("\n");
				},
				(item, name) => null,
				(item) => { return item.Id.ToString(); });
		}

		public static void WriteItemSetId()
		{
			ItemMgr.ForceInitialize();
			WriteEnum("ItemSetId", "Items", ItemMgr.Sets, false,
				(item) => { return true; },
				(item) => {
					return item.Name;
				},
				set => {
					var strs = new List<string>();
					foreach (var templ in set.Templates)
					{
						strs.Add(templ.ToString());
					}
					strs.Add(" \nBoni:");
					for (int i = 0; i < set.Boni.Length; i++)
					{
						var boni = set.Boni[i];
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
				(item) => { return item.Id.ToString(); });
		}

		public static void WriteNpcId()
		{
			SpellHandler.Initialize();
			FactionMgr.Initialize();
			NPCMgr.ForceInitialize();
			WriteEnum("NPCId", "NPCs", NPCMgr.Prototypes, false,
				(item) => { return item.Entry != null; },
				(item) => {
					return item.Entry.Name;
				},
				prototype => {
					var strs = new List<string>();
					if (prototype.Entry.Title.Length > 0)
					{
						strs.Add("Title: " + prototype.Entry.Title);
					}
					if (prototype.MinLevel > 0)
					{
						strs.Add("Level: " + prototype.MinLevel + (prototype.MaxLevel != prototype.MinLevel ? " - " + prototype.MaxLevel : ""));
					}
					if (prototype.DefaultFaction != null)
					{
						strs.Add("Faction: " + prototype.DefaultFaction);
					}
					if (prototype.Entry.Rank != CreatureRank.Normal)
					{
						strs.Add("Rank: " + prototype.Entry.Rank + (prototype.Entry.IsLeader ? " (Leader)" : ""));
					}
					else if (prototype.Entry.IsLeader)
					{
						strs.Add("Leader");
					}
					if (prototype.Entry.Spells != null && prototype.Entry.Spells.Length > 0)
					{
						strs.Add("Spells: " + prototype.Entry.Spells.ToString(", "));
					}
					if (prototype.Auras != null && prototype.Auras.Length > 0)
					{
						strs.Add("Auras: " + prototype.Auras.ToString(", "));
					}
					return strs.ToString("\n");
				},
				(item, name) => null,
				(item) => { return item.Entry.Id.ToString(); });
		}

		public static void WriteGOEntryId()
		{
			GOMgr.LoadAll();

			WriteEnum("GOEntryId", "GameObjects", GOMgr.Entries, false,
				(entry) => { return true; },
				(entry) => {
					return entry.Name.Trim().Length > 0 ? entry.Name.Trim() : "Unknown";
				},
				(entry) => {
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
				(entry) => { return entry.Id.ToString(); });
		}

		public static void WriteSpellFocusEnum()
		{
			var entries = SpellFocusObjectReader.Read();
			WriteEnum("SpellFocus", "Spells", entries,
				(item) => { return true; },
				(item) => {
					return item.Value.Trim().Length > 0 ? item.Value : "Unknown";
				},
				(item) => { return item.Key.ToString(); });
		}

		public static void WriteRideEnum()
		{
			WriteEnum("MountId", "NPCs", SpellHandler.ById,
				(item) => {
					return item.HasEffectWith((effect) => {
						if (effect.Aura == AuraType.Mounted && NPCMgr.Mounts.ContainsKey((MountId)item.Effects[0].MiscValue))
						{
							var mountId = item.Effects[0].MiscValue;
							var mount = NPCMgr.Mounts[(MountId) mountId];
							return mount != null;
						}
						return false;
					});
				},
				(item) => {
					return item.Name;
				},
				(item) => {
					var mountId = item.Effects[0].MiscValue;
					var mount = NPCMgr.Mounts[(MountId)mountId];
					var displayIds = mount.DisplayIds;
					return displayIds[0].ToString();
				});
		}

		public static void WriteZoneEnum()
		{
			WriteEnum("ZoneId", "World", World.Zones,
				(item) => { return true; },
				(item) => {
					return item.Name.Trim().Length > 0 ? item.Name : "Unnamed";
				},
				(item) => { return ((uint)item.Id).ToString(); });
		}

		public static void WriteMapEnum()
		{
			WriteEnum("MapId", "World", World.RegionInfos,
				(region) => { return true; },
				(region) => {
					return region.Name.Trim().Length > 0 ? region.Name : "Unnamed";
				},
				(region) => { return ((uint)region.Id).ToString(); });
		}

		public static void WriteTalentEnums()
		{
			WriteEnum("TalentId", "Talents", TalentMgr.Entries,
				(talent) => { return true; },
				(talent) => {
					return talent.Tree.Class + " " + talent.Tree.Name + " " + Regex.Replace(talent.Spells[0].Name, @"_\d", "");
				},
				(talent) => { return ((uint)talent.Id).ToString(); });

			WriteEnum("TalentTreeId", "Talents", TalentMgr.TalentTrees,
				(tree) => { return true; },
				(tree) => { return tree.Class + " " + tree.Name; },
				(tree) => { return ((uint)tree.Id).ToString(); });
		}

		public static void WriteSkillEnums()
		{
			WriteEnum("SkillId", "Skills", SkillHandler.ById,
				(skill) => { return true; },
				(skill) => { return skill.Name; },
				(skill) => { return ((uint)skill.Id).ToString(); });
		}

		public static void WriteSpellMechanicEnum()
		{
			WriteEnum<KeyValuePair<int, string>>("SpellMechanic", "Spells", Spell.DBCMechanicReader.Entries,
				(mech) => { return true; },
				(mech) => {
					var str = mech.Value;
					if (str.Length < 1)
					{
						str = "Unknown";
					}
					return str;
				},
				(mech) => {
					var str = mech.Key.ToString();
					return str;
				});
		}

		public static void WriteSpellId()
		{
			var noSpell = new Spell();
			noSpell.Name = "None";
			noSpell.Effects = new SpellEffect[0];

			SpellHandler.ById[(int)0] = noSpell;
			WriteEnum("SpellId", "Spells", SpellHandler.ById, false,
				(spell) => {
					return true;
				},
				(spell) => {
					return spell.FullName;
				},
				(spell) => {
					TalentEntry talent = spell.Talent;

					List<string> descs = new List<string>();

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

					if (spell.IsLearnSpell)
					{
						descs.Add("Teaches: " + spell.Effects[0].TriggerSpell);
					}
					else if (spell.Level > 0)
					{
						descs.Add("Level: " + spell.Level);
					}

					string desc = string.Join(", ", descs.ToArray());

					if (!string.IsNullOrEmpty(spell.BookDescription))
					{
						if (desc.Length > 0)
						{
							desc += "\n";
						}
						desc += spell.BookDescription;
					}

					return desc;
				},
				(spell, name) => {
					return null;
				},
				(spell) => {
					return spell.Id.ToString();
				});
		}

		public void WriteSpellRankEnum()
		{
			HashSet<string> ranks = new HashSet<string>();
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
			WriteEnum<string>("SpellRank", "Spells", ranksSorted,
				(desc) => {
					return true;
				},
				(desc) => {
					return desc;
				},
				(desc) => {
					return (rankCount++).ToString();
				});
		}

		public static void WriteRangeEnum()
		{
			DBCReader<DistanceEntry, DistanceConverter> ranges =
				new DBCReader<DistanceEntry, DistanceConverter>(
					RealmServer.RealmServer.Instance.Configuration.GetDBCFile("SpellRange.dbc"));

			WriteEnum<DistanceEntry>("Range", "Spells", ranges.Entries.Values,
				(entry) => { return true; },
				(entry) => { return entry.Name; },
				(entry) => { return entry.Distance.ToString(); });
		}

		public static void WriteFactionEnums()
		{
			FactionMgr.Initialize();

			WriteEnum<Faction>("FactionId", "Factions", FactionMgr.ById,
				(faction) => { return true; },
				(faction) => { return faction.Entry.Name; },
				(faction) => { return ((int)faction.Id).ToString(); });

			WriteEnum<Faction>("FactionRepListId", "Factions", FactionMgr.ByRepId,
				(faction) => { return true; },
				(faction) => { return faction.Entry.Name; },
				(faction) => { return ((int)faction.RepListId).ToString(); });

			WriteEnum<Faction>("FactionTemplateId", "Factions", FactionMgr.ByTemplateId,
				(faction) => { return true; },
				(faction) => { return faction.Entry.Name; },
				(faction) => { return ((int)faction.Template.Id).ToString(); });
		}

		public delegate bool Validator<T>(T entry);
		public delegate string GetField<T>(T entry);
		public delegate string GetField<T, S>(T entry, S str);

		public static void WriteEnum<T>(string enumName, string group, IEnumerable<T> values,
			Validator<T> validator,
			GetField<T> getNameDelegate,
			GetField<T> getIdDelegate)
		{

			WriteEnum(enumName, group, values, false,
				validator,
				getNameDelegate,
				null,
				null,
				getIdDelegate);
		}

		public static Regex NumRegex = new Regex(@"^\d+");
		public static void WriteEnum<T>(
			string enumName,
			string group,
			IEnumerable<T> values,
			bool hexadecimal,
			Validator<T> validator,
			GetField<T> getNameDelegate,
			GetField<T> getCommentDelegate,
			GetField<T, string> getDuplNameDelegate,
			GetField<T> getIdDelegate)
		{
			var dir = Path.Combine(Dir, group);
			Directory.CreateDirectory(dir);

			string file = Path.Combine(dir, enumName + ".cs");
			Console.Write("Writing enum {0} to {1}...", enumName, new DirectoryInfo(file).FullName);

			bool first = true;
			using (var writer = new CodeFileWriter(new StreamWriter(file),
				"WCell.Constants." + group, enumName, "enum"))
			{
				var lines = new List<string>(values.Count());

				var names = new Dictionary<string, int>(values.Count());

				foreach (var item in values)
				{
					if (item == null || !validator(item))
						continue;

					string name = getNameDelegate(item).Replace("%", "Percent");

					name = name.Replace("'s", "s");

					string[] parts = Regex.Split(name, @"\s+|[^\w\d_]+", RegexOptions.None);

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

					// against digits at the start
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

					string val = getIdDelegate(item);
					if (hexadecimal)
					{
						try
						{
							int ival = Convert.ToInt32(val);

							val = string.Format("0x{0:x}", ival);
						}
						catch { };
					}

					if (first)
					{
						first = false;
						if (long.Parse(val) > 0)
						{
							writer.WriteLine("None = 0,");
						}
					}

					string comment = "";
					if (getCommentDelegate != null)
					{
						comment = getCommentDelegate(item);
						if (comment != null)
						{
							string[] commentLines = comment.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
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
		}

		static Dictionary<string, string> numDict = (new Func<Dictionary<string, string>>(() => {
			Dictionary<string, string> numDict = new Dictionary<string, string>();
			numDict.Add("0", "Zero");
			numDict.Add("1", "One");
			numDict.Add("2", "Two");
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
			numDict.Add("22", "TwentyTwo");
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
			numDict.Add("53", "FiftyThree");
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
			numDict.Add("5000", "FiveThousand");
			return numDict;
		}))();

		public static string GetNumString(string num)
		{
			if (!numDict.ContainsKey(num))
			{
				var msg = "Number \"" + num + "\" is not defined - add it to the numDict!";
				//throw new ArgumentException();
				Console.WriteLine(msg);
				return "_" + num;
			}
			return numDict[num];
		}

		#region DBC readers
		private struct DistanceEntry
		{
			public uint Id;
			public string Name;
			public float Distance;
		}

		private class DistanceConverter : DBCRecordConverter<DistanceEntry>
		{
			public override DistanceEntry ConvertTo(byte[] rawData, ref int id)
			{
				DistanceEntry entry = new DistanceEntry();
				id = (int)(entry.Id = GetUInt32(rawData, 0));
				entry.Distance = GetFloat(rawData, 2);
				entry.Name = GetString(rawData, 4);
				return entry;
			}
		}
		#endregion
	}
}