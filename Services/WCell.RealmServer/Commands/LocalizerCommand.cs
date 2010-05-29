using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.RealmServer.Global;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;
using WCell.Util.Lang;

namespace WCell.RealmServer.Commands
{
	public class LocalizerCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Localizer", "Lang");
			Description = new TranslatableItem(LangKey.CmdLocalizerDescription);
		}

		public class ReloadLangCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Reload", "Resync");
				Description = new TranslatableItem(LangKey.CmdLocalizerSetLocaleDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				World.Pause(() =>
				{
					RealmLocalizer.Instance.Resync();
				});
				trigger.Reply(LangKey.Done);
			}
		}

		public class SetLocaleCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("SetLocale", "Locale");
				ParamInfo = new TranslatableItem(LangKey.CmdLocalizerSetLocaleParamInfo);
				Description = new TranslatableItem(LangKey.CmdLocalizerSetLocaleDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var user = trigger.Args.User;
				if (user != null)
				{
					user.Locale = trigger.Text.NextEnum(user.Locale);
					trigger.Reply(LangKey.Done);
				}
				else
				{
					trigger.Reply(LangKey.UnableToSetUserLocale);
				}
			}
		}
	}
}
