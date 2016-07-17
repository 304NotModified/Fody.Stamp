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
    /// The state of the image file.
    /// </summary>
    public enum MagicType : ushort
    {
        /// <summary>
        /// The executable image contains a 32-bit application.
        /// </summary>
        IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,

        /// <summary>
        /// The executable image contains a 32-bit application.
        /// </summary>
        IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b,

        /// <summary>
        /// The file is a ROM image.
        /// </summary>
        IMAGE_ROM_OPTIONAL_HDR_MAGIC = 0x107
    }
}
