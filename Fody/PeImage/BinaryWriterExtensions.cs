// ReSharper disable CommentTypo
// Copyright(c) 2016 Frederik Carlier
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
using System.Runtime.InteropServices;
using System.Text;

namespace Fody.PeImage
{
    /// <summary>
    /// Provides extension methods to the <see cref="BinaryWriter"/> class.
    /// </summary>
    internal static class BinaryWriterExtensions
    {
        /// <summary>
        /// Writes a struct at the current position.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the struct to write.
        /// </typeparam>
        /// <param name="writer">
        /// The writer to write the struct to.
        /// </param>
        /// <param name="value">
        /// The struct to write.
        /// </param>
        public static void WriteStruct<T>(this BinaryWriter writer, T value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var size = Marshal.SizeOf(typeof(T));
            var bytes = new byte[size];

            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            writer.Write(bytes);
        }

        /// <summary>
        /// Writes a null-terminated Unicode string.
        /// </summary>
        /// <param name="writer">
        /// The writer to read the string to.
        /// </param>
        /// <param name="value">
        /// The string to write.
        /// </param>
        public static void WriteUnicodeString(this BinaryWriter writer, string value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var data = Encoding.Unicode.GetBytes(value);
            writer.Write(data);

            if (value.Length == 0 || value[value.Length - 1] != '\0')
            {
                writer.Write(new byte[] { 0, 0 });
            }
        }

        /// <summary>
        /// Aligns the position of the underlying <see cref="Stream"/> to a 32-boundary.
        /// </summary>
        /// <param name="writer">
        /// The writer for which to align the underlying stream.
        /// </param>
        public static void Align(this BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var current = writer.BaseStream.Position;
            var actual = Helpers.Align(current);

            var padding = actual - current;

            for (var i = 0; i < padding; i++)
            {
                writer.Write((byte)0);
            }
        }
    }
}
