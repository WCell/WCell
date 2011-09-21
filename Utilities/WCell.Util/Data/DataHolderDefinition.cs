using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WCell.Util.Conversion;
using WCell.Util.DynamicAccess;

namespace WCell.Util.Data
{
	/// <summary>
	/// Contains Metadata about types that are saved/loaded to/from a persistent storage.
	/// </summary>
	public class DataHolderDefinition
	{
		private const int DynamicMaximumMembers = 80;

		public const string CacheGetterName = "GetAllDataHolders";

		private readonly string m_name;
		private readonly string m_DependingFieldName;
		private readonly DataHolderAttribute m_Attribute;
		private readonly Type m_Type;
		private readonly IProducer m_defaultProducer;
		private readonly Dictionary<object, IProducer> m_dependingProducers;

		private FlatSimpleDataField m_DependingField;
		private MethodInfo m_cacheGetter;

		public readonly Dictionary<string, IDataField> Fields =
			new Dictionary<string, IDataField>(StringComparer.InvariantCultureIgnoreCase);

		public DataHolderDefinition(string name, Type type, string dependingField, DataHolderAttribute attribute)
		{
			m_name = name;
			m_DependingFieldName = dependingField;
			m_Attribute = attribute;
			m_Type = type;

			var dependingProducerTypes = type.GetCustomAttributes<DependingProducer>();
			if (dependingProducerTypes.Length == 0)
			{
				m_dependingProducers = null;
			}
			else
			{
				m_dependingProducers = new Dictionary<object, IProducer>();
				foreach (var prodAttr in dependingProducerTypes)
				{
					var key = prodAttr.Key;
					//var keyType = key.GetType();
					//if (keyType.IsEnum)
					//{
					//    key = Convert.ChangeType(key, Enum.GetUnderlyingType(keyType));
					//}
					//if (key is uint)
					//{
					//    key = (int)((uint)key);
					//}
					//else if (key is ulong)
					//{
					//    key = (long)((ulong)key);
					//}
					m_dependingProducers.Add(key, prodAttr.Producer);
				}
			}

			if (type.IsAbstract)
			{
				if (m_dependingProducers == null)
				{
					throw new DataHolderException(
						"Cannot define DataHolder because it's Type is abstract and it did not define depending Producers: {0}",
						type.FullName);
				}
				if (m_DependingFieldName == null)
				{
					throw new DataHolderException(
						"Cannot define DataHolder because it's Type is abstract and it did not define the DependsOnField in the DataHolderAttribute: {0}",
						type.FullName);
				}
			}
			else
			{
				m_defaultProducer = new DefaultProducer(type);
			}

			try
			{
				GetDataFields(type, Fields, null);
				if (type.IsAbstract && m_DependingField == null)
				{
					throw new DataHolderException(
						"Cannot define DataHolder because it's DependsOnField (\"{0}\"), as defined in the DataHolderAttribute, does not exist: {1}",
						m_DependingFieldName, type.FullName);
				}
			}
			catch (Exception e)
			{
				throw new DataHolderException(e, "Unable to create DataHolderDefinition for: " + name);
			}
		}

		/// <summary>
		/// The name of the field whose values decides which Producer to use.
		/// </summary>
		public string DependingFieldName
		{
			get { return m_DependingFieldName; }
		}

		/// <summary>
		/// The field whose values decides which Producer to use.
		/// </summary>
		public FlatSimpleDataField DependingField
		{
			get { return m_DependingField; }
		}

		/// <summary>
		/// The Type of the DataHolder-class, defined through this.
		/// </summary>
		public Type Type
		{
			get { return m_Type; }
		}

		public string Name
		{
			get { return m_name; }
		}

		public bool SupportsCaching
		{
			get { return CacheGetter != null; }
		}

		/// <summary>
		/// The method that will yield all DataHolders of 
		/// this DataHolder's type.
		/// </summary>
		public MethodInfo CacheGetter
		{
			get
			{
				if (m_cacheGetter == null)
				{
					m_cacheGetter = Type.GetMethod(CacheGetterName);
					if (m_cacheGetter != null)
					{
						if (!m_cacheGetter.IsStatic)
						{
							throw new DataHolderException("Getter {0} must be static.", m_cacheGetter.GetFullMemberName());
						}

						var type = m_cacheGetter.ReturnType;
						var interfce = type.GetInterfaces().FirstOrDefault();
						var genArg1 = type.GetGenericArguments().FirstOrDefault();
						if (Type != genArg1 ||
							interfce == null ||
							!interfce.Name.StartsWith("IEnumerable"))
						{
							throw new DataHolderException("Getter {0} has wrong Type \"{1}\" - Expected: IEnumerable<{2}>",
														  m_cacheGetter.GetFullMemberName(), type.FullName, Type.Name);
						}
					}
				}
				return m_cacheGetter;
			}
		}

