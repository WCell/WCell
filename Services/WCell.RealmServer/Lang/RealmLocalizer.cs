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
	public class RealmLocalizer : Localizer<ClientLocale, RealmLangKey>
	{
		private static RealmLocalizer instance;

		public static RealmLocalizer Instance
		{
			get
			{
				if (!RealmServerConfiguration.Loaded)
				{
					throw new InvalidOperationException("Must not use RealmLocalizer before Configuration was loaded.");
				}

				try
				{
					instance = new RealmLocalizer(ClientLocale.English, RealmServerConfiguration.DefaultLocale, RealmServerConfiguration.LangDir);
					instance.LoadTranslations();
				}
				catch (Exception e)
				{
					throw new InitializationException(e, "Unable to load Localizations");
				}
				return instance;
			}
		}

		//[Initialization(InitializationPass.First, "Initialize Localizer")]
		//public static void InitLocalizer()
		//{
		//}

		public RealmLocalizer(ClientLocale baseLocale, ClientLocale defaultLocale, string folder)
			: base(baseLocale, defaultLocale, folder)
		{
		}

		/// <summary>
		/// TODO: Localize (use TranslatableItem)
		/// </summary>
		public static string FormatTimeSecondsMinutes(int seconds)
		{
			string time;
			if (seconds < 60)
			{
				time = seconds + " seconds";
			}
			else
			{
				var mins = seconds / 60;
				time = mins + (mins == 1 ? " minute" : " minutes");
				if (seconds % 60 != 0)
				{
					time += " and " + seconds + (seconds == 1 ? " second" : " seconds");
				}
			}
			return time;
		}
	}
}