using System;
using System.IO;
using System.Reflection;

namespace WCell.Util.Lang
{
	/// <summary>
	/// Localizer class that converts the elements of the Locale and Key enums to array indices to look up strings with minimal
	/// overhead. Values defined in supplied Enum types must all be positive and not too big.
	/// <typeparam name="Locale">
	/// int-Enum that contains a set of usable Locales. For every Locale, one XML file is created in the supplied folder to contain
	/// all pairs of keys their string-representations.
	/// </typeparam>
	/// <typeparam name="Key">int-Enum that contains language keys which are mapped to string values in an XML file</typeparam>
	/// </summary>
	public class Localizer<Locale, Key> : ILocalizer<Locale>
		where Locale : IConvertible
		where Key : IConvertible
	{
		static readonly int _MaxLocaleValue;
		static readonly int _MaxKeyValue;

		static Localizer()
		{
			_MaxLocaleValue = GetMaxEnumValue(typeof(Locale), 100);
			_MaxKeyValue = GetMaxEnumValue(typeof(Key), 50000);
		}

		private static int GetMaxEnumValue(Type type, int maxx)
		{
			var max = 0;
			foreach (IConvertible val in Enum.GetValues(type))
			{
				var num = val.ToInt32(null);
				if (num > max)
				{
					max = num;
				}
				else if (num < 0)
				{
					throw new InvalidDataException("Cannot use Enum " + type + " because it defines negative values.");
				}
			}

			if (max > maxx)
			{
				throw new ArgumentException(string.Format("Enum {0} has members with too big values ({1} > {2})",
					type, max, maxx));
			}
			return max;
		}

		public readonly Translation<Locale,Key>[] Translations;

		private Locale m_DefaultLocale;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="baseLocale">The locale of the translation that is the most complete (usually English)</param>
		/// <param name="defaultLocale"></param>
		/// <param name="folder"></param>
		public Localizer(Locale baseLocale, Locale defaultLocale, string folder)
		{
			BaseLocale = baseLocale;
			m_DefaultLocale = defaultLocale;
			Folder = folder;
			Translations = new Translation<Locale,Key>[_MaxLocaleValue + 1];
		}

		/// <summary>
		/// The BaseLocale is the locale of the translation that is the most complete (usually English)
		/// </summary>
		public Locale BaseLocale
		{
			get;
			private set;
		}

		public Locale DefaultLocale
		{
			get { return m_DefaultLocale; }
			set
			{
				m_DefaultLocale = value;
				VerifyIntegrity();
			}
		}

		/// <summary>
		/// The BaseTranslation is the translation that is the most complete (usually English)
		/// </summary>
		public Translation<Locale, Key> BaseTranslation
		{
			get;
			set;
		}

		public Translation<Locale, Key> DefaultTranslation
		{
			get;
			set;
		}

		public string Folder
		{
			get;
			set;
		}

		public int MaxLocaleValue
		{
			get { return _MaxLocaleValue; }
		}

		public int MaxKeyValue
		{
			get { return _MaxKeyValue; }
		}

		#region Loading & Verification
		/// <summary>
		/// Not Thread-Safe!
		/// </summary>
		public void LoadTranslations()
		{
			BaseTranslation = null;
			DefaultTranslation = null;
			foreach (Locale locale in Enum.GetValues(typeof(Locale)))
			{
				LoadTranslations(locale);
			}

			VerifyIntegrity();
		}

		/// <summary>
		/// Not Thread-Safe!
		/// </summary>
		public void Resync()
		{
			LoadTranslations();
		}

		/// <summary>
		/// Loads all translations for the given locale from the folder
		/// with the name of the locale.
		/// </summary>
		private void LoadTranslations(Locale locale)
		{
			var translation = Translation<Locale, Key>.Load(this, locale);
			if (translation != null)
			{
				translation.Localizer = this;
				Translations[locale.ToInt32(null)] = translation;

				if (BaseLocale.Equals(locale))
				{
					BaseTranslation = translation;
				}
				if (DefaultLocale.Equals(locale))
				{
					DefaultTranslation = translation;
				}
			}
		}

		private void VerifyIntegrity()
		{
			if (BaseTranslation == null)
			{
				throw new InvalidDataException("Could not find file for BaseLocale: " + Translation<Locale, Key>.GetFile(Folder, BaseLocale));
			}

			if (DefaultTranslation == null)
			{
				throw new InvalidDataException("Could not find file for DefaultLocale: " + Translation<Locale, Key>.GetFile(Folder, DefaultLocale));
			}

			for (int i = 0; i < Translations.Length; i++)
			{
				var trans = Translations[i];
				if (trans == null || trans.Locale.ToInt32(null) != i)
				{
					Translations[i] = DefaultTranslation;
				}
			}
		}
		#endregion

		#region Translations
		public Translation<Locale, Key> Translation(Locale locale)
		{
			return Translations[locale.ToInt32(null)];
		}

		public string Translate(TranslatableItem<Key> item)
		{
			return Translate(item.Key, item.Args);
		}

		public string Translate(Locale locale, TranslatableItem<Key> item)
		{
			return Translate(locale, item.Key, item.Args);
		}

		public string Translate(Key key, params object[] args)
		{
			return Translate(DefaultLocale, key);
		}

		public string Translate(Locale locale, Key key, params object[] args)
		{
			var trans = Translation(locale);
			if (trans == null)
			{
				trans = DefaultTranslation;
			}

			var msg = trans.Translate(key, args);
			if (string.IsNullOrEmpty(msg))
			{
				msg = DefaultTranslation.Translate(key, args);
				if (string.IsNullOrEmpty(msg))
				{
					msg = BaseTranslation.Translate(key, args);
					if (string.IsNullOrEmpty(msg))
					{
						msg = "No translation available for Key [" + key + "]";
					}
				}
			}
			return msg;
		}

		/// <summary>
		/// Get all translations of the given key, in an array which is indexed by Locale.
		/// You can use the returned array to get a translated string, like this:
		/// <code>
		/// var translations = GetTranslations(key);
		/// var translation = translation[(int)mylocale];
		/// </code>
		/// </summary>
		public string[] GetTranslations(Key key)
		{
			var strings = new string[MaxLocaleValue+1];
			foreach (var trans in Translations)
			{
				if (trans == null) continue;
				var num = trans.Locale.ToInt32(null);
				strings[num] = trans.GetValue(key);
			}
			return strings;
		}
		#endregion
	}
}