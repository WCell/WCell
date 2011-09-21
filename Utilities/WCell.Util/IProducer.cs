using System;
using System.Reflection;
using WCell.Util.Data;

namespace WCell.Util
{
	public interface IProducer
	{
		/// <summary>
		/// Creates a new object of Type T
		/// </summary>
		object Produce();
	}

	public interface IOneArgumentProducer
	{
		object Produce(object arg1);
	}

	public interface IProducer<T> : IProducer
	{
		/// <summary>
		/// Creates a new object of Type T
		/// </summary>
		new T Produce();
	}

	public class DefaultProducer : IProducer
	{
		private readonly Type m_Type;

		public DefaultProducer(Type type)
		{
			m_Type = type;
		}

		public Type Type
		{
			get { return m_Type; }
		}

		object IProducer.Produce()
		{
			try
			{
				return Activator.CreateInstance(m_Type);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Cannot create Object of Type: " + m_Type.FullName, e);
			}
		}

		public static implicit operator DefaultProducer(Type type)
		{
			return new DefaultProducer(type);
		}
	}

	public class CustomProducer : IProducer
	{
		private readonly Func<object> m_Creator;

		public CustomProducer(Func<object> creator)
		{
			m_Creator = creator;
		}

		public Func<object> Creator
		{
			get { return m_Creator; }
		}

		object IProducer.Produce()
		{
			return m_Creator();
		}

		public static implicit operator CustomProducer(Func<object> creator)
        {
			return new CustomProducer(creator);
        }
	}

	public class DefaultOneArgumenteProducer : IProducer, IOneArgumentProducer
	{
		private readonly Type m_Type;
		private readonly ConstructorInfo m_ctor;

		public DefaultOneArgumenteProducer(Type type)
		{
			m_Type = type;
			foreach (var ctor in type.GetConstructors(BindingFlags.Public))
			{
				if (ctor.GetParameters().Length == 1 && !ctor.ContainsGenericParameters)
				{
					m_ctor = ctor;
					break;
				}
			}
		}

		public DefaultOneArgumenteProducer(Type type, ConstructorInfo ctor)
		{
			m_Type = type;
			m_ctor = ctor;
		}

		public ConstructorInfo Ctor
		{
			get { return m_ctor; }
		}

		public Type Type
		{
			get { return m_Type; }
		}

		object IProducer.Produce()
		{
			throw new DataHolderException("Cannot call default ctor on dependent producer for Type: " + m_Type);
		}

		public object Produce(object arg1)
		{
			return m_ctor.Invoke(new[] { arg1 });
		}
	}

	public class DefaultArrayProducer : IProducer
	{
		private readonly Type m_Type;
		private readonly int m_Length;

		public DefaultArrayProducer(Type type, int length)
		{
			m_Type = type;
			m_Length = length;
		}

		public int Length
		{
			get { return m_Length; }
		}

		public Type Type
		{
			get { return m_Type; }
		}

		object IProducer.Produce()
		{
			return Array.CreateInstance(m_Type, m_Length);
		}
	}

	public class DefaultProducer<T> : IProducer<T>
	{
		/// <summary>
		/// Creates a new object of Type T
		/// </summary>
		public T Produce()
		{
			return Activator.CreateInstance<T>();
		}

		object IProducer.Produce()
		{
			return Activator.CreateInstance<T>();
		}
	}

	public class DefaultArrayProducer<T> : IProducer<T>
	{
		/// <summary>
		/// Creates a new object of Type T
		/// </summary>
		public T Produce()
		{
			return Activator.CreateInstance<T>();
		}

		object IProducer.Produce()
		{
			return Activator.CreateInstance<T>();
		}
	}
}