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
		public static int AuthenticationStoreMillis = 30000;

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
			m_timer = new TimerEntry(AuthenticationStoreMillis, 0, dl => Remove());
			m_timer.Start();
			AuthenticationServer.IOQueue.RegisterUpdatable(m_timer);
		}

		internal void StopTimer()
		{
			AuthenticationServer.IOQueue.UnregisterUpdatable(m_timer);
		}

		public void Remove()
		{
			StopTimer();
			AuthenticationServer.Instance.RemoveAuthenticationInfo(AccName);
		}
	}
}