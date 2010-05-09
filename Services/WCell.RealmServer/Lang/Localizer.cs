using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Constants;

namespace WCell.RealmServer.Lang
{
	/// <summary>
	/// 
	/// </summary>
	public static class Localizer
	{
		public static string Localize(this string[] texts, IRealmClient client)
		{
			return texts.Localize(client.Info.Locale);
		}

		public static string Localize(this string[] texts, ClientLocale locale)
		{
			var text = texts[(int)locale];
			if (string.IsNullOrEmpty(text))
			{
				text = texts[(int)RealmServerConfiguration.DefaultLocale];
				if (string.IsNullOrEmpty(text) && RealmServerConfiguration.DefaultLocale != ClientLocale.English)
				{
					text = texts[(int)ClientLocale.English];
				}
				if (text == null)
				{
					text = "";
				}
			}
			return text;
		}

		//public static string Localize(IRealmClient client, string key)
		//{
		//    return key;
		//}

		//public static string Localize(Character chr, string key)
		//{

		//    return key;
		//}

		//public static string Localize(string locale, string key)
		//{
		//    return key;
		//}

		//public static string Localize(IRealmClient client, int key)
		//{
		//    return key.ToString();
		//}

		//public static string Localize(Character chr, int key)
		//{

		//    return key.ToString();
		//}

		//public static string Localize(string locale, int key)
		//{
		//    return key.ToString();
		//}
	}
}
