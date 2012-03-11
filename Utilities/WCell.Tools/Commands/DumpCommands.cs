using WCell.Constants;
using WCell.Tools.Domi.Output;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.Tools.Commands
{
    public class DumpCommand : ToolCommand
    {
        public static bool TypesMapped;

        protected override void Initialize()
        {
            Init("Dump", "D");
            EnglishDescription = "Provides commands to create dump files.";
            EnsureTypesMapped();
        }

        public class DumpSpellsCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Spells", "S");
                EnglishDescription = "Dumps all spells";
            }

            public override void Process(CmdTrigger<ToolCmdArgs> trigger)
            {
                Dump(trigger);
            }

            public static void Dump(CmdTrigger<ToolCmdArgs> trigger)
            {
                trigger.Reply("Dumping Spells...");
                SpellOutput.WriteSpellsAndEffects();
                trigger.Reply("Dumped Spells to: " + SpellOutput.DumpFile);
            }
        }

        public class DumpNPCsCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("NPCs");
                EnglishDescription = "Dumps all NPCs";
            }

            public override void Process(CmdTrigger<ToolCmdArgs> trigger)
            {
                Dump(trigger);
            }

            public static void Dump(CmdTrigger<ToolCmdArgs> trigger)
            {
                trigger.Reply("Dumping NPCs...");
                NPCOutput.WriteNPCs();
                trigger.Reply("Dumped NPCs to: " + NPCOutput.DumpFile);
            }
        }

        public class DumpGOsCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("GOs");
                EnglishDescription = "Dumps all Gameobjects";
            }

            public override void Process(CmdTrigger<ToolCmdArgs> trigger)
            {
                Dump(trigger);
            }

            public static void Dump(CmdTrigger<ToolCmdArgs> trigger)
            {
                trigger.Reply("Dumping GOs...");
                GOOutput.WriteGOs();
                trigger.Reply("Dumped GOs to: " + GOOutput.DumpFile);
            }
        }

        public class DumpItemsCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Items");
                EnglishDescription = "Dumps all Items";
            }

            public override void Process(CmdTrigger<ToolCmdArgs> trigger)
            {
                Dump(trigger);
            }

            public static void Dump(CmdTrigger<ToolCmdArgs> trigger)
            {
                trigger.Reply("Dumping Items...");
                ItemOutput.WriteAllItemInfo();
                trigger.Reply("Dumped Items to: " + ItemOutput.DefaultDumpFile);
            }
        }

        public class DumpQuestsCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Quests");
                EnglishDescription = "Dumps all Quests";
            }

            public override void Process(CmdTrigger<ToolCmdArgs> trigger)
            {
                Dump(trigger);
            }

            public static void Dump(CmdTrigger<ToolCmdArgs> trigger)
            {
                trigger.Reply("Dumping Quests...");
                QuestOutput.WriteAllQuests();
                trigger.Reply("Dumped Quests to: " + QuestOutput.DumpFile);
            }
        }

        public class DumpATsCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("ATs");
                EnglishDescription = "Dumps all AreaTriggers";
            }

            public override void Process(CmdTrigger<ToolCmdArgs> trigger)
            {
                Dump(trigger);
            }

            public static void Dump(CmdTrigger<ToolCmdArgs> trigger)
            {
                trigger.Reply("Dumping AreaTriggers...");
                ATOutput.DumpToFile();
                trigger.Reply("Dumped AreaTriggers to: " + ATOutput.DumpFile);
            }
        }

        public static void EnsureTypesMapped()
        {
            if (!TypesMapped)
            {
                Utility.AddTypesToTypeMap(typeof(WCellInfo).Assembly);
                TypesMapped = true;
            }
        }

        public class DumpAllCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("All");
                EnglishDescription = "Dumps all into files";
            }

            public override void Process(CmdTrigger<ToolCmdArgs> trigger)
            {
                Dump(trigger);
            }

            public void Dump(CmdTrigger<ToolCmdArgs> trigger)
            {
                DumpSpellsCommand.Dump(trigger);
                DumpNPCsCommand.Dump(trigger);
                DumpGOsCommand.Dump(trigger);
                DumpItemsCommand.Dump(trigger);
                DumpQuestsCommand.Dump(trigger);
                DumpATsCommand.Dump(trigger);
            }
        }
    }
}