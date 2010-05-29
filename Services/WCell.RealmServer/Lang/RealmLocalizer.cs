using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.Util.Lang;
using WCell.Util.NLog;

namespace WCell.RealmServer.Lang
{
	public class RealmLocalizer : Localizer<ClientLocale, LangKey>
	{
		public static RealmLocalizer Instance;

		[Initialization(InitializationPass.First, "Initialize Localizer")]
		public static void InitLocalizer()
		{
			try
			{
				Instance = new RealmLocalizer(ClientLocale.English,
				                              RealmServerConfiguration.DefaultLocale, RealmServerConfiguration.LangDir);

				Instance.LoadTranslations();
			}
			catch (Exception e)
			{
				throw new InitializationException(e, "Unable to load Localizations");
			}
		}

		public RealmLocalizer(ClientLocale baseLocale, ClientLocale defaultLocale, string folder)
			: base(baseLocale, defaultLocale, folder)
		{
		}
	}
}
