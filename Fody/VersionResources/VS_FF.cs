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
    /// Contains a bitmask that specifies the Boolean attributes of the file.
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646997(v=vs.85).aspx"/>
    [Flags]
    public enum VS_FF
    {
        /// <summary>
        /// The file contains debugging information or is compiled with debugging features enabled.
        /// </summary>
        DEBUG = 0x00000001,

        /// <summary>
        /// The file's version structure was created dynamically; therefore, some of the members in this structure may be empty or incorrect.
        /// This flag should never be set in a file's <see cref="VS_VERSIONINFO"/> data.
        /// </summary>
        INFOINFERRED = 0x00000010,

        /// <summary>
        /// The file has been modified and is not identical to the original shipping file of the same version number.
        /// .</summary>
        PATCHED = 0x00000004,

        /// <summary>
        /// The file is a development version, not a commercially released product
        /// .</summary>
        PRERELEASE = 00000002,

        /// <summary>
        /// The file was not built using standard release procedures. If this flag is set, the StringFileInfo structure should contain
        /// a PrivateBuild entry.
        /// </summary>
        PRIVATEBUILD = 00000008,

        /// <summary>
        /// The file was built by the original company using standard release procedures but is a variation of the normal file of the
        /// same version number. If this flag is set, the StringFileInfo structure should contain a SpecialBuild entry.
        /// </summary>
        SPECIALBUILD = 00000020,
    }
}
