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
    /// The function of the file.
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646997(v=vs.85).aspx"/>
    [Flags]
    public enum VFT2
    {
        /// <summary>
        /// The file contains a communications driver.
        /// </summary>
        DRV_COMM = 0x0000000A,

        /// <summary>
        /// The file contains a display driver.
        /// </summary>
        DRV_DISPLAY = 0x00000004,

        /// <summary>
        /// The file contains an installable driver.
        /// </summary>
        DRV_INSTALLABLE = 0x00000008,

        /// <summary>
        /// The file contains a keyboard driver.
        /// </summary>
        DRV_KEYBOARD = 0x00000002,

        /// <summary>
        /// The file contains a language driver.
        /// </summary>
        DRV_LANGUAGE = 0x00000003,

        /// <summary>
        /// The file contains a mouse driver.
        /// </summary>
        DRV_MOUSE = 0x00000005,

        /// <summary>
        /// The file contains a network driver.
        /// </summary>
        DRV_NETWORK = 0x00000006,

        /// <summary>
        /// The file contains a printer driver.
        /// </summary>
        DRV_PRINTER = 0x00000001,

        /// <summary>
        /// The file contains a sound driver.
        /// </summary>
        DRV_SOUND = 0x00000009,

        /// <summary>
        /// The file contains a system driver.
        /// </summary>
        DRV_SYSTEM = 0x00000007,

        /// <summary>
        /// The file contains a versioned printer driver.
        /// </summary>
        DRV_VERSIONED_PRINTER = 0x0000000C,

        /// <summary>
        /// The driver type is unknown by the system.
        /// </summary>
        UNKNOWN = 0x00000000,
    }
}
