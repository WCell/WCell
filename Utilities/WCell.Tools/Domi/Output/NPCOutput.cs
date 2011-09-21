using System;
using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Constants.Pets;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util;
using WCell.Util.Toolshed;

namespace WCell.Tools.Domi.Output
{
	public static class NPCOutput
	{
	    public static readonly string DumpFile = ToolConfig.OutputDir + "NPCs.txt";

		[Tool]
		public static void WriteNPCs()
		{
			Tools.StartRealm();
			NPCMgr.Initialize();
			NPCMgr.LoadNPCDefs();

			using (var writer = new IndentTextWriter(DumpFile))
			{
				foreach (var entry in NPCMgr.GetAllEntries())
				{
					entry.Dump(writer);
					writer.WriteLine();
					writer.WriteLine(" ################################### ");
					writer.WriteLine();
				}
			}

			WriteVehicles();
		}

		[Tool]
		public static void WriteVehicles()
		{
			Tools.StartRealm();
			NPCMgr.Initialize();
			NPCMgr.LoadNPCDefs();

			using (var writer = new IndentTextWriter(ToolConfig.OutputDir + "Vehicles.txt"))
			{
				foreach (var entry in NPCMgr.GetAllEntries())
				{
					if (entry.IsVehicle)
					{
						writer.WriteLine(entry);
					}
				}

				foreach (var entry in NPCMgr.GetAllEntries())
				{
					if (entry.IsVehicle)
					{
						entry.Dump(writer);
						writer.WriteLine();
						writer.WriteLine(" ################################### ");
						writer.WriteLine();
					}
				}
			}
		}


		public static void WriteClasses()
		{
			WritePerByte(true, 1, "Class", typeof (ClassId), typeof (int));
		}

		public static void WriteRaces()
		{
			WritePerByte(true, 0, "Race", typeof (RaceId), typeof (byte));
		}

		public static void WriteSheath()
		{
			WritePerByte(false, 0, "SheathState", typeof (SheathType), typeof (byte));
		}

		public static void WritePetState()
		{
			WritePerByte(false, 2, "PetState", typeof (PetState), typeof (byte));
		}

		public static void WritePerByte(bool bytes0, int no, string name, Type enumType, Type convertType)
		{
			using (var writer = new StreamWriter(ToolConfig.OutputDir + "/NPCBytes" + name + ".txt", false))
			{
				var set = new Dictionary<uint, NPCSpawnEntry>();
				foreach (var spawn in NPCMgr.SpawnEntries)
				{
					if (spawn == null)
						continue;

					set[spawn.Entry.Id] = spawn;
				}

				foreach (var spawn in set.Values)
				{
					var data = spawn.AddonData ?? spawn.Entry.AddonData;
					if (data != null)
					{
						uint byteSet = ((bytes0 ? data.Bytes : data.Bytes2) >> (no * 8)) & 0xff;
						if (byteSet != 0)
						{
							string str;
							if (enumType != null)
							{
								var obj = Convert.ChangeType(byteSet, convertType);
								str = Enum.Format(enumType, obj, "g");
							}
							else
								str = byteSet.ToString();
							writer.WriteLine("Spawn #{0} (Entry:{1}, {2}): {3}", spawn.SpawnId, spawn.Entry.Id, spawn.Entry.DefaultName, str);
							//return;
						}
					}
				}
			}
		}

		public static void WriteArr<T>(StreamWriter writer, string name, IEnumerable<T> col)
		{
			var strs = Utility.ToJoinedStringArr(col, 10, ", ");
			var br = "{";
			writer.Write("{0}[] {1} = new {0}[] {2}\n\t", typeof (T).Name, name, br);
			writer.WriteLine(string.Join(",\n\t", strs));
			writer.WriteLine("};");
			writer.WriteLine();
		}
	}
}