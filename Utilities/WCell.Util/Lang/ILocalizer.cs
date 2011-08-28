using System;

namespace WCell.Util.Lang
{
	public interface ILocalizer<L>
		where L : IConvertible
	{
		string Folder
		{
			get;
			set;
		}

		int MaxLocaleValue
		{
			get;
		}

		int MaxKeyValue
		{
			get;
		}
	}
}