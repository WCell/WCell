using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.MPQTool.StormLibWrapper
{
    public enum OpenArchiveFlags : uint
    {
        /// <summary>
        /// Always creates new MPQ. 
        /// Fails if the file exists or not an MPQ.
        /// </summary>
        CreateNew = 0x0001,
        /// <summary>
        /// Always creates new MPQ. 
        /// If the file exists, it will be overwriten
        /// </summary>
        ForceCreate = 0x0002,
        /// <summary>
        /// Opens an existing MPQ. 
        /// If the file doesn't exist, the function fails.
        /// </summary>
        OpenExisting = 0x0003,
        /// <summary>
        /// Opens an existing MPQ. 
        /// If the file doesn't exist or it's not an MPQ, the function creates new MPQ.
        /// </summary>
        OpenOrCreate  = 0x0004, 
        /// <summary>
        /// Don't load the internal listfile
        /// </summary>
        IgnoreListFile = 0x0010,
        /// <summary>
        /// Don't open the attributes
        /// </summary>
        IgnoreAttributes = 0x0020,
        /// <summary>
        /// Always open the archive as MPQ v 1.00, ignore the "wFormatVersion" variable in the header
        /// </summary>
        OpenAsV1_0 = 0x0040,
        /// <summary>
        /// On files with MPQ_FILE_SECTOR_CRC, the CRC will be checked when reading file
        /// </summary>
        ForceCRCCheck = 0x0080,
        /// <summary>
        /// Also add the (attributes) file
        /// </summary>
        CreateAttributes = 0x0100,
        /// <summary>
        /// Creates archive with size up to 4GB
        /// </summary>
        CreateArchiveV1_0 = 0x00000000,
        /// <summary>
        /// Create archive with size > 4GB
        /// </summary>
        CreateArchiveV2_0 = 0x00010000
    }
}
