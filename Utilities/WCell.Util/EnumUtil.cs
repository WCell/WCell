using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace WCell.Util
{
	public static class EnumUtil
	{
		public static T Parse<T>(string input)
		{
			return (T)Enum.Parse(typeof(T), input);
		}

		/// <summary>
		/// TODO: Put big enums in dictionaries
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="input"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse<T>(string input, out T result)
		{
			if (input.Length > 0)
			{
				if ((char.IsDigit(input[0]) || (input[0] == '-')) || (input[0] == '+'))
				{
					var underlyingType = Enum.GetUnderlyingType(typeof(T));
					try
					{
						var obj = Convert.ChangeType(input, underlyingType, CultureInfo.InvariantCulture);
						result = (T)Enum.ToObject(typeof(T), obj);
						return true;
					}
					catch (FormatException)
					{
					}
				}
				else
				{
					Dictionary<string, object> dict;
					if (Utility.EnumValueMap.TryGetValue(typeof(T), out dict))
					{
						object obj;
						if (dict.TryGetValue(input.Trim(), out obj))
						{
							result = (T)obj;
							return true;
						}
					}
				}
			}
			result = default(T);
			return false;
		}
	}
}