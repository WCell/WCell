using System;
using NLog;
using WCell.Constants;
using WCell.Constants.Updates;
using WCell.Core.Cryptography;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Graphics;
using WCell.Util.Threading;
using WCell.Util.Variables;

// Commands used on Character-objects

namespace WCell.RealmServer.Commands
{
	#region Save
	public class SaveCommand : RealmServerCommand
	{
		protected SaveCommand() { }

		protected override void Initialize()
		{
			Init("Save");
			EnglishParamInfo = "";
			EnglishDescription = "Updates all changes on the given Character";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			trigger.Reply("Saving...");
			var chr = (Character)trigger.Args.Target;

			RealmServer.IOQueue.AddMessage(new Message(() =>
			{
				if (chr == null)
				{
					return;
				}

				if (chr.SaveNow())
				{
					trigger.Reply("Done.");
				}
				else
				{
					trigger.Reply("Could not save \"" + chr + "\" to DB.");
				}
			}));
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Player; }
		}
	}
	#endregion

	#region Email
	public class EmailCommand : RealmServerCommand
	{
		protected EmailCommand() { }

		protected override void Initialize()
		{
			Init("Email", "SetEmail");
			EnglishParamInfo = "<email>";
			EnglishDescription = "Sets the Account's current email address.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var email = trigger.Text.NextWord();

			if (!Utility.IsValidEMailAddress(email))
			{
				trigger.Reply("Invalid Mail address.");
			}
			else
			{
				trigger.Reply("Setting mail address to " + email + "...");

				RealmServer.IOQueue.AddMessage(new Message(() =>
				{
					var chr = ((Character)trigger.Args.Target);

					if (chr.Account.SetEmail(email))
					{
						trigger.Reply("Done.");
					}
					else
					{
						trigger.Reply("Could not change email-address.");
					}
				}));
			}
		}
	}
	#endregion

	#region Password
	/// <summary>
	/// TODO: Figure out how to verify password
	/// TODO: PW should be queried after the command has been executed
	/// </summary>
	public class PasswordCommand : RealmServerCommand
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		protected PasswordCommand() { }

		protected override void Initialize()
		{
			Init("Password", "Pass");
			EnglishParamInfo = "<oldpw> <newpw> <newpw>";
			EnglishDescription = "Changes your password.";
		}

		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Player; }
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var self = trigger.Args.Character == trigger.Args.Target;
			if (!self && trigger.Args.Character != null &&
				trigger.Args.Character.Role < RoleStatus.Admin)
			{
				trigger.Reply("Only Admins or Account-owners are allowed to change Account passwords.");
				return;
			}

			string oldPass;
			if (self)
			{
				oldPass = trigger.Text.NextWord();
			}
			else
			{
				oldPass = null;
			}

			var pass = trigger.Text.NextWord();
			var newPassConfirm = trigger.Text.NextWord();

			if (pass.Length < SecureRemotePassword.MinPassLength)
			{
				trigger.Reply("Account password must at least be {0} characters long.", SecureRemotePassword.MinPassLength);
			}
			else if (pass.Length > SecureRemotePassword.MaxPassLength)
			{
				trigger.Reply("Account password length must not exceed {0} characters.", SecureRemotePassword.MaxPassLength);
			}
			else if (pass != newPassConfirm)
			{
				trigger.Reply("Passwords don't match.");
			}
			else
			{
				trigger.Reply("Setting password...");
				//if (trigger.Args.Target != trigger.Args.Character && trigger.Args.Character != null)
				//{
				//    log.Info("{0} is changing {1}'s Password.", trigger.Args.Character, trigger.Args.Target);
				//}

				RealmServer.IOQueue.AddMessage(new Message(() =>
				{
					var chr = ((Character)trigger.Args.Target);

					if (chr.Account.SetPass(oldPass, pass))
					{
						trigger.Reply("Done.");
					}
					else
					{
						trigger.Reply("Unable to set Password. Make sure your old password is correct.");
					}
				}));
			}
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.Player; }
		}
	}
	#endregion

	#region Account
	//public class AccountCommand : RealmServerCommand
	//{
	//    protected override void Initialize()
	//    {
	//        Init("Account", "Acc");
	//        ParamInfo = "";
	//        Description = "Provides several commands to retrieve and change Account information.";
	//    }

	//    public override ObjectTypeCustom TargetTypes
	//    {
	//        get
	//        {
	//            return ObjectTypeCustom.Player;
	//        }
	//    }
	//}
	#endregion

	#region Where
	public class WhereCommand : RealmServerCommand
	{
		protected WhereCommand() { }

		protected override void Initialize()
		{
			Init("Where", "Wh");
			EnglishDescription = "Tells you information about your location";
		}

		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Player; }
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			WorldObject obj = trigger.Args.Target;

			var zone = obj.Zone;
			string zoneStr;
			if (zone != null)
			{
				zoneStr = zone.Id.ToString();

				while ((zone = zone.ParentZone) != null)
				{
					zoneStr += " in " + zone;
				}
			}
			else
			{
				zoneStr = "<null>";
			}

			var height = obj.TerrainHeight;
			trigger.Reply("Map: " + obj.Map + ", Zone: " + zoneStr);
			trigger.Reply("Position X: " + obj.Position.X + ", Y: " + obj.Position.Y + ", Z: " + obj.Position.Z + ", O: " + obj.Orientation +
				"(" + Math.Abs(obj.Position.Z - height) + " yds from Ground)");

			int tileX, tileY;
			PositionUtil.GetTileXYForPos(obj.Position, out tileX, out tileY);
			trigger.ReplyFormat("Tile: X={0}, Y={1} (Collision Data {2})", tileX, tileY, 
				obj.Map.Terrain.IsAvailable(tileX, tileY) ? 
					ChatUtility.Colorize("Available", Color.Green) :
					ChatUtility.Colorize("Unavailable", Color.Red));
		}

		public override ObjectTypeCustom TargetTypes
		{
			get { return ObjectTypeCustom.All; }
		}
	}
	#endregion

	#region Info
	public class InfoCommand : RealmServerCommand
	{
		[Variable("MaxPlayerListCount")]
		public static int MaxListCount = 100;

		protected InfoCommand() { }

		public override RoleStatus RequiredStatusDefault
		{
			get { return RoleStatus.Player; }
		}

		protected override void Initialize()
		{
			Init("Info", "I", "Address", "Addr");
			EnglishParamInfo = "[-l]";
			EnglishDescription = "Gives some server info. -l lists all players (if not too many).";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			trigger.Reply(RealmServer.Title + " located at: " + RealmServerConfiguration.ExternalAddress + " (Required Client: " + WCellInfo.RequiredVersion.BasicString + ")");
			trigger.Reply("Server has been running for {0}.", RealmServer.RunTime.Format());
			trigger.Reply("There are {0} players online (Alliance: {1}, Horde: {2}).",
						  World.CharacterCount, World.AllianceCharCount, World.HordeCharCount);

			var mod = trigger.Text.NextModifiers();
			if (mod == "l")
			{
				if (World.CharacterCount <= MaxListCount)
				{
					var i = 0;
					foreach (var chr in World.GetAllCharacters())
					{
						i++;
						trigger.Reply("{0}. {1} ({2})", i, chr.Name, chr.FactionGroup);
					}
				}
				else
				{
					trigger.Reply("Too many players to list.");
				}
			}
		}
	}
	#endregion

	#region GodMode
	public class GodModeCommand : RealmServerCommand
	{
		protected GodModeCommand() { }

		protected override void Initialize()
		{
			Init("GodMode", "GM");
			EnglishParamInfo = "[0|1]";
			EnglishDescription = "Toggles the GodMode";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var target = (Character)trigger.Args.Target;
			var mode = (!trigger.Text.HasNext && !target.GodMode) || trigger.Text.NextBool();
			target.GodMode = mode;
			trigger.Reply("GodMode " + (mode ? "ON" : "OFF"));
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

	#region Notify
	public class NotifyCommand : RealmServerCommand
	{
		protected NotifyCommand() { }

		protected override void Initialize()
		{
			Init("Notify", "SendNotification");
			EnglishParamInfo = "<text>";
			EnglishDescription = "Notifies the target with a flashing message.";
		}

		public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
		{
			var msg = trigger.Text.Remainder;
			if (msg.Length > 0)
			{
				((Character)trigger.Args.Target).Notify(msg);
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
}