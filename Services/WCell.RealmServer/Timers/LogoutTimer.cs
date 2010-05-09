/*************************************************************************
 *
 *   file		: LogoutTimer.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-08 19:14:17 +0100 (fr, 08 feb 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 119 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.World;

namespace WCell.RealmServer.Timers
{
    public class LogoutTimer : OneShotActionTimer
    {
        private Character m_logoutCharacter;

        public LogoutTimer(Character chr)
        {
            m_logoutCharacter = chr;
        }

        public override long OnTick(object state)
        {
            //Starts the character logout process
            m_logoutCharacter.Logout();

            return -1;
        }
    }
}