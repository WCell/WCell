/*************************************************************************
 *
 *   file		: IObjectPool.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-16 21:33:51 +0100 (lø, 16 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1197 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

namespace Cell.Core
{
	/// <summary>
	/// Interface for an object pool.
	/// </summary>
	/// <seealso cref="zzObjectPoolMgr"/>
	/// <remarks>
	/// An object pool holds reusable objects. See <see cref="zzObjectPoolMgr"/> for more details.
	/// </remarks>
	public interface IObjectPool
	{
		/// <summary>
		/// Amount of available objects in pool
		/// </summary>
		int AvailableCount
		{
			get;
		}

		/// <summary>
		/// Amount of objects that have been obtained but not recycled.
		/// </summary>
		int ObtainedCount
		{
			get;
		}

		/// <summary>
		/// Enqueues an object in the pool to be reused.
		/// </summary>
		/// <param name="obj">The object to be put back in the pool.</param>
		void Recycle(object obj);

		/// <summary>
		/// Grabs an object from the pool.
		/// </summary>
		/// <returns>An object from the pool.</returns>
		object ObtainObj();
	}
}