using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using WCell.Util.Data;
using WCell.Util.DB.Xml;
using WCell.Util.Conversion;
using WCell.Util.Variables;
using System.Diagnostics;

namespace WCell.Util.DB
{
	public class LightDBDefinitionSet
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public readonly DataHolderDefinition[] DataHolderDefinitions;

		public readonly Dictionary<Type, DataHolderDefinition> DataHolderDefinitionMap =
			new Dictionary<Type, DataHolderDefinition>();
		public readonly Dictionary<string, TableDefinition> TableDefinitionMap = new Dictionary<string, TableDefinition>();

		/// <summary>
		/// DefaultTables are the tables that contain the core data of each DataHolder.
		/// It is ensured that a DataHolder is only valid if it exists in all its DefaultTables.
		/// </summary>
		public readonly Dictionary<string, TableDefinition[]> DefaultTables = new Dictionary<string, TableDefinition[]>();

		internal Dictionary<TableDefinition, List<DataHolderDefinition>> m_tableDataHolderMap =
			new Dictionary<TableDefinition, List<DataHolderDefinition>>();

		private DataHolderTableMapping[] m_mappings;

		private DefVersion m_DbVersionLocation;

		public LightDBDefinitionSet(DataHolderDefinition[] dataHolderDefinitions)
		{
			DataHolderDefinitions = dataHolderDefinitions;
			foreach (var def in dataHolderDefinitions)
			{
				DataHolderDefinitionMap.Add(def.Type, def);
			}
		}

		#region Properties
		public DataHolderTableMapping[] Mappings
		{
			get { return m_mappings; }
		}

		public DefVersion DBVersionLocation
		{
			get { return m_DbVersionLocation; }
		}
		#endregion

		public void Clear()
		{
			if (m_mappings != null)
			{
				m_mappings = null;
				TableDefinitionMap.Clear();
				DefaultTables.Clear();
				m_tableDataHolderMap.Clear();
			}
		}

		public DataHolderDefinition GetDefinition(Type t)
		{
			DataHolderDefinition def;
			DataHolderDefinitionMap.TryGetValue(t, out def);
			return def;
		}

		#region Get and Ensure Tables
		public TableDefinition[] GetDefaultTables(DataHolderDefinition def)
		{
			TableDefinition[] tables;
			DefaultTables.TryGetValue(def.Name, out tables);
			return tables;
		}

		public TableDefinition[] GetDefaultTables(string dataHolderName)
		{
			TableDefinition[] tables;
			DefaultTables.TryGetValue(dataHolderName, out tables);
			return tables;
		}

		public TableDefinition GetTable(string tableName)
		{
			TableDefinition table;
			TableDefinitionMap.TryGetValue(tableName, out table);
			return table;
		}

		public TableDefinition[] EnsureTables(string tableName, DataHolderDefinition def)
		{
			return EnsureTables(tableName, GetDefaultTables(def));
		}

		public TableDefinition[] EnsureTables(string tableName, TableDefinition[] defaultTables)
		{
			TableDefinition[] tables;
			if (defaultTables.Length == 0)
			{
				tables = new TableDefinition[1];
				tables[0] = EnsureTable(tableName);
			}
			else
			{
				tables = new TableDefinition[defaultTables.Length];
			}
			for (var i = 0; i < defaultTables.Length; i++)
			{
				tables[i] = EnsureTable(tableName, defaultTables[i]);
			}
			return tables;
		}

		public TableDefinition EnsureTable(string tableName, TableDefinition defaultTable)
		{
			TableDefinition table;
			if (tableName != null)
			{
				table = EnsureTable(tableName);
			}
			else
			{
				table = defaultTable;
			}
			return table;
		}

		private TableDefinition EnsureTable(string name)
		{
			TableDefinition table;
			if (!TableDefinitionMap.TryGetValue(name, out table))
			{
				throw new DataHolderException(
					"Invalid DataHolder-definition refers to undefined Table \"{0}\" (use the <Table> node to define it in the Table.xml file): " + name, name);
			}
			return table;
		}
		#endregion

