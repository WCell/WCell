/*************************************************************************
 *
 *   file		: IUsable.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 19:35:36 +0800 (Thu, 31 Jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

namespace WCell.RealmServer.Entities
{
    /// <summary>
    /// Anything that is usable: Items and GameObjects so far
    /// </summary>
    public interface IOwned
    {
        // TODO: Add general methods for usability?

        Unit Owner
        {
            get;
        }
    }
}