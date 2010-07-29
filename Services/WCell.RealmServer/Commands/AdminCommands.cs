using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cell.Core;
using NLog;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Lang;
using WCell.Util.Threading;
using WCell.Core.Variables;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.RealmServer.Privileges;
using WCell.Util.Commands;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Content;
using WCell.RealmServer.Misc;
using WCell.Util.NLog;

namespace WCell.RealmServer.Commands
{
	#region Shutdown
	public class ShutdownCommand : RealmServerCommand
	{
		protected ShutdownCommand() { }

		public override RoleStatus RequiredStatusDefault
		{
			get
			{
				return RoleStatus.Admin;
			}
		}

		protected override void Initialize()
		{
			Init("Shutdown");
			EnglishParamInfo = "[<delay before shutdown in seconds>]";
			EnglishDescription = "Shuts down the server after the given delay (default = 10s). " +
				"Once started, calling this command again will cancel the shutdown-sequence.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			if (RealmServer.IsPreparingShutdown)
			{
				RealmServer.Instance.CancelShutdown();
			}
			else
			{
				var delay = trigger.Text.NextUInt(10);
				RealmServer.Instance.ShutdownIn(delay * 1000);
			}
		}
	}
	#endregion

	#region Broadcast
	public class BroadcastCommand : RealmServerCommand
	{
		protected BroadcastCommand() { }

		protected override void Initialize()
		{
			Init("Broadcast");
			EnglishParamInfo = "<text>";
			EnglishDescription = "Broadcasts the given text throughout the world.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var prefix = trigger.Args.User != null ? trigger.Args.User.Name + ": " : "";
			World.Broadcast(prefix + trigger.Text.Remainder);
		}
	}
	#endregion

	#region Kick
	public class KickCommand : RealmServerCommand
	{
		protected KickCommand() { }

		protected override void Initialize()
		{
			Init("Kick", "Boot");	// the aliases for this command
			EnglishParamInfo = "[-n <name>][-d <seconds>] [<reason>]";
			EnglishDescription = "Kicks your current target with an optional delay in seconds (default: 20 - can be 0) and an optional reason.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			if (trigger is IngameCmdTrigger && trigger.Args.Target == trigger.Args.Character)
			{
				// make sure you don't kick yourself by accident
				trigger.Reply("You cannot kick yourself.");
			}
			else
			{
				var chr = trigger.Args.Target as Character;

				var mod = trigger.Text.NextModifiers();
				if (chr == null)
				{
					if (!mod.Contains("n") || !trigger.Text.HasNext)
					{
						trigger.Reply(RealmLangKey.CmdKickMustProvideName);
						return;
					}
					else
					{
						var name = trigger.Text.NextWord();
						chr = World.GetCharacter(name, false);
						if (chr == null)
						{
							trigger.Reply(RealmLangKey.PlayerNotOnline, name);
							return;
						}
					}
				}

				var delay = Character.DefaultLogoutDelay;

				// check for different delay
				if (mod.Contains("d"))
				{
					delay = trigger.Text.NextFloat(delay);
				}

				// optional reason
				var reason = trigger.Text.Remainder.Trim();

				// kick: 
				// Sits the char down and renders him/her unable to do anything for the given delay, after which he/she gets disconnected
				chr.Kick(trigger.Args.User, reason, delay);
			}
		}
	}
	#endregion

