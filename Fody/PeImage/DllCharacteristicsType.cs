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

// ReSharper disable IdentifierTypo
namespace Fody.PeImage
{
    /// <summary>
    /// The DLL characteristics a image.
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms680339(v=vs.85).aspx"/>
    public enum DllCharacteristicsType : ushort
    {
        /// <summary>
        /// Reserved.
        /// </summary>
        RES_0 = 0x0001,

        /// <summary>
        /// Reserved.
        /// </summary>
        RES_1 = 0x0002,

        /// <summary>
        /// Reserved.
        /// </summary>
        RES_2 = 0x0004,

        /// <summary>
        /// Reserved.
        /// </summary>
        RES_3 = 0x0008,

        /// <summary>
        /// The DLL can be relocated at load time.
        /// </summary>
        IMAGE_DLL_CHARACTERISTICS_DYNAMIC_BASE = 0x0040,

        /// <summary>
        /// Code integrity checks are forced. If you set this flag and a section contains
        /// only uninitialized data, set the <see cref="IMAGE_SECTION_HEADER.PointerToRawData"/>
        /// for that section to zero; otherwise, the image will fail to load because the
        /// digital signature cannot be verified.
        /// </summary>
        IMAGE_DLL_CHARACTERISTICS_FORCE_INTEGRITY = 0x0080,

        /// <summary>
        /// The image is compatible with data execution prevention (DEP).
        /// </summary>
        IMAGE_DLL_CHARACTERISTICS_NX_COMPAT = 0x0100,

        /// <summary>
        /// The image is isolation aware, but should not be isolated.
        /// </summary>
        IMAGE_DLLCHARACTERISTICS_NO_ISOLATION = 0x0200,

        /// <summary>
        /// The image does not use structured exception handling (SEH). No handlers can
        /// be called in this image.
        /// </summary>
        IMAGE_DLLCHARACTERISTICS_NO_SEH = 0x0400,

        /// <summary>
        /// Do not bind the image.
        /// </summary>
        IMAGE_DLLCHARACTERISTICS_NO_BIND = 0x0800,

        /// <summary>
        /// Reserved.
        /// </summary>
        RES_4 = 0x1000,

        /// <summary>
        /// A WDM driver.
        /// </summary>
        IMAGE_DLLCHARACTERISTICS_WDM_DRIVER = 0x2000,

        /// <summary>
        /// Reserved.
        /// </summary>
        RES_5 = 0x4000,

        /// <summary>
        /// The image is terminal server aware.
        /// </summary>
        IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = 0x8000
    }
}
