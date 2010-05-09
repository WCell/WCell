using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using WCell.Util.DB;

namespace WCell.Util.Data
{
	#region Interfaces
	public interface IFlatDataFieldAccessor : IDataFieldAccessor
	{
		//void Set(IDictionary<string, object> values, IDataHolder rootObj, object value);

		object Get(IDataHolder obj);

		void Set(IDataHolder obj, object value);
	}

	public interface IDataFieldAccessor
	{
		DataHolderDefinition DataHolderDefinition
		{
			get;
		}
	}

	public interface INestedDataField : IDataFieldBase
	{
		IGetterSetter Accessor
		{
			get;
		}

		IProducer Producer
		{
			get;
		}

		Dictionary<string, IDataField> InnerFields
		{
			get;
		}

		MemberInfo BelongsTo
		{
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rootObject"></param>
		/// <returns></returns>
		object GetTargetObject(IDataHolder rootObject);
	}

	public interface IDataFieldBase
	{
		INestedDataField Parent
		{
			get;
		}

		DataHolderDefinition DataHolderDefinition
		{
			get;
		}
	}

	public interface IDataField : IDataFieldBase
	{
		string Name
		{
			get;
		}

		string FullName
		{
			get;
		}

		IGetterSetter Accessor
		{
			get;
		}

		MemberInfo MappedMember
		{
			get;
		}

		DataFieldType DataFieldType
		{
			get;
		}
	}
	#endregion

	public abstract class DataFieldBase : IDataFieldBase
	{
		protected DataHolderDefinition m_DataHolderDefinition;
		protected IGetterSetter m_accessor;
		protected INestedDataField m_parent;

		protected DataFieldBase(DataHolderDefinition dataHolder, IGetterSetter accessor, INestedDataField parent)
		{
			m_DataHolderDefinition = dataHolder;
			m_parent = parent;
			m_accessor = accessor;
		}

		public INestedDataField Parent
		{
			get { return m_parent; }
			internal set { m_parent = value; }
		}

		public DataHolderDefinition DataHolderDefinition
		{
			get { return m_DataHolderDefinition; }
		}

		public IGetterSetter Accessor
		{
			get { return m_accessor; }
		}

		public abstract IDataField Copy(INestedDataField parent);
	}

	public abstract class DataField : DataFieldBase, IDataField
	{
		protected MemberInfo m_mappedMember;
		protected string m_name, m_fullName;

		protected DataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor, MemberInfo mappedMember, INestedDataField parent) :
			base(dataHolder, accessor, parent)
		{
			m_mappedMember = mappedMember;
			m_name = name;
			//m_fullName = parent;
		}

		public string Name
		{
			get { return m_name; }
		}

		public string FullName
		{
			get { return m_fullName; }
		}

		public MemberInfo MappedMember
		{
			get { return m_mappedMember; }
		}

		public abstract DataFieldType DataFieldType { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rootObject"></param>
		/// <returns></returns>
		public object GetTargetObject(IDataHolder rootObject)
		{
			if (m_parent == null)
			{
				return rootObject;
			}

			var targetObj = m_parent.GetTargetObject(rootObject);

			var obj = m_parent.Accessor.Get(targetObj);
			if (obj == null)
			{
				obj = m_parent.Producer.Produce();
				m_parent.Accessor.Set(rootObject, obj);
			}
			return obj;
		}

		public override string ToString()
		{
			return m_name;
		}
	}

	public class FlatSimpleDataField : DataField, IFlatDataFieldAccessor
	{
		public FlatSimpleDataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
			MemberInfo mappedMember, INestedDataField parent) :
			base(dataHolder, name, accessor, mappedMember, parent)
		{
		}

		public object Get(IDataHolder obj)
		{
			var targetObj = GetTargetObject(obj);
			return m_accessor.Get(targetObj);
		}

		public void Set(IDataHolder obj, object value)
		{
			var targetObj = GetTargetObject(obj);
			m_accessor.Set(targetObj, value);
			if (targetObj.GetType().IsValueType)
			{
				var parent = m_parent;
				object parentTarget;
				while (parent != null)
				{
					parentTarget = parent.GetTargetObject(obj);
					parent.Accessor.Set(parentTarget, targetObj);
					targetObj = parentTarget;
					parent = parent.Parent;
					if (!parentTarget.GetType().IsValueType)
					{
						break;
					}
				}
			}
		}

