using System;
using IRCAddon;
using WCell.RealmServer;
using WCell.RealmServer.Commands;
using WCell.Intercommunication.DataTypes;
using Squishy.Irc;
using WCell.Util.Commands;
using IRCAddon.Commands;
using WCellStr = WCell.Util.Strings.StringStream;

namespace WCellAddon.IRCAddon
{
	public static class WCellUtil
	{
		/// <summary>
		/// Maps AuthNames of the IRC-network to ingame Account names. 
		/// Should be saved to/loaded from a file. 
		/// Should be maintainable of People with high enough priv levels (check Role.IsAdmin)
		/// </summary>
		/// <summary>
		/// Adds an AuthResolved handler
		/// </summary>
		/// <param name="irc"></param>
		public static void Init(IrcClient irc)
		{
			irc.AuthMgr.AuthResolved += usr =>
			{
				if (!RealmServer.Instance.AuthClient.IsConnected)
				{
					usr.Msg("AuthServer is offline, authentication disabled.");
					return;
				}
				var acc = GetAccount(usr.AuthName);
				if (acc != null)
				{
					var wcellUser = new WCellUser(acc, usr);
					var args = new WCellArgs(acc);
					usr.Args = args;

					if (args.Account.Role >= RoleStatus.Staff)
					{
						args.CmdArgs = new RealmServerCmdArgs(wcellUser, false, null);
					}
				}
				else
				{
					// User cannot use commands because he does not have a verified Account
					// maybe send him a link to register online
					usr.Msg("You do not have sufficient rights");
				}
			};
		}

		/// <summary>
		/// Returns the Account of the User with the given authName.
		/// </summary>
		/// <param name="authName"></param>
		/// <returns></returns>
		public static RealmAccount GetAccount(string authName)
		{
			var accName = GetAccName(authName);
			return accName != null
					? RealmServer.Instance.GetOrRequestAccount(accName)
					: null;
		}

		public static string GetAccName(string authName)
		{
			string accName;
			AccountAssociationsList.AccAssDict.TryGetValue(authName, out accName);
			return accName;
		}

		public static bool HandleCommand(WCellUser wcellUser, IrcUser user, IrcChannel chan, string text)
		{
			var uArgs = user.Args as WCellArgs;

			if (uArgs != null)
			{
				if (uArgs.Account.Role >= RoleStatus.Staff)
				{
					var cmdArgs = uArgs.CmdArgs;
					var trigger = new WCellCmdTrigger(wcellUser, chan, new WCellStr(text), cmdArgs);

					bool isDouble;
					RealmCommandHandler.ConsumeCommandPrefix(trigger.Text, out isDouble);
					trigger.Args.Double = isDouble;

					// init the trigger and check if the given args are valid
					if (trigger.InitTrigger())
					{
						RealmCommandHandler.Instance.ExecuteInContext(trigger, OnExecuted, OnFail);
					}
					return true;
				}
			}
			return false;
		}

		public static bool CommandExists(string alias)
		{
			return RealmCommandHandler.Instance.Get(alias) != null;	
		}

		private static void OnExecuted(CmdTrigger<RealmServerCmdArgs> obj)
		{
		}

		private static void OnFail(CmdTrigger<RealmServerCmdArgs> obj)
		{
		}
	}
}
