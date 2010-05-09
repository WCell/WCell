using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WCell.MPQTool.DBC;

namespace WCell.MPQTool.DBC.Compare
{
	public class DBCDirComparer
	{
		int maxTestAmount = int.MaxValue;
		string newDir, oldDir;

		public DBCDirComparer(string newDir, string oldDir)
		{
			this.newDir = newDir;
			this.oldDir = oldDir;
		}

		public void Compare()
		{
			Compare(Console.Out);
		}

		public void Compare(TextWriter writer)
		{
			var newFiles = new HashSet<string>(Directory.GetFiles(newDir, "*.dbc").Select((file) => new FileInfo(file).Name), 
			                                   StringComparer.InvariantCultureIgnoreCase);

			var oldFiles = new HashSet<string>(Directory.GetFiles(oldDir, "*.dbc").Select((file) => new FileInfo(file).Name),
			                                   StringComparer.InvariantCultureIgnoreCase);

			var addedFiles = newFiles.Except(oldFiles);

			var removedFiles = oldFiles.Except(newFiles);

			var existingFiles = newFiles.Except(addedFiles);

			if (addedFiles.Count() > 0)
			{
				writer.WriteLine("{0} files have been added:", addedFiles.Count());
				foreach (var file in addedFiles)
				{
					writer.WriteLine("\t" + new FileInfo(file).Name);
					var reader = new DBCReader(Path.Combine(newDir, file));
					writer.WriteLine("\t\tColumn Count: " + reader.ColumnCount);
					writer.WriteLine("\t\tRow Count: " + reader.RecordCount);
					writer.WriteLine("\t\tHas Strings: " + reader.HasStrings);
					if (reader.IrregularColumnSize)
					{
						writer.WriteLine("\t\tColumn Size IRREGULAR!");
					}
				}
			}

			if (removedFiles.Count() > 0)
			{
				writer.WriteLine("{0} files have been deprecated (not present anymore):", removedFiles.Count());
				foreach (var file in addedFiles)
				{
					writer.WriteLine("\t" + new FileInfo(file).Name);
				}
			}

			writer.WriteLine();
			writer.WriteLine("#######################################");
			writer.WriteLine("Changes in files{0}:", maxTestAmount != int.MaxValue ? " - Testing with first " + maxTestAmount + " rows" : "");
			writer.WriteLine();

			foreach (var file in existingFiles)
			{
				var comparer = new DBCFileComparer(Path.Combine(oldDir, file), Path.Combine(newDir, file), maxTestAmount, null);
				if (comparer.NewReader.IrregularColumnSize)
				{
					writer.WriteLine("Skipping {0} (IRREGULAR Column Size)", file);
				}
				else if (comparer.NewReader.ColumnCount != comparer.OldReader.ColumnCount &&
				         (comparer.NewReader.ColumnCount < 2 || comparer.OldReader.ColumnCount < 2))
				{
					writer.WriteLine("Skipping {0} (Only 1 column or less)", file);
				}
				else
				{
					comparer.Compare(writer);
				}
			}
		}

		public string NewDir
		{
			get
			{
				return newDir;
			}
		}

		public string OldDir
		{
			get
			{
				return oldDir;
			}
		}
	}
}