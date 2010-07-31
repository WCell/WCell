using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WCell.Util.Conversion;
using WCell.Util.Data;
using WCell.Util.DB.Xml;

namespace WCell.Util.DB
{
	/// <summary>
	/// Static container and utility class for Table- and DataHolder-mappings
	/// </summary>
	public static class LightDBMgr
	{
		public delegate void DataFieldHandler(LightDBDefinitionSet defs, IDataField dataField, IDataFieldDefinition fieldDef,
			Dictionary<string, List<SimpleDataColumn>> mappedFields);

		public delegate DataFieldDefinition DataFieldCreator(IDataField field);

		public static readonly DataFieldHandler[] DataFieldHandlers = new DataFieldHandler[(int)DataFieldType.Count];
		public static readonly DataFieldCreator[] DataFieldCreators = new DataFieldCreator[(int)DataFieldType.Count];

		static LightDBMgr()
		{
			DataFieldHandlers[(int)DataFieldType.FlatSimple] = MapFlatSimple;
			DataFieldHandlers[(int)DataFieldType.NestedSimple] = MapNestedSimple;
			DataFieldHandlers[(int)DataFieldType.FlatArray] = MapFlatArray;
			DataFieldHandlers[(int)DataFieldType.NestedArray] = MapNestedArray;

			DataFieldCreators[(int)DataFieldType.FlatSimple] = CreateFlatSimple;
			DataFieldCreators[(int)DataFieldType.NestedSimple] = CreatedNestedSimple;
			DataFieldCreators[(int)DataFieldType.FlatArray] = CreateFlatArray;
			DataFieldCreators[(int)DataFieldType.NestedArray] = CreateNestedArray;
		}

		#region Default Mappers
		public static void MapFlatSimple(this LightDBDefinitionSet defs, IDataField dataField, IDataFieldDefinition fieldDef,
			Dictionary<string, List<SimpleDataColumn>> mappedFields)
		{
			var tableField = ((SimpleFlatFieldDefinition)fieldDef);
			AddMapping(defs, defs.GetDefaultTables(dataField.DataHolderDefinition), tableField, mappedFields,
				(FlatSimpleDataField)dataField, dataField.MappedMember);
		}

		public static void MapNestedSimple(this LightDBDefinitionSet defs, IDataField dataField, IDataFieldDefinition fieldDef,
			Dictionary<string, List<SimpleDataColumn>> mappedFields)
		{
			var nestedFieldDefinition = ((INestedFieldDefinition)fieldDef);
			nestedFieldDefinition.EnsureFieldsNotNull(dataField.DataHolderDefinition.Name);
			defs.AddFieldMappings(nestedFieldDefinition.Fields, ((NestedSimpleDataField)dataField).InnerFields, mappedFields);
		}

		public static void MapFlatArray(this LightDBDefinitionSet defs, IDataField dataField, IDataFieldDefinition fieldDef,
			Dictionary<string, List<SimpleDataColumn>> mappedFields)
		{
			var arrFieldDef = (FlatArrayFieldDefinition)fieldDef;
			var arrDataField = (FlatArrayDataField)dataField;

			var defaultTables = defs.EnsureTables(arrFieldDef.Table, dataField.DataHolderDefinition);
			var cols = arrFieldDef.GetColumns(arrDataField.Length);
			var accessors = arrDataField.ArrayAccessors;
			for (var i = 0; i < cols.Length; i++)
			{
				var col = cols[i];
				AddMapping(defs, defaultTables, col, mappedFields, (IFlatDataFieldAccessor)accessors[i], dataField.MappedMember);
			}
		}

		public static void MapNestedArray(this LightDBDefinitionSet defs, IDataField dataField, IDataFieldDefinition fieldDef,
			Dictionary<string, List<SimpleDataColumn>> mappedFields)
		{
			var arrField = (NestedArrayFieldDefinition)fieldDef;
			var arrDataField = (NestedArrayDataField)dataField;

			var defaultTables = defs.EnsureTables(arrField.Table, dataField.DataHolderDefinition);
			var segments = arrField.Segments;

			for (var i = 0; i < segments.Length; i++)
			{
				var segment = segments[i];
				var cols = segment.GetColumns(arrDataField.Length);

				for (var j = 0; j < cols.Length; j++)
				{
					var col = cols[j];
					var arrAccessor = (NestedArrayAccessor)arrDataField.ArrayAccessors[j];

					IDataField fieldAccessor;
					if (!arrAccessor.InnerFields.TryGetValue(segment.Name, out fieldAccessor))
					{
						throw new DataHolderException("NestedArray definition {0} refered to non-existing field {1}", arrDataField,
													  segment);
					}

					AddMapping(defs, defaultTables, col, mappedFields,
						(IFlatDataFieldAccessor)fieldAccessor, fieldAccessor.MappedMember);
				}
			}
		}

