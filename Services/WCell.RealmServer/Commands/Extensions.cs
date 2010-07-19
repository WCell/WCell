using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	public static class Extensions
	{
		public static string Translate(this CmdTrigger<RealmServerCmdArgs> trigger, LangKey key, params object[] args)
		{
			return RealmLocalizer.Instance.Translate(trigger.GetLocale(), key, args);
		}

		public static string Translate(this CmdTrigger<RealmServerCmdArgs> trigger, TranslatableItem item)
		{
			return RealmLocalizer.Instance.Translate(trigger.GetLocale(), item);
		}

		public static void Reply(this CmdTrigger<RealmServerCmdArgs> trigger, LangKey key, params object[] args)
		{
			trigger.Reply(RealmLocalizer.Instance.Translate(trigger.GetLocale(), key, args));
		}

		public static void ReplyFormat(this CmdTrigger<RealmServerCmdArgs> trigger, LangKey key, params object[] args)
		{
			trigger.ReplyFormat(RealmLocalizer.Instance.Translate(trigger.GetLocale(), key, args));
		}

		public static ClientLocale GetLocale(this CmdTrigger<RealmServerCmdArgs> trigger)
		{
			return trigger != null && trigger.Args.User != null
			       	? trigger.Args.User.Locale
			       	: RealmServerConfiguration.DefaultLocale;
		}
	}
}