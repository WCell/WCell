using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StormLibWrapper
{
    /// <summary>
    /// Represents an MPQ file.
    /// </summary>
    public class MpqFile : IDisposable
    {
        private IntPtr fileHandle;

        private readonly MpqArchive archive;
        private MpqStream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="MpqFile"/> class.
        /// </summary>
        /// <param name="archive">The MPQ archive this file is in.</param>
        /// <param name="fileHandle">The file handle.</param>
        public MpqFile(MpqArchive archive, IntPtr fileHandle)
        {
            if (archive == null)
                throw new ArgumentNullException("archive");
            if (fileHandle == IntPtr.Zero)
                throw new ArgumentNullException("fileHandle");

            this.archive = archive;
            this.fileHandle = fileHandle;
        }

        /// <summary>
        /// Gets the MPQ archive this file is in.
        /// </summary>
        public MpqArchive Archive
        {
            get { return archive; }
        }

        /// <summary>
        /// Gets a value indicating whether the file is open.
        /// </summary>
        public bool IsOpen
        {
            get { return fileHandle != IntPtr.Zero; }
        }

        /// <summary>
        /// Gets the size of the file.
        /// </summary>
        public long Size
        {
            get
            {
                return MpqFile.GetFileSize(fileHandle);
            }
        }

        /// <summary>
        /// Gets a stream of the content of the file.
        /// </summary>
        /// <returns>An instance of the <see cref="MpqStream"/> class.</returns>
        public MpqStream GetStream()
        {
            if (stream == null)
                stream = new MpqStream(this, fileHandle);

            return stream;
        }

        /// <summary>
        /// Closes the file.
        /// </summary>
        public void Close()
        {
            if (fileHandle == IntPtr.Zero)
                throw new InvalidOperationException("MPQ file is not open.");

            // Close the file
            NativeMethods.CloseFile(fileHandle);

            // Zero out the handle
            fileHandle = IntPtr.Zero;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // TODO: Implement correct IDisposable pattern with notion of MpqStream disposing
            if (this.IsOpen)
                this.Close();
        }

        /// <summary>
        /// Gets the file size of an MPQ file using its handle.
        /// </summary>
        /// <param name="fileHandle">The handle of the MPQ file.</param>
        /// <returns>The size of the file, in bytes.</returns>
        public static long GetFileSize(IntPtr fileHandle)
        {
            uint highBits;
            uint lowBits = NativeMethods.GetFileSize(fileHandle, out highBits);

            return (highBits << 32) + lowBits;
        }
    }
}
