using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.Util.Threading;

namespace WCell.Core.Database
{
	public class AsyncQueryProc<T, V> : AsyncQuery where V : ActiveRecordBase
	{
		private T m_targetParameter;
		private Func<V> m_queryProc;
		private Action<T, V> m_callback;

		public AsyncQueryProc(Func<V> querier)
			: this(querier, null, default(T))
		{
		}

		public AsyncQueryProc(Func<V> querier, Action<T, V> callback)
			: this(querier, callback, default(T))
		{
		}

		public AsyncQueryProc(Func<V> querier, Action<T, V> callback, T target)
		{
			if (querier == null)
				throw new ArgumentNullException("querier");

			m_targetParameter = target;
			m_queryProc = querier;
			m_callback = callback;
		}

		public override void Execute()
		{
			try
			{
				// execute our query delegate
				V result = m_queryProc();

				if (m_callback != null)
				{
					m_callback(m_targetParameter, result);
					m_callback = null;
				}
			}
			catch (Exception ex)
			{
				s_log.ErrorException("Failed to execute async query!", ex);
			}

			m_queryProc = null;
		}
	}
}