		public IDataField GetField(string name)
		{
			IDataField field;
			Fields.TryGetValue(name, out field);
			return field;
		}

		public object CreateHolder(object firstValue)
		{
			if (m_dependingProducers != null)
			{
				//if (firstValue is uint)
				//{
				//    firstValue = (int)((uint)firstValue);
				//}
				//else if (firstValue is int)
				//{
				//    firstValue = (int)firstValue;
				//}
				//else if (firstValue is ulong)
				//{
				//    firstValue = (long)((ulong)firstValue);
				//}
				//else if (firstValue is long)
				//{
				//    firstValue = (long)firstValue;
				//}
				var originalType = m_DependingField.MappedMember.GetVariableType();
				var currentType = firstValue.GetType();
				if (currentType != originalType)
				{
					if (originalType.IsEnum)
					{
						var underlyingType = Enum.GetUnderlyingType(originalType);
						if (currentType != originalType)
						{
							firstValue = Convert.ChangeType(firstValue, underlyingType);
						}
						firstValue = Enum.Parse(originalType, firstValue.ToString());
					}
				}

				IProducer producer;
				if (m_dependingProducers.TryGetValue(firstValue, out producer))
				{
					return producer.Produce();
				}
			}
			if (m_defaultProducer == null)
			{
				var str = firstValue is Array ? ((object[])firstValue).ToString(", ") : firstValue;
				throw new DataHolderException(
					"Could not create DataHolder \"{0}\" because Value \"{1}\" did not have a Producer assigned (Make sure that the Types match)",
					this, str);
			}
			return m_defaultProducer.Produce();
		}

		public static IProducer CreateProducer(Type type)
		{
			return new DefaultProducer(type);
		}

		private static IProducer CreateArrayProducer(Type type, int length)
		{
			return new DefaultArrayProducer(type, length);
		}

		#region GetDataFields
		private void GetDataFields(Type type, IDictionary<string, IDataField> fields, INestedDataField parent)
		{
			var members =
				type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetField | BindingFlags.SetProperty);
			var count = members.Length;

			Dictionary<MemberInfo, IGetterSetter> accessors;
			if (count < DynamicMaximumMembers && type.IsClass)
			{
				// only create dynamic accessors if there are less than x members
				accessors = AccessorMgr.GetOrCreateAccessors(type);
			}
			else
			{
				accessors = null;
			}

