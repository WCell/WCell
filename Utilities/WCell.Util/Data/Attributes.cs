using System;
using System.Collections.Generic;

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
		public Type AccessorType;

        /// <summary>
        /// Used to read data from the db as this type
        /// </summary>
        public Type ReadType;

		/// <summary>
		/// Used if this object is actually of a different type then it's field/property declares
		/// </summary>
		public Type ActualType;

		public PersistentAttribute()
		{
		}

		/// <summary>
		/// Used to convert the type in the Db to this type
		/// </summary>
		public PersistentAttribute(Type readType)
		{
		    ReadType = readType;
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