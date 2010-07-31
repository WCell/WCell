using System;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.Util.Lang;

namespace WCell.AuthServer.Lang
{
	public class AuthLocalizer : Localizer<ClientLocale, AuthLangKey>
	{
		private static AuthLocalizer instance;

		public static AuthLocalizer Instance
		{
			get
			{
				if (!AuthServerConfiguration.Loaded)
				{
					throw new InvalidOperationException("Must not use RealmLocalizer before Configuration was loaded.");
				}

				try
				{
					instance = new AuthLocalizer(ClientLocale.English,
												  AuthServerConfiguration.DefaultLocale, AuthServerConfiguration.LangDir);

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

		public AuthLocalizer(ClientLocale baseLocale, ClientLocale defaultLocale, string folder)
			: base(baseLocale, defaultLocale, folder)
		{
		}
	}
}