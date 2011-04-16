using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Util.Data;
using System.IO;
using System.Text;

namespace WCell.Util.DB
{
	public class LightDBMapper
	{
		public const int CacheVersion = 5;

		private DataHolderTableMapping m_mapping;
		private IDbWrapper m_db;
		readonly Dictionary<Type, Dictionary<object, IDataHolder>> dataHolderMap =
			new Dictionary<Type, Dictionary<object, IDataHolder>>();
		private bool m_fetched;
		private readonly List<IDataHolder> m_toUpdate = new List<IDataHolder>();
		private readonly List<IDataHolder> m_toInsert = new List<IDataHolder>();
		private readonly List<IDataHolder> m_toDelete = new List<IDataHolder>();

		public LightDBMapper(DataHolderTableMapping mapping, IDbWrapper db)
		{
			m_mapping = mapping;
			m_db = db;
			db.Prepare(this);
		}

		public DataHolderTableMapping Mapping
		{
			get { return m_mapping; }
		}

		public IDbWrapper Wrapper
		{
			get { return m_db; }
		}

		/// <summary>
		/// Whether this Mapper has already fetched its contents
		/// </summary>
		public bool Fetched
		{
			get { return m_fetched; }
		}

		#region Created Objects
		public Dictionary<object, IDataHolder> GetObjectMap<T>() where T : IDataHolder
		{
			return dataHolderMap.GetOrCreate(typeof(T));
		}

		public void AddObject<T>(object id, T obj) where T : IDataHolder
		{
			var map = dataHolderMap.GetOrCreate(typeof(T));
			map.Add(id, obj);
		}

		/// <summary>
		/// Adds an Array of DataHolders where the index (int)
		/// within the Array is the key.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		public void AddObjectsInt<T>(T[] objs)
			where T : class, IDataHolder
		{
			var map = dataHolderMap.GetOrCreate(typeof(T));
			for (var i = 0; i < objs.Length; i++)
			{
				var obj = objs[i];
				if (obj != null)
				{
					map.Add(i, obj);
				}
			}
		}

		/// <summary>
		/// Adds an Array of DataHolders where the index (uint)
		/// within the Array is the key.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		public void AddObjectsUInt<T>(T[] objs)
			where T : class, IDataHolder
		{
			var map = dataHolderMap.GetOrCreate(typeof(T));
			for (var i = 0; i < objs.Length; i++)
			{
				var obj = objs[i];
				if (obj != null)
				{
					map.Add((uint)i, obj);
				}
			}
		}
		#endregion

		#region Read
		/// <summary>
		/// Fetches all sets of objects, defined through the given mapping.
		/// </summary>
		/// <returns></returns>
		public void Fetch()
		{
			// Map of objects, mapped by DataHolder-Type, DataHolder-Id and field-Name
			//var objects = new Dictionary<Type, Dictionary<object, Dictionary<string, object>>>();
			var actions = new List<Action>(10000);
			var tables = m_mapping.TableDefinitions;
			DataHolderDefinition holderDef = null;
			for (var t = 0; t < tables.Length; t++)
			{
				var table = tables[t];
				try
				{
					using (var rs = m_db.CreateReader(table, t))
					{
						var mappedFields = table.ColumnDefinitions;
						while (rs.Read())
						{
							var id = table.GetId(rs);

							DataHolderDefinition lastDataHolderDef = null;
							IDataHolder lastDataHolder = null;
							object value = null;
							SimpleDataColumn column = null;
							int columnN;
							try
							{
								for (columnN = 0; columnN < mappedFields.Length; columnN++)
								{
									column = mappedFields[columnN];
									value = null;
									value = column.ReadValue(rs);
									for (var i = 0; i < column.FieldList.Count; i++)
									{
										var dataField = column.FieldList[i];
										holderDef = dataField.DataHolderDefinition;
										if (holderDef == null)
										{
											throw new DataHolderException(
												"Invalid DataField did not have a DataHolderDefinition: " +
												dataField);
										}

										IDataHolder holder;
										if (lastDataHolderDef != dataField.DataHolderDefinition || lastDataHolder == null)
										{
											// only get the DataHolder, if its different from the last one
											var dataHolders = dataHolderMap.GetOrCreate(holderDef.Type);
											if (!dataHolders.TryGetValue(id, out holder))
											{
												// create new DataHolder object if this is one of the DataHolder's default tables
												if (!table.IsDefaultTable)
												{
													// ignore DataHolders that are not defined in their DefaultTables
													LightDBMgr.OnInvalidData(
														"Invalid DataHolder was not defined in its Default-Tables - " +
														"DataHolder: {0}; Id(s): {1}; Table: {2}", holderDef, id, table);
													// little trick to skip the parent loop
													columnN = mappedFields.Length;
													break;
												}

												// init with Id(s)
												dataHolders.Add(id, holder = (IDataHolder)holderDef.CreateHolder(value));
												//table.SetDefaults(id, holder);
												actions.Add(holder.FinalizeDataHolder);
											}

											lastDataHolder = holder;
											lastDataHolderDef = holderDef;
										}
										else
										{
											holder = lastDataHolder;
										}

										// set value
										dataField.Set(holder, value);
									}
								}
							}
							catch (Exception e)
							{
								var ids = (id is Array ? ((object[])id) : new[] { id });
								throw new DataHolderException(e, "Unable to parse data-cell (Column: {0}, Id(s): {1}{2})", column,
									ids.ToString(", ", (idObj) => {
										return idObj.GetType().IsEnum ? Convert.ChangeType(idObj, Enum.GetUnderlyingType(idObj.GetType())) : idObj;
									}), value != null ? (", Value: " + value) : "");
							}
						}
					}
				}
				catch (Exception e)
				{
					throw new DataHolderException(e, "Failed to read from Table \"{0}\" {1}", table,
						holderDef != null ? ("DataHolder: " + holderDef) : "");
				}
			}

			for (var i = 0; i < actions.Count; i++)
			{
				var action = actions[i];
				action();
			}

			dataHolderMap.Clear();
			m_fetched = true;
		}
		#endregion

