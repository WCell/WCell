using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.RealmServer;
using WCell.Util.Lang;

namespace WCell.Addons.Default.Lang
{
	public class DefaultAddonLocalizer : Localizer<ClientLocale, AddonMsgKey>
	{
		private static DefaultAddonLocalizer instance;

		public static DefaultAddonLocalizer Instance
		{
			get { return instance; }
		}

		[Initialization(InitializationPass.First)]
		public static void InitializeLocalizer()
		{
			var k = AddonMsgKey.WSOnStart;
			Console.WriteLine("Hello + " + k);
			try
			{
				instance = new DefaultAddonLocalizer(ClientLocale.English,RealmServerConfiguration.DefaultLocale, DefaultAddon.Instance.LangDir);
				instance.LoadTranslations();
			}
			catch (Exception e)
			{
				throw new InitializationException(e, "Unable to load Localizations");
			}
		}

		//[Initialization(InitializationPass.First, "Initialize Localizer")]
		//public static void InitLocalizer()
		//{
		//}

		public DefaultAddonLocalizer(ClientLocale baseLocale, ClientLocale defaultLocale, string folder)
			: base(baseLocale, defaultLocale, folder)
		{
		}
	}
}
