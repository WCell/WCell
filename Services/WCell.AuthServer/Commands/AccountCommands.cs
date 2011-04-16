using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Commands;
using WCell.AuthServer.Database;
using WCell.AuthServer.Accounts;
using WCell.Constants;
using WCell.Core.Cryptography;
using WCell.AuthServer.Privileges;
using WCell.Util;

namespace WCell.AuthServer.Commands
{

	public class AccountCommand : AuthServerCommand
	{
		protected override void Initialize()
		{
			Init("Account", "Acc");
			EnglishParamInfo = "<name>|-i <id>";
			EnglishDescription = "Provides Commands to manage/manipulate Accounts. -i ";
		}

		public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
		{
			var subAlias = trigger.Text.NextWord();

			SubCommand subCmd;
			if (m_subCommands.TryGetValue(subAlias, out subCmd))
			{
				if (((AccountSubCommand)subCmd).RequiresAccount)
				{
					var acc = trigger.Args.Account;
					if (acc == null)
					{
						if (!trigger.Text.HasNext)
						{
							trigger.Reply("You did not specify an Account.");
							trigger.Reply(subCmd.CreateUsage(trigger));
							return;
						}

						var mods = trigger.Text.NextModifiers();

						if (mods == "i")
						{
							var id = trigger.Text.NextInt();
							acc = AccountMgr.GetAccount(id);

							if (acc == null)
							{
								trigger.Reply("There is no Account with Id: " + id);
								return;
							}
						}
						else
						{
							var name = trigger.Text.NextWord();
							acc = AccountMgr.GetAccount(name);
							if (acc == null)
							{
								trigger.Reply("There is no Account with Name: " + name);
								return;
							}
						}

						trigger.Args.Account = acc;
					}
				}
				subCmd.Process(trigger);
			}
			else
			{
				trigger.Reply("SubCommand not found: " + subAlias);
				trigger.Text.Skip(trigger.Text.Length);
				mgr.DisplayCmd(trigger, this, false, false);
			}
		}

		public abstract class AccountSubCommand : SubCommand
		{
			public virtual bool RequiresAccount { get { return true; } }

			public override string CreateUsage(string paramInfo)
			{
				if (RequiresAccount)
				{
					paramInfo = Aliases.ToString("|") + " (<AccountName>|-i <id>) " + " " + paramInfo;
					paramInfo = m_parentCmd.CreateUsage(paramInfo);
					return paramInfo;
				}
				else
				{
					return base.CreateUsage(paramInfo);
				}
			}
		}

		public class AddAccountCommand : AccountSubCommand
		{
			protected override void Initialize()
			{
				Init("Add", "A", "Create");
				EnglishParamInfo = "<AccountName> <PW> [<Role> [<email> [<clientId>]]]";
				EnglishDescription = "Adds a new account with the given Name and PW and optionally specified Role and ClientId";
			}

			public override bool RequiresAccount
			{
				get { return false; }
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var name = trigger.Text.NextWord();

				if (AccountMgr.DoesAccountExist(name))
				{
					trigger.Reply("The account \"{0}\" already exists!", name);
				}
				else
				{
					if (!AccountMgr.NameValidator(ref name))
					{
						trigger.Reply("Invalid Account-Name: " + name);
						return;
					}

					var pw = trigger.Text.NextWord();

					if (pw.Length == 0)
					{
						trigger.Reply("Password required.");
					}
					else
					{
						var role = trigger.Text.NextWord();
						var email = trigger.Text.NextWord();
						ClientId clientId;
						if (trigger.Text.HasNext)
						{
							clientId = trigger.Text.NextEnum((ClientId)int.MaxValue);
							if (clientId == (ClientId)int.MaxValue)
							{
								trigger.Reply("Invalid ClientId specified - Choose either of: " +
									Enum.GetValues(typeof(ClientId)).OfType<object>().ToString(", "));
							}
						}
						else
						{
							clientId = ClientId.Wotlk;
						}

						role = PrivilegeMgr.Instance.GetRoleOrDefault(role);

						var acc = AccountMgr.Instance.CreateAccount(name, pw, email, role, clientId);

						if (acc != null)
						{
							trigger.Reply("Account \"{0}\" created (Role: {1}, Email: {2}, ClientId: {3})",
								name,
								acc.RoleGroupName,
								acc.EmailAddress,
								clientId);
						}
						else
						{
							// cannot really happen
							trigger.Reply("Failed to create Account \"{0}\"!", name);
						}
					}
				}
			}
		}

		public class DeleteAccountCommand : AccountSubCommand
		{
			protected override void Initialize()
			{
				Init("Delete", "Del");
				EnglishParamInfo = "<ConfirmName>";
				EnglishDescription = "Deletes the account with the given name. Type the name twice for confirmation.";
			}

			public override bool RequiresAccount
			{
				get { return true; }
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var acc = trigger.Args.Account;
				var name = acc.Name.ToUpper();
				var name2 = trigger.Text.NextWord().ToUpper();

				if (name != name2)
				{
					trigger.Reply("The names do not match.");
				}
				else
				{
					acc.DeleteAndFlush();
					trigger.Reply("The account \"{0}\" has been deleted.", name);
				}
			}
		}

		public class AccountInfoCommand : AccountSubCommand
		{
			protected override void Initialize()
			{
				Init("Info", "I");
				EnglishParamInfo = "";
				EnglishDescription = "Shows information about the given Account.";
			}

			public override bool RequiresAccount
			{
				get { return true; }
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var acc = trigger.Args.Account;

				trigger.Reply(acc.Details);
			}
		}

