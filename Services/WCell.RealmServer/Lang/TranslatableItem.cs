using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Lang
{
	public class TranslatableItem : Util.Lang.TranslatableItem<LangKey>
	{
		public TranslatableItem(LangKey key, params object[] args)
			: base(key, args)
		{
		}
	}
}
