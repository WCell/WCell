using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Core.Timers
{
	public enum TimerPriority
	{
		Always,
		OneSec,
		OnePointFiveSec,
		FiveSec,
		TenSec,
		ThirtySec,
		OneMin,
		OneHour,
		End
	}
}
