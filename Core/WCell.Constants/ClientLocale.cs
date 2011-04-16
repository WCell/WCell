using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants
{
	public enum ClientLocale
	{
		/// <summary>
		/// Any english locale
		/// </summary>
		English = 0,

		/// <summary>
		/// Korean
		/// </summary>
		Korean,

		/// <summary>
		/// French
		/// </summary>
		French,

		/// <summary>
		/// German
		/// </summary>
		German,

		/// <summary>
		/// Simplified Chinese
		/// </summary>
		ChineseSimplified,

		/// <summary>
		/// Traditional Chinese
		/// </summary>
		ChineseTraditional,

		/// <summary>
		/// Spanish.
		/// Also esMX.
		/// </summary>
		Spanish,

		/// <summary>
		/// Russian
		/// </summary>
		Russian = 7,

		End
	}

	public static class ClientLocales
	{
		public static readonly Dictionary<string, ClientLocale> LocaleMap = 
			new Dictionary<string, ClientLocale>(StringComparer.InvariantCultureIgnoreCase);

		static ClientLocales()
		{
			LocaleMap["enUS"] = ClientLocale.English;
			LocaleMap["enGB"] = ClientLocale.English;
			LocaleMap["koKR"] = ClientLocale.Korean;
			LocaleMap["frFR"] = ClientLocale.French;
			LocaleMap["deDE"] = ClientLocale.German;
			LocaleMap["zhCN"] = ClientLocale.ChineseSimplified;
			LocaleMap["zhTW"] = ClientLocale.ChineseTraditional;
			LocaleMap["esES"] = ClientLocale.Spanish;
			LocaleMap["esMX"] = ClientLocale.Spanish;
			LocaleMap["ruRU"] = ClientLocale.Russian;
		}

		public static bool Lookup(string localeStr, out ClientLocale locale)
		{
			if (!LocaleMap.TryGetValue(localeStr.Substring(0, 4), out locale))
			{
				// 
				return false;
			}
			return true;
		}
	}
}