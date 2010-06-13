using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace StormLibWrapper
{
    /*
    // Storm file function prototypes
    BOOL  WINAPI SFILE(OpenArchive)(LPCSTR lpFileName, DWORD dwPriority, DWORD dwFlags, HANDLE *hMPQ);
    BOOL  WINAPI SFILE(CloseArchive)(HANDLE hMPQ);
    BOOL  WINAPI SFILE(GetArchiveName)(HANDLE hMPQ, LPCSTR lpBuffer, DWORD dwBufferLength);
    BOOL  WINAPI SFILE(OpenFile)(LPCSTR lpFileName, HANDLE *hFile);
    BOOL  WINAPI SFILE(OpenFileEx)(HANDLE hMPQ, LPCSTR lpFileName, DWORD dwSearchScope, HANDLE *hFile);
    BOOL  WINAPI SFILE(CloseFile)(HANDLE hFile);
    DWORD WINAPI SFILE(GetFileSize)(HANDLE hFile, LPDWORD lpFileSizeHigh);
    BOOL  WINAPI SFILE(GetFileArchive)(HANDLE hFile, HANDLE *hMPQ);
    BOOL  WINAPI SFILE(GetFileName)(HANDLE hFile, LPCSTR lpBuffer, DWORD dwBufferLength);
    DWORD WINAPI SFILE(SetFilePointer)(HANDLE hFile, long lDistanceToMove, PLONG lplDistanceToMoveHigh, DWORD dwMoveMethod);
    BOOL  WINAPI SFILE(ReadFile)(HANDLE hFile,LPVOID lpBuffer,DWORD nNumberOfBytesToRead,LPDWORD lpNumberOfBytesRead,LPOVERLAPPED lpOverlapped);
    LCID  WINAPI SFILE(SetLocale)(LCID nNewLocale);
    BOOL  WINAPI SFILE(GetBasePath)(LPCSTR lpBuffer, DWORD dwBufferLength);
    BOOL  WINAPI SFILE(SetBasePath)(LPCSTR lpNewBasePath);

    // Storm (de)compression functions
    BOOL  WINAPI SCOMP(Compress)  (char * pbOutBuffer, int * pdwOutLength, char * pbInBuffer, int dwInLength, int uCmp, int uCmpType, int nCmpLevel);
    BOOL  WINAPI SCOMP(Decompress)(char * pbOutBuffer, int * pdwOutLength, char * pbInBuffer, int dwInLength);
*/

    /// <summary>
    /// Contains the native StormLib methods used by the MPQ classes.
    /// </summary>
    public static class NativeMethods
    {
        public const string StormLibDllName = "StormLib.dll";
        public const string StormLibFolder = "../../../../StormLib/Bin/";

        public static void InitAPI()
        {
            // must do this, so it finds the dlls and other files (since those paths arent configurable)
            //Environment.CurrentDirectory = StormLibFolder;

            // copy over necessary dll files
            //foreach (var dll in Dlls)
            //{
            //    EnsureDll(dll);
            //}
        }

        private static void EnsureDll(string dllName)
        {
            var targetFile = new FileInfo(Path.Combine(Environment.CurrentDirectory, dllName));
            if (!targetFile.Exists)
            {
                var file = new FileInfo(Path.Combine(StormLibFolder, dllName));
                file.CopyTo(targetFile.FullName);
            }
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
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenFileEx(IntPtr archiveHandle, [In] [MarshalAs(UnmanagedType.LPStr)] string fileName, OpenFileFlags searchScope, out IntPtr fileHandle);


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


        /// Return Type: BOOL->int
        ///hFile: HANDLE->void*
        ///hMPQ: HANDLE*
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
