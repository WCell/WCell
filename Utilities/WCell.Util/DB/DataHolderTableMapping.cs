using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Data;

namespace WCell.Util.DB
{
	/// <summary>
	/// Defines the relation between a set of DataHolder and the tables that belong to them.
	/// This is a many-to-many relationship:
	/// One DataHolder can use multiple tables and multiple tables can be used by the same DataHolder.
	/// It is ensured that all default-tables have a lower index than non-default ones in the <see cref="TableDefinitions"/>-array.
	/// </summary>
	public class DataHolderTableMapping
	{
		private static readonly TableComparer DefaultTableComparer = new TableComparer();

		public DataHolderDefinition[] DataHolderDefinitions;
		
		/// <summary>
		/// All Tables that are used by all <see cref="DataHolderDefinitions"/>.
		/// It is ensured that all default-tables have a lower index than non-default ones.
		/// </summary>
		public TableDefinition[] TableDefinitions;

		public DataHolderTableMapping(DataHolderDefinition[] dataHolderDefs, TableDefinition[] tableDefinitions)
		{
			DataHolderDefinitions = dataHolderDefs;
			TableDefinitions = tableDefinitions;
			Array.Sort(TableDefinitions, DefaultTableComparer);
		}
		
		public DataHolderDefinition GetDataHolderDefinition(Type t)
		{
			foreach (var def in DataHolderDefinitions)
			{
				if (def.Type == t)
				{
					return def;
				}
			}
			return null;
		}

		public override string ToString()
		{
			return string.Format("Mapping of DataHolders ({0}) to Tables ({1})",
				DataHolderDefinitions.ToString(", "),
				TableDefinitions.ToString(", "));
		}

		public class TableComparer : IComparer<TableDefinition>
		{
			public int Compare(TableDefinition x, TableDefinition y)
			{
				return x.IsDefaultTable == y.IsDefaultTable ? 0 :
					(x.IsDefaultTable ? -1 : 1);
			}
		}
	}
}