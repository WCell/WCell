using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.MPQTool.StormLibWrapper
{
    public enum OpenFileFlags : uint
    {
        /// <summary>
        /// Open the file from the MPQ archive
        /// </summary>
        FromMPQ = 0x00000000, 
        /// <summary>
        /// The 'szFileName' parameter is actually the file index
        /// </summary>
        ByIndex = 0x00000001, 
        /// <summary>
        /// Reserved for StormLib internal use
        /// </summary>
        AnyLocale_Reserved = 0x00000002,
        /// <summary>
        /// Open the file from the MPQ archive
        /// </summary>
        LocalFile = 0xFFFFFFFF 
    }
}