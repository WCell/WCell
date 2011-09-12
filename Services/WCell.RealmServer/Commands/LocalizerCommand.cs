using WCell.RealmServer.Global;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	public class LocalizerCommand : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Localizer", "Lang");
			Description = new TranslatableItem(RealmLangKey.CmdLocalizerDescription);
		}

		public class ReloadLangCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Reload", "Resync");
				Description = new TranslatableItem(RealmLangKey.CmdLocalizerSetLocaleDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				World.ExecuteWhilePaused(() =>
				{
					RealmLocalizer.Instance.Resync();
				});
				trigger.Reply(RealmLangKey.Done);
			}
		}

		public class SetLocaleCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("SetLocale", "Locale");
				ParamInfo = new TranslatableItem(RealmLangKey.CmdLocalizerSetLocaleParamInfo);
				Description = new TranslatableItem(RealmLangKey.CmdLocalizerSetLocaleDescription);
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var user = trigger.Args.User;
				if (user != null)
				{
					user.Locale = trigger.Text.NextEnum(user.Locale);
					trigger.Reply(RealmLangKey.Done);
				}
				else
				{
					trigger.Reply(RealmLangKey.UnableToSetUserLocale);
				}
			}
		}
	}
}