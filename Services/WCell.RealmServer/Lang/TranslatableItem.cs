using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;

namespace WCell.RealmServer.Lang
{
	public class TranslatableItem : Util.Lang.TranslatableItem<RealmLangKey>
	{
		public TranslatableItem(RealmLangKey key, params object[] args)
			: base(key, args)
		{
		}

		public string Translate(ClientLocale locale)
		{
			return RealmLocalizer.Instance.Translate(locale, this);
		}
	}
}
