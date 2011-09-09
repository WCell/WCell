/*************************************************************************
 *
 *   file		: ObjectPool.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-08-15 05:57:30 +0200 (fr, 15 aug 2008) $
 
 *   revision		: $Rev: 594 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Core.Initialization;

namespace WCell.AuthServer
{
    /// <summary>
    /// Classes for initializing types in the object pool.
    /// </summary>
    public class ObjectPool
    {
        /// <summary>
        /// Initializes all required types for object pool usage.
        /// </summary>
        [Initialization(InitializationPass.Fifth, "Initialize object pool types")]
        public static void InitializeObjectPoolTypes()
        {
        }
    }
}