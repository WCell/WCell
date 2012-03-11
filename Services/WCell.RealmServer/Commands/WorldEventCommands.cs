using System;
using System.Linq;
using WCell.Constants.Updates;
using WCell.RealmServer.Global;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class WorldEventCommands : RealmServerCommand
    {
        protected override void Initialize()
        {
            Init("Event", "E");
            EnglishDescription = "Provides commands to manage World Events.";
        }

        #region Start/Stop

        public class StartEventCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Start", "S");
                EnglishParamInfo = "[-t [[-[smhdw] [<seconds>] [<minutes>] [<hours>] [<days>] [<weeks>]] [<Event Id>]]";
                EnglishDescription = "Starts a world event with the given event id with the default duration or the time provided." +
                    "Example: Use '.Event Start -t -m 20 4' to start event 4 for 20 minutes, or '.Event Start 4' to start event 4 with the default duration ";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var mod = trigger.Text.NextModifiers();
                TimeSpan? duration = null;
                if (mod == "t")
                {
                    duration = trigger.Text.NextTimeSpan();
                    if (duration == null)
                    {
                        trigger.Reply("Invalid Duration specified {0}", EnglishParamInfo);
                        return;
                    }
                }
                var id = trigger.Text.NextUInt();

                var worldEvent = WorldEventMgr.GetEvent(id);
                if (worldEvent == null)
                {
                    trigger.Reply("Invalid World Event {0} specified", id);
                    if (id == 0)
                    {
                        trigger.Reply("Usage:" + EnglishParamInfo);
                    }
                    return;
                }
                worldEvent.TimeUntilNextStart = TimeSpan.Zero;
                worldEvent.TimeUntilEnd = duration ?? worldEvent.Duration;
            }
        }

        public class EndEventCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("End", "E");
                EnglishDescription = "Stops a world event with the given event id.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var id = trigger.Text.NextUInt();

                var worldEvent = WorldEventMgr.GetEvent(id);
                if (worldEvent == null)
                {
                    trigger.Reply("Invalid World Event {0} specified", id);
                    if (id == 0)
                    {
                        trigger.Reply("Usage:" + EnglishParamInfo);
                    }
                    return;
                }

                WorldEvent.CalculateEventDelays(worldEvent);
            }
        }

        #endregion Start/Stop

        #region Find/List

        public class ListEventsCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("List", "L");
                EnglishDescription = "Lists all active events.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var activeEvents = WorldEventMgr.ActiveEvents.Where(worldEvent => worldEvent != null);
                foreach (var worldEvent in activeEvents)
                {
                    trigger.Reply("{0}: {1}, Ends {2}", worldEvent.Id, worldEvent.Description, DateTime.Now + worldEvent.TimeUntilEnd);
                }
                trigger.Reply("{0} Events found.", activeEvents.Count());
            }
        }

        public class FindEventCommand : SubCommand
        {
            private const int MinSearchChars = 3;

            protected override void Initialize()
            {
                Init("Find", "F");
                EnglishParamInfo = "<search text>";
                EnglishDescription = "Search for Events whose name contains the specified text.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var eventName = trigger.Text.Remainder.Trim();

                if (eventName.Length >= MinSearchChars)
                {
                    var eventsFound = WorldEventMgr.AllEvents.Where(wEvent => wEvent != null &&
                        (wEvent.Description.IndexOf(eventName, StringComparison.InvariantCultureIgnoreCase) >= 0));

                    foreach (var worldEvent in eventsFound)
                    {
                        trigger.Reply("{0}: {1}", worldEvent.Id, worldEvent.Description);
                    }
                    trigger.Reply("{0} Events found.", eventsFound.Count());
                }
                else
                {
                    trigger.Reply("Argument eventName requires at least {0} characters", MinSearchChars);
                }
            }
        }

        #endregion Find/List

        #region Disable

        public class DisableEventCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Remove", "R");
                EnglishParamInfo = "<Event Id>";
                EnglishDescription = "Disables the given world event.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var id = trigger.Text.NextUInt();

                var worldEvent = WorldEventMgr.GetEvent(id);
                if (worldEvent == null)
                {
                    trigger.Reply("Invalid World Event {0} specified", id);
                    if (id == 0)
                    {
                        trigger.Reply("Usage:" + EnglishParamInfo);
                    }
                    return;
                }

                worldEvent.TimeUntilNextStart = null;
                WorldEventMgr.StopEvent(worldEvent);
            }
        }

        #endregion Disable

        #region Edit

        public class EditDurationEventCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Duration", "D");
                EnglishParamInfo = "[-[smhdw] [<seconds>] [<minutes>] [<hours>] [<days>] [<weeks>] [<Event Id>]]";
                EnglishDescription = "Edits the duration of a world event";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var duration = trigger.Text.NextTimeSpan();
                var id = trigger.Text.NextUInt();

                var worldEvent = WorldEventMgr.GetEvent(id);
                if (worldEvent == null)
                {
                    trigger.Reply("Invalid World Event {0} specified", id);
                    return;
                }

                if (duration == null)
                {
                    trigger.Reply("Invalid Duration specified {0}", EnglishParamInfo);
                    return;
                }

                if (duration > worldEvent.Occurence)
                {
                    trigger.Reply("Invalid Duration {0} specified, must be less than the occurence {1}", duration, worldEvent.Occurence);
                    return;
                }
                worldEvent.Duration = (TimeSpan)duration;
                WorldEvent.CalculateEventDelays(worldEvent);
            }
        }

        public class EditOccuranceEventCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Occurence", "O");
                EnglishParamInfo = "[-[smhdw] [<seconds>] [<minutes>] [<hours>] [<days>] [<weeks>] [<Event Id>]]";
                EnglishDescription = "Edits the occurance of a world event";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var occurence = trigger.Text.NextTimeSpan();
                var id = trigger.Text.NextUInt();

                var worldEvent = WorldEventMgr.GetEvent(id);
                if (worldEvent == null)
                {
                    trigger.Reply("Invalid World Event {0} specified", id);
                    return;
                }

                if (occurence == null)
                {
                    trigger.Reply("Invalid Occurence specified {0}", EnglishParamInfo);
                    return;
                }

                if (occurence < worldEvent.Duration)
                {
                    trigger.Reply("Invalid Occurence {0} specified, must be greater than the duration {1}", occurence, worldEvent.Duration);
                    return;
                }
                worldEvent.Occurence = (TimeSpan)occurence;
                WorldEvent.CalculateEventDelays(worldEvent);
            }
        }

        #endregion Edit

        #region Info

        public class EventInfoCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Info", "I");
                EnglishParamInfo = "<Event Id>";
                EnglishDescription = "Shows information about the world event";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var id = trigger.Text.NextUInt();

                var worldEvent = WorldEventMgr.GetEvent(id);
                if (worldEvent == null)
                {
                    trigger.Reply("Invalid World Event {0} specified", id);
                    if (id == 0)
                    {
                        trigger.Reply("Usage:" + EnglishParamInfo);
                    }
                    return;
                }

                trigger.Reply("Event ({0}) {1}", id, worldEvent.Description);
                trigger.Reply("Absolute start date: {0}", worldEvent.From);
                trigger.Reply("Absolute end date:   {0}", worldEvent.Until);
                trigger.Reply("Duration:            {0}", worldEvent.Duration);
                trigger.Reply("Occurence interval:  {0}", worldEvent.Occurence);
                trigger.Reply("Next start in:       {0}", worldEvent.TimeUntilNextStart);
                trigger.Reply("Next end in:         {0}", worldEvent.TimeUntilEnd);
                trigger.Reply("Holiday Id:          {0}", worldEvent.HolidayId);
                trigger.Reply("GO Spawn count:      {0}", worldEvent.GOSpawns.Count);
                trigger.Reply("NPC Spawn count:     {0}", worldEvent.NPCSpawns.Count);
                trigger.Reply("NPC Data count:      {0}", worldEvent.ModelEquips.Count);
                trigger.Reply("Quest count:         {0}", worldEvent.QuestIds.Count);
            }
        }

        #endregion Info

        public override bool RequiresCharacter
        {
            get
            {
                return false;
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.None;
            }
        }
    }
}
