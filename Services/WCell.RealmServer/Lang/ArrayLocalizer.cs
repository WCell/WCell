using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Constants;

namespace WCell.RealmServer.Lang
{
	/// <summary>
	/// Extension class that gives tools to select array elements, based on ClientLocale
	/// </summary>
	public static class ArrayLocalizer
	{
		public static string Localize(this string[] texts, IRealmClient client)
		{
			return texts.Localize(client.Info.Locale);
		}

		/// <summary>
		/// Returns the entry at the index that equals the numeric value of locale
		/// </summary>
		public static string Localize(this string[] texts, ClientLocale locale)
		{
			var text = texts[(int)locale];
			if (string.IsNullOrEmpty(text))
			{
				return LocalizeWithDefaultLocale(texts);
			}
			return text;
		}

		/// <summary>
		/// Returns the entry at the index that equals the numeric value of the default locale
		/// </summary>
		public static string LocalizeWithDefaultLocale(this string[] texts)
		{
			var text = texts[(int) RealmServerConfiguration.DefaultLocale];
			if (string.IsNullOrEmpty(text) && RealmServerConfiguration.DefaultLocale != ClientLocale.English)
			{
				text = texts[(int) ClientLocale.English];
			}
			if (text == null)
			{
				text = "";
			}
			return text;
		}
	}
}