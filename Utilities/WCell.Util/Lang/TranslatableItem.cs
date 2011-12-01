using System;

namespace WCell.Util.Lang
{
	public class TranslatableItem<K>
		where K : IConvertible
	{
		public K Key;
		public object[] Args;

		public TranslatableItem(K key, params object[] args)
		{
			Key = key;
			Args = args;
		}
	}
}