using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.Constants.GameObjects;
using WCell.Constants.Skills;
using WCell.RealmServer.Database;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Toolshed;

namespace WCell.Tools.Domi.Output
{
	[Tool]
	public static class GOOutput
	{
	    public static readonly string DumpFile = ToolConfig.OutputDir + "/GOs.txt";

		private static string dir = ToolConfig.OutputDir + "/gos";

		static GOOutput()
		{
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
		}

		public delegate void GOWriteHandler(StreamWriter writer, GOEntry entry);

		private static void Init()
		{
			RealmWorldDBMgr.Initialize();
			SpellHandler.LoadSpells();
			GOMgr.LoadAll();
		}

		public static void WriteGOFile(string name, GOWriteHandler handler)
		{
			Init();
			using (var writer = new StreamWriter(dir + "/" + name + ".txt", false))
			{
				var entries = GOMgr.Entries.Values;

				foreach (var entry in entries)
				{
					handler(writer, entry);
				}
			}
		}

		public static void WriteGOs()
		{
			Init();
			using (StreamWriter writer = new StreamWriter(DumpFile, false))
			{
				foreach (var entry in GOMgr.Entries.Values)
				{
					entry.Write(writer, "");

					writer.WriteLine();
					writer.WriteLine("##############################################");
					writer.WriteLine();
				}
			}
		}

		public static void WriteTraps()
		{
			WriteGOFile("Traps", (writer, entry) =>{
			                     	if (entry.Type == GameObjectType.Trap)
			                     	{
			                     		writer.WriteLine("Name: " + entry.DefaultName);
			                     		writer.WriteLine("Id: " + entry.Id);

			                     		writer.WriteLine("###################################");
			                     		writer.WriteLine();
			                     	}
			                     });
		}

		public static void WriteGOsByTypes()
		{
			Init();
			var entries = new Dictionary<GameObjectType, List<GOEntry>>();

			foreach (var entry in GOMgr.Entries.Values)
			{
				List<GOEntry> list;
				if (!entries.TryGetValue(entry.Type, out list))
				{
					entries.Add(entry.Type, list = new List<GOEntry>());
				}

				list.Add(entry);
			}

			foreach (var pair in entries)
			{
				using (StreamWriter writer = new StreamWriter(dir + "/" + pair.Key + ".txt", false))
				{
					foreach (var entry in pair.Value)
					{
						entry.Write(writer, "");

						writer.WriteLine();
						writer.WriteLine("##############################################");
						writer.WriteLine();
					}
				}
			}
		}

		public static void Write(this GOEntry entry, TextWriter writer, string indent)
		{
			writer.WriteLine(indent + "GO: " + entry.DefaultName + string.Format(" (Id: {0} [{1}], Type: {2})", 
				entry.Id, entry.GOId, entry.Type));

			indent += "\t";

			var type = entry.GetType();


			if (entry.Lock != null)
			{
				var attrs = new List<string>(2);
				if (entry.Lock.IsUnlocked)
				{
					attrs.Add("Open");
				}
				if (entry.Lock.CanBeClosed)
				{
					attrs.Add("Closable");
				}

				writer.WriteLine(indent + "Lock (Id: {0}{1})", entry.Lock.Id, attrs.Count > 0 ? ("; " + attrs.ToString(", ")) : "");

				if (entry.Lock.Keys.Length > 0)
				{
					writer.WriteLine(indent + "\tPossible Keys: " + entry.Lock.Keys.ToString(", "));
				}
				if (entry.Lock.OpeningMethods.Length > 0)
				{
					writer.WriteLine(indent + "\tOpening Methods:");
					foreach (var method in entry.Lock.OpeningMethods)
					{
						string reqStr;
						if (method.RequiredSkill != SkillId.None)
						{
							reqStr = string.Format(" needs {0} in {1}", method.RequiredSkillValue, method.RequiredSkill);
						}
						else
						{
							reqStr = "";
						}

						writer.WriteLine(indent + "\t{0}{1}", method.InteractionType, reqStr);
					}
				}
			}

			foreach (var field in type.GetFields())
			{
				if (field.Name != "Names" && field.Name != "Id" && field.Name != "Fields" &&
				    field.Name != "InteractRadius" && field.Name != "Type" && field.Name != "Lock" &&
					field.Name != "LockId" && field.Name != "LinkedTrapId" && field.Name != "Templates" &&
					field.Name != "Id" && field.Name != "GOId")
				{
					var val = field.GetValue(entry);
					WriteValue(writer, field.Name, indent, val);
				}
			}

			foreach (var prop in type.GetProperties())
			{
			    var val = prop.GetValue(entry, null);

				WriteValue(writer, prop.Name, indent, val);
			}

		    if (entry.SpawnEntries.Count > 0)
			{
				writer.WriteLine(indent + "Templates:");
				for (var i = 0; i < entry.SpawnEntries.Count; i++)
				{
					var template = entry.SpawnEntries[i];
					writer.WriteLine("\tTemplate #" + (i + 1) + ":");
					template.Write(writer, indent);
					writer.WriteLine();
				}
			}
		}

		static void WriteValue(TextWriter writer, string name, string indent, object val)
		{
			if (val == null || val is Delegate || IsDefaultVal(val))
			{
				return;
			}

			var strVal = Utility.GetStringRepresentation(val);
			if (strVal.Length > 0)
			{
				writer.WriteLine(indent + "{0}: {1}", name, strVal);
			}
		}

		private static bool IsDefaultVal(object val)
		{
			// TODO: Ouch - fixme
			if ((val is int && ((int) val) == 0) ||
			    (val is uint && ((uint) val == 0 || (uint) val == uint.MaxValue)) ||
			    (val is float && ((float) val) == 0) ||
			    (val is bool && !(bool) val))
			{
				return true;
			}
			return false;
		}

		public static void Write(this GOSpawnEntry templ, TextWriter writer, string indent)
		{
			writer.WriteLine("{0}\tMap: {1}", indent, templ.MapId);
			writer.WriteLine("{0}\tState: {1}", indent, templ.State);
			if (templ.RespawnSeconds != 0)
			{
				writer.WriteLine("{0}\tRespawn: {1}", indent, templ.RespawnSeconds);
			}
			if (templ.DespawnSeconds != 0)
			{
				writer.WriteLine("{0}\tDespawn: {1}", indent, templ.DespawnSeconds);
			}
		}
	}
}