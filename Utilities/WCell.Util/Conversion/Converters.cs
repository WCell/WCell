using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Conversion
{
	public static class Converters
	{
		public static IConverterProvider Provider
		{
			get;
			set;
		}

		public static IFieldReader GetReader(string typeName)
		{
			if (typeName == null)
			{
				return null;
			}

			var type = Utility.GetType(typeName);
			return GetReader(type);
		}

		public static IFieldReader GetReader(Type type)
		{
			if (Provider == null)
			{
				throw new InvalidOperationException("Provider must be set before accessing any Type-readers.");
			}
			return Provider.GetReader(type);
		}
	}
}
