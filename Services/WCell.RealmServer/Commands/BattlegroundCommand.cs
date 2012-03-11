using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class BattlegroundCommand : RealmServerCommand
    {
        protected BattlegroundCommand() { }

        protected override void Initialize()
        {
            Init("Battleground", "BG");
        }

        #region Cfg

        public class BattlegroundCfgCommand : BGSubCommand
        {
            protected override void Initialize()
            {
                Init("Config", "Cfg");
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                TriggerSubCommand(trigger);
            }

            public class BattlegroundCfgLoadCommand : SubCommand
            {
                protected override void Initialize()
                {
                    Init("Reload", "L");
                    EnglishDescription = "Reloads the Battleground config.";
                }

                public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
                {
                    BattlegroundConfig.LoadSettings();
                }
            }
        }

        #endregion Cfg

        #region List

        public class BattlegroundListCommand : BGSubCommand
        {
            protected BattlegroundListCommand() { }

            protected override void Initialize()
            {
                Init("List", "L");
                EnglishParamInfo = "[<bgid>]";
                EnglishDescription = "Shows an overview over all existing BGs. " +
                    "Optionally, you can choose to only show BGs of a given type.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                IEnumerable<Battleground> instances;
                if (trigger.Text.HasNext)
                {
                    var bgId = trigger.Text.NextEnum(BattlegroundId.End);
                    if (bgId == BattlegroundId.End)
                    {
                        trigger.Reply("Invalid BattlegroundId.");
                        return;
                    }
                    instances = BattlegroundMgr.Instances.GetInstances(bgId);
                }
                else
                {
                    instances = BattlegroundMgr.Instances.GetAllInstances();
                }

                List(trigger, instances);
            }

            /// <summary>
            /// Shaky method - BG can be disposed at any point during the iteration
            /// </summary>
            public static int List(CmdTrigger<RealmServerCmdArgs> trigger, IEnumerable<Battleground> bgs)
            {
                var count = bgs.Count();
                if (count > 0)
                {
                    trigger.Reply("Found {0} Battlegrounds:", count);

                    foreach (var bg in bgs)
                    {
                        BattlegroundInfoCommand.DisplayInfo(trigger, bg);
                    }
                }
                else
                {
                    trigger.Reply("Found no (matching) Battlegrounds.");
                }

                return count;
            }
        }

        #endregion List

        #region Info

        public class BattlegroundInfoCommand : BGSubCommand
        {
            protected override void Initialize()
            {
                Init("Info", "I");
                EnglishParamInfo = "[-i <BGId> <InstanceId>]";
                EnglishDescription = "Shows some information about the current or (if -s switch is used) given Battleground.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var bg = GetBG(trigger);

                if (bg != null)
                {
                    DisplayInfo(trigger, bg);
                }
            }

            public static void DisplayInfo(CmdTrigger<RealmServerCmdArgs> trigger, Battleground bg)
            {
                trigger.Reply(bg.ToString());
                trigger.Reply(" " + bg.GetTeam(BattlegroundSide.Alliance));
                trigger.Reply(" " + bg.GetTeam(BattlegroundSide.Horde));
            }
        }

        #endregion Info

        #region Prepare

        public class BattlegroundPrepareCommand : BGSubCommand
        {
            protected BattlegroundPrepareCommand() { }

            protected override void Initialize()
            {
                Init("Prepare");
                EnglishParamInfo = "[-i <BGId> <InstanceId>]";
                EnglishDescription = "Starts preparation time.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var bg = GetBG(trigger);

                if (bg != null)
                {
                    if (bg.Status < BattlegroundStatus.Preparing)
                        bg.StartPreparation();
                    else
                        trigger.Reply("The battleground is already in progress !");
                }
            }
        }

        #endregion Prepare

        #region Create

        public class BattlegroundCreateCommand : SubCommand
        {
            protected BattlegroundCreateCommand() { }

            protected override void Initialize()
            {
                Init("Create", "C");
                EnglishParamInfo = "[-[i]|[e][l <level>]] <BGId>";
                EnglishDescription = "Creates a new Instance of the given BG type. " +
                    "-e enters it right away. " +
                    "-i invites the target to the target's Faction's Team. " +
                    "-l determines the level range of the newly created instance.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var mod = trigger.Text.NextModifiers();
                var target = trigger.Args.Target;

                int level;
                if (mod.Contains("l"))
                {
                    level = trigger.Text.NextInt();
                    if (level < 1)
                    {
                        trigger.Reply("Invalid level.");
                        return;
                    }
                }
                else
                {
                    if (target == null)
                    {
                        trigger.Reply("You need to specify a level with the -l switch.");
                        return;
                    }
                    level = target.Level;
                }

                var bgid = trigger.Text.NextEnum(BattlegroundId.End);
                var templ = BattlegroundMgr.GetTemplate(bgid);

                if (templ == null)
                {
                    trigger.Reply("Invalid BGId: {0}", bgid);
                    return;
                }
                if (level < templ.MinLevel || level > templ.MaxLevel)
                {
                    trigger.Reply("Invalid level: Must be between {0} and {1}", templ.MinLevel, templ.MaxLevel);
                    return;
                }

                var queue = templ.GetQueue(level);
                var instance = queue.CreateBattleground();

                if (instance != null)
                {
                    trigger.Reply("Battleground created: " + instance);
                    if (mod.Length == 0) return;

                    // process modifiers
                    var chr = target as Character;

                    if (mod.Contains("i"))
                    {
                        // invite
                        if (chr == null)
                        {
                            // requires Character
                            trigger.Reply("-i modifier requires a Character target: " + mod);
                            return;
                        }
                        var team = instance.GetTeam(chr.FactionGroup.GetBattlegroundSide());
                        team.Invite(chr);
                    }

                    if (mod.Contains("e"))
                    {
                        // enter
                        if (chr == null)
                        {
                            // requires Character
                            trigger.Reply("-e modifier requires a Character target: " + mod);
                            return;
                        }
                        instance.TeleportInside(chr);
                    }
                }
                else
                {
                    trigger.Reply("Unable to create Battleground: " + bgid);
                }
            }
        }

        #endregion Create

        #region Enter

        public class BattlegroundEnterCommand : BGSubCommand
        {
            protected BattlegroundEnterCommand() { }

            protected override void Initialize()
            {
                Init("Enter", "E");
                EnglishParamInfo = "[-i <BGId> <InstanceId>] [g]";
                EnglishDescription = "Teleports the Character target into the given instance. " +
                    "g teleports the entire group.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var chr = trigger.Args.Target as Character;
                if (chr == null)
                {
                    trigger.Reply("No Character given.");
                    return;
                }

                var mod = trigger.Text.NextWord();
                var instance = GetBG(trigger);

                ICharacterSet chrs;
                if (mod.Contains("g"))
                {
                    chrs = chr.Group;
                    if (chrs == null)
                    {
                        trigger.Reply(chr + " is not in Group.");
                        return;
                    }
                }
                else
                {
                    chrs = chr;
                }

                if (instance == null)
                {
                    return;
                }

                chrs.ForeachCharacter(instance.TeleportInside);
            }
        }

        #endregion Enter

        #region Join

        public class BattlegroundJoinCommand : BGSubCommand
        {
            protected BattlegroundJoinCommand() { }

            protected override void Initialize()
            {
                Init("Invite", "Inv", "Join");
                EnglishParamInfo = "[-i <BGId> <InstanceId>] [[g][s <side>]]";
                EnglishDescription = "Invites oneself or the target to an existing Battleground. " +
                    "Use s to select a side. " +
                    "Use g to select the entire Group of the Character.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var chr = trigger.Args.Target as Character;
                if (chr == null)
                {
                    trigger.Reply("No Character given.");
                    return;
                }

                var instance = GetBG(trigger);
                var mod = trigger.Text.NextWord();

                if (instance == null)
                {
                    return;
                }

                BattlegroundSide side;
                if (mod.Contains("s"))
                {
                    side = trigger.Text.NextEnum(BattlegroundSide.End);
                    if (side == BattlegroundSide.End)
                    {
                        trigger.Reply("Invalid side (Horde or Alliance)");
                        return;
                    }
                }
                else
                {
                    side = chr.FactionGroup.GetBattlegroundSide();
                }

                ICharacterSet chrs;
                if (mod.Contains("g"))
                {
                    chrs = chr.Group;
                    if (chrs == null)
                    {
                        trigger.Reply(chr + " is not in Group.");
                        return;
                    }
                }
                else
                {
                    chrs = chr;
                }

                var team = instance.GetTeam(side);
                team.Invite(chrs);
            }
        }

        #endregion Join

        #region Delete

        public class BattlegroundDeleteCommand : BGSubCommand
        {
            protected BattlegroundDeleteCommand() { }

            protected override void Initialize()
            {
                Init("Delete", "Del");
                EnglishParamInfo = "[-i <BGId> <InstanceId>]";
                EnglishDescription = "Deletes the Battleground of the given Map with the given Id, or the current one if no arguments are supplied.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var bg = GetBG(trigger);

                if (bg != null)
                {
                    bg.Delete();
                    trigger.Reply("Battleground Deleted");
                }
            }
        }

        #endregion Delete

        public abstract class BGSubCommand : SubCommand
        {
            public Battleground GetBG(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var chr = trigger.Args.Target as Character;
                Battleground bg;
                if (trigger.Text.NextModifiers() == "i" ||
                    chr == null ||
                    (bg = chr.Map as Battleground) == null)
                {
                    var bgId = trigger.Text.NextEnum(BattlegroundId.End);
                    if (bgId == BattlegroundId.End)
                    {
                        trigger.Reply("Invalid BattlegroundId.");
                        bg = null;
                    }
                    else
                    {
                        var id = trigger.Text.NextUInt();
                        bg = BattlegroundMgr.Instances.GetInstance(bgId, id);
                        if (bg == null)
                        {
                            trigger.Reply("Invalid id (" + id + ") for " + bgId);
                        }
                    }
                }
                return bg;
            }
        }
    }

    public class DeserterCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            Init("FlagDeserter", "Deserter");
            EnglishDescription = "Flags the target as Deserter who is then " +
                "kicked out of the Battleground (if he/she is in any) and " +
                "is rendered unable to join any Battleground for a limited amount of time.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            trigger.Args.Target.SpellCast.TriggerSelf(SpellId.Deserter);
            trigger.Reply("Done.");
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }
    }
}