using System;
using System.Collections.Generic;
using System.IO;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Quests;
using WCell.Util;
using WCell.Util.Toolshed;

namespace WCell.Tools.Domi.Output
{
    public static class QuestOutput
    {
        public static readonly string DumpFile = ToolConfig.OutputDir + "Quests.txt";

        public static void Foreach(Action<QuestTemplate> action)
        {
            foreach (var quest in QuestMgr.Templates)
            {
                if (quest != null)
                {
                    action(quest);
                }
            }
        }

        [Tool]
        public static void WriteAllQuests()
        {
            Tools.StartRealm();
            QuestMgr.LoadAll();
            ItemMgr.LoadAll();
            NPCMgr.LoadNPCDefs();
            GOMgr.LoadAll();
            AreaTriggerMgr.Initialize();

            WriteQuests("Quests", null);
        }

        public static void WriteQuests(string fileName,
            //Func<QuestTemplate, Dictionary<string, QuestTemplate>, bool> filter,
                                       Func<TextWriter, QuestTemplate, bool> extraOuput)
        {
            using (var writer = new IndentTextWriter(new StreamWriter(ToolConfig.OutputDir + fileName + ".txt", false)))
            {
                var quests = new List<QuestTemplate>(10000);
                foreach (var quest in QuestMgr.Templates)
                {
                    if (quest == null)
                        continue;

                    //if (filter == null || filter(quest, quests))
                    //{
                    //    quests[quest.Title] = quest;
                    //}
                    quests.Add(quest);
                }

                writer.WriteLine("Found {0} Quests:", quests.Count);
                writer.WriteLine();

                if (extraOuput != null)
                {
                    foreach (var quest in quests)
                    {
                        writer.WriteLine("{0} (Id: {1})", quest.DefaultTitle, quest.Id);

                        extraOuput(writer, quest);
                    }
                }

                writer.WriteLine();
                writer.WriteLine("##########################################");
                writer.WriteLine();

                foreach (var spell in quests)
                {
                    DumpQuest(writer, spell);
                }
            }
        }

        public static void DumpQuest(IndentTextWriter writer, QuestTemplate quest)
        {
            writer.WriteLine("Quest: " + quest);
            writer.IndentLevel++;
            quest.Dump(writer);
            writer.IndentLevel--;

            writer.WriteLine();
            writer.WriteLine("##################################################################");
            writer.WriteLine();
        }
    }
}