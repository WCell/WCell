/*************************************************************************
 *
 *   file		: StatisticsTimer.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-08 00:55:09 +0800 (Sun, 08 Jun 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 458 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.AuthServer.Network;
using WCell.Util.Variables;
using Utility=WCell.Util.Utility;

namespace WCell.AuthServer.Stats
{
	public class AuthStats : Statistics<AuthStats>
	{
		[Initialization(InitializationPass.Tenth)]
		public static void Init()
		{
			inited = true;
			StatsPostDelay = s_interval;
		}

		/// <summary>
		/// The delay for posting the stats
		/// </summary>
		public static int StatsPostDelay
		{
			get { return instance != null ? instance.StatsPostInterval : 0; }
			set
			{
				if (!inited)
				{
					s_interval = value;
					return;
				}

				if (instance == null)
				{
					if (value == 0)
					{
						// init
						return;
					}
				}
				Instance.StatsPostInterval = value;
			}
		}

		public override long TotalBytesSent
		{
			get { return AuthClient.TotalBytesSent; }
		}

		public override long TotalBytesReceived
		{
			get { return AuthClient.TotalBytesReceived; }
		}

		public override void GetStats(ICollection<string> list)
		{
			list.Add(string.Format("+ Uptime: {0}", Utility.Format(AuthenticationServer.RunTime)));
			list.Add("+ Accounts online: " + AuthenticationServer.Instance.LoggedInAccounts.Count);
			base.GetStats(list);
		}
	}
}
