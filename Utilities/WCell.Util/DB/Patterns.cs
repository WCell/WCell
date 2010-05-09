using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.DB
{
	public static class Patterns
	{
		public const string ArrayFieldIndex = "{#}";

		public static string Compile(string pattern, int i)
		{
			return pattern.Replace(ArrayFieldIndex, i.ToString());
		}
	}
}
