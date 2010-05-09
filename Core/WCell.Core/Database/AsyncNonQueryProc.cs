using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.Util.Threading;

namespace WCell.Core.Database
{
	public class AsyncNonQueryProc : AsyncQuery
	{
		private Action m_queryProc;
		private Action m_callback;

		public AsyncNonQueryProc(Action querier)
			: this(querier, null)
		{ }

		public AsyncNonQueryProc(Action querier, Action callback)
		{
			if (querier == null)
				throw new ArgumentNullException("querier");

			m_queryProc = querier;
			m_callback = callback;
		}

		public override void Execute()
		{
			try
			{
				// execute our query delegate
				m_queryProc();

				if (m_callback != null)
				{
					m_callback();
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
