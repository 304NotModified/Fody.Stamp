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

using Fody.PeImage;
using System;
using System.Runtime.InteropServices;
// ReSharper disable IdentifierTypo

namespace Fody.VersionResources
{
    /// <summary>
    /// Contains version information for a file. This information is language and code page independent.
    /// </summary>
    /// <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646997(v=vs.85).aspx"/>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct VS_FIXEDFILEINFO
    {
        /// <summary>
        /// Contains the value <c>0xFEEF04BD</c>. This is used with the szKey member of the <see cref="VS_VERSIONINFO"/> structure
        /// when searching a file for the <see cref="VS_FIXEDFILEINFO"/> structure.
        /// </summary>
        public uint Signature;

        /// <summary>
        /// The binary version number of this structure. The high-order word of this member contains the major version number,
        /// and the low-order word contains the minor version number.
        /// </summary>
        public uint StructVersion;

        /// <summary>
        /// The most significant 32 bits of the file's binary version number. This member is used with
        /// <see cref="FileVersionLS"/> to form a 64-bit value used for numeric comparisons.
        /// </summary>
        public uint FileVersionMS;

        /// <summary>
        /// The least significant 32 bits of the file's binary version number. This member is used with
        /// <see cref="FileVersionMS"/> to form a 64-bit value used for numeric comparisons.
        /// </summary>
        public uint FileVersionLS;

        /// <summary>
        /// The most significant 32 bits of the binary version number of the product with which this file was distributed.
        /// This member is used with <see cref="ProductVersionLS"/> to form a 64-bit value used for numeric comparisons.
        /// </summary>
        public uint ProductVersionMS;

        /// <summary>
        /// The least significant 32 bits of the binary version number of the product with which this file was distributed.
        /// This member is used with <see cref="ProductVersionMS"/> to form a 64-bit value used for numeric comparisons.
        /// </summary>
        public uint ProductVersionLS;

        /// <summary>
        /// Contains a bitmask that specifies the valid bits in <see cref="FileFlags"/>.
        /// A bit is valid only if it was defined when the file was created.</summary>
        public uint FileFlagMask;

        /// <summary>
        /// Contains a bitmask that specifies the Boolean attributes of the file.
        /// This member can include one or more of the following values.
        /// </summary>
        public VS_FF FileFlags;

        /// <summary>
        /// The operating system for which this file was designed.
        /// </summary>
        public VOS FileOS;

        /// <summary>
        /// The general type of file.
        /// </summary>
        public VFT FileType;

        /// <summary>
        /// The function of the file. The possible values depend on the value of <see cref="FileType"/>.
        /// </summary>
        public VFT2 FileSubtype;

        /// <summary>
        /// The most significant 32 bits of the file's 64-bit binary creation date and time stamp
        /// .</summary>
        public uint FileDateMS;

        /// <summary>
        /// The least significant 32 bits of the file's 64-bit binary creation date and time stamp.
        /// </summary>
        public uint FileDateLS;

        /// <summary>
        /// Gets a value indicating whether the structure is valid.
        /// </summary>
        public bool IsValid
        {
            get { return Signature == 0xFEEF04BD; }
        }

        /// <summary>
        /// Gets or sets the file version number.
        /// </summary>
        public Version FileVersion
        {
            get
            {
                return new Version(
                    Helpers.HiWord(FileVersionMS),
                    Helpers.LoWord(FileVersionMS),
                    Helpers.HiWord(FileVersionLS),
                    Helpers.LoWord(FileVersionLS));
            }

            set
            {
                FileVersionMS = (uint)value.Major << 16;
                FileVersionMS += (uint)value.Minor;

                FileVersionLS = (uint)value.Build << 16;
                FileVersionLS += (uint)value.Revision;
            }
        }

        /// <summary>
        /// Gets or sets the product version number.
        /// </summary>
        public Version ProductVersion
        {
            get
            {
                return new Version(
                    Helpers.HiWord(ProductVersionMS),
                    Helpers.LoWord(ProductVersionMS),
                    Helpers.HiWord(ProductVersionLS),
                    Helpers.LoWord(ProductVersionLS));
            }

            set
            {
                ProductVersionMS = (uint)value.Major << 16;
                ProductVersionMS += (uint)value.Minor;

                ProductVersionLS = (uint)value.Build << 16;
                ProductVersionLS += (uint)value.Revision;
            }
        }
    }
}