		public class SetPropCommand : AccountSubCommand
		{
			protected override void Initialize()
			{
				Init("SetProp", "Set", "S");
				EnglishParamInfo = "<Property> <Value>";
				EnglishDescription = "Sets the given Account property to the given value.";
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var prop = trigger.Text.NextWord();

				var subCmd = SelectSubCommand(prop);
				if (subCmd == null)
				{
					trigger.Reply("Invalid Account-property: " + prop);
					mgr.DisplayCmd(trigger, this, false, false);
				}
				else
				{
					subCmd.Process(trigger);
				}
			}

			public class EmailCommand : SubCommand
			{
				protected override void Initialize()
				{
					Init("Email", "Mail");
					EnglishParamInfo = "<new@mail.address>";
					EnglishDescription = "Changes the Account's EMail address.";
				}

				public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
				{
					var mail = trigger.Text.NextWord();
					if (Utility.IsValidEMailAddress(mail))
					{
						trigger.Args.Account.EmailAddress = mail;
						trigger.Args.Account.SaveAndFlush();
						trigger.Reply("Changed eMail of Account {0} to: {1}", trigger.Args.Account, mail);
					}
					else
					{
						trigger.Reply("Invalid EMail-Address: {0}", mail);
					}
				}
			}

			public class PassCommand : SubCommand
			{
				protected override void Initialize()
				{
					Init("Pass", "PW", "Password");
					EnglishParamInfo = "<pass>";
					EnglishDescription = "Changes the Account's password.";
				}

				public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
				{
					var pass = trigger.Text.NextWord();
					if (pass.Length < SecureRemotePassword.MinPassLength)
					{
						trigger.Reply("Account password must at least be {0} characters long.", SecureRemotePassword.MinPassLength);
					}
					else if (pass.Length > SecureRemotePassword.MaxPassLength)
					{
						trigger.Reply("Account password length must not exceed {0} characters.", SecureRemotePassword.MaxPassLength);
					}
					else
					{
						trigger.Args.Account.Password = SecureRemotePassword.GenerateCredentialsHash(trigger.Args.Account.Name, pass);
						trigger.Args.Account.SaveAndFlush();
						trigger.Reply("Password has been changed.");
					}
				}
			}

			public class RoleCommand : SubCommand
			{
				protected override void Initialize()
				{
					Init("Role", "R");
					EnglishParamInfo = "<RoleName>";
					EnglishDescription = "Changes the Account's Role.";
				}

				public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
				{
					var roleName = trigger.Text.NextWord();
					var role = PrivilegeMgr.Instance.GetRoleGroup(roleName);
					if (roleName.Length == 0 || role == null)
					{
						trigger.Reply("Invalid Role specified - Does not exist: " + roleName);
						RolesCommand.DisplayRoles(trigger);
					}
					else
					{
						trigger.Args.Account.RoleGroupName = role.Name;
						trigger.Args.Account.SaveAndFlush();
						trigger.Reply("Role of Account \"{0}\" changed to: " + role.Name, trigger.Args.Account);
					}
				}
			}

			public class SetClientIdCommand : SubCommand
			{
				protected override void Initialize()
				{
					Init("ClientId", "C");
					EnglishParamInfo = "<ClientId>";
					EnglishDescription = "Changes the Account's ClientId.";
				}

				public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
				{
					var clientId = trigger.Text.NextEnum((ClientId)int.MaxValue);
					if (!Enum.IsDefined(typeof(ClientId), clientId))
					{
						trigger.Reply("Invalid ClientId specified - Choose either of: " + Enum.GetValues(typeof(ClientId)).OfType<object>().ToString(", "));
					}
					else
					{
						trigger.Args.Account.ClientId = clientId;
						trigger.Args.Account.SaveAndFlush();
						trigger.Reply("ClientId of Account \"{0}\" changed to: " + clientId, trigger.Args.Account);
					}
				}
			}

			public override bool RequiresAccount
			{
				get
				{
					return true;
				}
			}
		}

		public class AccountListCommand : AccountSubCommand
		{
			public static int MaxListCount = 100;

			public static int MaxListDetailsCount = 6;

			protected override void Initialize()
			{
				Init("List", "L");
				EnglishParamInfo = "<name-part>";
				EnglishDescription = "Lists all accounts whose name contains the given string. (Max: " + MaxListCount + ")";
			}

			public override bool RequiresAccount
			{
				get { return false; }
			}

			public override void Process(CmdTrigger<AuthServerCmdArgs> trigger)
			{
				var match = trigger.Text.NextWord();

				AccountMgr.Instance.Lock.EnterReadLock();

				var accs = new List<Account>();
				var more = false;

				try
				{
					foreach (var acc in AccountMgr.Instance.AccountsById.Values)
					{
						if (match.Length == 0 ||
							acc.Name.IndexOf(match, StringComparison.InvariantCultureIgnoreCase) > -1)
						{
							if (accs.Count >= MaxListCount)
							{
								more = true;
								break;
							}
							accs.Add(acc);
						}
					}
				}
				finally
				{
					AccountMgr.Instance.Lock.ExitReadLock();
				}

				trigger.Reply("Found {0} matching accounts:", accs.Count);
				for (var i = 0; i < accs.Count; i++)
				{
					var acc = accs[i];
					if (accs.Count <= MaxListDetailsCount)
					{
						trigger.Reply(acc.Details);
					}
					else
					{
						trigger.Reply(acc.ToString());
					}
				}

				if (more)
				{
					trigger.Reply("(more ...)");
				}
			}
		}
	}
}