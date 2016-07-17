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
    /// The operating system for which this file was designed.
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646997(v=vs.85).aspx"/>
    [Flags]
    public enum VOS
    {
        /// <summary>
        /// The file was designed for MS-DOS.
        /// </summary>
        DOS = 0x00010000,

        /// <summary>
        /// The file was designed for Windows NT.
        /// </summary>
        NT = 0x00040000,

        /// <summary>
        /// The file was designed for 16-bit Windows.
        /// </summary>
        WINDOWS16 = 0x00000001,

        /// <summary>
        /// The file was designed for 32-bit Windows.
        /// </summary>
        WINDOWS32 = 0x00000004,

        /// <summary>
        /// The file was designed for 16-bit OS/2.
        /// </summary>
        OS216 = 0x00020000,

        /// <summary>
        /// The file was designed for 32-bit OS/2.
        /// </summary>
        OS232 = 0x00030000,

        /// <summary>
        /// The file was designed for 16-bit Presentation Manager.
        /// </summary>
        PM16 = 0x00000002,

        /// <summary>
        /// The file was designed for 32-bit Presentation Manager.
        /// </summary>
        PM32 = 0x00000003,

        /// <summary>
        /// The operating system for which the file was designed is unknown to the system.
        /// </summary>
        UNKNOWN = 0x00000000,

        /// <summary>
        /// The file was designed for 16-bit Windows running on MS-DOS.
        /// </summary>
        DOS_WINDOWS16 = DOS | WINDOWS16,

        /// <summary>
        /// The file was designed for 32-bit Windows running on MS-DOS.
        /// </summary>
        DOS_WINDOWS32 = DOS | WINDOWS32,

        /// <summary>
        /// The file was designed for Windows NT.
        /// </summary>
        NT_WINDOWS32 = NT | WINDOWS32,

        /// <summary>
        /// The file was designed for 16-bit Presentation Manager running on 16-bit OS/2.
        /// </summary>
        OS216_PM16 = OS216 | PM16,

        /// <summary>
        /// The file was designed for 32-bit Presentation Manager running on 32-bit OS/2.
        /// </summary>
        OS232_PM32 = OS232 | PM32,
    }
}
