 // ReSharper disable once CommentTypo
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

// ReSharper disable CommentTypo
namespace Fody.PeImage
{
    /// <summary>
    /// The subsystem required to run a image.
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms680339(v=vs.85).aspx"/>
    public enum SubSystemType : ushort
    {
        /// <summary>
        /// Unknown subsystem.
        /// </summary>
        IMAGE_SUBSYSTEM_UNKNOWN = 0,

        /// <summary>
        /// No subsystem required (device drivers and native system processes).
        /// </summary>
        IMAGE_SUBSYSTEM_NATIVE = 1,

        /// <summary>
        /// Windows graphical user interface (GUI) subsystem.
        /// </summary>
        IMAGE_SUBSYSTEM_WINDOWS_GUI = 2,

        /// <summary>
        /// Windows character-mode user interface (CUI) subsystem.
        /// </summary>
        IMAGE_SUBSYSTEM_WINDOWS_CUI = 3,

        /// <summary>
        /// OS/2 CUI subsystem.
        /// </summary>
        IMAGE_SUBSYSTEM_OS2_CUI = 5,

        /// <summary>
        /// POSIX CUI subsystem.
        /// </summary>
        IMAGE_SUBSYSTEM_POSIX_CUI = 7,

        /// <summary>
        /// Windows CE system.
        /// </summary>
        IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 9,

        /// <summary>
        /// Extensible Firmware Interface (EFI) application.
        /// </summary>
        IMAGE_SUBSYSTEM_EFI_APPLICATION = 10,

        /// <summary>
        /// EFI driver with boot services.
        /// </summary>
        IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER = 11,

        /// <summary>
        /// EFI driver with run-time services.
        /// </summary>
        IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER = 12,

        /// <summary>
        /// EFI ROM image.
        /// </summary>
        IMAGE_SUBSYSTEM_EFI_ROM = 13,

        /// <summary>
        /// Xbox system.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        IMAGE_SUBSYSTEM_XBOX = 14,

        /// <summary>
        /// Boot application.
        /// </summary>
        IMAGE_SUBSYSTEM_WINDOWS_BOOT_APPLICATION = 16,
    }
}
