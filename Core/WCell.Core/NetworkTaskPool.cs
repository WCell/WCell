using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cell.Core.Collections;
using System.Diagnostics;
using NLog;
using WCell.Core.Localization;

namespace WCell.Core
{
	public class NetworkTaskPool : AsyncTaskPool
	{
		public NetworkTaskPool()
			: base(WCellDef.MAX_NETWORK_SEND_DELTA)
		{
		}
	}
}
