using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Commands;

namespace WCell.AuthServer.Commands
{
	public class Realmsommand : AuthServerCommand
	{
		protected override void Initialize()
		{
			Init("Realm", "Realms");
		}

		public class RealmListCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("List", "L");
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				if (AuthenticationServer.RealmCount > 0)
				{
					int i = 1;
					foreach (var realm in AuthenticationServer.Realms)
					{
						trigger.Reply("{0}. {1}", i++, realm);
					}
				}
				else
				{
					trigger.Reply("There are no registered Realms.");
				}
			}
		}

		public class RealmDeleteCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Delete", "Del", "D");
				EnglishParamInfo = "<name or number>";
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var realm = GetRealm(trigger);
				if (realm != null)
				{
					realm.Disconnect(true);
					trigger.Reply("Deleted Realm {0}", realm);
				}
			}
		}

		/// <summary>
		/// Returns the realm, specified by the next word or number, if the Realm could be found, else sends a reply.
		/// </summary>
		/// <param name="trigger"></param>
		/// <returns></returns>
		public static RealmEntry GetRealm(CmdTrigger<AuthServerCmdArgs> trigger)
		{
			var arg = trigger.Text.Remainder;
			uint no;
			var count = AuthenticationServer.RealmCount;
			if (count > 0)
			{
				if (uint.TryParse(arg, out no))
				{
					if (count < no)
					{
						trigger.Reply("Invalid Realm Number - Must be between 1 and {0}", count);
					}
					else
					{
						return AuthenticationServer.GetRealmByNumber((int)no);
					}
				}
				else
				{
					return AuthenticationServer.GetRealmByName(arg);
				}
			}
			else
			{
				trigger.Reply("There are no registered Realms.");
			}
			return null;
		}
	}
}