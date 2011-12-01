using System.Runtime.InteropServices;

namespace WCell.MPQTool.StormLibWrapper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FileFindData
    {
        /// <summary>
        /// Name of the found file
        /// </summary>
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH)] public string FilePath;

        /// <summary>
        /// Plain name of the found file
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)] public string PlainName;

        /// <summary>
        /// Hash table index for the file
        /// </summary>
        public uint HashIndex;

        /// <summary>
        /// Block table index for the file
        /// </summary>
        public uint BlockIndex;

        /// <summary>
        /// Uncompressed size of the file, in bytes
        /// </summary>
        public uint FileSize;

        /// <summary>
        /// MPQ file flags
        /// </summary>
        public uint FileFlags;
        /// <summary>
        /// Compressed file size
        /// </summary>
        public uint CompSize;

        /// <summary>
        /// Low 32-bits of the file time (0 if not present)
        /// </summary>
        public uint FileTimeLo;

        /// <summary>
        /// High 32-bits of the file time (0 if not present)
        /// </summary>
        public uint FileTimeHi;

        /// <summary>
        /// Locale version
        /// </summary>
        public uint Locale;
    }
}