		#region Insert + Update + Delete
		public List<UpdateKeyValueList> GetUpdateList(IDataHolder obj)
		{
			return GetKeyValuePairs(obj, table => new UpdateKeyValueList(table, GetWherePairs(table, obj)));
		}

		public List<KeyValueListBase> GetInsertList(IDataHolder obj)
		{
			return GetKeyValuePairs(obj, table => new KeyValueListBase(table));
		}

		List<T> GetKeyValuePairs<T>(IDataHolder obj, Func<TableDefinition, T> listCreator)
			where T: KeyValueListBase
		{
			var dataHolderDef = m_mapping.GetDataHolderDefinition(obj.GetType());
			var lists = new List<T>(m_mapping.TableDefinitions.Length);
			for (var i = 0; i < m_mapping.TableDefinitions.Length; i++)
			{
				var table = m_mapping.TableDefinitions[i];
				for (var j = 0; j < table.ColumnDefinitions.Length; j++)
				{
					var col = table.ColumnDefinitions[j];
					for (var k = 0; k < col.FieldList.Count; k++)
					{
						var field = col.FieldList[k];
						var def = field.DataHolderDefinition;
						if (def == dataHolderDef)
						{
							var list = lists.FirstOrDefault(l => l.TableName == table.Name);
							if (list == null)
							{
								lists.Add(list = listCreator(table));
							}

							var value = field.Get(obj);
							list.AddPair(col.ColumnName, value.ToString());
						}
					}
				}
			}
			return lists;
		}

		/// <summary>
		/// Marks this Object to be updated upon next flush
		/// </summary>
		public void Update(IDataHolder obj)
		{
			lock (m_toUpdate)
			{
				m_toUpdate.Add(obj);
			}
		}

		/// <summary>
		/// Marks this Object to be inserted upon next flush
		/// </summary>
		public void Insert(IDataHolder obj)
		{
			lock (m_toInsert)
			{
				m_toInsert.Add(obj);
			}
		}

		/// <summary>
		/// Marks this Object to be deleted upon next flush
		/// </summary>
		public void Delete(IDataHolder obj)
		{
			lock (m_toDelete)
			{
				m_toDelete.Add(obj);
			}
		}

		/// <summary>
		/// Ignores all changes that have not been commited yet.
		/// </summary>
		public void IgnoreUnflushedChanges()
		{
			lock (m_toUpdate)
			{
				m_toUpdate.Clear();
			}

			lock (m_toInsert)
			{
				m_toInsert.Clear();
			}

			lock (m_toDelete)
			{
				m_toDelete.Clear();
			}
		}

		/// <summary>
		/// Commits all inserts and updates to the underlying Database
		/// </summary>
		public void Flush()
		{
			Flush(m_toUpdate, obj =>
				GetUpdateList(obj).ForEach(list => m_db.Update(list)));

			Flush(m_toInsert, obj => GetInsertList(obj).ForEach(m_db.Insert));

			Flush(m_toDelete, obj => GetWherePairs(obj).ForEach(m_db.Delete));
		}

		void Flush(List<IDataHolder> list, Action<IDataHolder> callback)
		{
			lock (list)
			{
				var i = 0;
				try
				{
					for (; i < list.Count; i++)
					{
						var obj = list[i];
						callback(obj);
					}
				}
				finally
				{
					if (i < list.Count)
					{
						list.RemoveRange(0, i);
					}
					else
					{
						list.Clear();
					}
				}
			}
		}

