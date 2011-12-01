/*************************************************************************
 *
 *   file		: StatisticsTimer.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-08 00:55:09 +0800 (Sun, 08 Jun 2008) $

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
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Stats
{
    public class RealmStats : Statistics<RealmStats>
	{
		[Initialization(InitializationPass.Tenth)]
		public static void Init()
		{
			inited = true;
			StatsPostDelay = s_interval;
		}

		/// <summary>
		/// 
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
					if (value > 0)
					{
						// init
						instance = new RealmStats();
					}
					else
					{
						return;
					}
				}
				Instance.StatsPostInterval = value;
			}
		}

		public override void GetStats(ICollection<string> list)
		{
			list.Add(string.Format("+ Uptime: {0}", Utility.Format(RealmServer.RunTime)));
			list.Add(string.Format("+ Players Online: {0} (Horde: {1}, Alliance: {2})",
				World.CharacterCount, World.HordeCharCount, World.AllianceCharCount));
			base.GetStats(list);
			list.Add("+ Map Load Average: " + Map.LoadAvgStr);
		}

    	public override long TotalBytesSent
    	{
			get { return RealmClient.TotalBytesSent; }
    	}

    	public override long TotalBytesReceived
    	{
			get { return RealmClient.TotalBytesReceived; }
    	}
    }
}