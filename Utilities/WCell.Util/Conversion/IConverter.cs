using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Conversion
{
	public interface IConverter
	{
		object Convert(object input);
	}
}
