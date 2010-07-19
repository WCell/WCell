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
		public static string[] StandardCodingTextFiles = new[] { ".sln", "proj", ".txt", ".cs", ".xml" };

		/// <summary>
		/// Converts all line endings of all text files in wcell from crlf to lf
		/// </summary>
		[Tool]
		public static void ConvertLines()
		{
			ConvertLines(ToolConfig.WCellRoot, StandardCodingTextFiles);
		}

		public static void ConvertLines(string directory, params string[] suffixes)
		{
			foreach (var file in Directory.GetFileSystemEntries(directory))
			{
				if (Directory.Exists(file))
				{
					ConvertLines(file, suffixes);
				}
				else
				{
					if (suffixes.Any(suffix => suffix.Length > 0 && file.EndsWith(suffix)))
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