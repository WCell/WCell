using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.PacketAnalysis.Xml
{
	public static class ChangeLog
	{
		/// <summary>
		/// Used to allow simple conversion etc
		/// </summary>
		public static string[][] Changes = new string[][] {
			// 0 to 1
			new string[] {
				"Switch: Conditions renamed to Condition",
			},
			// 1 to 2
			new string[] {
				"You can now specify a list of Segments, without having to add the Complex Node",
				"The Definitions is not required twice anymore",
				"Version is now an Attribute of the Definitins-node",
				"Switch: Conditions 'If' statement is now an Attribute",
				"Switch: Conditions have a new Attribute 'Comparison' that decides how the given value should be compared",
				"Comparison is either of: Equal, GraterThan, LessThan, GreaterOrEqual, LessOrEqual"
			},
			// 2 to 3
			new string[] {
				"CHANGE: Conditional has been replaced with Switch (see Wiki for more info)",
				"CHANGE: The segment that is to be compared with the Switch, is specified in the Switch's CompareWith-attribute",
				"CHANGE: Lists' \"ListType\" is now just \"Type\"",
				"When Switches compare with List-Segments, the value for comparison is the List's length",
				"Values of conditons can now be simple arithmetical expressions - Operators and operands always have to be seperated by at least one space",
				"Valid operators are: | (binary Or), & (binary And), ^(binary XOr) , + (plus), - (minus), * (multiply), / (divide)",
				"New Switch-Comparisons: \"And\" and \"AndNot\" to match against flag-fields and more",
				"The new StaticList type is a list of fixed size, which is defined in the \"Length\" - attribute",
				"Empty Packet structures are now also supported"
			},
			// 3 to 4
			new string[] {
				"CHANGE: The content of the former Condition's 'Comparison'-attribute is now the name of the former 'If'-attribute",
				"CHANGE: 'Condition' is now called 'Case' to reflect the switch/case - relationship better"
			}
		};

		public static string GetChangeLog(int oldVersion, int newVersion)
		{
			StringBuilder sb = new StringBuilder();
			for (var i = oldVersion; i < newVersion; i++)
			{
				sb.AppendFormat("Changes from {0} to {1}:\n", i , i+1);
				foreach (var line in Changes[i])
				{
					sb.AppendLine("\t" + line);
				}
			}
			return sb.ToString();
		}
	}
}