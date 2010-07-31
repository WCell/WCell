using System;
using System.IO;
using System.Xml.Serialization;

namespace WCell.Util.Lang
{
	[XmlRoot("Translation")]
	public class Translation<L, K> : XmlFile<Translation<L, K>>
		where L : IConvertible
		where K : IConvertible
	{
		static readonly string Extension = ".xml";

		public static string GetFile(string folder, L locale)
		{
			return Path.Combine(folder, locale + Extension);
		}

		public static Translation<L, K> Load(ILocalizer<L> localizer, L locale)
		{
			var file = GetFile(localizer.Folder, locale);
			if (!File.Exists(file))
			{
				return null;
			}
			try
			{
				var t = Load(file);
				t.Localizer = localizer;
				t.SortItems();
				return t;
			}
			catch (Exception e)
			{
				throw new IOException("Unable to load Localization file " + file, e);
			}
		}

		[XmlArray("Items")]
		[XmlArrayItem("Item")]
		public TranslatedItem<K>[] Items;

		[XmlIgnore]
		public ILocalizer<L> Localizer
		{
			get;
			internal set;
		}

		[XmlIgnore]
		public L Locale
		{
			get;
			private set;
		}

		protected override void OnLoad()
		{
		}

		public string GetValue(K key)
		{
			var item = Items[key.ToInt32(null)];
			if (item != null)
			{
				return item.Value;
			}
			return null;
		}

		public string Translate(K key, params object[] args)
		{
			var item = Items[key.ToInt32(null)];
			if (item != null)
			{
				return string.Format(item.Value, args);
			}
			return null;
		}

		private void SortItems()
		{
			// sort items
			var sortedItems = new TranslatedItem<K>[Localizer.MaxKeyValue + 1];
			if (Items != null)
			{
				foreach (var item in Items)
				{
					sortedItems[item.Key.ToInt32(null)] = item;
				}
			}
			Items = sortedItems;
		}
	}
}