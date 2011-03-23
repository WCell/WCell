/*************************************************************************
 *
 *   file		: Program.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (lø, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Util.Logging;

namespace WCell.RealmServer
{
    /// <summary>
    /// Base class for starting the realm server.
    /// </summary>
    public class Program
    {
		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Starts up the realm server.
        /// </summary>
        public static void Start()
		{
			RealmServer.Instance.Start();
        }
    }
}