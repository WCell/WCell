/*************************************************************************
 *
 *   file		: VariablePurifier.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-11-24 13:55:59 +0100 (ti, 24 nov 2009) $
 
 *   revision		: $Rev: 1142 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace WCell.Tools
{
	public class VariablePurifier
	{
		public static Predicate<string> VarFilter;

		/// <summary>
		/// Can purify all c-style constant-names in the given file
		/// </summary>
		public static void SimplePurify(string filename)
		{
			PurifyFiles(
				"./WCell.Tools/",
				(file) => file.Name == filename,
				false,
				null,
				new string[] { });
		}

		public static void PurifySpellVars()
		{
			VarFilter = delegate(string var)
			{
				Console.Write("Change {0}? (y/n): ", var);
				bool change = Console.ReadKey().KeyChar == 'y';
				if (change)
				{
					Console.WriteLine(" - Changing...");
				}
				else
				{
					Console.WriteLine(" - Skipping...");
				}
				return change;
			};

			PurifyFiles(
				"./WCell.RealmServer/Spells",
				CsFileFilter,
				false,
				"./backups",
				new[]
					{
						"TODO",
						"DBC",
						"IO"
					});
		}

		public static readonly Predicate<FileInfo> CsFileFilter =
			delegate(FileInfo file) { return file.Extension.Equals(".cs", StringComparison.InvariantCultureIgnoreCase); };

		private static Regex upperCaseVars = new Regex(@"([A-Z](?:([\dxA-Z]+)[_]?)+)[^\d\w_]");

		public static void PurifyFiles(string dirName,
									   Predicate<FileInfo> filter, bool recursive, string backupDir, string[] ignore)
		{
			DirectoryInfo dir = new DirectoryInfo(dirName);
			PurifyFiles(dir, filter, recursive, backupDir, ignore);
		}

		public static void PurifyFiles(DirectoryInfo dir,
									   Predicate<FileInfo> filter, bool recursive, string backupDir, string[] ignore)
		{
			foreach (FileSystemInfo file in dir.GetFileSystemInfos())
			{
				if ((file.Attributes & FileAttributes.Directory) != 0)
				{
					PurifyFiles(file as DirectoryInfo, filter, recursive, backupDir, ignore);
				}
				else
				{
					if (filter(file as FileInfo))
						PurifyFile(file.FullName, backupDir, ignore);
				}
			}
		}

		public static void PurifyFile(string file, string backupDir, string[] ignore)
		{
			string content = File.ReadAllText(file);

			FileInfo finfo = new FileInfo(file);

			Console.WriteLine("Purifying contents of {0}... ", finfo.Name);
			string purified = Purify(content, true, ignore);
			if (purified != null)
			{
				if (backupDir != null)
				{
					Directory.CreateDirectory(backupDir);
					string backupFile = backupDir + Path.PathSeparator + finfo.Name;
					File.WriteAllText(backupFile, content);
				}
				File.WriteAllText(file, purified);

				Console.WriteLine("Done. ({1}Backup created)", finfo.Name,
								  (backupDir != null) ? "" : "NO ");
			}
			else
			{
				Console.WriteLine("Nothing found.");
			}
		}

		public static string Purify(string text, bool verbose, string[] ignore)
		{
			HashSet<string> ignoreSet = new HashSet<string>(ignore);
			//text = "A12_BC";
			Match match = upperCaseVars.Match(text);
			StringBuilder result = new StringBuilder(text.Length);
			int pos = 0;
			int count = 0;
			while (match.Success)
			{
				string var = match.Groups[1].Value;
				// variable parts seperated by "_"
				CaptureCollection captures = match.Groups[2].Captures;

				result.Append(text.Substring(pos, match.Index - pos));
				//result.AppendLine();

				// check
				if (!ignoreSet.Contains(var) && (VarFilter == null || VarFilter(var)))
				{
					// purify
					result.Append(var[0] + captures[0].Value.ToLower());
					for (int i = 1; i < captures.Count; i++)
					{
						string part = captures[i].Value;
						result.Append(part[0] + part.Substring(1).ToLower());
					}
					count++;
				}
				else
				{
					result.Append(var);
				}

				// next match
				pos = match.Index + var.Length;
				match = match.NextMatch();
			}

			if (count > 0)
			{
				// also write the last bit
				result.Append(text.Substring(pos));

				if (verbose)
				{
					Console.Write("Purified {0} variables - ", count);
				}

				return result.ToString();
			}
			// nothing happened
			return null;
		}
	}
}