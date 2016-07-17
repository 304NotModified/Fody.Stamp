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

namespace Fody.PeImage
{
    /// <summary>
    /// Provides helper methods for working with WinPE files.
    /// </summary>
    internal class Helpers
    {
        /// <summary>
        /// Retrieves the high-order word from the specified 32-bit value.
        /// </summary>
        /// <param name="value">
        /// The value to be converted.
        /// </param>
        /// <returns>
        /// The high-order word of the specified value.
        /// </returns>
        public static ushort HiWord(uint value)
        {
            return (ushort)((value & 0xFFFF0000) >> 16);
        }

        /// <summary>
        /// Retrieves the low-order word from the specified value.
        /// </summary>
        /// <param name="value">
        /// The value to be converted.
        /// </param>
        /// <returns>
        /// The low-order word of the specified value.
        /// </returns>
        public static ushort LoWord(uint value)
        {
            return (ushort)(value & 0x0000FFFF);
        }

        /// <summary>
        /// Aligns a value to the next 32-bit boundary.
        /// </summary>
        /// <param name="value">
        /// The value to align.
        /// </param>
        /// <returns>
        /// The aligned value.
        /// </returns>
        public static long Align(long value)
        {
            return value + 3 & 0xFFFFFFFC;
        }
    }
}