		static void AddMapping(LightDBDefinitionSet defs, TableDefinition[] defaultTables, SimpleFlatFieldDefinition fieldDef,
			Dictionary<string, List<SimpleDataColumn>> mappedFields, IFlatDataFieldAccessor accessor, MemberInfo member)
		{
			var column = fieldDef.Column;

			var tables = defs.EnsureTables(fieldDef.Table, defaultTables);
			object defaultValue;
			if (!String.IsNullOrEmpty(fieldDef.DefaultStringValue))
			{
				defaultValue = Utility.Parse(fieldDef.DefaultStringValue, member.GetVariableType());
			}
			else
			{
				if (String.IsNullOrEmpty(column))
				{
					return;
				}
				defaultValue = null;
			}

			if (accessor.DataHolderDefinition.Type.Name.EndsWith("TrainerEntry"))
			{
				defs.ToString();
			}

			foreach (var table in tables)
			{
				var dataHolders = defs.m_tableDataHolderMap.GetOrCreate(table);
				if (!dataHolders.Contains(accessor.DataHolderDefinition))
				{
					dataHolders.Add(accessor.DataHolderDefinition);
				}

				var mappedFieldMap = mappedFields.GetOrCreate(table.Name);

				SimpleDataColumn dataColumn;

				if (String.IsNullOrEmpty(column))
				{
					// use default value
					mappedFieldMap.Add(dataColumn = new SimpleDataColumn(fieldDef.Name, defaultValue));
				}
				else
				{
					dataColumn = mappedFieldMap.Find((cmpField) => cmpField.ColumnName == column);
					if (dataColumn == null)
					{
						mappedFieldMap.Add(dataColumn = new SimpleDataColumn(column, Converters.GetReader(member.GetActualType())));
					}
				}

				dataColumn.FieldList.Add(accessor);
			}
		}
		#endregion

		public static void EnsureFieldsNotNull(this IHasDataFieldDefinitions container)
		{
			if (container.Fields == null)
			{
				throw new DataHolderException(container.GetType().Name + " \"{0}\" did not define any fields.", container);
			}
		}

		public static void EnsureFieldsNotNull(this IHasDataFieldDefinitions field, string dataHolderName)
		{
			if (field.Fields == null)
			{
				throw new DataHolderException("Field \"{0}\" of DataHolder \"{1}\" did not define any fields.", field, dataHolderName);
			}
		}

		#region Creators
		private static DataFieldDefinition CreateNestedArray(IDataField dataField)
		{
			var arrField = (NestedArrayDataField)dataField;
			var innerFields = ((NestedArrayAccessor[])arrField.ArrayAccessors)[0].InnerFields;
			var segments = new FlatArrayFieldDefinition[innerFields.Count];
			int i = 0;
			foreach (var innerField in innerFields.Values)
			{
				var segment = new FlatArrayFieldDefinition {
					Name = innerField.Name,
					Offset = 1,
					Pattern = ""
				};
				segments[i++] = segment;
			}
			return new NestedArrayFieldDefinition {
				Name = dataField.Name,
				Segments = segments
			};
		}

		private static DataFieldDefinition CreateFlatArray(IDataField dataField)
		{
			return new FlatArrayFieldDefinition {
				Name = dataField.Name,
				Offset = 1,
				Pattern = ""
			};
		}

		private static DataFieldDefinition CreatedNestedSimple(IDataField dataField)
		{
			var nestedField = (NestedSimpleDataField)dataField;
			var fields = new DataFieldDefinition[nestedField.InnerFields.Count];
			int i = 0;
			foreach (var innerField in nestedField.InnerFields.Values)
			{
				fields[i++] = DataFieldCreators[(int)innerField.DataFieldType](innerField);
			}

			return new NestedSimpleFieldDefinition {
				Name = dataField.Name,
				Fields = fields
			};
		}

		private static DataFieldDefinition CreateFlatSimple(IDataField dataField)
		{
			return new SimpleFlatFieldDefinition {
				Name = dataField.Name,
				Column = ""
			};
		}
		#endregion

		#region XML
		public static void SaveAllStubs(string dir, IEnumerable<DataHolderDefinition> dataHolderDefs)
		{
			foreach (var def in dataHolderDefs)
			{
				SaveDefinitionStub(Path.Combine(dir, def.Name + ".xml"), def);
			}
		}

		public static void SaveDefinitionStub(string file, DataHolderDefinition dataHolderDef)
		{
			var def = new XmlDataHolderDefinition {
				Name = dataHolderDef.Name,
				DefaultTables = new[] { " " },
				Fields = new DataFieldDefinition[dataHolderDef.Fields.Count]
			};

			int i = 0;
			foreach (var field in dataHolderDef.Fields.Values)
			{
				def.Fields[i++] = DataFieldCreators[(int)field.DataFieldType](field);
			}

			var cfg = new LightRecordXmlConfig {
				FileName = file,
				DataHolders = new[] { def }
			};
			cfg.Save();
		}

		#endregion

		public static Action<string> InvalidDataHandler;

		public static void OnInvalidData(string msg, params object[] args)
		{
			msg = String.Format(msg, args);
			if (InvalidDataHandler != null)
			{
				InvalidDataHandler(msg);
			}
			else
			{
				throw new DataHolderException(msg);
			}
		}
	}
}