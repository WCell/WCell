using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core;
using WCell.Util.Commands;
using WCell.RealmServer.Commands;
using WCell.Constants;

namespace WCell.Tools.Commands
{
	public class LocaleCommand : ToolCommand
	{
		protected override void Initialize()
		{
			Init("Locale", "Loc");
			EnglishDescription = "Sets the currently used Locale.";
		}

		public override void Process(CmdTrigger<ToolCmdArgs> trigger)
		{
			if (!trigger.Text.HasNext)
			{
				trigger.Reply("Locale: " + ToolConfig.Locale);
			}
			else
			{
				var loc = trigger.Text.NextEnum(ClientLocale.End);
				if (loc == ClientLocale.End)
				{
					trigger.Reply("Invalid Locale.");
					return;
				}
				ToolConfig.Locale = loc;
				trigger.Reply("Locale changed to: " + loc);
			}
		}
	}
}