using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.Util.Threading;
using WCell.Util.NLog;

namespace WCell.Core.Database
{
	public class AsyncNonQueryGenericProc<T> : AsyncQuery
	{
		private Action m_queryProc;
		private Action<T> m_callback;
		private T m_parameter;

		public AsyncNonQueryGenericProc(Action querier)
			: this(querier, null, default(T))
		{ }

		public AsyncNonQueryGenericProc(Action querier, Action<T> callback)
			: this(querier, callback, default(T))
		{
		}

		public AsyncNonQueryGenericProc(Action querier, Action<T> callback, T parameter)
		{
			if (querier == null)
				throw new ArgumentNullException("querier");

			m_queryProc = querier;
			m_callback = callback;
			m_parameter = parameter;
		}

		public T Parameter
		{
			get { return m_parameter; }
			set { m_parameter = value; }
		}

		public override void Execute()
		{
			try
			{
				// execute our query delegate
				m_queryProc();

				if (m_callback != null)
				{
					m_callback(m_parameter);
					m_callback = null;
				}
			}
			catch (Exception ex)
			{
				LogUtil.ErrorException(ex, "Failed to execute async query!");
			}

			if (m_parameter != null)
			{
				m_parameter = default(T);
			}

			m_queryProc = null;
		}
	}

	public class AsyncNonQueryGenericProc<T, T2> : AsyncQuery
	{
		private Action m_queryProc;
		private Action<T, T2> m_callback;
		private T m_parameterOne;
		private T2 m_parameterTwo;

		public AsyncNonQueryGenericProc(Action querier)
			: this(querier, null, default(T), default(T2))
		{ }

		public AsyncNonQueryGenericProc(Action querier, Action<T, T2> callback)
			: this(querier, callback, default(T), default(T2))
		{
		}

		public AsyncNonQueryGenericProc(Action querier, Action<T, T2> callback, T parameterOne, T2 parameterTwo)
		{
			if (querier == null)
				throw new ArgumentNullException("querier");

			m_queryProc = querier;
			m_callback = callback;
			m_parameterOne = parameterOne;
			m_parameterTwo = parameterTwo;
		}

		public T ParameterOne
		{
			get { return m_parameterOne; }
			set { m_parameterOne = value; }
		}

		public T2 ParameterTwo
		{
			get { return m_parameterTwo; }
			set { m_parameterTwo = value; }
		}

		public override void Execute()
		{
			try
			{
				// execute our query delegate
				m_queryProc();

				if (m_callback != null)
				{
					m_callback(m_parameterOne, m_parameterTwo);
					m_callback = null;
				}
			}
			catch (Exception ex)
			{
				LogUtil.ErrorException(ex, "Failed to execute async query!");
			}

			if (m_parameterOne != null)
			{
				m_parameterOne = default(T);
			}

			if (m_parameterTwo != null)
			{
				m_parameterTwo = default(T2);
			}

			m_queryProc = null;
		}
	}
}
