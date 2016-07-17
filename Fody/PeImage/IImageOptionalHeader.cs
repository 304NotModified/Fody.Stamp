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
    /// Provides a common interface for the <see cref="IMAGE_OPTIONAL_HEADER_32"/> and <see cref="IMAGE_OPTIONAL_HEADER_64"/> structures.
    /// </summary>
    internal interface IImageOptionalHeader
    {
        /// <summary>
        /// Gets the number of directory entries in the remainder of the optional header.
        /// Each entry describes a location and size.
        /// </summary>
        uint NumberOfRvaAndSizes { get; }

        /// <summary>
        /// Gets the state of the image file.
        /// </summary>
        MagicType Magic { get; }

        /// <summary>
        /// Gets the preferred address of the first byte of the image when it is loaded in memory.
        /// This value is a multiple of 64K bytes. The default value for DLLs is <c>0x10000000</c>.
        /// The default value for applications is <c>0x00400000</c>, except on Windows CE
        /// where it is <c>0x00010000</c>.
        /// </summary>
        ulong ImageBase { get; }

        /// <summary>
        /// Gets the image file checksum.
        /// </summary>
        uint CheckSum { get; }

        /// <summary>
        /// Gets the offset at which the checksum is stored
        /// </summary>
        int CheckSumOffset { get; }
    }
}
