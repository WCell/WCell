using System;
using System.IO;
using System.Reflection;

namespace WCell.Util.Lang
{
	/// <summary>
	/// 
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
				LoadTranslation(locale);
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

		private void LoadTranslation(Locale locale)
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
			return Translate(key, DefaultLocale);
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
		#endregion
	}
}
