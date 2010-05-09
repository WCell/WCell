using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Variables
{
	public static class ConfigUtil
	{
		public static string GetFormattedValue(this IVariableDefinition def)
		{
			return Utility.GetStringRepresentation(def.Value);
		}
	}
}
