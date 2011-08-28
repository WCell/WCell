using System.Collections.Generic;
using System.Text;
using WCell.Util.DB;
using DialectType = NHibernate.Dialect.Dialect;

namespace WCell.Core.Database
{
	public class SqlUtil
	{
		public static string BuildSelect(string[] columns, string from)
		{
			return BuildSelect(columns, from, null);
		}

		public static string BuildSelect(string[] columns, string from, string suffix)
		{
			var dlct = DatabaseUtil.Dialect;
			

			var sb = new StringBuilder();
			sb.Append("SELECT ");
			for (int i = 0; i < columns.Length; i++)
			{
				var column = columns[i];
				sb.Append(dlct.QuoteForColumnName(column));
				if (i < columns.Length - 1)
				{
					sb.Append(",");
				}
			}
			sb.Append(" FROM ");
			sb.Append(from);

			if (suffix != null)
			{
				sb.Append(" " + suffix);
			}

			return sb.ToString();
		}

		public static string BuildInsert(KeyValueListBase liste)
		{
			var pairs = liste.Pairs;
			var count = pairs.Count;

			// create the basic statement
			var sb = PrepareInsertBuilder(liste);

			// append values
			sb.Append("(");
			for (var i = 0; i < count; i++)
			{
				var pair = pairs[i];
				sb.Append(GetValueString(pair.Value));
				if (i < count - 1)
				{
					sb.Append(",");
				}
			}
			sb.Append(")");

			return sb.ToString();
		}

		public static string BuildUpdate(UpdateKeyValueList list)
		{
			return BuildUpdate(list, BuildWhere(list.Where));
		}

		public static string BuildUpdate(KeyValueListBase liste, string where)
		{
			var sb = new StringBuilder();
			sb.Append("UPDATE " + liste.TableName + " SET ");
			AppendKeyValuePairs(sb, liste.Pairs, ", ");
			sb.Append(" WHERE ");
			sb.Append(where);

			return sb.ToString();
		}

		public static string BuildDelete(string table, string where)
		{
			var sb = new StringBuilder();
			sb.Append("DELETE * FROM ");
			sb.Append(table);
			sb.Append(" WHERE ");
			sb.Append(where);

			return sb.ToString();
		}

		public static string BuildWhere(List<KeyValuePair<string, object>> pairs)
		{
			var sb = new StringBuilder();
			AppendKeyValuePairs(sb, pairs, " AND ");
			return sb.ToString();
		}

		public static void AppendKeyValuePairs(StringBuilder sb, List<KeyValuePair<string, object>> pairs, string connector)
		{
			for (var i = 0; i < pairs.Count; i++)
			{
				var pair = pairs[i];
				sb.Append(DatabaseUtil.Dialect.QuoteForColumnName(pair.Key) + " = " + GetValueString(pair.Value));
				if (i < pairs.Count - 1)
				{
					sb.Append(connector);
				}
			}
		}

		public static string GetValueString(object obj)
		{
			//if (obj.GetType() == typeof(string))
			{
				return DatabaseUtil.Dialect.OpenQuote.ToString() + obj + DatabaseUtil.Dialect.CloseQuote;
			}
			//return obj.ToString();
		}

		private static StringBuilder PrepareInsertBuilder(KeyValueListBase liste)
		{
			var pairs = liste.Pairs;
			var count = pairs.Count;

			var sb = new StringBuilder(150);
			sb.Append("INSERT INTO " + DatabaseUtil.Dialect.QuoteForTableName(liste.TableName) + " (");
			for (var i = 0; i < count; i++)
			{
				var pair = pairs[i];
				sb.Append(DatabaseUtil.Dialect.QuoteForColumnName(pair.Key));
				if (i < count - 1)
				{
					sb.Append(",");
				}
			}
			sb.Append(") VALUES ");
			return sb;
		}
	}
}