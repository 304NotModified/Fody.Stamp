// ReSharper disable CommentTypo
// Copyright(c) 2014 Quamotion bvba
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;

namespace Fody.PeImage
{
    /// <summary>
    /// Represents parts of a <see cref="Stream"/>, from a start byte offset for a given length.
    /// </summary>
    public class SubStream : Stream
    {
        /// <summary>
        /// The parent stream of this stream.
        /// </summary>
        private Stream stream;

        /// <summary>
        /// The offset, that is, the location at which the <see cref="SubStream"/> starts.
        /// </summary>
        private long subStreamOffset;

        /// <summary>
        /// The length of the <see cref="SubStream"/>.
        /// </summary>
        private long subStreamLength;

        /// <summary>
        /// A value indicating whether the parent stream should be closed when this
        /// <see cref="SubStream"/> is closed, or not.
        /// </summary>
        private bool leaveParentOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubStream"/> class.
        /// </summary>
        /// <param name="stream">
        /// The parent stream of this stream.
        /// </param>
        /// <param name="offset">
        /// The offset at which the stream starts.
        /// </param>
        /// <param name="length">
        /// The length of the <see cref="SubStream"/>.
        /// </param>
        /// <param name="leaveParentOpen">
        /// A value indicating whether the parent stream should be closed when this
        /// <see cref="SubStream"/> is closed, or not.
        /// </param>
        public SubStream(Stream stream, long offset, long length, bool leaveParentOpen = false)
        {
            this.stream = stream;
            subStreamOffset = offset;
            subStreamLength = length;
            this.leaveParentOpen = leaveParentOpen;

            Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return stream.CanRead;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return stream.CanSeek;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return stream.CanWrite; }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                return subStreamLength;
            }
        }

        /// <summary>
        /// Gets or sets the position within the current <see cref="SubStream"/>.
        /// </summary>
        public override long Position
        {
            get
            {
                return stream.Position - Offset;
            }

            set
            {
                stream.Position = value + Offset;
            }
        }

        /// <summary>
        /// Gets the parent stream of this <see cref="SubStream"/>.
        /// </summary>
        internal Stream Stream
        {
            get
            {
                return stream;
            }
        }

        /// <summary>
        /// Gets the offset at which the <see cref="SubStream"/> starts.
        /// </summary>
        internal long Offset
        {
            get
            {
                return subStreamOffset;
            }
        }

        /// <summary>
        /// Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
        /// Instead of calling this method, ensure that the stream is properly disposed.
        /// </summary>
        public override void Close()
        {
            if (!leaveParentOpen)
            {
                stream.Close();
            }

            base.Close();
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            stream.Flush();
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset
        /// and (offset + count - 1) replaced by the bytes read from the current source.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin storing the data read from the current stream.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to be read from the current stream.
        /// </param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are
        /// not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // Make sure we don't pass the size of the substream
            var bytesRemaining = Length - Position;
            var bytesToRead = Math.Min(count, bytesRemaining);

            return stream.Read(buffer, offset, (int)bytesToRead);
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position
        /// within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. This method copies count bytes from buffer to the current stream.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin copying bytes to the current stream.
        /// </param>
        /// <param name="count">
        /// The number of bytes to be written to the current stream.
        /// </param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">
        /// The byte to write to the stream.
        /// </param>
        public override void WriteByte(byte value)
        {
            stream.WriteByte(value);
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">
        /// A byte offset relative to the origin parameter.
        /// </param>
        /// <param name="origin">
        /// A value of type SeekOrigin indicating the reference point used to obtain the new position.
        /// </param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    offset += subStreamOffset;
                    break;

                case SeekOrigin.End:
                    var enddelta = subStreamOffset + subStreamLength - stream.Length;
                    offset += enddelta;
                    break;
                case SeekOrigin.Current:
                    offset += subStreamOffset;
                    break;
            }

            return stream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">
        /// The desired length of the current stream in bytes.
        /// </param>
        public override void SetLength(long value)
        {
            subStreamLength = value;
        }

        /// <summary>
        /// Updates the size of this <see cref="SubStream"/> relative to its parent stream.
        /// </summary>
        /// <param name="offset">
        /// The new offset.
        /// </param>
        /// <param name="length">
        /// The new length.
        /// </param>
        public void UpdateWindow(long offset, long length)
        {
            subStreamOffset = offset;
            subStreamLength = length;
        }
    }
}