		private List<KeyValueListBase> GetWherePairs(IDataHolder obj)
		{
			var type = obj.GetType();
			var lists = new List<KeyValueListBase>(3);
			for (var i = 0; i < m_mapping.TableDefinitions.Length; i++)
			{
				var table = m_mapping.TableDefinitions[i];
				if (table.DataHolderDefinitions.Contains(def => def.Type == type))
				{
					lists.Add(new KeyValueListBase(table, GetWherePairs(table, obj)));
				}
			}
			return lists;
		}

		private List<KeyValuePair<string, object>> GetWherePairs(TableDefinition table, IDataHolder obj)
		{
			var dataHolderDef = m_mapping.GetDataHolderDefinition(obj.GetType());
			var pairs = new List<KeyValuePair<string, object>>(2);
			foreach (var col in table.PrimaryColumns)
			{
				foreach (var field in col.DataColumn.FieldList)
				{
					var def = field.DataHolderDefinition;
					if (def == dataHolderDef)
					{
						var value = field.Get(obj);
						pairs.Add(new KeyValuePair<string, object>(col.Name, value.ToString()));
					}
				}
			}
			return pairs;
		}

		#endregion

		#region Caching
		public bool SupportsCaching
		{
			get
			{
				for (var i = 0; i < m_mapping.DataHolderDefinitions.Length; i++)
				{
					var def = m_mapping.DataHolderDefinitions[i];
					if (!def.SupportsCaching)
					{
						return false;
					}
				}
				return true;
			}
		}

		public void SaveCache(string filename)
		{
			using (var writer = new BinaryWriter(
				new FileStream(filename, FileMode.Create, FileAccess.Write)))
			{
				WriteHeader(writer);

				for (var i = 0; i < m_mapping.DataHolderDefinitions.Length; i++)
				{
					var def = m_mapping.DataHolderDefinitions[i];
					var holders = def.CacheGetter.Invoke(null, new object[0]);
					var contentStream = new BinaryContentStream(def);
					contentStream.WriteAll(writer, (IEnumerable)holders);
				}
			}
		}

		public bool LoadCache(string filename)
		{
			using (var reader = new BinaryReader(
				new FileStream(filename, FileMode.Open, FileAccess.Read)))
			{
				if (!ReadCacheHeader(reader))
				{
					return false;
				}

				var initors = new List<Action>((int)reader.BaseStream.Length / 1000);

				for (var i = 0; i < m_mapping.DataHolderDefinitions.Length; i++)
				{
					var def = m_mapping.DataHolderDefinitions[i];
					var contentStream = new BinaryContentStream(def);
					contentStream.LoadAll(reader, initors);
				}

				if (initors.Count == 0 || reader.BaseStream.Position != reader.BaseStream.Length)
				{
					// empty cache file or invalid cache size -> Consider it a fail
					return false;
				}

				foreach (var initor in initors)
				{
					initor();
				}
			}
			return true;
		}

		internal void WriteHeader(BinaryWriter writer)
		{
			writer.Write(CacheVersion);
			writer.Write(m_mapping.DataHolderDefinitions.Length);

			for (var i = 0; i < m_mapping.DataHolderDefinitions.Length; i++)
			{
				var def = m_mapping.DataHolderDefinitions[i];
				var bytes = Encoding.UTF8.GetBytes(def.CreateIdString());
				writer.Write((ushort)bytes.Length);
				writer.Write(bytes);
			}
		}

		/// <summary>
		/// Reads the (semi-)unique signature of all DataHolders to prevent the worst
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		internal bool ReadCacheHeader(BinaryReader reader)
		{
			if (reader.ReadInt32() != CacheVersion)
			{
				return false;
			}

			if (reader.BaseStream.Position == reader.BaseStream.Length)
			{
				return false;
			}

			var defCount = reader.ReadInt32();
			if (defCount != m_mapping.DataHolderDefinitions.Length)
			{
				return false;
			}

			for (var i = 0; i < m_mapping.DataHolderDefinitions.Length; i++)
			{
				var def = m_mapping.DataHolderDefinitions[i];
				var curId = def.CreateIdString();
				var idLen = reader.ReadUInt16();
				var idBytes = reader.ReadBytes(idLen);
				var idStr = Encoding.UTF8.GetString(idBytes);

				if (curId != idStr)
				{
					return false;
				}
			}
			return true;
		}
		#endregion

		public override string ToString()
		{
			return "Mapper for: " + m_mapping.DataHolderDefinitions.TransformArray(def => def.Name).ToString(", ");
		}
	}
}