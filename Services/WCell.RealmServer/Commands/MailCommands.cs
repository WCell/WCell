using WCell.Constants.Items;
using WCell.RealmServer.Items;
using WCell.Util.Commands;
using WCell.RealmServer.Mail;
using WCell.Constants;
using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Commands
{
	public class MailCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Mail");
		}

		// TODO: Support text-based mail reading for remote clients
		// TODO: Support reading mails of others, but prevent lower staff members from reading higher staff members' mai
		public class ReadMailCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Read", "S");
				EnglishParamInfo = "";
				EnglishDescription = "Read all mails";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var chr = trigger.Args.Character;
				if (chr == null)
				{
					trigger.Reply("Cannot read Mails if no Character is given (yet).");
				}
				chr.MailAccount.SendMailList();
			}
		}

		public class SendMailCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Send", "S");
				EnglishParamInfo = "[-i[c][m] <ItemId> [<CoD>] [<money>]] <receiver> <subject>, <text>";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var text = trigger.Text;
				var mod = text.NextModifiers();

				var items = new List<ItemRecord>();
				var cod = 0u;
				var money = 0u;
				if (mod.Contains("i"))
				{
					var itemId = trigger.Text.NextEnum(ItemId.None);
					var template = ItemMgr.GetTemplate(itemId);
					if (template == null)
					{
						trigger.Reply("Invalid ItemId.");
						return;
					}
					var item = ItemRecord.CreateRecord(template);
					items.Add(item);
					item.SaveLater();
				}
				
				if (mod.Contains("c"))
				{
					cod = text.NextUInt();
				}

				var recipientName = trigger.Text.NextWord();
				if (recipientName.Length == 0)
				{
					trigger.Reply("Could not send mail - Unknown Recipient: " + recipientName);
					return;
				}
				var subject = trigger.Text.NextWord(",");
				var msg = trigger.Text.Remainder;

				var err = MailMgr.SendMail(recipientName, subject, msg, MailStationary.GM, items, money, cod, trigger.Args.User);
				if (err == MailError.OK)
				{
					trigger.Reply("Done.");
				}
				else
				{
					trigger.Reply("Could not send mail: " + err);
				}
			}
		}
	}
}