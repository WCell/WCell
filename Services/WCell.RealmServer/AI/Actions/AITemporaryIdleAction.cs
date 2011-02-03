using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions
{
	public class AITemporaryIdleAction : IAIAction
	{
		private int m_Millis;
		private ProcTriggerFlags m_Flags;
		private Action m_Callback;
		private DateTime startTime;
		private UpdatePriority m_priority = UpdatePriority.Active;

		public AITemporaryIdleAction(int millis, ProcTriggerFlags flags, Action callback)
		{
			m_Millis = millis;
			m_Flags = flags;
			m_Callback = callback;
		}

		public Unit Owner
		{
			get { return null; }
		}

		public UpdatePriority Priority
		{
			get { return m_priority; }
		}

		public bool IsGroupAction
		{
			get { return false; }
		}

		public ProcTriggerFlags InterruptFlags
		{
			get { return m_Flags; }
		}

		public void Start()
		{
			startTime = DateTime.Now;
		}

		public void Update()
		{
			var millis = (uint)(DateTime.Now - startTime).TotalMilliseconds;
			if (millis >= m_Millis)
			{
				if (m_Callback != null)
				{
					m_Callback();
					m_Callback = null;
				}
			}
			else
			{
				var diff = m_Millis - millis;
				if (diff > 10000)
				{
					m_priority = UpdatePriority.Background;
				}
				else
				{
					m_priority = UpdatePriority.Active;
				}
			}
		}

		public void Stop()
		{
		}

		public void Dispose()
		{
		}

	}
}