		#region Loading

		public void LoadTableDefinitions(string file)
		{
			var cfg = BasicTableDefinitions.Load(file);

			m_DbVersionLocation = cfg.DBVersion;

			foreach (var tableDef in cfg.Tables)
			{
				if (TableDefinitionMap.ContainsKey(tableDef.Name))
				{
					throw new DataHolderException("Duplicate Table definition \"{0}\" in File {1}", tableDef.Name, file);
				}

				var arrayConstraints = new Dictionary<string, ArrayConstraint>();
				if (tableDef.ArrayConstraints != null)
				{
					foreach (var arrayConstraint in tableDef.ArrayConstraints)
					{
						arrayConstraints.Add(arrayConstraint.Column, arrayConstraint);
					}
				}

				var primaryColumns = tableDef.PrimaryColumns;
				if (primaryColumns == null || primaryColumns.Length == 0)
				{
					throw new DataHolderException("TableDefinition did not define any PrimaryColumns: " + tableDef);
				}

				var table = new TableDefinition(tableDef.Name, primaryColumns, arrayConstraints,
					tableDef.Variables ?? VariableDefinition.EmptyArray)
					{
						MainDataHolderName = tableDef.MainDataHolder
					};

				TableDefinitionMap.Add(tableDef.Name, table);
			}
		}

		public void LoadDataHolderDefinitions(string dir)
		{
			LoadDataHolderDefinitions(new DirectoryInfo(dir));
		}

		/// <summary>
		/// Make sure to call <see cref="LoadTableDefinitions"/> prior to this.
		/// </summary>
		/// <param name="dir"></param>
		public void LoadDataHolderDefinitions(DirectoryInfo dir)
		{
			var fieldMap = new Dictionary<string, List<SimpleDataColumn>>();
			foreach (var cfg in LightRecordXmlConfig.LoadAll(dir))
			{
				RegisterDefintion(cfg, fieldMap);
			}

			FinishLoading(fieldMap);
		}

