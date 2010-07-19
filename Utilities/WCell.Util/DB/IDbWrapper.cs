using System.Collections.Generic;
using System.Data;
using WCell.Util.Data;

namespace WCell.Util.DB
{
	public interface IDbWrapper
	{
		void Prepare(LightDBMapper mapper);

		IDataReader CreateReader(TableDefinition def, int tableIndex);

		/// <summary>
		/// Queries the Database with the given query and returns the
		/// corresponding reader to read the results from
		/// </summary>
		IDataReader Query(string query);

		/// <summary>
		/// Insert new DataHolder into Database
		/// </summary>
		void Insert(KeyValueListBase list);

		/// <summary>
		/// Update DataHolder in Database
		/// </summary>
		void Update(UpdateKeyValueList list);

		void Delete(KeyValueListBase list);
	}
}