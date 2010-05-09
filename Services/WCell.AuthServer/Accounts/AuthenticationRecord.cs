using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Timers;
using WCell.Intercommunication.DataTypes;

namespace WCell.AuthServer.Accounts
{
	/// <summary>
	/// Holds client-sent AuthenticationInfo
	/// and disposes itself after the global timeout.
	/// </summary>
	public class AuthenticationRecord
	{
		public static float AuthenticationStoreSeconds = 30;

		public readonly string AccName;
		public readonly AuthenticationInfo AuthInfo;

		TimerEntry m_timer;

		public AuthenticationRecord(string accName, AuthenticationInfo srp)
		{
			AccName = accName;
			AuthInfo = srp;
		}

		internal void StartTimer()
		{
			m_timer = new TimerEntry(AuthenticationStoreSeconds, 0f, dl => Remove());
			m_timer.Start();
			AuthenticationServer.Instance.RegisterUpdatable(m_timer);
		}

		internal void StopTimer()
		{
			AuthenticationServer.Instance.UnregisterUpdatable(m_timer);
		}

		public void Remove()
		{
			StopTimer();
			AuthenticationServer.Instance.RemoveAuthenticationInfo(AccName);
		}
	}
}
