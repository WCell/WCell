using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WCell.MPQTool.StormLibWrapper
{
    public class MpqStream : Stream
    {
        private readonly MpqFile mpqFile;
        private readonly IntPtr fileHandle;
        private long position;

        /// <summary>
        /// Initializes a new instance of the <see cref="MpqStream"/> class.
        /// </summary>
        /// <param name="mpqFile">The MPQ file that owns this stream.</param>
        /// <param name="fileHandle">The file handle.</param>
        public MpqStream(MpqFile mpqFile, IntPtr fileHandle)
        {
            if (mpqFile == null)
                throw new ArgumentNullException("mpqFile");
            if (fileHandle == IntPtr.Zero)
                throw new ArgumentNullException("fileHandle");

            this.mpqFile = mpqFile;
            this.fileHandle = fileHandle;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Flush()
        {
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <value></value>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Length
        {
            get
            {
                return MpqFile.GetFileSize(fileHandle);
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <value></value>
        /// <returns>The current position within the stream.</returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get { return position; }
            set
            {
                var distanceToMove = (int)value;
                var distanceToMoveHigh = (int)(value >> 32);

                uint newPosition = NativeMethods.SetFilePointer(fileHandle, distanceToMove, ref distanceToMoveHigh, (uint)SeekOrigin.Begin);

                if (newPosition == 0xFFFFFFFF)
                    throw new StormLibException("Could not set file pointer.");

                position = ((long)distanceToMoveHigh << 32) + newPosition;
            }
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="buffer"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Count must be positive.");
            if (buffer.Length < offset + count)
                throw new ArgumentException("Buffer is too small to hold data.", "buffer");

            if (count == 0)
                return 0;

            // Allocate a native buffer for the file content
            IntPtr nativeBuffer = Marshal.AllocCoTaskMem(count);

            uint bytesRead;
            NativeMethods.ReadFile(fileHandle, nativeBuffer, (uint)count, out bytesRead, (IntPtr)null);

            // Copy between the native and the managed buffer
            Marshal.Copy(nativeBuffer, buffer, 0, (int)bytesRead);

            // Free up the memory allocated for the native buffer
            Marshal.FreeCoTaskMem(nativeBuffer);

            // Update position
            position += bytesRead;

            // Return the number of bytes read
            return (int)bytesRead;
        }

        /// <summary>
        /// Reads from the stream and returns a native buffer. The buffer must be freed using <see cref="Marshal.FreeCoTaskMem"/>.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <param name="buffer">The native buffer that contains the bytes read.</param>
        /// <returns>The number of bytes read.</returns>
        public int ReadCoTaskMem(int count, out IntPtr buffer)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Count must be positive.");

            buffer = Marshal.AllocCoTaskMem(count);

            uint bytesRead;
            ErrorHandler.ThrowOnFailure(NativeMethods.ReadFile(fileHandle, buffer, (uint)count, out bytesRead, (IntPtr)null), "Could not read file content.");

            // Update position
            position += bytesRead;

            return (int)bytesRead;
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            var distanceToMove = (int)offset;
            var distanceToMoveHigh = (int)(Math.Abs(offset) >> 32) * Math.Sign(offset);

            uint newPosition = NativeMethods.SetFilePointer(fileHandle, distanceToMove, ref distanceToMoveHigh, (uint)origin);

            if (newPosition == 0xFFFFFFFF)
                throw new StormLibException("Could not set file pointer.");

            return position = ((long)distanceToMoveHigh << 32) + newPosition;
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void SetLength(long value)
        {
            throw new InvalidOperationException("Stream is read-only.");
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="buffer"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("Stream is read-only.");
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (mpqFile != null)
                    mpqFile.Dispose();
            }
        }
    }
}