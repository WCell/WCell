/*************************************************************************
 *
 *   file		: DefaultCommands.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-26 21:35:40 +0800 (Thu, 26 Feb 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 770 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core;
using WCell.RealmServer.Privileges;
using WCell.RealmServer.Stats;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Intercommunication.DataTypes;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Commands
{
    #region Set
    /// <summary>
    /// 
    /// </summary>
    public class SetCommand : RealmServerCommand
    {
        protected SetCommand() { }

        protected override void Initialize()
        {
            Init("Set", "S");
            EnglishParamInfo = "<prop.subprop.otherprop.etc> <value>";
            EnglishDescription = "Sets the value of the given prop";
        }

        public override RoleStatus RequiredStatusDefault
        {
            get
            {
                return RoleStatus.Admin;
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.All;
            }
        }

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			Set(trigger, trigger.EvalNextOrTargetOrUser());
		}

        /// <summary>
        /// Whether we have a valid target.
        /// </summary>
        public static bool Set(CmdTrigger<RealmServerCmdArgs> trigger, Object target)
        {
            if (target == null)
            {
                trigger.Reply("Nothing selected.");
            }
            else if (trigger.Text.HasNext)
            {
                object propHolder;
                var prop = ReflectUtil.Instance.GetProp(trigger.Args.User.Role, target, trigger.Text.NextWord(),
                    target.GetType(), out propHolder);

                SetProp(propHolder, prop, trigger);
            }
            else
            {
                trigger.Reply("Invalid arguments.");
                trigger.Reply(trigger.Command.CreateInfo(trigger));
            }
            return true;
        }

        public static void SetProp(object propHolder, MemberInfo prop, CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (prop != null && ReflectUtil.Instance.CanWrite(prop, trigger.Args.User.Role))
            {
                var expr = trigger.Text.Remainder.Trim();
                if (expr.Length == 0)
                {
                    trigger.Reply("No expression given");
                }
                else
                {
                    var exprType = prop.GetVariableType();
                    object actualValue = null;

                    if (exprType.IsInteger())
                    {
                        object error = null;
                        long value = 0;
                        if (!Utility.Eval(exprType, ref value, expr, ref error, false))
                        {
                            trigger.Reply("Invalid expression: " + error);
                            return;
                        }
                        actualValue = ConvertActualType(value, exprType);
                    }
                    else
                    {
                        if (!Utility.Parse(expr, exprType, ref actualValue))
                        {
                            trigger.Reply("Could not change value \"{0}\" to Type: {1}", expr, exprType);
                            return;
                        }
                    }

                    prop.SetUnindexedValue(propHolder, actualValue);
                    string strValue;
                    if (exprType.IsEnum)
                    {
                        strValue = Enum.Format(exprType, actualValue, "g");
                    }
                    else
                    {
                        strValue = actualValue.ToString();
                    }
                    trigger.Reply("{0} is now {1}.", prop.Name, strValue);
                }
            }
            else
            {
                trigger.Reply("Invalid field.");
            }
        }

        public static object ConvertActualType(long val, Type type)
        {
            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }
            return Convert.ChangeType(val, type);
        }
    }
    #endregion

    #region Get
    /// <summary>
    /// General set-command. Can be used to register further set-handlers
    /// </summary>
    public class GetCommand : RealmServerCommand
    {
        protected GetCommand() { }

        protected override void Initialize()
        {
            Init("Get", "G");
            EnglishParamInfo = "<prop.subprop.otherprop.etc>";
            EnglishDescription = "Gets the value of the given prop";
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            GetAndReply(trigger, trigger.EvalNextOrTargetOrUser());
        }

        public override object Eval(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            return Eval(trigger, trigger.EvalNextOrTargetOrUser());
        }

        public static void GetAndReply(CmdTrigger<RealmServerCmdArgs> trigger, object target)
        {
            if (trigger.Text.HasNext)
            {
                var propName = trigger.Text.NextWord();
                object val;

				if (ReflectUtil.Instance.GetPropValue(trigger.Args.User.Role, target, ref propName, out val))
                {
                    trigger.Reply("{0} is: {1}", propName, val != null ? Utility.GetStringRepresentation(val) : "<null>");
                }
                else
                {
                    trigger.Reply("Invalid property.");
                }
                return;
            }
            else
            {
                trigger.Reply("Invalid arguments:");
                trigger.Reply(trigger.Command.CreateInfo(trigger));
            }
        }

        public static object Eval(CmdTrigger<RealmServerCmdArgs> trigger, object target)
        {
            var propName = trigger.Text.NextWord();
            object val;

            ReflectUtil.Instance.GetPropValue(trigger.Args.User.Role, target, ref propName, out val);
            return val;
        }
    }
    #endregion

    #region ModProp
    public class ModPropCommand : RealmServerCommand
    {
        protected ModPropCommand() { }

        public override RoleStatus RequiredStatusDefault
        {
            get
            {
                return RoleStatus.Admin;
            }
        }

        protected override void Initialize()
        {
            Init("Mod", "M");
            EnglishParamInfo = "<prop> <op> <value> [<op> <value> [<op> <value>...]]";
            EnglishDescription = "Modifies the given prop by applying it with an evaluated expression";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (trigger.Args.Target == null)
            {
                trigger.Reply("Nothing selected.");
            }
            else
            {
                ModProp(trigger, trigger.Args.Target);
            }
        }

        public static void ModProp(CmdTrigger<RealmServerCmdArgs> trigger, object target)
        {
            var accessName = trigger.Text.NextWord();
            object propHolder;
			var prop = ReflectUtil.Instance.GetProp(trigger.Args.User.Role, target, accessName,
                                                     target.GetType(), out propHolder);

            ModProp(propHolder, prop, trigger);
        }

        public static void ModProp(object propHolder, MemberInfo prop, CmdTrigger<RealmServerCmdArgs> trigger)
        {
			if (prop != null && ReflectUtil.Instance.CanWrite(prop, trigger.Args.User.Role))
            {
                var exprType = prop.GetVariableType();
                if (!exprType.IsInteger())
                {
                    trigger.Reply("Can only modify Integer-values.");
                }
                else
                {
                    var expr = trigger.Text.Remainder.Trim();
                    if (expr.Length == 0)
                    {
                        trigger.Reply("No expression given");
                    }
                    else
                    {
                        object error = null;
                        var value = Convert.ToInt64(prop.GetUnindexedValue(propHolder));

                        if (!Utility.Eval(exprType, ref value, expr, ref error, true))
                        {
                            trigger.Reply("Invalid expression: " + error);
                        }
                        else
                        {
                            var actualValue = TryParseEnum(value, exprType);
                            prop.SetUnindexedValue(propHolder, actualValue);
                            string strValue;
                            if (exprType.IsEnum)
                            {
                                strValue = Enum.Format(exprType, actualValue, "g");
                            }
                            else
                            {
                                strValue = value.ToString();
                            }
                            trigger.Reply("Success: {0} is now {1}.", prop.Name, strValue);
                        }
                    }
                }
            }
            else
            {
                trigger.Reply("Invalid field.");
            }
        }

        public static object TryParseEnum(long val, Type type)
        {
            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }
            return Convert.ChangeType(val, type);
        }
    }
    #endregion

    #region Call
    /// <summary>
    /// Calls methods! :D
    /// Should be for developers (and Admins) only
    /// </summary>
    public class CallCommand : RealmServerCommand
    {
        private static CallCommand s_instance;
        public static CallCommand Instance
        {
            get { return s_instance; }
        }

        protected CallCommand()
        {
            s_instance = this;
        }

        public override RoleStatus RequiredStatusDefault
        {
            get
            {
                return RoleStatus.Admin;
            }
        }

        protected override void Initialize()
        {
            Init("Call", "C");
            EnglishParamInfo = "<some.Method> [param1, [param2 [...]]]";
            EnglishDescription = "Calls the given method with the given params";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Call(trigger, trigger.EvalNextOrTargetOrUser());
        }

        public override object Eval(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            return Eval(trigger, trigger.EvalNextOrTargetOrUser());
        }

        public static void Call(CmdTrigger<RealmServerCmdArgs> trigger, Object obj)
        {
            if (trigger.Text.HasNext)
            {
                var accessName = trigger.Text.NextWord();
                var args = trigger.Text.Remainder.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < args.Length; i++)
                {
                    args[i] = args[i].Trim();
                }
                try
                {
                    object result;
					if (ReflectUtil.Instance.CallMethod(trigger.Args.Character.Role, obj,
                        ref accessName, args, out result))
                    {
                        trigger.Reply("Success! {0}", result != null ? ("- Return value: " + result) : "");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    trigger.Reply("Exception thrown: " + ex);
                    return;
                }
            }
            trigger.Reply("Invalid method, parameter count or parameters.");
        }

        public static object Eval(CmdTrigger<RealmServerCmdArgs> trigger, Object obj)
        {
            if (trigger.Text.HasNext)
            {
                var accessName = trigger.Text.NextWord();
                var args = trigger.Text.Remainder.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < args.Length; i++)
                {
                    args[i] = args[i].Trim();
                }
                try
                {
                    object result;
					if (ReflectUtil.Instance.CallMethod(trigger.Args.Character.Role, obj,
                        ref accessName, args, out result))
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    trigger.Reply("Exception thrown: " + ex);
                    return null;
                }
            }
            trigger.Reply("Invalid method, parameter count or parameters.");
            return null;
        }
    }
    #endregion

    #region Remove
    public class RemoveCommand : RealmServerCommand
    {
        protected RemoveCommand() { }

        protected override void Initialize()
        {
            Init("Remove", "Delete", "Del");
            EnglishDescription = "Deletes the current target (NPC or GO)";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var target = trigger.Args.SelectedUnitOrGO;

            if (target != null)
            {
                target.Delete();
                trigger.Reply("Removed {0}.", target);
            }
            else
            {
                trigger.Reply("Nothing selected.");
            }
        }
    }
    #endregion

    #region ClearArea
    public class ClearAreaCommand : RealmServerCommand
    {
        protected ClearAreaCommand() { }

        public static int DefaultRadius = 10;

        protected override void Initialize()
        {
            Init("ClearArea");
            EnglishParamInfo = "[<radius>]";
            EnglishDescription = "Clears all Objects, Corpses and NPCs around yourself in the given or default radius (" + DefaultRadius + "), up to a max of 100 yards.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var radius = trigger.Text.NextInt(DefaultRadius);
            radius = Math.Max(1, radius);
            radius = Math.Min(100, radius);

            var objects = trigger.Args.Target.GetObjectsInRadius(radius, ObjectTypes.All, false, 0);

            foreach (var obj in objects)
            {
                if (!(obj is Character))
                {
                    obj.Delete();
                }
            }
            trigger.Reply("Removed {0} Objects and NPCs within {1} yards.", objects.Count, radius);
        }


        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.All;
            }
        }
    }
    #endregion

    #region Distance
    public class DistanceCommand : RealmServerCommand
    {
        protected DistanceCommand() { }

        protected override void Initialize()
        {
            Init("Distance", "Dist");
            EnglishDescription = "Measures the distance between you and the currently target object (including selected GOs).";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var target = trigger.Args.SelectedUnitOrGO;

            if (target != null)
            {
                trigger.Reply("The distance is: " + trigger.Args.Character.GetDistance(target));
            }
            else
            {
                trigger.Reply("Nothing selected.");
            }
        }
    }
    #endregion

    #region Stats
    public class ServerStatsCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            Init("Stats");
            EnglishDescription = "Provides commands to show and manage server-statistics.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            foreach (var line in RealmStats.Instance.GetFullStats())
            {
                trigger.Reply(line);
            }
        }

        //public class DisplayStatsCommand : SubCommand
        //{
        //    protected override void Initialize()
        //    {
        //        Init("Display", "Show", "D");
        //        Description = "Displays the current Server Stats";
        //    }

        //    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        //    {
        //        foreach (var line in RealmStats.Instance.GetFullStats())
        //        {
        //            trigger.Reply(line);
        //        }
        //    }
        //}
    }
    #endregion

    #region Drain Soul
    public class ChannelCommand : RealmServerCommand
    {
        #region Overrides of BaseCommand<RealmServerCmdArgs>

        protected override void Initialize()
        {
            Init("Channel");
            EnglishParamInfo = "<spellid>";
            EnglishDescription = "Channels the given Spell on the current Target.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var caster = trigger.Args.Target;
            if (caster == null)
            {
                trigger.Reply("No valid caster");
                return;
            }
            var target = caster.Target;
            if (target == null)
            {
                target = trigger.Args.Character;
                if (target == null)
                {
                    trigger.Reply("No valid target");
                    return;
                }
            }

            caster.CancelSpellCast();

            var spellId = trigger.Text.NextEnum(SpellId.None);
            if (spellId == SpellId.None)
            {
                if (caster.ChannelSpell != SpellId.None)
                {
                    caster.ChannelSpell = spellId;
                    caster.ChannelObject = null;
                }
                else
                {
                    trigger.Reply("Invalid SpellId.");
                }
                return;
            }

            //SpellHandler.SendCastStart(caster, spellId, 0, target);
            //SpellHandler.SendSpellGo(caster, spellId, target);
            //SpellHandler.SendChannelStart(caster, spellId, 33600000);

            caster.ChannelSpell = spellId;
            caster.ChannelObject = target;
        }

        public override bool NeedsCharacter
        {
            get
            {
                return true;
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.All;
            }
        }

        #endregion
    }
    #endregion

    #region Pin
    public class PinCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            Init("Pin", "PinDown", "Freeze");
            EnglishDescription = "Pins the targeted Unit down. Pinned down Units cannot move, fight or logout.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var target = trigger.Args.Target;
            if (target == trigger.Args.User)
            {
                target = target.Target;
                if (target == null)
                {
                    trigger.Reply("You cannot pin yourself down.");
                    return;
                }
            }

            target.IsPinnedDown = trigger.Text.NextBool(!target.IsPinnedDown);
            trigger.Reply("{0} has been {1}.", target.Name, target.IsPinnedDown ? "Pinned" : "Unpinned");
        }

        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.Unit;
            }
        }
    }
    #endregion

    #region ListPlayers
    public class ListPlayersCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            Init("ListPlayers");
            EnglishParamInfo = "[-[rfcna] [<Region>]|[<Faction>]|[<Class>]|[<namepart>]|[<accountnamepart>]]";
            EnglishDescription = "Lists all currently logged in Players.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (World.CharacterCount == 0)
            {
                trigger.Reply("There are no characters online.");
                return;
            }

            var mod = trigger.Text.NextModifiers();
            //var matches = new List<Character>();
            var matches = World.GetAllCharacters();

            MapId region;
            FactionId faction;
            ClassId classId;
            string namepart;
            string accnamepart;

            if (mod.Contains("r"))
            {
                region = trigger.Text.NextEnum(MapId.End);

                var rgn = World.GetRegion(region);
                if (rgn != null)
                {
                    if (rgn.CharacterCount > 0)
                    {
                        matches.Clear();
                        foreach (var chr in rgn.Characters)
                        {
                            matches.Add(chr);
                        }
                    }
                    else
                    {
                        trigger.Reply("There are no characters in region: {0}", rgn.Name);
                        return;
                    }
                }
                else
                {
                    trigger.Reply("Invalid region id.");
                    return;
                }
            }

            if (mod.Contains("f"))
            {
                faction = trigger.Text.NextEnum(FactionId.End);
                if (faction != FactionId.None && faction != FactionId.End)
                {
                    foreach (var chr in matches)
                    {
                        if (chr.FactionId != faction)
                        {
                            matches.Remove(chr);
                        }
                    }
                }
                else
                {
                    trigger.Reply("Invalid FactionID.");
                    return;
                }
            }

            if (mod.Contains("c"))
            {
                classId = trigger.Text.NextEnum(ClassId.End);
                if (classId == ClassId.End)
                {
                    trigger.Reply("Invalid class.");
                    return;
                }

                foreach (var chr in matches)
                {
                    if (chr.Class != classId)
                    {
                        matches.Remove(chr);
                    }
                }
            }

            if (mod.Contains("n"))
            {
                namepart = trigger.Text.NextWord();
                if (namepart.Length > 1)
                {
                    foreach (var chr in matches)
                    {
                        if (!chr.Name.Contains(namepart))
                        {
                            matches.Remove(chr);
                        }
                    }
                }
                else
                {
                    foreach (var chr in matches)
                    {
                        if (!chr.Name.StartsWith(namepart))
                        {
                            matches.Remove(chr);
                        }
                    }
                }
            }

            if (mod.Contains("a"))
            {
                accnamepart = trigger.Text.NextWord();
                if (accnamepart.Length > 1)
                {
                    foreach (var chr in matches)
                    {
                        if (!chr.Account.Name.Contains(accnamepart))
                        {
                            matches.Remove(chr);
                        }
                    }
                }
                else
                {
                    foreach (var chr in matches)
                    {
                        if (!chr.Name.StartsWith(accnamepart))
                        {
                            matches.Remove(chr);
                        }
                    }
                }
            }

            if (matches.Count == World.CharacterCount)
            {
                trigger.Reply("Onliny Players:");
            }

            else if (matches.Count == 0)
            {
                trigger.Reply("No players match the given conditions.");
            }

            else
            {
                trigger.Reply("Matching Players:");
            }
            foreach (var chr in matches)
            {
                trigger.Reply(chr.ToString());
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
    #endregion
}

