//
// Author: James Nies
// Date: 3/22/2005
// Description: The PropertyAccessor class uses this interface 
//		for creating a type at runtime for accessing an individual
//		property on a target object.
//
// *** This code was written by James Nies and has been provided to you, ***
// *** free of charge, for your use.  I assume no responsibility for any ***
// *** undesired events resulting from the use of this code or the		 ***
// *** information that has been provided with it .						 ***
//

using System;
using System.Reflection;

namespace WCell.Util
{
	public delegate void SubmitValueHandler(object value);

	/// <summary>
	/// The IPropertyAccessor interface defines a property
	/// accessor.
	/// </summary>
	public interface IIndexedGetterSetter
	{
		/// <summary>
		/// </summary>
		/// <returns>Value.</returns>
		object Get(object key, int index);

		/// <summary>
		/// </summary>
		/// <param name="value">Value.</param>
		void Set(object key, int index, object value);
	}

	/// <summary>
	/// </summary>
	public interface IValueSetter
	{
		/// <summary>
		/// </summary>
		void Set(object value);
	}

	/// <summary>
	/// </summary>
	public interface IGetterSetter
	{
		/// <summary>
		/// </summary>
		/// <returns>Value.</returns>
		object Get(object key);

		/// <summary>
		/// </summary>
		/// <param name="value">Value.</param>
		void Set(object key, object value);
	}

	/// <summary>
	/// </summary>
	public class CustomGetterSetter : IGetterSetter
	{
		private readonly Func<object, object> m_Getter;
		private readonly Action<object, object> m_Setter;

		public CustomGetterSetter(Func<object, object> getter, 
			Action<object, object> setter)
		{
			m_Getter = getter;
			m_Setter = setter;
		}

		public object Get(object key)
		{
			return m_Getter(key);
		}

		public void Set(object key, object value)
		{
			m_Setter(key, value);
		}
	}

	public class DefaultVariableAccessor : IGetterSetter
	{
		private MemberInfo m_member;
		public DefaultVariableAccessor(MemberInfo member)
		{
			if (member is FieldInfo || member is PropertyInfo)
			{
				m_member = member;
			}
			else
			{
				throw new Exception("Invalid member: " + member);
			}
		}

		public object Get(object key)
		{
			if (m_member is FieldInfo)
			{
				return ((FieldInfo) m_member).GetValue(key);
			}
			if (m_member is PropertyInfo)
			{
				return ((PropertyInfo)m_member).GetValue(key, new object[0]);
			}
			throw new Exception("Invalid member: " +m_member);
		}

		public void Set(object key, object value)
		{
			if (m_member is FieldInfo)
			{
				((FieldInfo)m_member).SetValue(key, value);
			}
			else if (m_member is PropertyInfo)
			{
				((PropertyInfo)m_member).SetValue(key, value, new object[0]);
			}
		}
	}
}