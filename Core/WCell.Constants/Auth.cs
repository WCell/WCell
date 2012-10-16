/*************************************************************************
 *
 *   file		    : ClientInformationUtil.cs
 *   copyright      : (C) The WCell Team
 *   email		    : info@wcell.org
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

namespace WCell.Constants
{
    public enum ClientType
    {
        Test = 0,
        Beta = 1,
        Normal = 2,
        Installing = 3,
        Invalid = 4
    }

    /// <summary>
    /// Possible operating systems of the client
    /// </summary>
    public enum OperatingSystem
    {
        /// <summary>
        /// Mac OSX
        /// </summary>
        OSX,
        /// <summary>
        /// Any supported version of Windows
        /// </summary>
        Win
    }

    /// <summary>
    /// Possible CPU architectures of the client
    /// </summary>
    public enum ProcessorArchitecture
    {
        /// <summary>
        /// x86 architecture (AMD, Intel, Via)
        /// </summary>
        x86,
        /// <summary>
        /// PowerPC architecture (all pre-2006Q1 Apple computers)
        /// </summary>
        PPC
    }
}
