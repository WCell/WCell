/*************************************************************************
 *
 *   file		: ToggleRegionUpdates.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-04 21:43:29 +0200 (fr, 04 apr 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 224 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.RealmServer.World;

namespace WCell.RealmServerConsole.Commands
{
    /// <summary>
    /// Command for toggling all of the regions on or off 
    /// (turning updates on or off)
    /// </summary>
    public class ToggleRegionUpdates
    {
        /// <summary>
        /// Toggles all the regions on or off
        /// </summary>
        /// <param name="arguments">the arguments of the command</param>
        [ConsoleCommand("togglergnupdates")]
        public static void Execute(string[] arguments)
        {
        }
    }
}