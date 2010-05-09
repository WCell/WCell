/*************************************************************************
 *
 *   file		: IGroupConverter.cs
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

namespace WCell.RealmServer.Groups
{
	/// <summary>
	/// Defines an interface that allows one type of group to convert itself into another.
	/// </summary>
	/// <typeparam name="T">the type of group to convert to</typeparam>
    interface IGroupConverter<T> where T : Group
    {
		/// <summary>
		/// Converts one type of group to another.
		/// </summary>
		/// <returns>the newly converter group object</returns>
        T ConvertTo();
    }
}
