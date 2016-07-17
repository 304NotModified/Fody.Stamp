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

using System.Runtime.InteropServices;

namespace Fody.PeImage
{
    /// <summary>
    /// Each resource data entry describes a leaf node in the resource directory tree.
    /// It contains an offset, relative to the beginning of the resource
    /// directory of the data for the resource, a size field that gives the number
    /// of bytes of data at that offset, a CodePage that should be used when
    /// decoding code point values within the resource data.
    /// Typically for new applications the code page would be the unicode code page.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_RESOURCE_DATA_ENTRY
    {
        /// <summary>
        /// The OffsetToData and Size fields specify the location (as a relative virtual address within the resource section)
        /// and size (in bytes) of the resource data.
        /// Although an RVA is not the same as a file offset, the equivalent file offset can be calculated by subtracting
        /// the resource section's RVA from OffsetToData's RVA value, and adding the difference to the offset of the root directory.
        /// </summary>
        public uint OffsetToData;

        /// <summary>
        /// Size of resource directory
        /// </summary>
        public uint Size;

        /// <summary>
        /// The CodePage field identifies the code page (a coded character set) used to decode
        /// code points (code page values) within the resource data.
        /// Although any valid code page number can appear in this field (such as 437, which describes the
        /// original IBM PC's character set, or 65501, which describes Unicode UTF-8),
        /// this field often contains 0 (standard Roman alphabet, numerals, punctuation, accented characters).
        /// </summary>
        public uint CodePage;

        /// <summary>
        /// Reserved. Must be zero.
        /// </summary>
        public uint Reserved;
    }
}
