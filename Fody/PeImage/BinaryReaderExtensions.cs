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
    /// Provides extension methods to the <see cref="BinaryReader"/> class making it easier to interact with WinPE file formats.
    /// </summary>
    internal static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads a struct from a binary stream.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the struct to read.
        /// </typeparam>
        /// <param name="reader">
        /// The reader to read the struct from.
        /// </param>
        /// <returns>
        /// A new instance of <typeparamref name="T"/>, with the value of the struct that was read.
        /// </returns>
        public static T ReadStruct<T>(this BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            var size = Marshal.SizeOf(typeof(T));
            var data = reader.ReadBytes(size);

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var value = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return value;
        }

        /// <summary>
        /// Reads a null-terminated Unicode string.
        /// </summary>
        /// <param name="reader">
        /// The reader to read the string from.
        /// </param>
        /// <returns>
        /// The string that was read from the reader.
        /// </returns>
        public static string ReadUnicodeString(this BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            var builder = new StringBuilder();

            char c;

            while ((c = reader.ReadUnicodeChar()) != '\0')
            {
                builder.Append(c);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Reads a Unicode character.
        /// </summary>
        /// <param name="reader">
        /// The reader to read the character from.
        /// </param>
        /// <returns>
        /// The character that was read.
        /// </returns>
        public static char ReadUnicodeChar(this BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            var data = reader.ReadBytes(2);
            return Encoding.Unicode.GetString(data)[0];
        }

        /// <summary>
        /// Alignes the underlying string to a 32-bit (4-byte) boundary.
        /// </summary>
        /// <param name="reader">
        /// The reader to align.
        /// </param>
        public static void Align(this BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            var stream = reader.BaseStream;
            stream.Position = Helpers.Align(stream.Position);
        }
    }
}
