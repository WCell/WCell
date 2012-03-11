using System;
using System.Collections.Generic;
using System.IO;

namespace WCell.MPQTool.StormLibWrapper
{
    /// <summary>
    /// Represents an MPQ archive.
    /// </summary>
    public class MpqArchive : IDisposable
    {
        private const OpenArchiveFlags flags = OpenArchiveFlags.OpenExisting;
        private const uint priority = 0;

        private IntPtr handle;

        /// <summary>
        /// Initializes a new instance of the <see cref="MpqArchive"/> class.
        /// </summary>
        /// <param name="path">The path to the MPQ archive.</param>
        public MpqArchive(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            ErrorHandler.ThrowOnFailure(NativeMethods.OpenArchive(path, priority, flags, out handle),
                                        "Could not open MPQ archive.");
        }

        /// <summary>
        /// Gets a value indicating whether the MPQ archive is open.
        /// </summary>
        public bool IsOpen
        {
            get { return handle != IntPtr.Zero; }
        }

        /// <summary>
        /// Checks for the existence of a file in the MPQ archive.
        /// </summary>
        /// <param name="filePath">The relative path to the file in the archive.</param>
        /// <returns>True, if the file exists in the MPQ archive; False otherwise.</returns>
        public bool FileExists(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");

            var retVal = NativeMethods.HasFile(handle, filePath);

            return (((long)retVal & 0xFF) > 0x00);
            //// Try to open the file
            //if (fileHandle == IntPtr.Zero)
            //    return false;

            //// File could be opened, immediately close it
            //NativeMethods.CloseFile(fileHandle);

            //return true;
        }

        /// <summary>
        /// Opens a file in the MPQ archive.
        /// </summary>
        /// <param name="filePath">The relative path to the file in the archive.</param>
        /// <returns>An instance of the <see cref="MpqFile"/> class, representing the file.</returns>
        /// <exception cref="StormLibException">Thrown when the file could not be opened.</exception>
        public MpqFile OpenFile(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");

            IntPtr fileHandle;
            var retVal = NativeMethods.OpenFileEx(handle, filePath, OpenFileFlags.FromMPQ, out fileHandle);
            var success = (((long)retVal & 0xFF) > 0x00);

            ErrorHandler.ThrowOnFailure((!success || (fileHandle != IntPtr.Zero)), "Could not open MPQ file.");

            // Create new MPQ file
            return new MpqFile(this, fileHandle);
        }

        /// <summary>
        /// Closes the MPQ archive.
        /// </summary>
        public void Close()
        {
            if (handle == IntPtr.Zero)
                throw new StormLibException("MPQ archive already closed.");

            // Close the archive
            NativeMethods.CloseArchive(handle);

            // Zero out the handle
            handle = IntPtr.Zero;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.IsOpen)
                this.Close();
        }

        /// <summary>
        /// Checks whether a file is a valid MPQ archive that can be opened.
        /// </summary>
        /// <param name="path">The path to the MPQ archive.</param>
        /// <returns>True, if the file is an MPQ archive; False otherwise.</returns>
        public static bool IsValidMpqArchive(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            // If the file doesn't exist, return false
            if (!File.Exists(path))
                return false;

            IntPtr handle;
            if (!NativeMethods.OpenArchive(path, priority, flags, out handle))
                return false;

            // Close the archive immediately
            NativeMethods.CloseArchive(handle);

            return true;
        }

        public List<string> FindAllFiles(string searchMask)
        {
            var pathList = new List<string>();

            string filePath;
            var searchHandle = FindFirstFile(searchMask, out filePath);
            if (searchHandle == IntPtr.Zero) return pathList;

            pathList.Add(filePath);

            while (FindNextFile(searchHandle, out filePath))
            {
                pathList.Add(filePath);
            }

            CloseSearch(searchHandle);
            return pathList;
        }

        private IntPtr FindFirstFile(string searchMask, out string filePath)
        {
            FileFindData data;
            var searchHandle = NativeMethods.ListFileFindFirst(handle, null, searchMask, out data);
            filePath = data.FilePath;

            return searchHandle;
        }

        private static bool FindNextFile(IntPtr searchHandle, out string filePath)
        {
            filePath = null;
            if (searchHandle == IntPtr.Zero) return false;

            FileFindData data;
            var handle = NativeMethods.ListFileFindNext(searchHandle, out data);
            var success = (((long)handle & 0xFF) != 0x00);

            if (!success) return false;

            if (data.FilePath == "") return false;

            filePath = data.FilePath;
            return true;
        }

        private static bool CloseSearch(IntPtr searchHandle)
        {
            return NativeMethods.CloseSearch(searchHandle);
        }
    }
}