		public override DataFieldType DataFieldType
		{
			get { return DataFieldType.FlatSimple; }
		}

		public override string ToString()
		{
			return Name;
		}

		public override IDataField Copy(INestedDataField parent)
		{
			return new FlatSimpleDataField(m_DataHolderDefinition, m_name, m_accessor, m_mappedMember, parent);
		}
	}

	#region Nested
	public abstract class NestedDataField : DataField, INestedDataField
	{
		protected readonly Dictionary<string, IDataField> m_innerFields = new Dictionary<string, IDataField>(StringComparer.InvariantCultureIgnoreCase);
		protected readonly IProducer m_Producer;

		protected NestedDataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor, MemberInfo mappedMember, IProducer producer, INestedDataField parent)
			: base(dataHolder, name, accessor, mappedMember, parent)
		{
			m_Producer = producer;
		}

		public Dictionary<string, IDataField> InnerFields
		{
			get { return m_innerFields; }
		}

		public MemberInfo BelongsTo
		{
			get { return MappedMember; }
		}

		public IProducer Producer
		{
			get { return m_Producer; }
		}
	}

	public class NestedSimpleDataField : NestedDataField
	{
		public NestedSimpleDataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
			MemberInfo mappedMember, IProducer producer, INestedDataField parent)
			: base(dataHolder, name, accessor, mappedMember, producer, parent)
		{

		}

		public IEnumerator GetEnumerator()
		{
			return m_innerFields.Values.GetEnumerator();
		}

		public override DataFieldType DataFieldType
		{
			get { return DataFieldType.NestedSimple; }
		}

		public override IDataField Copy(INestedDataField parent)
		{
			return new NestedSimpleDataField(m_DataHolderDefinition, m_name, m_accessor, m_mappedMember, m_Producer, parent);
		}
	}
	#endregion

	#region Arrays
	public abstract class ArrayDataField : DataField, IIndexedGetterSetter
	{
		protected int m_length;
		protected readonly IProducer m_arrProducer;
		protected IDataFieldAccessor[] m_ArrayAccessors;

		protected ArrayDataField(DataHolderDefinition dataHolder, string name,
			IGetterSetter accessor, MemberInfo mappedMember, INestedDataField parent, int length, IProducer arrProducer)
			: base(dataHolder, name, accessor, mappedMember, parent)
		{
			m_length = length;
			m_arrProducer = arrProducer;
		}

		/// <summary>
		/// The minimal required length of this Array
		/// </summary>
		public int Length
		{
			get { return m_length; }
		}

		public IProducer ArrayProducer
		{
			get { return m_arrProducer; }
		}

		public IDataFieldAccessor[] ArrayAccessors
		{
			get
			{
				return m_ArrayAccessors;
			}
		}

		public Array GetArray(object arrayContainer)
		{
			var arr = (Array)m_accessor.Get(arrayContainer);
			if (arr == null)
			{
				arr = (Array)m_arrProducer.Produce();
				m_accessor.Set(arrayContainer, arr);
			}
			return arr;
		}

		/// <summary>
		/// Returns the object at the given index (might be null).
		/// </summary>
		/// <param name="arrayContainer"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public object Get(object arrayContainer, int index)
		{
			return GetArray(arrayContainer).GetValue(index);
		}

		public void Set(object arrayContainer, int index, object value)
		{
			try
			{
				ArrayUtil.SetValue(GetArray(arrayContainer), index, value);
			}
			catch (Exception e)
			{
				throw new Exception("Failed to set Array-element: " + this, e);
			}
		}
	}

	public class FlatArrayDataField : ArrayDataField
	{
		public FlatArrayDataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
			MemberInfo mappedMember, int length, IProducer arrProducer, INestedDataField parent) :
			base(dataHolder, name, accessor, mappedMember, parent, length, arrProducer)
		{
			m_ArrayAccessors = new FlatArrayAccessor[m_length];
			for (var i = 0; i < m_length; i++)
			{
				m_ArrayAccessors[i] = new FlatArrayAccessor(this, i);
			}
		}

		public override DataFieldType DataFieldType
		{
			get { return DataFieldType.FlatArray; }
		}

		public override IDataField Copy(INestedDataField parent)
		{
			return new FlatArrayDataField(m_DataHolderDefinition, m_name, m_accessor, m_mappedMember, m_length, m_arrProducer, parent);
		}
	}

	public class FlatArrayAccessor : IFlatDataFieldAccessor
	{
		private readonly FlatArrayDataField m_ArrayField;
		private readonly int m_Index;

		public FlatArrayAccessor(FlatArrayDataField arrayField, int index)
		{
			m_ArrayField = arrayField;
			m_Index = index;
		}

		public int Index
		{
			get { return m_Index; }
		}

		public FlatArrayDataField ArrayField
		{
			get { return m_ArrayField; }
		}

		public DataHolderDefinition DataHolderDefinition
		{
			get { return m_ArrayField.DataHolderDefinition; }
		}

		public object Get(IDataHolder obj)
		{
			var targetObj = m_ArrayField.GetTargetObject(obj);
			return m_ArrayField.Get(targetObj, m_Index);
		}

		public void Set(IDataHolder obj, object value)
		{
			var targetObj = m_ArrayField.GetTargetObject(obj);
			m_ArrayField.Set(targetObj, m_Index, value);
		}

		public override string ToString()
		{
			return m_ArrayField.Name + "[" + m_Index + "]";
		}
	}
	#endregion

	public class NestedArrayDataField : ArrayDataField, INestedDataField
	{
		private readonly IProducer m_Producer;

		public NestedArrayDataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
			MemberInfo mappedMember,
			IProducer producer, IProducer arrayProducer, int length, INestedDataField parent)
			: base(dataHolder, name, accessor, mappedMember, parent, length, arrayProducer)
		{
			m_Producer = producer;
			m_ArrayAccessors = new NestedArrayAccessor[m_length];
			for (var i = 0; i < m_length; i++)
			{
				m_ArrayAccessors[i] = new NestedArrayAccessor(this, i);
			}
		}

		public IProducer Producer
		{
			get { return m_Producer; }
		}

		public Dictionary<string, IDataField> InnerFields
		{
			// awful code - I don't know, what I was thinking
			get { return ((NestedArrayAccessor)m_ArrayAccessors[0]).InnerFields; }
		}

		public MemberInfo BelongsTo
		{
			get { return MappedMember; }
		}

		public override DataFieldType DataFieldType
		{
			get { return DataFieldType.NestedArray; }
		}

		public override IDataField Copy(INestedDataField parent)
		{
			return new NestedArrayDataField(m_DataHolderDefinition, m_name,
				m_accessor, m_mappedMember, m_Producer, m_arrProducer, m_length, parent);
		}
	}

	public class NestedArrayAccessor : INestedDataField, IGetterSetter, IDataFieldAccessor
	{
		private readonly NestedArrayDataField m_ArrayField;
		private readonly int m_Index;
		private readonly Dictionary<string, IDataField> m_innerFields =
			new Dictionary<string, IDataField>(StringComparer.InvariantCultureIgnoreCase);

		public NestedArrayAccessor(NestedArrayDataField arrayField, int index)
		{
			m_ArrayField = arrayField;
			m_Index = index;
		}

		public Dictionary<string, IDataField> InnerFields
		{
			get { return m_innerFields; }
		}

		public MemberInfo BelongsTo
		{
			get { return m_ArrayField.MappedMember; }
		}

		public int Index
		{
			get { return m_Index; }
		}

		public NestedArrayDataField ArrayField
		{
			get { return m_ArrayField; }
		}

		public INestedDataField Parent
		{
			get { return m_ArrayField.Parent; }
		}

		public IGetterSetter Accessor
		{
			get { return this; }
		}

		public DataHolderDefinition DataHolderDefinition
		{
			get { return m_ArrayField.DataHolderDefinition; }
		}

		public IProducer Producer
		{
			get { return m_ArrayField.Producer; }
		}

		public object GetTargetObject(IDataHolder rootObject)
		{
			return m_ArrayField.GetTargetObject(rootObject);
		}

		public override string ToString()
		{
			return m_ArrayField.Name + "[" + m_Index + "]";
		}

		public object Get(object arrayContainer)
		{
			var arr = m_ArrayField.GetArray(arrayContainer);
			var obj = arr.GetValue(m_Index);
			if (obj == null)
			{
				obj = m_ArrayField.Producer.Produce();
				ArrayUtil.SetValue(arr, m_Index, obj);
			}
			return obj;
		}

		public void Set(object arrayContainer, object value)
		{
			m_ArrayField.Set(arrayContainer, m_Index, value);
		}
	}
}