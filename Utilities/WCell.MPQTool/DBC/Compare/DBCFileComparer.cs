using System;
using System.Collections.Generic;
using System.IO;

using Cell.Core;
using WCell.Util;

namespace WCell.MPQTool.DBC.Compare
{
	public class DBCFileComparer
	{
		/// <summary>
		/// If we have more than this percentage in changes, the column probably moved (or we have strings)
		/// </summary>
		public static float MinColumnChangePct = 40f;
		/// <summary>
		/// If we compare 2 columns and have at least this percentage of matches, we can assume that the column moved here
		/// </summary>
		public static float MinColumnMatchPct = 90f;
		/// <summary>
		/// The minimum percentage of unique string indeces to qualify for a string column
		/// </summary>
		public static float MinStringUniquePct = 40f;
		/// <summary>
		/// The minimum percentage of possible string indeces to qualify for a string column (0 counts as invalid)
		/// </summary>
		public static float MinStringMatchPct = 90f;

		DBCReader oldReader, newReader;
		CellType[] knownColumnStructure;
		int testAmount;


		public DBCFileComparer(string oldFile, string newFile, int testAmount, CellType[] knownColumnStructure)
		{
			oldReader = new DBCReader(oldFile);
			newReader = new DBCReader(newFile);
			this.knownColumnStructure = knownColumnStructure;
			this.testAmount = testAmount;
		}

		public void Compare(TextWriter writer)
		{
			bool structureChange = false;
			string change = "";
			if (oldReader.ColumnCount != newReader.ColumnCount)
			{
				structureChange = true;
				change = string.Format("Column Count {0} from {1} to {2}", oldReader.ColumnCount > newReader.ColumnCount ? "decreased" : "increased",
				                       oldReader.ColumnCount, newReader.ColumnCount);
			}
			if (oldReader.RecordSize != newReader.RecordSize)
			{
				if (structureChange)
				{
					change += " AND ";
				}

				structureChange = true;
				change += string.Format("Record Size {0} from {1} to {2}", oldReader.RecordSize > newReader.RecordSize ? "decreased" : "increased",
				                        oldReader.RecordSize, newReader.RecordSize);
			}
			if (newReader.RecordSize != (newReader.ColumnCount * 4))
			{
				change += " (Row Length IRREGULAR: " + newReader.RecordSize + ")";
			}

			if (structureChange)
			{
				var name = new FileInfo(oldReader.FileName).Name.Replace(".dbc", "");
				writer.WriteLine("{0}, {1} Records ({2})", name, newReader.RecordCount, change);

				testAmount = Math.Min(testAmount, oldReader.RecordCount);
				testAmount = Math.Min(testAmount, newReader.RecordCount);

				// arrays, indexed by id (assume first column to be id column)
				var oldRows = new byte[oldReader.RecordCount * 3][];
				var newRows = new byte[newReader.RecordCount * 3][];
				uint highestNewId = 0, highestOldId = 0;
				for (uint i = 0; i < testAmount; i++)
				{
					var oldRow = oldReader.GetRow(i);
					var newRow = newReader.GetRow(i);

					var oldId = oldRow.GetUInt32(0);
					var newId = oldRow.GetUInt32(0);

					ArrayUtil.Set(ref oldRows, oldId, oldRow);
					ArrayUtil.Set(ref newRows, newId, newRow);

					highestNewId = Math.Max(highestNewId, newId);
					highestOldId = Math.Max(highestNewId, oldId);
				}


				//var cols = Math.Max(oldReader.ColumnCount, newReader.ColumnCount);
				var minColCount = Math.Min(oldReader.ColumnCount, newReader.ColumnCount);
				int[] changes = new int[minColCount];

				uint highestId = Math.Max(highestNewId, highestOldId);
				int addedCount = 0, deprecatedCount = 0;
				for (uint i = 1; i <= highestId; i++)
				{
					var oldRow = oldRows.Get(i);
					var newRow = newRows.Get(i);

					if ((oldRow != null) != (newRow != null))
					{
						if (newRow == null)
						{
							// row doesn't exit anymore
							deprecatedCount++;
						}
						else
						{
							// row got added
							addedCount++;
						}
					}
					else if (oldRow != null && newRow != null)
					{
						for (uint j = 1; j < changes.Length; j++)
						{
							if (oldRow.GetUInt32(j) != newRow.GetUInt32(j))
							{
								changes[j]++;
							}
						}
					}
				}

				if (addedCount > 0)
				{
					writer.WriteLine("- {0} new records", addedCount);
				}
				if (deprecatedCount > 0)
				{
					writer.WriteLine("- {0} deprecated records", deprecatedCount);
				}

				List<uint> changedCols = new List<uint>();
				for (uint col = 0; col < changes.Length; col++)
				{
					var percent = changes[col] / (float)testAmount * 100;
					if (percent >= MinColumnChangePct)
					{
						writer.Write("\t\tField {0}: {1} changes ({2}%", col + 1, changes[col], percent);
						var strChance = oldReader.GetStringMatchPct(col, MinStringUniquePct);
						bool mightBeStringCol;
						if (strChance >= MinStringMatchPct)
						{
							mightBeStringCol = true;
							writer.Write(" - String column probability: " + strChance + "%");
						}
						else
						{
							mightBeStringCol = false;
						}
						writer.Write(")");
						changedCols.Add(col);
						//continue;

						var matches = 0;

						if (!mightBeStringCol)
						{
							for (uint newCol = 1; newCol < newReader.ColumnCount; newCol++)
							{
								var matchCount = 0;
								var zeros = 0;
								for (uint r = 1; r <= highestId; r++)
								{
									var oldRow = oldRows.Get(r);
									var newRow = newRows.Get(r);

									if (oldRow != null && newRow != null)
									{
										var oldVal = oldRow.GetUInt32(col);
										var newVal = newRow.GetUInt32(newCol);
										if (oldVal == 0)
										{
											zeros++;
										}
										if (oldVal == newVal)
										{
											matchCount++;
										}
									}
								}

								if (newCol == 1)
								{
									writer.Write(" and {0} Zeros - Might have moved to:", zeros);
								}
								var matchPercent = matchCount / (float)testAmount * 100;
								if (matchPercent >= MinColumnMatchPct)
								{
									// possible column match
									writer.Write(newCol + " (" + matchPercent + "%), ");
									matches++;
								}
							}
						}
						else
						{
							writer.Write(" - Might have moved to: ");
							for (uint newCol = 1; newCol < newReader.ColumnCount; newCol++) {
								var newStrChance = newReader.GetStringMatchPct(newCol, MinStringUniquePct);
								if (newStrChance >= MinStringMatchPct)
								{
									writer.Write(newCol + " (" + newStrChance + "%), ");
									matches++;
								}
							}
						}

						if (matches == 0)
						{
							writer.Write("-none-");
						}
						writer.WriteLine();
					}
				}

				foreach (var col in changedCols)
				{
				}
			}
		}

		public DBCReader OldReader
		{
			get
			{
				return oldReader;
			}
		}

		public DBCReader NewReader
		{
			get
			{
				return newReader;
			}
		}

	}
}