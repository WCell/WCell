using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WCell.MPQTool.StormLibWrapper
{
    /// <summary>
    /// Contains the native StormLib methods used by the MPQ classes.
    /// </summary>
    public static class NativeMethods
    {
        public const int MAX_PATH = 260;
        public const string StormLibDllName = "StormLib.dll";
        public static string StormLibFolder = "../../Libraries/Binaries/Release/Stormlib/";

        static NativeMethods()
        {
            //InitAPI();
        }

        public static string StormLib64
        {
            get
            {
                return (StormLibFolder + "StormLib64.dll");
            }
        }

        public static string StormLib32
        {
            get
            {
                return (StormLibFolder + "StormLib32.dll");
            }
        }

        public static void InitAPI()
        {
            if (Environment.Is64BitProcess)
            {
                File.Copy(Path.GetFullPath(StormLib64), StormLibDllName, true);
                return;
            }

            File.Copy(Path.GetFullPath(StormLib32), StormLibDllName, true);
        }

        /// Return Type: BOOL->int
        ///lpFileName: LPCSTR->CHAR*
        ///dwPriority: DWORD->unsigned int
        ///dwFlags: DWORD->unsigned int
        ///hMPQ: HANDLE*
        [DllImport(StormLibDllName, EntryPoint = "SFileOpenArchive")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenArchive([In] [MarshalAs(UnmanagedType.LPStr)] string fileName, uint priority, OpenArchiveFlags flags, out IntPtr archiveHandle);

        /// Return Type: BOOL->int
        ///hMPQ: HANDLE->void*
        [DllImport(StormLibDllName, EntryPoint = "SFileCloseArchive")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseArchive(IntPtr archiveHandle);

        /// Return Type: BOOL->int
        ///hMPQ: HANDLE->void*
        ///lpBuffer: LPCSTR->CHAR*
        ///dwBufferLength: DWORD->unsigned int
        [DllImport(StormLibDllName, EntryPoint = "SFileGetArchiveName")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetArchiveName(IntPtr archiveHandle, [In] [MarshalAs(UnmanagedType.LPStr)] string bffer, uint bufferLength);

        /// Return Type: BOOL->int
        ///hMPQ: HANDLE->void*
        ///lpFileName: LPCSTR->CHAR*
        ///dwSearchScope: DWORD->unsigned int
        ///hFile: HANDLE*
        [DllImport(StormLibDllName, EntryPoint = "SFileOpenFileEx")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        public static extern IntPtr OpenFileEx(IntPtr archiveHandle, [In] [MarshalAs(UnmanagedType.LPStr)] string fileName, OpenFileFlags searchScope, out IntPtr fileHandle);

        [DllImport(StormLibDllName, EntryPoint = "SFileHasFile")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        public static extern IntPtr HasFile(IntPtr archiveHandle, [In] [MarshalAs(UnmanagedType.LPStr)] string fileName);

        /// <summary>
        /// Returns data about the first file in the ListFile that matches the given search mask
        /// </summary>
        /// <param name="archiveHandle">Handle of the open archive.</param>
        /// <param name="fileName">Name of the listfile that will be used for searching.
        /// If this parameter is NULL, the function searches the MPQ internal listfile (if any).</param>
        /// <param name="searchMask">Name of the search mask. "*" will return all files.</param>
        /// <param name="fileData">A FileFindData structure with the search result.</param>
        /// <returns>A handle for the Search object for use with ListFileFindNext.</returns>
        [DllImport(StormLibDllName, EntryPoint = "SListFileFindFirstFile")]
        public static extern IntPtr ListFileFindFirst(IntPtr archiveHandle, [In] [MarshalAs(UnmanagedType.LPStr)] string fileName, [In] [MarshalAs(UnmanagedType.LPStr)] string searchMask, out FileFindData fileData);

        /// <summary>
        /// Finds the next file in the list file with the attributes defined in ListFileFindFirst.
        /// </summary>
        /// <param name="searchHandle">Handle to the search object created with ListFileFindFirst</param>
        /// <param name="fileData">A FileFindData structure with the search result.</param>
        /// <returns>A handle for the updated Search object</returns>
        [DllImport(StormLibDllName, EntryPoint = "SListFileFindNextFile")]
        public static extern IntPtr ListFileFindNext(IntPtr searchHandle, out FileFindData fileData);

        [DllImport(StormLibDllName, EntryPoint = "SListFileFindClose")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseSearch(IntPtr searchHandle);

        /// Return Type: BOOL->int
        ///hFile: HANDLE->void*
        [DllImport(StormLibDllName, EntryPoint = "SFileCloseFile")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseFile(IntPtr fileHandle);

        /// Return Type: DWORD->unsigned int
        ///hFile: HANDLE->void*
        ///lpFileSizeHigh: LPDWORD->DWORD*
        [DllImport(StormLibDllName, EntryPoint = "SFileGetFileSize")]
        public static extern uint GetFileSize(IntPtr fileHandle, out uint fileSize);

        [DllImport(StormLibDllName, EntryPoint = "SFileGetFileArchive")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileArchive(IntPtr fileHandle, out IntPtr archiveHandle);

        /// Return Type: BOOL->int
        ///hFile: HANDLE->void*
        ///lpBuffer: LPCSTR->CHAR*
        ///dwBufferLength: DWORD->unsigned int
        [DllImport(StormLibDllName, EntryPoint = "SFileGetFileName")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileName(IntPtr fileHandle, [In] [MarshalAs(UnmanagedType.LPStr)] string buffer, uint bufferLength);

        /// Return Type: DWORD->unsigned int
        ///hFile: HANDLE->void*
        ///lDistanceToMove: int
        ///lplDistanceToMoveHigh: PLONG->LONG*
        ///dwMoveMethod: DWORD->unsigned int
        [DllImport(StormLibDllName, EntryPoint = "SFileSetFilePointer")]
        public static extern uint SetFilePointer(IntPtr fileHandle, int distanceToMove, ref int distanceToMoveHigh, uint moveMethod);

        /// Return Type: BOOL->int
        ///hFile: HANDLE->void*
        ///lpBuffer: LPVOID->void*
        ///nNumberOfBytesToRead: DWORD->unsigned int
        ///lpNumberOfBytesRead: LPDWORD->DWORD*
        ///lpOverlapped: LPOVERLAPPED->_OVERLAPPED*
        [DllImport(StormLibDllName, EntryPoint = "SFileReadFile")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadFile(IntPtr fileHandle, IntPtr buffer, uint numberOfBytes, out uint numberOfBytesRead, IntPtr overlapped);

        /// Return Type: LCID->DWORD->unsigned int
        ///nNewLocale: LCID->DWORD->unsigned int
        [DllImport(StormLibDllName, EntryPoint = "SFileSetLocale")]
        public static extern uint SetLocale(uint newLocale);

        /// Return Type: BOOL->int
        ///lpBuffer: LPCSTR->CHAR*
        ///dwBufferLength: DWORD->unsigned int
        [DllImport(StormLibDllName, EntryPoint = "SFileGetBasePath")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetBasePath([In] [MarshalAs(UnmanagedType.LPStr)] string buffer, uint bufferLength);

        /// Return Type: BOOL->int
        ///lpNewBasePath: LPCSTR->CHAR*
        [DllImport(StormLibDllName, EntryPoint = "SFileSetBasePath")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetBasePath([In] [MarshalAs(UnmanagedType.LPStr)] string newBasePath);

        /// Return Type: BOOL->int
        ///pbOutBuffer: char*
        ///pdwOutLength: int*
        ///pbInBuffer: char*
        ///dwInLength: int
        ///uCmp: int
        ///uCmpType: int
        ///nCmpLevel: int
        [DllImport(StormLibDllName, EntryPoint = "SCompCompress")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Compress(IntPtr outBuffer, ref int outLength, IntPtr inBuffer, int inLength, int cmp, int cmpType, int cmpLevel);

        /// Return Type: BOOL->int
        ///pbOutBuffer: char*
        ///pdwOutLength: int*
        ///pbInBuffer: char*
        ///dwInLength: int
        [DllImport(StormLibDllName, EntryPoint = "SCompDecompress")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Decompress(IntPtr outBuffer, ref int outLength, IntPtr inBuffer, int inLength);
    }
}