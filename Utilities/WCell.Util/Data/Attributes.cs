using System;
using System.Collections.Generic;
using WCell.Util.Conversion;

namespace WCell.Util.Data
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class NotPersistentAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class PersistentAttribute : DBAttribute
	{
		public int Length;

		/// <summary>
		/// A custom variable accessor.
		/// </summary>
		public IGetterSetter Accessor;

		/// <summary>
		/// Used to convert data before getting/setting.
		/// [NIY]
		/// </summary>
		public IConverter Converter;

		public PersistentAttribute()
		{
		}

		/// <summary>
		/// Used to convert the type in the Db to this type
		/// </summary>
		public PersistentAttribute(IConverter converter)
		{
			Converter = converter;
		}

		public PersistentAttribute(string name)
		{
			Name = name;
		}

		public PersistentAttribute(int arrLength)
		{
			Length = arrLength;
		}

		public PersistentAttribute(string name, int arrLength)
		{
			Length = arrLength;
			Name = name;
		}

		public PersistentAttribute(IGetterSetter accessor)
		{
			Accessor = accessor;
		}

		public PersistentAttribute(string name, IGetterSetter accessor)
		{
			Name = name;
			Accessor = accessor;
		}

		public PersistentAttribute(string name, IGetterSetter accessor, int arrLength)
		{
			Accessor = accessor;
			Length = arrLength;
			Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class DataHolderAttribute : DBAttribute
	{
		public bool Inherit;
		public bool RequirePersistantAttr;

	    public readonly IDictionary<object, IProducer> DependingProducers = new Dictionary<object, IProducer>();

		public DataHolderAttribute()
		{
		}

		public DataHolderAttribute(string dependsOnField)
		{
			DependsOnField = dependsOnField;
		}

		public DataHolderAttribute(IEnumerable<KeyValuePair<object, IProducer>> dependingPoducers)
		{
			foreach (var pair in dependingPoducers)
			{
				DependingProducers.Add(pair);
			}
		}

	    /// <summary>
	    /// The field that delivers the values to decide the depending Producer
	    /// </summary>
	    public string DependsOnField { get; set; }
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DependingProducer : Attribute
	{
		public object Key;
		public IProducer Producer;

		public DependingProducer(object id, Type type)
		{
			Key = id;
			Producer = new DefaultProducer(type);
		}

		public DependingProducer(object id, IProducer producer)
		{
			Key = id;
			Producer = producer;
		}

		public DependingProducer(object id, Func<object> creator)
		{
			Key = id;
			Producer = new CustomProducer(creator);
		}

		public DependingProducer(object id, CustomProducer producer)
		{
			Key = id;
			Producer = producer;
		}
	}

	public abstract class DBAttribute : Attribute
	{
		public string Name;
	}
}