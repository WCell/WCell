using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using WCell.Util.Xml;

namespace WCell.Util.Variables
{
	public class TypeVariableDefinition : IComparable, IVariableDefinition, IXmlSerializable
	{
		public static readonly object[] EmptyObjectArray = new object[0];

		public static readonly Type GenericListType = typeof(IList<>);
		private const string ENUMERABLE_ITEM_NAME = "Item";

		internal MemberInfo m_Member;
		private bool m_isXmlSerializable;
		private Type m_collectionType;

		/// <summary>
		/// The object that holds the field or property (or null if static)
		/// </summary>

		public readonly Object Object;

		public bool Serialized;
		private bool m_readOnly;

		public TypeVariableDefinition()
		{
		}

		public TypeVariableDefinition(string name, MemberInfo member, bool serialized, bool readOnly)
		{
			Name = name;
			Member = member;
			Serialized = serialized;
			m_readOnly = readOnly;
		}

		public TypeVariableDefinition(string name, object obj, MemberInfo member, bool serialized, bool readOnly) :
			this(name, member, serialized, readOnly)
		{
			Object = obj;
		}

		public string Name
		{
			get;
			internal set;
		}

		public bool IsReadOnly
		{
			get
			{
				return m_readOnly;
			}
			internal set { m_readOnly = value; }
		}

		public MemberInfo Member
		{
			get { return m_Member; }
			internal set
			{
				m_Member = value;
				FullName = GetSafeName();

				var varType = m_Member.GetVariableType();
				m_isXmlSerializable = varType.GetInterface("IXmlSerializable") != null;
				var interfaceType = varType.GetInterface("IEnumerable");
				if (interfaceType != null && varType != typeof(string))
				{
					if (varType.IsArray)
					{
						m_collectionType = varType.GetElementType();
					}
					else
					{
						var listType = varType.GetInterface(GenericListType.Name);
						//TODO:
						//varType.GetGenericArguments
						if (listType == null)
						{
							throw new Exception(
								"Cannot create TypeVariableDefinition for IEnumerable, unless it is an Array or implements IList<T>.");
						}
						m_collectionType = listType.GetGenericArguments().First();
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private string GetSafeName()
		{
			var name = m_Member.DeclaringType.FullName;

			name = name.Replace("+", ".");		// + is used for nested classes
			name = name.Replace("#", ".");		// just in case
			return name + "." + Name;
		}

		public string FullName
		{
			//get { return m_Member.DeclaringType.FullName + "." + m_Member.Name; }
			get;
			private set;
		}

		public Type VariableType
		{
			get
			{
				return m_Member.GetVariableType();
			}
		}

		public object Value
		{
			get
			{
				return m_Member.GetUnindexedValue(Object);
			}
			set
			{
				m_Member.SetUnindexedValue(Object, value);
			}
		}

		public string TypeName
		{
			get
			{
				return VariableType.Name;
			}
		}

		public bool TrySet(string strValue)
		{
			if (IsReadOnly)
			{
				return false;
			}

			Value = TryParse(strValue, VariableType);
			return Value != null;
		}

		static object TryParse(string strValue, Type type)
		{
			object valueObj = null;
			if (Utility.Parse(strValue, type, ref valueObj))
			{
				return valueObj;
			}
			return null;
		}

		public int CompareTo(object obj)
		{
			if (obj is TypeVariableDefinition)
			{
				return ((TypeVariableDefinition)obj).Name.CompareTo(Name);
			}
			return -1;
		}

		public void ReadXml(XmlReader reader)
		{
			var origVal = Value;
			try
			{
				var type = m_Member.GetVariableType();
				if (m_isXmlSerializable)
				{
					if (Value == null)
					{
						Value = Activator.CreateInstance(type);
					}
					((IXmlSerializable) Value).ReadXml(reader);
				}
				else if (type.IsSimpleType())
				{
					var str = reader.ReadString();
					TrySet(str);
				}
				else
				{
					if (m_collectionType != null)
					{
						IList collection;
						if (m_Member.GetVariableType().IsArray)
						{
							//collection = (IList)Activator.CreateInstance(varType);
							collection = new List<object>();
							ReadCollection(reader, collection);
							var arr = Array.CreateInstance(m_collectionType, collection.Count);
							for (var i = 0; i < collection.Count; i++)
							{
								ArrayUtil.SetValue(arr, i, collection[i]);
							}
							Value = arr;
						}
						else
						{
							collection = (IList) Activator.CreateInstance(type);
							ReadCollection(reader, collection);
							Value = collection;
						}
					}
					else
					{
						// should never happen due to the initial checks
						throw new NotImplementedException("Cannot serialize Variable because it has an invalid Type: " + type);
					}
				}
			}
			catch (Exception e)
			{
				// reset value
				Value = origVal;
				throw e;
			}
		}

		void ReadCollection(XmlReader reader, IList col)
		{
			while (true)
			{
				reader.Read();
				reader.SkipEmptyNodes();

				if (reader.NodeType == XmlNodeType.EndElement)
				{
					// nothing here
					return;
				}
				else if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.Name == ENUMERABLE_ITEM_NAME)
					{
						var str = reader.ReadString();
						var value = TryParse(str, m_collectionType);
						if (value != null)
						{
							col.Add(value);
						}
					}
				}

				reader.SkipEmptyNodes();
				reader.ReadEndElement();
			}
		}

		public virtual void WriteXml(XmlWriter writer)
		{
			if (IsReadOnly)
			{
				throw new InvalidOperationException("Tried to write ReadOnly Variable \"" + this + "\" to XML-Stream");
			}
			if (Value == null)
			{
				throw new ArgumentException("Tried to write null-value to XML: " + this);
			}

			var type = m_Member.GetVariableType();
			if (m_isXmlSerializable)
			{
				((IXmlSerializable)Value).WriteXml(writer);
			}
			else if (type.IsSimpleType())
			{
				writer.WriteString(Value.ToString());
			}
			else if (m_collectionType != null)
			{
				writer.WriteCollection((IEnumerable)Value, ENUMERABLE_ITEM_NAME);
			}
			else
			{
				// in theory this could never happen due to the initial checks
				throw new NotImplementedException("Cannot serialize Variable because it has an invalid Type: " + type);
			}
		}

		public XmlSchema GetSchema()
		{
			throw new System.NotImplementedException(GetType() + " does not support any XmlSchema.");
		}

		public override string ToString()
		{
			return Name + " (" + FullName + ")";
		}
	}
}