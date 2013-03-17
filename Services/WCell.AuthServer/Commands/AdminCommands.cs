using System;
using System.Collections.Generic;
using System.Linq;
using WCell.AuthServer.Database;
using WCell.AuthServer.Database.Entities;
using WCell.AuthServer.Privileges;
using WCell.Core;
using WCell.Util.Commands;
using WCell.AuthServer.Accounts;
using WCell.AuthServer.Firewall;
using WCell.Util;

namespace WCell.AuthServer.Commands
{
	#region Shutdown
	public class ShutdownCommand : AuthServerCommand
	{
		protected ShutdownCommand() { }

		protected override void Initialize()
		{
			Init("Shutdown");
			EnglishParamInfo = "[<delay before shutdown in seconds>]";
			EnglishDescription = "Shuts down the server after the given delay (default = 10s).";
		}

		public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
		{
			var delay = trigger.Text.NextUInt(10);
			AuthenticationServer.Instance.ShutdownIn(delay * 1000);
		}
	}
	#endregion

	#region DB
	public class DBCommand : AuthServerCommand
	{
		protected override void Initialize()
		{
			Init("DB", "Database");
			EnglishParamInfo = "";
			EnglishDescription = "Offers commands to manage/manipulate DB-Settings.";
		}

		public class DropDBCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Drop", "Purge");
				EnglishParamInfo = "";
				EnglishDescription = "WARNING: This drops and re-creates the entire internal WCell Database Schema.";
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				trigger.Reply("Recreating Database Schema...");
				AuthDBMgr.DatabaseProvider.CreateSchema();
				AccountMgr.Instance.ResetCache();
				trigger.Reply("Done.");
			}
		}

		public class DBInfoCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Info", "?");
				EnglishParamInfo = "";
				EnglishDescription = "Shows some info about the DB currently being used.";
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var databaseProvider = AuthDBMgr.DatabaseProvider;
				var session = databaseProvider.Session;

				trigger.Reply("DB Provider: " + databaseProvider.CurrentDialect.GetType().Name);
				trigger.Reply(" State: " + session.Connection.State);
				trigger.Reply(" Database: " + session.Connection.Database);
				trigger.Reply(" Connection String: " + session.Connection.ConnectionString);
			}
		}
	}
	#endregion

	#region Roles
	public class RolesCommand : AuthServerCommand
	{
		protected override void Initialize()
		{
			Init("Roles");
			EnglishDescription = "Offers Commands to interact with Roles.";
		}

		public class ListRolesCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("List", "L");
				EnglishDescription = "Lists all Roles.";
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				DisplayRoles(trigger);
			}
		}

		public static void DisplayRoles(CmdTrigger<AuthServerCmdArgs> trigger)
		{
			trigger.Reply("Available Roles:");
			var roles = PrivilegeMgr.Instance.RoleGroups.Values.ToArray();
			Array.Sort(roles, (role1, role2) => role1.Rank - role2.Rank);
			foreach (var r in roles)
			{
				trigger.Reply(" {0} ({1}", r.Name, r.Rank);
			}
		}
	}
	#endregion

	#region Resync Accounts
	public class ResyncAccountsCommand : AuthServerCommand
	{
		protected override void Initialize()
		{
			Init("Resync", "ResyncAccounts");
			EnglishParamInfo = "[-f]";
			EnglishDescription = "Updates all changes to accounts from the DB. Don't use this if caching is not activated. " +
				"-f switch enforces a complete resync (purge cache and cache everything again) instead of only updating changes.";
		}

		public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
		{
			if (!AccountMgr.Instance.IsCached)
			{
				trigger.Reply("Cannot resync Accounts if caching is not used. Use the Cache-Command to toggle caching.");
			}
			else
			{
				var mod = trigger.Text.NextModifiers();
				trigger.Reply("Resyncing...");
				if (mod == "f")
				{
					AccountMgr.Instance.ResetCache();
				}
				else
				{
					AccountMgr.Instance.Resync();
				}
				trigger.Reply("Done.");
			}
		}
	}
	#endregion

	#region ToggleCached
	public class ToggleCachedCommand : AuthServerCommand
	{
		protected override void Initialize()
		{
			Init("ToggleCached", "SetCached");
			EnglishParamInfo = "[0/1]";
			EnglishDescription = "Toggles caching of Accounts";
		}

		public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
		{
			var cached = (trigger.Text.HasNext && trigger.Text.NextBool()) || !AccountMgr.Instance.IsCached;
			if (cached == AccountMgr.Instance.IsCached)
			{
				trigger.Reply("Caching was already " + (cached ? "On" : "Off"));
			}
			else
			{
				trigger.Reply("{0} caching...", (cached ? "Activating" : "Deactivating"));
				AccountMgr.Instance.IsCached = true;
				trigger.Reply("Done.");
			}
		}
	}
	#endregion

	#region Globals
	/// <summary>
	/// 
	/// </summary>
	public class ConfigCommand : AuthServerCommand
	{
		protected override void Initialize()
		{
			Init("Config", "Cfg");
			EnglishDescription = "Provides commands to manage the Configuration. Use the -a switch to select an Addon's config.";
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

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var cfg = CommandUtil.GetConfig(AuthServerConfiguration.Instance, trigger);
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

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var cfg = CommandUtil.GetConfig(AuthServerConfiguration.Instance, trigger);
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

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var cfg = CommandUtil.GetConfig(AuthServerConfiguration.Instance, trigger);
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

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var cfg = CommandUtil.GetConfig(AuthServerConfiguration.Instance, trigger);
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

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var cfg = CommandUtil.GetConfig(AuthServerConfiguration.Instance, trigger);
				if (cfg != null)
				{
					cfg.Load();
					trigger.Reply("Done.");
				}
			}
		}
	}
	#endregion

	#region Ban
	public class BanCommand : AuthServerCommand
	{
		protected override void Initialize()
		{
			Init("Bans", "Ban");
			EnglishDescription = "Provides commands to manage and overview active IP bans.";
		}

		public class ListBansCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("List", "L");
				EnglishParamInfo = "[<mask>]";
				EnglishDescription = "Lists all BanEntries or only those that match the given mask.";
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var mask = trigger.Text.NextWord();
				ICollection<BanEntry> bans;
				if (mask.Length > 0)
				{
					bans = BanMgr.GetBanList(mask);
				}
				else
				{
					bans = BanMgr.AllBans;
				}

				trigger.Reply("Found {0} {1}{2}", 
					bans.Count, bans.Count == 1 ? "Entry" : "Entries", bans.Count > 0 ? ":" : ".");
				
				var i = 0;
				foreach (var ban in bans)
				{
					i++;
					trigger.Reply("{0}. {1}", i, ban);
				}
			}
		}

		public class AddBanCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Add", "A");
				EnglishParamInfo = "<mask> [-[smhdw] [<seconds>] [<minutes>] [<hours>] [<days>] [<weeks>]] [<reason>]";
				EnglishDescription = "Adds a new Ban on the given mask and optionally a time until the Ban will be lifted and a reason.";
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var mask = trigger.Text.NextWord();
				var bytes = BanMgr.GetBytes(mask);
				if (BanMgr.IsInvalid(bytes))
				{
					trigger.Reply("Invalid Mask: " + mask);
					return;
				}

				var time = trigger.Text.NextTimeSpan();
				var reason = trigger.Text.Remainder;

				var ban = BanMgr.AddBan(time, mask, reason);
				trigger.Reply("Added new Ban: " + ban);
			}
		}

		public class LiftBanCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Lift", "Remove", "Delete", "Del");
				EnglishParamInfo = "<mask>";
				EnglishDescription = "Removes all bans that match the given mask.";
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var mask = trigger.Text.NextWord();
				var bytes = BanMgr.GetBytes(mask);
				if (BanMgr.IsInvalid(bytes))
				{
					trigger.Reply("Invalid Mask: " + mask);
					return;
				}

				var bans = BanMgr.GetBanList(mask);

				if (bans.Count > 0)
				{
					foreach (var ban in bans)
					{
						AuthDBMgr.DatabaseProvider.Delete(ban);
					}
					trigger.Reply("Deleted {0} matching Ban(s): " + bans.ToString(", "), bans.Count);
				}
				else
				{
					trigger.Reply("No BanEntries found matching Mask: " + mask);
				}
			}
		}
	}
	#endregion
}