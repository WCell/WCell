using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WCell.AuthServer
{
	public class LoginFailInfo
	{
		public DateTime LastAttempt;
		public readonly WaitHandle Handle;
		public int Count;

		public LoginFailInfo(DateTime lastAttempt)
		{
			Count = 1;
			LastAttempt = lastAttempt;
			Handle = new EventWaitHandle(false, EventResetMode.ManualReset);
		}
	}
}