		private void FinishLoading(Dictionary<string, List<SimpleDataColumn>> fieldMap)
		{
			// bind mapping-objects to tables
			foreach (var pair in fieldMap)
			{
				var table = GetTable(pair.Key);
				var fields = pair.Value.ToArray();

				if (!string.IsNullOrEmpty(table.MainDataHolderName))
				{
					DataHolderDefinition defaultDef = DataHolderDefinitions.Where(dataHolderDef =>
						dataHolderDef.Name.Equals(table.MainDataHolderName,
						StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

					if (defaultDef == null)
					{
						throw new DataHolderException("Table \"{0}\" refered to invalid MainDataHolder: {1}", table,
													  table.MainDataHolderName);
					}
					table.SetMainDataHolder(defaultDef, false);
				}

				try
				{
					var primCols = new List<PrimaryColumn>();
					primCols.AddRange(table.PrimaryColumns);

					// find Primary Columns, set defaults and consider depending fields
					for (var i = 0; i < fields.Length; i++)
					{
						var field = fields[i];
						var col = primCols.Where(primCol => primCol.Name == field.ColumnName).FirstOrDefault();
						if (col != null)
						{
							col.DataColumn = field;
							field.IsPrimaryKey = true;
							primCols.Remove(col);
							//var tmp = fields[0];
							//fields[0] = field;
							//fields[i] = tmp;
						}

						if (field.ColumnName == null && field.DefaultValue == null)
						{
							LightDBMgr.OnInvalidData("Field-definition \"{0}\" did not define a Column nor a DefaultValue.", field);
						}
					}

					// put the depending Fields first (if any)
					for (var i = 0; i < fields.Length; i++)
					{
						var field = fields[i];
						for (var j = 0; j < field.FieldList.Count; j++)
						{
							var dataField = field.FieldList[j];
							var dataHolder = dataField.DataHolderDefinition;
							if (dataHolder.DependingField == dataField)
							{
								var tmp = fields[0];
								fields[0] = field;
								fields[i] = tmp;
							}
						}
					}

					//if (primCols.Count() > 0)
					//{
					//    throw new DataHolderException("Table-definition \"{0}\" refered to non-existing PrimaryKey(s): {1}",
					//        pair.Key, primCols.ToString(", "));
					//}

					if (primCols.Count > 0 && table.MainDataHolder == null)
					{
						throw new DataHolderException("Table \"{0}\" referenced PrimaryColumn(s) ({1}) but did not define a MainDataHolder explicitely.",
							table, primCols.ToString(", "));
					}

					// custom primary columns and variables
					if (primCols.Count > 0 || (table.Variables != null && table.Variables.Length > 0))
					{
						var varCount = table.Variables != null ? table.Variables.Length : 0;
						var extraFields = varCount + primCols.Count;
						var tmpArr = new SimpleDataColumn[fields.Length + extraFields];
						Array.Copy(fields, 0, tmpArr, extraFields, fields.Length);
						fields = tmpArr;

						if (varCount > 0)
						{
							InitVars(table, fields);
						}

						var i = varCount;
						foreach (var primaryCol in primCols)
						{
							// PrimaryKey-field is not explicitely mapped but only used as a reference
							var proxyField = new DataFieldProxy(primaryCol.Name, table.MainDataHolder);

							var reader = Converters.GetReader(primaryCol.TypeName);
							if (reader == null)
							{
								throw new DataHolderException("Invalid Type \"" + primaryCol.TypeName
															  + "\" for PrimaryColumn \"" + primaryCol.Name + "\" in definition for Table: " +
															  table);
							}
							var col = new SimpleDataColumn(primaryCol.Name, reader, 0) { IsPrimaryKey = true };
							col.FieldList.Add(proxyField);
							fields[i++] = primaryCol.DataColumn = col;
						}
					}

					var index = 0;
					for (var i = 0; i < fields.Length; i++)
					{
						var field = fields[i];

						if (field.DefaultValue == null)
						{
							field.m_index = index++;
						}
						else
						{
							field.Reader = null;
						}
					}

					TableDefinitionMap[pair.Key].ColumnDefinitions = fields;
				}
				catch (Exception ex)
				{
					throw new DataHolderException(ex, "Unable to setup Table \"{0}\" (MainDataHolder: \"{1}\")", table, table.MainDataHolder);
				}
			}

			m_mappings = CreateDataHolderTableMappings(m_tableDataHolderMap, DataHolderDefinitions);
		}

		private static void InitVars(TableDefinition table, SimpleDataColumn[] fields)
		{
			var i = 0;
			foreach (var variable in table.Variables)
			{
				IDataField field;
				if (!table.MainDataHolder.Fields.TryGetValue(variable.Name, out field) || !(field is IFlatDataFieldAccessor))
				{
					throw new DataHolderException("Table \"{0}\" defined invalid Variable {1}. Name does not refer to an actual property within DataHolder {2}.",
						table, variable, table.MainDataHolder);
				}

				try
				{
					var defaultValue = variable.Eval(field.MappedMember.GetVariableType());
					var fld = new SimpleDataColumn(variable.Name, defaultValue);
					fld.FieldList.Add((IFlatDataFieldAccessor)field);
					fields[i++] = fld;
				}
				catch (Exception e)
				{
					throw new DataHolderException(e, "Unable to parse default-value \"{0}\" to Type \"{1}\" from Variable \"{2}\"",
												  variable.StringValue, field.MappedMember.Name, variable.Name);
				}
			}
		}

		public static DataHolderTableMapping[] CreateDataHolderTableMappings(
			Dictionary<TableDefinition, List<DataHolderDefinition>> tableDataHolderMap,
			DataHolderDefinition[] dataHolderDefinitions
			)
		{
			// find exact mappings and split them up into as small parts as possible
			// this way we will only read the data from tables that is actually
			// mapped to DataHolders and we will read every cell only once

			var tablesByHolder = new Dictionary<DataHolderDefinition, List<TableDefinition>>();
			foreach (var pair in tableDataHolderMap)
			{
				if (pair.Value == null)
				{
					log.Warn("Table-definition \"{0}\" has no used columns (and can possibly be removed from the config).");
				}
				else
				{
					pair.Key.DataHolderDefinitions = pair.Value.ToArray();
					foreach (var dataHolder in pair.Key.DataHolderDefinitions)
					{
						tablesByHolder.GetOrCreate(dataHolder).Add(pair.Key);
					}
				}
			}

			var mappings = new List<DataHolderTableMapping>();
			var allDefs = new HashSet<DataHolderDefinition>(tablesByHolder.Keys.ToArray());

			var dataHolders = new HashSet<DataHolderDefinition>();
			var tables = new HashSet<TableDefinition>();
			foreach (var holder in tablesByHolder.Keys)
			{
				if (AddTables(allDefs, holder, tablesByHolder, dataHolders, tables))
				{
					var mapping = new DataHolderTableMapping(dataHolders.ToArray(), tables.ToArray());
					mappings.Add(mapping);
					dataHolders.Clear();
					tables.Clear();
				}
			}

			foreach (var table in tableDataHolderMap.Keys)
			{
				foreach (var field in table.ColumnDefinitions)
				{
					if (field is IDataFieldBase)
					{
						var holderDef = ((IDataFieldBase)field).DataHolderDefinition;
						if (!dataHolders.Contains(holderDef))
						{
							var mapping = mappings.Find(map => map.TableDefinitions.Contains(table));
							var tableHolders = mapping.DataHolderDefinitions;
							mapping.DataHolderDefinitions = new DataHolderDefinition[tableHolders.Length + 1];
							Array.Copy(tableHolders, mapping.TableDefinitions, tableHolders.Length);
							mapping.DataHolderDefinitions[tableHolders.Length] = holderDef;
						}
					}
				}
			}

			return mappings.ToArray();
		}

		static bool AddTables(ICollection<DataHolderDefinition> allDefs,
			DataHolderDefinition def,
			Dictionary<DataHolderDefinition, List<TableDefinition>> dataHolderToTable,
			HashSet<DataHolderDefinition> dataHolders,
			HashSet<TableDefinition> tables)
		{
			if (allDefs.Contains(def))
			{
				allDefs.Remove(def);
				dataHolders.Add(def);
				foreach (var table in dataHolderToTable[def])
				{
					tables.Add(table);
					foreach (var dataHolder in table.DataHolderDefinitions)
					{
						dataHolders.Add(dataHolder);
						AddTables(allDefs, dataHolder, dataHolderToTable, dataHolders, tables);
					}
				}
				return true;
			}
			return false;
		}

		#endregion

		#region Parse Mappings
		private void RegisterDefintion(LightRecordXmlConfig cfg, Dictionary<string, List<SimpleDataColumn>> fieldMap)
		{
			var dataHolders = DataHolderDefinitions;
			XmlDataHolderDefinition lastDef = null;
			foreach (XmlDataHolderDefinition dataRawDef in cfg)
			{
				try
				{
					if (dataRawDef.Name == null)
					{
						throw new DataHolderException("Invalid DataHolder-definition has no name ({0}).",
							lastDef == null ? "First in file" : "After: " + lastDef);
					}

					dataRawDef.EnsureFieldsNotNull();

					var dataDef = dataHolders.Where(def =>
						def.Name.Equals(dataRawDef.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

					if (dataDef == null)
					{
						LightDBMgr.OnInvalidData("Invalid DataHolder-definition refers to non-existing DataHolder: " + dataRawDef.Name);
						continue;
					}

					TableDefinition[] oldTables, defaultTables;
					// Set DefaultTable
					//if (dataRawDef.DefaultTables == null)
					//{
					//    throw new DataHolderException("DataHolder-definition did not specify any DefaultTable(s): " + dataRawDef);
					//}

					if (DefaultTables.TryGetValue(dataDef.Name, out defaultTables))
					{
						oldTables = defaultTables;
					}
					else
					{
						oldTables = null;
						if (dataRawDef.DefaultTables != null)
						{
							defaultTables = new TableDefinition[dataRawDef.DefaultTables.Length];
						}
						else
						{
							defaultTables = new TableDefinition[0];
						}
					}

					for (var i = 0; i < defaultTables.Length; i++)
					{
						var tableName = dataRawDef.DefaultTables[i].Trim();

						TableDefinition defaultTable;
						if (!TableDefinitionMap.TryGetValue(tableName, out defaultTable))
						{
							throw new DataHolderException("DefaultTable \"{0}\" of DataHolder \"{1}\" is not defined - " +
														  "Make sure to define the table in the Table collection.", tableName, dataRawDef);
						}
						//if (DefaultTables.ContainsKey(dataDef.Name))
						//{
						//    throw new DataHolderException("Found duplicate DataHolder-definition: " + dataDef.Name + " ({0})", cfg.FileName);
						//}

						defaultTables[i] = defaultTable;
						defaultTable.SetMainDataHolder(dataDef, true);
					}

					DefaultTables[dataDef.Name] = defaultTables;

					if (dataRawDef.DataHolderName.Contains("Trainer"))
					{
						ToString();
					}
					AddFieldMappings(dataRawDef.Fields, dataDef.Fields, fieldMap);
					if (oldTables != null)
					{
						var offset = oldTables.Length;
						Array.Resize(ref oldTables, oldTables.Length + defaultTables.Length);
						Array.Copy(defaultTables, 0, oldTables, offset, defaultTables.Length);
						DefaultTables[dataDef.Name] = oldTables;
					}
					lastDef = dataRawDef;
				}
				catch (Exception e)
				{
					throw new DataHolderException(e, "Failed to parse DataHolder-definition \"" + dataRawDef + "\" from {0}", cfg.FileName);
				}
			}
		}

		internal void AddFieldMappings(IEnumerable<IDataFieldDefinition> fieldDefs,
			IDictionary<string, IDataField> dataFields,
			Dictionary<string, List<SimpleDataColumn>> mappedFields)
		{
			foreach (var dataField in dataFields.Values)
			{
				AddFieldMapping(fieldDefs, dataField, mappedFields);
			}
		}

		internal void AddFieldMapping(IEnumerable<IDataFieldDefinition> fieldDefs,
			IDataField dataField, Dictionary<string, List<SimpleDataColumn>> mappedFields)
		{
			var fieldDef = fieldDefs.Where(def =>
				def.Name.Equals(dataField.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

			if (fieldDef == null)
			{
				var type = dataField.MappedMember.DeclaringType.Name;
				LightDBMgr.OnInvalidData("DataField \"" + dataField + "\" in Type " +
					type + " (DataHolder: {0}) is not mapped.", dataField.DataHolderDefinition);
				return;
			}
			else if (fieldDef.DataFieldType != dataField.DataFieldType)
			{
				var type = dataField.MappedMember.DeclaringType.FullName;
				throw new DataHolderException("DataField \"" + dataField + "\" in Type " + type + " is {0}, but was defined as: {1}",
					dataField.DataFieldType, fieldDef.DataFieldType);
			}

			LightDBMgr.DataFieldHandlers[(int)dataField.DataFieldType](this, dataField, fieldDef, mappedFields);
		}
		#endregion
	}
}
