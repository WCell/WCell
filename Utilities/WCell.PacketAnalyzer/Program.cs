/*************************************************************************
 *
 *   file		: Program.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-05-29 17:37:37 +0200 (to, 29 maj 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 426 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Core.Initialization;

namespace WCell.PacketAnalysis
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
		public static void Main()
        {
			InitMgr.Initialize(typeof(PacketParser).Assembly);
			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new MainForm());
        }
    }
}
