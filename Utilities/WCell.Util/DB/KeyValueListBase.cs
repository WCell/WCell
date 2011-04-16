using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.DB
{
	public class KeyValueListBase
	{
		public readonly TableDefinition Table;

		public readonly List<KeyValuePair<string, object>> Pairs;
		public KeyValueListBase(TableDefinition table)
		{
			Table = table;
			Pairs = new List<KeyValuePair<string, object>>();
		}

		public KeyValueListBase(TableDefinition table, List<KeyValuePair<string, object>> pairs)
			: this(table)
		{
			Pairs = pairs;
		}

		public string TableName
		{
			get { return Table.Name; }
		}

		public void AddPair(string key, object value)
		{
			Pairs.Add(new KeyValuePair<string, object>(key, value));
		}
	}

	public class UpdateKeyValueList : KeyValueListBase
	{
		public readonly List<KeyValuePair<string, object>> Where;

		public UpdateKeyValueList(TableDefinition table)
			: base(table)
		{
			Where = new List<KeyValuePair<string, object>>();
		}

		public UpdateKeyValueList(TableDefinition table, List<KeyValuePair<string, object>> where)
			: this(table)
		{
			Where = where;
		}
	}
}