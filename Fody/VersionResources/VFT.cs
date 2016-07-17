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

namespace Fody.VersionResources
{
    /// <summary>
    /// The general type of file.
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646997(v=vs.85).aspx"/>
    [Flags]
    public enum VFT
    {
        /// <summary>
        /// The file contains an application.
        /// </summary>
        APP = 0x00000001,

        /// <summary>
        /// The file contains a DLL.
        /// </summary>
        DLL = 0x00000002,

        /// <summary>
        /// The file contains a device driver. If dwFileType is VFT_DRV, dwFileSubtype contains a more specific description of the driver.
        /// </summary>
        DRV = 0x00000003,

        /// <summary>
        /// The file contains a font. If dwFileType is VFT_FONT, dwFileSubtype contains a more specific description of the font file.
        /// </summary>
        FONT = 0x00000004,

        /// <summary>
        /// The file contains a static-link library.
        /// </summary>
        STATIC_LIB = 0x00000007,

        /// <summary>
        /// The file type is unknown to the system.
        /// </summary>
        UNKNOWN = 0x00000000,

        /// <summary>
        /// The file contains a virtual device.
        /// </summary>
        VXD = 0x00000005,
    }
}
