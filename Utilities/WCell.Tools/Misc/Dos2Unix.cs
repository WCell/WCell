using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WCell.Util;
using WCell.Util.Toolshed;

namespace WCell.Tools.Misc
{
	public static class Dos2Unix
	{
		[Tool]
		public static void Convert(string directory, string suffix1, string suffix2, string suffix3, string suffix4)
		{
			Convert(directory, new[] { suffix1, suffix2, suffix3, suffix4 });
		}

		//[Tool]
		public static void Convert(string directory, string suffix1, string suffix2, string suffix3)
		{
			Convert(directory, new[] { suffix1, suffix2, suffix3 });
		}

		//[Tool]
		public static void Convert(string directory, string suffix1, string suffix2)
		{
			Convert(directory, new[] { suffix1, suffix2 });
		}

		//[Tool]
		public static void Convert(string directory, string suffix1)
		{
			Convert(directory, new[] { suffix1 });
		}

		public static void Convert(string directory, params string[] suffixes)
		{
			foreach (var file in Directory.GetFileSystemEntries(directory))
			{
				if (Directory.Exists(file))
				{
					Convert(file, suffixes);
				}
				else
				{
					if (suffixes.Any(suffix => file.EndsWith(suffix)))
					{
						// found the right kind of file
						var lines = File.ReadLines(file);
						var text = string.Join("\n", lines);
						File.WriteAllText(file, text);
					}
				}
			}
		}
	}
}