using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Conversion
{
	public interface IConverterProvider
	{
		IFieldReader GetReader(Type type);
	}
}
