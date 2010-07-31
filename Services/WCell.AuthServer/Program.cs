/*************************************************************************
 *
 *   file		: Program.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-12-02 15:19:13 +0100 (ti, 02 dec 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 694 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;
using System;
using WCell.Core.Database;

namespace WCell.AuthServer
{
    /// <summary>
    /// Base class for starting the authentication server.
    /// </summary>
    public class Program
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Starts up the authentication server.
        /// </summary>
        public static bool Start()
        {
			AuthenticationServer.Instance.Start();
			return AuthenticationServer.Instance.IsRunning;
        }
    }
}