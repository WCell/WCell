using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WCell.Util.Data
{
	/// <summary>
	/// Static container and utility class for DataHolder-information
	/// </summary>
	public static class DataHolderMgr
	{
		public static readonly Dictionary<string, DataHolderDefinition> DataHolderDefinitions =
			new Dictionary<string, DataHolderDefinition>(StringComparer.InvariantCultureIgnoreCase);


		public static void CreateAndStoreDataHolderDefinitions(Assembly asm)
		{
			foreach (var type in asm.GetTypes())
			{
				if (type.GetCustomAttributes<DataHolderAttribute>().Count() > 0)
				{
					var holder = CreateDataHolderDefinition(type);
					DataHolderDefinitions.Add(holder.Name, holder);
				}
			}
		}


		public static Dictionary<string, DataHolderDefinition> CreateDataHolderDefinitionMap(Assembly asm)
		{
			var dataHolderDefinitions = new Dictionary<string, DataHolderDefinition>();
			foreach (var type in asm.GetTypes())
			{
				if (type.GetCustomAttributes<DataHolderAttribute>().Count() > 0)
				{
					var holder = CreateDataHolderDefinition(type);
					dataHolderDefinitions.Add(holder.Name, holder);
				}
			}
			return dataHolderDefinitions;
		}


		public static DataHolderDefinition[] CreateDataHolderDefinitionArray(Assembly asm)
		{
			return CreateDataHolderDefinitionList(asm).ToArray();
		}


		public static List<DataHolderDefinition> CreateDataHolderDefinitionList(Assembly asm)
		{
			var dataHolderDefinitions = new List<DataHolderDefinition>();
			foreach (var type in asm.GetTypes())
			{
				if (type.GetCustomAttributes<DataHolderAttribute>().Count() > 0)
				{
					var holder = CreateDataHolderDefinition(type);
					dataHolderDefinitions.Add(holder);
				}
			}
			return dataHolderDefinitions;
		}

		public static DataHolderDefinition CreateDataHolderDefinition<T>() where T : IDataHolder
		{
			return CreateDataHolderDefinition(typeof(T));
		}

		public static DataHolderDefinition CreateDataHolderDefinition(Type type)
		{
			if (type.GetInterface("IDataHolder") == null)
			{
				throw new ArgumentException("DataHolder-Type must implement IDataHolder: " + type.FullName);
			}

			string name, decidingField;

			var attr = ((DataHolderAttribute[])type.GetCustomAttributes(typeof(DataHolderAttribute), false)).FirstOrDefault();

			if (attr == null)
			{
				decidingField = null;
				name = type.Name;
				//throw new DataHolderException("DataHolder \"{0}\" did not have required DataHolderAttribute.", type.FullName);
			}
			else
			{
				name = string.IsNullOrEmpty(attr.Name) ? type.Name : attr.Name;
				decidingField = attr.DependsOnField;
			}

			return new DataHolderDefinition(name, type, decidingField, attr);
		}
	}
}