			foreach (var member in members)
			{
				if (member.IsReadonly())
				{
					continue;
				}

				if (member.GetCustomAttributes<NotPersistentAttribute>().Length > 0)
				{
					continue;
				}

				var dbAttrs = member.GetCustomAttributes<DBAttribute>();
				var persistentAttribute = dbAttrs.Where(attribute => attribute is PersistentAttribute).FirstOrDefault() as PersistentAttribute;
				if (persistentAttribute == null && m_Attribute.RequirePersistantAttr)
				{
					// DataHolder only maps fields/properties with PersistantAttribute
					continue;
				}

				var rawType = member.GetVariableType();
				var isArr = rawType.IsArray;

				string varName;
				IGetterSetter accessor;
				IFieldReader reader;
				Type memberType;
				if (persistentAttribute != null)
				{
					// persistent attribute
					memberType = persistentAttribute.ActualType ?? member.GetActualType();
					varName = persistentAttribute.Name ?? member.Name;
					if (persistentAttribute.AccessorType != null)
					{
						var accessorObj = Activator.CreateInstance(persistentAttribute.AccessorType);
						if (!(accessorObj is IGetterSetter))
						{
							throw new DataHolderException("Accessor for Persistent members must be of type IGetterSetter - "
								+ "Found accessor of type {0} for member {1}", accessorObj.GetType(), member.GetFullMemberName());
						}
						accessor = (IGetterSetter)accessorObj;
					}
					else
					{
						accessor = accessors != null ? accessors[member] : new DefaultVariableAccessor(member);
					}
					reader = Converters.GetReader(persistentAttribute.ReadType ?? memberType);
				}
				else
				{
					memberType = member.GetActualType();
					varName = member.Name;
					//accessor = new DefaultVariableAccessor(member);
					accessor = (accessors != null ? accessors[member] : new DefaultVariableAccessor(member));
					reader = Converters.GetReader(memberType);
				}

				// check array constraints
				if (isArr)
				{
					if (rawType.GetArrayRank() > 1 || memberType.IsArray)
					{
						throw new DataHolderException("Cannot define Type {0} of {1} because its a multi-dimensional Array.", rawType,
													  member.GetFullMemberName());
					}
				}

				IDataField field;
				if (reader == null)
				{
					// create reader
					if (type.IsAbstract)
					{
						throw new DataHolderException(
							"Cannot define member \"{0}\" of DataHolder \"{1}\" because it's Type ({2}) is abstract.",
							member.GetFullMemberName(), this, memberType.FullName);
					}

					IProducer producer;
					if (memberType.IsClass)
					{
						producer = CreateProducer(memberType);
					}
					else
					{
						// value type does not need a producer
						producer = null;
					}

					if (isArr)
					{
						// complex (nested) type
						var length = GetArrayLengthByAttr(persistentAttribute, member);
						var nestedField = new NestedArrayDataField(this, varName, accessor, member,
																   producer, CreateArrayProducer(memberType, length), length, parent);

						var dataFields = new Dictionary<string, IDataField>(StringComparer.InvariantCultureIgnoreCase);
						//Console.WriteLine("Getting field for: " + nestedField);

						GetDataFields(memberType, dataFields, nestedField);
						foreach (var dataField in dataFields.Values)
						{
							for (var i = 0; i < nestedField.ArrayAccessors.Length; i++)
							{
								var arrAccessor = (NestedArrayAccessor)nestedField.ArrayAccessors[i];
								var newField = ((DataFieldBase)dataField).Copy(arrAccessor);
								arrAccessor.InnerFields.Add(newField.Name, newField);
							}
						}

						field = nestedField;
					}
					else
					{
						// simple nested type
						var nestedField = new NestedSimpleDataField(this, varName, accessor, member, producer, parent);
						//Console.WriteLine("Getting field for: " + nestedField);

						GetDataFields(memberType, nestedField.InnerFields, nestedField);
						if (nestedField.InnerFields.Count == 0)
						{
							throw new DataHolderException("Cannot define " + member.GetFullMemberName() +
														  " as Nested because it does not have any inner fields.");
						}
						else
						{
							field = nestedField;
						}
					}
				}
				else
				{
					if (isArr)
					{
						//var nestedField = new (this, varName, accessor, member, producer, parent);
						var length = GetArrayLengthByAttr(persistentAttribute, member);
						field = new FlatArrayDataField(this, varName, accessor,
							member, length,
							CreateArrayProducer(memberType, length),
							parent);
					}
					else
					{
						field = new FlatSimpleDataField(this, varName, accessor, member, parent);
						if (varName == m_DependingFieldName)
						{
							m_DependingField = (FlatSimpleDataField)field;
						}
					}
				}

				fields.Add(field.Name, field);
			}

			if (fields.Count() - count == 0)
			{
				throw new ArgumentException("Invalid data Type has no persistent members: " + type.FullName);
			}
		}

		private static int GetArrayLengthByAttr(PersistentAttribute attr, MemberInfo member)
		{
			var len = attr != null ? attr.Length : 0;
			if (len < 1)
			{
				throw new DataHolderException(
					"Cannot define Array-member {0} because it did not define a minimal length through the PersistentAttribute.",
					member);
			}
			return len;
		}

		#endregion

		public string CreateIdString()
		{
			var str = new StringBuilder(Fields.Values.Count * 15);
			foreach (var field in Fields.Values)
			{
				str.Append(string.Format("{0}:{1},", field.MappedMember.GetActualType().Name, field.Name));
			}
			return str.ToString();
		}

		public override string ToString()
		{
			return m_name;
		}

		public IEnumerator GetEnumerator()
		{
			return Fields.Values.GetEnumerator();
		}
	}
}