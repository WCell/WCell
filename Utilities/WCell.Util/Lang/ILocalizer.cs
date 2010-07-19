using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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