	#region Ban
	public class BanCommand : RealmServerCommand
	{
		protected BanCommand() { }

		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Admin; }
		}

		protected override void Initialize()
		{
			Init("Ban");
			EnglishParamInfo = "[-[smhdw] [<seconds>] [<minutes>] [<hours>] [<days>] [<weeks>]]";
			EnglishDescription = "Deactivates the given Account. Reactivation time can optionally also be specified.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var chr = trigger.Args.Target as Character;
			var banner = trigger.Args.User;

			if (chr != null && chr == banner)
			{
				chr = chr.Target as Character;
			}
			if (chr == null || chr == banner)
			{
				trigger.Reply("Invalid Target.");
				return;
			}

			if (banner != null && chr.Role >= banner.Role)
			{
				trigger.Reply("Cannot ban Users of higher or equal Rank.");
				return;
			}

			var time = trigger.Text.NextTimeSpan();

			DateTime? until;
			if (time != null)
			{
				until = DateTime.Now + time;
			}
			else
			{
				until = null;
			}

			var timeStr = until != null ? "until " + until : "(indefinitely)";
			trigger.Reply("Banning Account {0} ({1}) {2}...", chr.Account.Name, chr.Name,
				timeStr);

			RealmServer.Instance.AddMessage(new Message(() =>
			{
				var context = chr.ContextHandler;
				var acc = chr.Account;
				if (acc == null || context == null)
				{
					trigger.Reply("Character logged off.");
					return;
				}

				if (acc.SetAccountActive(false, until))
				{
					context.AddMessage(() =>
					{
						if (chr.IsInWorld)
						{
							chr.Kick(banner, "Banned " + timeStr, 5);
						}
						trigger.Reply("Done.");
					});
				}
				else
				{
					trigger.Reply("Could not ban Account.");
				}
			}));
		}
	}
	#endregion

	#region SetRole
	public class SetRoleCommand : RealmServerCommand
	{
		protected SetRoleCommand() { }

		public override RoleStatus RequiredStatusDefault
		{
			get
			{
				return RoleStatus.Admin;
			}
		}

		protected override void Initialize()
		{
			Init("SetRole", "Role", "SetPriv");
			EnglishParamInfo = "<RoleName>";
			EnglishDescription = "Sets the Account's Role which determines the User's rights and privileges.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var user = trigger.Args.User;
			var roleName = trigger.Text.NextWord();
			var role = PrivilegeMgr.Instance.GetRole(roleName);
			if (role == null)
			{
				trigger.Reply("Role \"{0}\" does not exist.", roleName);
			}
			else if (user != null && user.Role <= role && user.Role < RoleGroupInfo.HighestRole.Rank)
			{
				trigger.Reply("You are not allowed to set the \"{0}\"-Role.", role.Name);
				return;
			}
			else
			{
				var chr = ((Character)trigger.Args.Target);
				var oldRole = chr.Account.Role;

				// Since setting the role is a task sent to the Auth-Server, this is a blocking call
				// and thus must not be executed within the Region context (which is the default context for Commands)
				RealmServer.Instance.AddMessage(new Message(() =>
				{
					if (chr.Account.SetRole(role))
					{
						chr.SendSystemMessage("You Role has changed from {0} to {1}.", oldRole, role.Name);
						trigger.Reply("{0}'s Account's ({1}) Role has changed from {2} to: {3}",
							chr.Name,
							chr.Account,
							oldRole,
							role);
					}
					else
					{
						trigger.Reply("Role information could not be saved.");
					}
				}));
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get
			{
				return ObjectTypeCustom.Player;
			}
		}
	}
	#endregion

	#region Global
	public class GlobalCommand : RealmServerCommand
	{
		public override RoleStatus RequiredStatusDefault
		{
			get
			{
				return RoleStatus.Admin;
			}
		}

		protected override void Initialize()
		{
			Init("Global");
			EnglishParamInfo = "[-pi] <command + command args>";
			EnglishDescription = "Executes the given command on everyone ingame. Use carefully! " +
				"-p Only on Players (exclude staff members). " +
				"-i Include self";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var mod = trigger.Text.NextModifiers();
			var cmd = RealmCommandHandler.Instance.GetCommand(trigger);
			if (cmd == null || !cmd.Enabled ||
				(trigger.Args.User != null && !trigger.Args.User.Role.MayUse(cmd.RootCmd)))
			{
				trigger.Reply("Invalid Command.");
				return;
			}

			var chrTrigger = trigger.Silent(new RealmServerCmdArgs(trigger.Args));
			var playersOnly = mod.Contains("p");
			var inclSelf = mod.Contains("i");

			var chrCount = 0;
			World.CallOnAllChars(chr =>
			{
				if (chr.Role <= trigger.Args.Role)
				{
					if ((!playersOnly || chr.Role.Status == RoleStatus.Player) &&
						(inclSelf || chr != trigger.Args.User))
					{
						chrTrigger.Args.Target = chr;
						RealmCommandHandler.Instance.Execute(chrTrigger, cmd, true);
						chrCount++;
					}
				}
			},
			() => trigger.Reply("Done. - Called Command on {0} Characters.", chrCount));

		}
	}
	#endregion

	#region Cache
	public class CacheCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Cache");
			EnglishDescription = "Provides commands to manage the server-side cache of static data.";
		}

		public class PurgeCacheCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Purge");
				EnglishDescription = "Removes all cache-files.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				ContentHandler.PurgeCache();
				trigger.Reply("Done.");
			}
		}
	}
	#endregion

	#region Variables
	/// <summary>
	/// 
	/// </summary>
	public class ConfigCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Config", "Cfg");
			EnglishDescription = "Provides commands to manage the Configuration.";
		}

		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Admin; }
		}

		public class SetGlobalCommand : SubCommand
		{
			protected SetGlobalCommand() { }

			protected override void Initialize()
			{
				Init("Set", "S");
				EnglishParamInfo = "<globalVar> <value>";
				EnglishDescription = "Sets the value of the given global variable.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var cfg = CommandUtil.GetConfig(RealmServerConfiguration.Instance, trigger);
				if (cfg != null)
				{
					CommandUtil.SetCfgValue(cfg, trigger);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public class GetGlobalCommand : SubCommand
		{
			protected GetGlobalCommand() { }

			protected override void Initialize()
			{
				Init("Get", "G");
				EnglishParamInfo = "<globalVar>";
				EnglishDescription = "Gets the value of the given global variable.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var cfg = CommandUtil.GetConfig(RealmServerConfiguration.Instance, trigger);
				if (cfg != null)
				{
					CommandUtil.GetCfgValue(cfg, trigger);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public class ListGlobalsCommand : SubCommand
		{
			protected ListGlobalsCommand() { }

			protected override void Initialize()
			{
				Init("List", "L");
				EnglishParamInfo = "[<name Part>]";
				EnglishDescription = "Lists all global variables. If specified only shows variables that contain the given name Part.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var cfg = CommandUtil.GetConfig(RealmServerConfiguration.Instance, trigger);
				if (cfg != null)
				{
					CommandUtil.ListCfgValues(cfg, trigger);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public class SaveConfigCommand : SubCommand
		{
			protected SaveConfigCommand() { }

			protected override void Initialize()
			{
				Init("Save");
				EnglishParamInfo = "";
				EnglishDescription = "Saves the current configuration.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var cfg = CommandUtil.GetConfig(RealmServerConfiguration.Instance, trigger);
				if (cfg != null)
				{
					cfg.Save(true, false);
					trigger.Reply("Done.");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public class LoadConfigCommand : SubCommand
		{
			protected LoadConfigCommand() { }

			protected override void Initialize()
			{
				Init("Load");
				EnglishParamInfo = "";
				EnglishDescription = "Loads the configuration again.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var cfg = CommandUtil.GetConfig(RealmServerConfiguration.Instance, trigger);
				if (cfg != null)
				{
					cfg.Load();
					trigger.Reply("Done.");
				}
			}
		}
	}
	#endregion

	#region AuthServer
	public class AuthRemoteCommand : RealmServerCommand
	{
		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Admin; }
		}

		protected override void Initialize()
		{
			Init("AuthRemote", "Auth");
			EnglishParamInfo = "<Command <args>>";
			EnglishDescription = "Executes a command on the AuthServer.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			if (RealmServer.Instance.AuthClient.Channel == null || !RealmServer.Instance.AuthClient.IsConnected)
			{
				trigger.Reply("Connection to AuthServer is currently not established.");
				return;
			}

			var response = RealmServer.Instance.AuthClient.Channel.ExecuteCommand(trigger.Text.Remainder);
			if (response != null)
			{
				if (response.Replies.Count > 0)
				{
					foreach (var reply in response.Replies)
					{
						trigger.Reply(reply);
					}
				}
				else
				{
					trigger.Reply("Done.");
				}
			}
			else
			{
				trigger.Reply("Failed to execute remote-command.");
			}
		}
	}
	#endregion

	#region Exceptions
	public class ExceptionCommand : RealmServerCommand
	{
		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Admin; }
		}

		protected override void Initialize()
		{
			Init("Exception", "Excep");
		}

		public class ListExceptionCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("List", "L");
				EnglishParamInfo = "[<match>]";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var match = trigger.Text.Remainder;

				var count = 0;
				for (var i = 0; i < ExceptionHandler.Exceptions.Count; i++)
				{
					var ex = ExceptionHandler.Exceptions[i];
					if (match.Length == 0 || ex.ToString().IndexOf(match, StringComparison.InvariantCultureIgnoreCase) > -1)
					{
						++count;
					}
				}

				if (count == 0)
				{
					trigger.Reply("No Exceptions have been triggered so far.");
				}
				else
				{
					trigger.Reply("Found {0} Exceptions:", count);
					for (var i = 0; i < ExceptionHandler.Exceptions.Count; i++)
					{
						var ex = ExceptionHandler.Exceptions[i];
						if (match.Length == 0 || ex.ToString().IndexOf(match, StringComparison.InvariantCultureIgnoreCase) > -1)
						{
							trigger.Reply("{0}. {1}", i+1, ex);
						}
					}
				}
			}
		}

		public class ShowExceptionCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Show", "S");
				EnglishParamInfo = "[<index>]";
				EnglishDescription = "If no index is given, will show the last Exception.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var index = trigger.Text.NextUInt(uint.MaxValue);

				ExceptionInfo excep;
				if (index >= ExceptionHandler.Exceptions.Count)
				{
					excep = ExceptionHandler.Exceptions.LastOrDefault();
				}
				else
				{
					excep = ExceptionHandler.Exceptions[(int)index];
				}

				if (excep != null)
				{
					trigger.Reply(excep + (excep.Exception.StackTrace == null ? " (No StackTrace available)" : ""));
					if (excep.Exception.StackTrace != null)
					{
						var lines = excep.Exception.StackTrace.Split('\n');
						foreach (var line in lines)
						{
							trigger.Reply(" " + line.Trim());
						}
					}
				}
				else
				{
					trigger.Reply("Invalid index specified.");
				}
			}
		}

		//public class ThrowExceptionCommand : SubCommand
		//{
		//    protected override void Initialize()
		//    {
		//        Init("Throw");
		//    }

		//    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		//    {
		//        try
		//        {
		//            throw new Exception("TEST EXCEPTION");
		//        }
		//        catch (Exception e)
		//        {
		//            LogUtil.ErrorException(e);
		//        }
		//    }
		//}
	}
	#endregion

	#region IPC
	public class RealmIPCCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("IPC");
			EnglishParamInfo = "[0/1]";
			//Description = "Provides commands to manage the IPC-device that connects Realm- and Auth-Server. Use -0 to turn it off and -1 to turn it on.";
			EnglishDescription = "Toggles the IPC-device that connects Realm- and Auth-Server.";
		}

		public override RoleStatus RequiredStatusDefault
		{
			get
			{
				return RoleStatus.Admin;
			}
		}

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

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			//var mods = trigger.Text.NextModifiers();
			bool run;
			if (trigger.Text.HasNext)
			{
				run = trigger.Text.NextBool();
			}
			else
			{
				// toggle
				run = !RealmServer.Instance.AuthClient.IsRunning;
			}

			RealmServer.Instance.AuthClient.IsRunning = run;

			//trigger.Reply("Done. - IPC-Client is now {0}.", run ? "Online" : "Offline");
			trigger.Reply("Done.");
			//base.Process(trigger);
		}
	}
	#endregion

	#region Log
	public class LogCommand : RealmServerCommand
	{
		protected LogCommand() { }

		protected override void Initialize()
		{
			base.Init("Log");
			EnglishDescription = "Gets and sets logging settings.";
		}

		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Admin; }
		}

		public override bool RequiresCharacter
		{
			get { return false; }
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.None; }
		}

		#region Logging Level
		public class ToggleLevelCommand : SubCommand
		{
			protected ToggleLevelCommand() { }

			protected override void Initialize()
			{
				base.Init("ToggleLevel", "Level", "Lvl", "TL");
				EnglishParamInfo = "<Trace|Debug|Info|Warn|Error|Fatal> [<1/0>]";
				EnglishDescription = "Globally toggles whether messages of the corresponding level should be logged (to console, as well as to file or any other target that is specified).";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var level = LogLevel.FromString(trigger.Text.NextWord());
				var isSet = trigger.Text.HasNext && trigger.Text.NextBool();

				foreach (var rule in LogManager.Configuration.LoggingRules)
				{
					bool enable = (isSet) || (!rule.IsLoggingEnabledForLevel(level));
					if (enable)
					{
						rule.EnableLoggingForLevel(level);
					}
					else
					{
						rule.DisableLoggingForLevel(level);
					}
					trigger.Reply("{0}-Messages for \"{1}\" {2}.", level, rule.LoggerNamePattern, enable ? "enabled" : "disabled");
				}
			}
		}
		#endregion
	}
	#endregion

	#region Dump
	public class DumpThreadPoolInfoCommand : RealmServerCommand
	{
		protected DumpThreadPoolInfoCommand() { }

		protected override void Initialize()
		{
			base.Init("DumpTPInfo");
			EnglishDescription = "Dumps information about the thread pool.";
		}

		public override RoleStatus RequiredStatusDefault
		{
			get
			{
				return RoleStatus.Admin;
			}
		}

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

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			int minTpThreads, maxTpThreads, availTpThreads, minIOCPThreads, maxIOCPThreads, availIOCPThreads;
			ThreadPool.GetMinThreads(out minTpThreads, out minIOCPThreads);
			ThreadPool.GetMaxThreads(out maxTpThreads, out maxIOCPThreads);
			ThreadPool.GetAvailableThreads(out availTpThreads, out availIOCPThreads);

			trigger.Reply("[Thread Pool] {0} available worker threads out of {1} ({2} minimum)",
							availTpThreads.ToString(), maxTpThreads.ToString(), minTpThreads.ToString());
			trigger.Reply("[Thread Pool] {0} available IOCP threads out of {1} ({2} minimum)",
							availIOCPThreads.ToString(), maxIOCPThreads.ToString(), minIOCPThreads.ToString());
		}
	}

	public class DumpNetworkInfoCommand : RealmServerCommand
	{
		protected DumpNetworkInfoCommand() { }

		protected override void Initialize()
		{
			Init("DumpNetworkInfo");
			EnglishDescription = "Dumps network information including data sent and received, buffer pools, etc.";
		}

		public override RoleStatus RequiredStatusDefault
		{
			get
			{
				return RoleStatus.Admin;
			}
		}

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

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			long totalDataSent, totalDataReceived, totalAllocMemory;
			int bufferPoolSize, bufferPoolAvail;

			totalDataSent = RealmClient.TotalBytesSent;
			totalDataReceived = RealmClient.TotalBytesReceived;

			bufferPoolSize = BufferManager.Default.TotalSegmentCount;
			bufferPoolAvail = BufferManager.Default.AvailableSegmentsCount;
			totalAllocMemory = BufferManager.GlobalAllocatedMemory;

			trigger.Reply("[Network] Total data sent: {0}, Total data received: {1}",
							WCellUtil.FormatBytes(totalDataSent),
							WCellUtil.FormatBytes(totalDataReceived));
			trigger.Reply("[Buffers] {0} available packet buffers out of {1}",
							bufferPoolAvail.ToString(), bufferPoolSize.ToString());
			trigger.Reply("[Buffers] {0} allocated globally", WCellUtil.FormatBytes(totalAllocMemory));
		}
	}
	#endregion
}