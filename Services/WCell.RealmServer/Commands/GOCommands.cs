/*************************************************************************
 *
 *   file		: GOCommands.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-04-23 15:13:50 +0200 (fr, 23 apr 2010) $

 *   revision		: $Rev: 1282 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Global;
using WCell.Util.Collections;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class GOCommand : RealmServerCommand
    {
        protected GOCommand() { }

        protected override void Initialize()
        {
            base.Init("GO", "GOs", "GameObject");
            EnglishDescription = "Used for interaction with and creation of GameObjects";
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.None; }
        }

        public override bool RequiresCharacter
        {
            get { return true; }
        }

        #region Spawn

        public class SpawnCommand : SubCommand
        {
            protected SpawnCommand() { }

            protected override void Initialize()
            {
                Init("Spawn", "Create", "Add");
                EnglishParamInfo = "[-c] [<GOId>]";
                EnglishDescription = "Creates a new GameObject with the given id at the current position. " +
                    "-c spawns the closest GO in the Area and teleports you there.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var mod = trigger.Text.NextModifiers();
                var id = trigger.Text.NextEnum(GOEntryId.End);

                var entry = GOMgr.GetEntry(id, false);

                var target = trigger.Args.Target;
                var map = target != null ? target.Map : World.Kalimdor;

                if (mod == "c")
                {
                    // spawn closest
                    GOSpawnEntry closest;

                    if (entry != null)
                    {
                        // Entry specified
                        closest = entry.SpawnEntries.GetClosestEntry(target);
                    }
                    else
                    {
                        // no entry, just spawn any nearby Template
                        var templates = GOMgr.GetSpawnPoolTemplatesByMap(map.Id);
                        closest = templates == null ? null : templates.GetClosestEntry(target);
                    }

                    if (closest == null)
                    {
                        trigger.Reply("No valid SpawnEntries found (Entry: {0})", entry);
                    }
                    else
                    {
                        closest.Spawn(map);
                        trigger.Reply("Spawned: " + closest.Entry);
                        if (target != null)
                        {
                            target.TeleportTo(map, closest.Position);
                        }
                    }
                }
                else
                {
                    if (entry != null)
                    {
                        // spawn a new GO
                        var go = entry.Spawn(trigger.Args.Target, trigger.Args.Target);
                        trigger.Reply("Successfully spawned new GO: {0}.", go.Name);
                    }
                    else
                    {
                        trigger.Reply("Invalid GO.");
                    }
                }
            }
        }

        #endregion Spawn

        #region Select

        public class SelectCommand : SubCommand
        {
            protected SelectCommand() { }

            protected override void Initialize()
            {
                Init("Select", "Sel");
                EnglishParamInfo = "";
                EnglishDescription = "Selects the next GameObject in front of you";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Select(trigger);
            }

            public static void Select(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var go = GOSelectMgr.Instance.SelectClosest(trigger.Args.Character);
                if (go != null)
                {
                    if (!trigger.Args.HasCharacter)
                    {
                        trigger.Args.Context = go;
                    }
                    trigger.Reply("Selected: " + go);
                }
                else
                {
                    trigger.Reply("No Object in front of you within {0} yards.", GOSelectMgr.MaxSearchRadius);
                }
            }
        }

        public class DeselectCommand : SubCommand
        {
            protected DeselectCommand() { }

            protected override void Initialize()
            {
                Init("Deselect", "Des");
                EnglishParamInfo = "";
                EnglishDescription = "Deselects your currently selected GameObject";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                GOSelectMgr.Instance.Deselect(trigger.Args.Character.ExtraInfo);
                trigger.Args.Context = null;
                trigger.Reply("Done.");
            }
        }

        #endregion Select

        #region Set / Get / Call

        public class GoSetCommand : SubCommand
        {
            protected GoSetCommand() { }

            protected override void Initialize()
            {
                Init("Set", "S");
                EnglishParamInfo = "<some.prop> <someValue>";
                EnglishDescription = "Sets properties on the currently selected GO";
            }

            public override RoleStatus DefaultRequiredStatus
            {
                get
                {
                    return RoleStatus.Admin;
                }
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                SetCommand.Set(trigger, trigger.Args.Character.ExtraInfo.SelectedGO);
            }
        }

        public class GoGetCommand : SubCommand
        {
            protected GoGetCommand() { }

            protected override void Initialize()
            {
                Init("Get", "G");
                EnglishParamInfo = "<some.prop>";
                EnglishDescription = "Gets properties on the currently selected GO";
            }

            public override RoleStatus DefaultRequiredStatus
            {
                get
                {
                    return RoleStatus.Admin;
                }
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                //var info = trigger.Args.Character.ExtraInfo;
                //if (info != null)
                //{
                //    Commands.GetCommand.Get(trigger, info.SelectedGO);
                //}
                //else
                //{
                //    Commands.GetCommand.Get(trigger, null);
                //}
                Commands.GetCommand.GetAndReply(trigger, trigger.Args.Character.ExtraInfo.SelectedGO);
            }
        }

        public class GoCallCommand : SubCommand
        {
            protected GoCallCommand() { }

            protected override void Initialize()
            {
                Init("Call");
                EnglishParamInfo = "<some.method> [arg1 [, arg2, ...]]";
                EnglishDescription = "Calls the given method with parameters on the currently selected GO";
            }

            public override RoleStatus DefaultRequiredStatus
            {
                get
                {
                    return RoleStatus.Admin;
                }
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Commands.CallCommand.Call(trigger, trigger.Args.Character.ExtraInfo.SelectedGO);
            }
        }

        #endregion Set / Get / Call

        #region Anim

        public class AnimCommand : SubCommand
        {
            protected AnimCommand() { }

            protected override void Initialize()
            {
                Init("Anim", "Animation");
                EnglishParamInfo = "<animValue>";
                EnglishDescription = "Animates the selected GO with the given parameter";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var go = trigger.Args.Character.ExtraInfo.SelectedGO;
                if (go == null)
                {
                    trigger.Reply("No object selected.");
                }
                else
                {
                    var value = trigger.Text.NextUInt(1);
                    go.SendCustomAnim(value);
                }
            }
        }

        #endregion Anim

        #region Toggle

        public class GoToggleCommand : SubCommand
        {
            protected GoToggleCommand() { }

            protected override void Initialize()
            {
                Init("Toggle", "T");
                EnglishParamInfo = "[<value>]";
                EnglishDescription = "Toggles the state on the selected GO or the one in front of you";
            }

            public override RoleStatus DefaultRequiredStatus
            {
                get { return RoleStatus.Staff; }
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var go = trigger.Args.Character.ExtraInfo.SelectedGO;
                if (go == null)
                {
                    SelectCommand.Select(trigger);
                }
                if (go != null)
                {
                    var state = trigger.Text.HasNext ? trigger.Text.NextBool() : !go.IsEnabled;
                    go.IsEnabled = state;
                    trigger.Reply("{0} is now {1}", go, state == true ? "enabled" : "disabled");
                }
            }
        }

        #endregion Toggle
    }

    #region Highlight

    public class HighlightGOCommand : RealmServerCommand
    {
        protected HighlightGOCommand() { }

        public static readonly SynchronizedDictionary<Character, Dictionary<DynamicObject, GameObject>> Highlighters =
            new SynchronizedDictionary<Character, Dictionary<DynamicObject, GameObject>>();

        protected override void Initialize()
        {
            Init("HighlightGOs", "HLGOs");
            EnglishParamInfo = "[0/1]";
            EnglishDescription = "Highlights all GOs around yourself";

            // uh, let's reconsider this a little
            Enabled = true;
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Dictionary<DynamicObject, GameObject> highlighters;
            var exists = Highlighters.TryGetValue(trigger.Args.Character, out highlighters);
            var create = trigger.Text.NextBool() || (!trigger.Text.HasNext && !exists);

            if (!create)
            {
                if (exists)
                {
                    foreach (var dynObj in highlighters.Keys)
                    {
                        dynObj.Delete();
                    }
                    highlighters.Clear();
                    Highlighters.Remove(trigger.Args.Character);
                }
                trigger.Reply("GO Highlighters OFF");
            }
            else
            {
                if (!exists)
                {
                    Highlighters.Add(trigger.Args.Character, highlighters = new Dictionary<DynamicObject, GameObject>());
                }
                else
                {
                    foreach (var dynObj in highlighters.Keys)
                    {
                        dynObj.Delete();
                    }
                    highlighters.Clear();
                }

                var caster = trigger.Args.Character;

                var gos = caster.GetObjectsInRadius(50f, ObjectTypes.GameObject, false, 0);
                foreach (GameObject go in gos)
                {
                    var map = go.Map;
                    var pos = go.Position;
                    pos.Z += 7 * go.ScaleX;						// make it appear above the object

                    var dO = new DynamicObject(caster, SpellId.ABOUTTOSPAWN, 5, map, pos);
                    highlighters.Add(dO, go);
                }
                trigger.Reply("Highlighting {0} GameObjects", highlighters.Count);
            }
        }

        public override bool RequiresCharacter
        {
            get
            {
                return true;
            }
        }
    }

    #endregion Highlight

    #region Portal

    public class PortalCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            Init("Portal");
            EnglishParamInfo = "<target loc>";
            EnglishDescription = "Creates a new Portal to the given Target location.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var locationName = trigger.Text.Remainder;
            var target = trigger.Args.Target;

            if (locationName.Length < 2)
            {
                trigger.Reply("Invalid search term: " + locationName);
            }
            else if (trigger.Args.Character != null)
            {
                var locs = WorldLocationMgr.GetMatches(locationName);

                if (locs.Count == 0)
                {
                    trigger.Reply("No matches found for: " + locationName);
                    return;
                }
                else if (locs.Count > 20)
                {
                    trigger.Reply("Found too many matches ({0}), please narrow down the location.", locs.Count);
                    return;
                }
                else if (locs.Count == 1)
                {
                    // single location
                    CreatePortal(target, locs[0]);
                }
                else
                {
                    // multiple locations
                    trigger.Args.Character.StartGossip(WorldLocationMgr.CreateTeleMenu(locs, (convo, loc) => CreatePortal(target, loc)));
                }
            }
            else
            {
                var loc = WorldLocationMgr.GetFirstMatch(locationName);
                if (loc != null)
                {
                    CreatePortal(target, loc);
                }
                else
                {
                    trigger.Reply("No matches found for: " + locationName);
                }
            }
        }

        private void CreatePortal(WorldObject at, IWorldLocation target)
        {
            // create portal
            var portal = Portal.Create(at, target);
            at.PlaceInFront(portal);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }
    }

    #endregion Portal
}