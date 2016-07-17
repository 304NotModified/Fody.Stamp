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
using System.Runtime.InteropServices;

namespace Fody.PeImage
{
    /// <summary>
    /// Represents the optional header format.
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms680339(v=vs.85).aspx"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_OPTIONAL_HEADER_32 : IImageOptionalHeader
    {
        /// <summary>
        /// The state of the image file.
        /// </summary>
        public MagicType Magic;

        /// <summary>
        /// The major version number of the linker.
        /// </summary>
        public byte MajorLinkerVersion;

        /// <summary>
        /// The minor version number of the linker.
        /// </summary>
        public byte MinorLinkerVersion;

        /// <summary>
        /// The size of the code section, in bytes, or the sum of all such sections
        /// if there are multiple code sections.
        /// </summary>
        public uint SizeOfCode;

        /// <summary>
        /// The size of the initialized data section, in bytes, or the sum of all such
        /// sections if there are multiple initialized data sections.
        /// </summary>
        public uint SizeOfInitializedData;

        /// <summary>
        /// The size of the uninitialized data section, in bytes, or the sum of all such
        /// sections if there are multiple uninitialized data sections.
        /// </summary>
        public uint SizeOfUninitializedData;

        /// <summary>
        /// A pointer to the entry point function, relative to the image base address.
        /// For executable files, this is the starting address. For device drivers,
        /// this is the address of the initialization function. The entry point function
        /// is optional for DLLs. When no entry point is present, this member is zero.
        /// </summary>
        public uint AddressOfEntryPoint;

        /// <summary>
        /// A pointer to the beginning of the code section, relative to the image base.
        /// </summary>
        public uint BaseOfCode;

        /// <summary>
        /// A pointer to the beginning of the data section, relative to the image base.
        /// </summary>
        public uint BaseOfData;

        /// <summary>
        /// The preferred address of the first byte of the image when it is loaded in memory.
        /// This value is a multiple of 64K bytes. The default value for DLLs is <c>0x10000000</c>.
        /// The default value for applications is <c>0x00400000</c>, except on Windows CE
        /// where it is <c>0x00010000</c>.
        /// </summary>
        public uint ImageBase;

        /// <summary>
        /// The alignment of sections loaded in memory, in bytes. This value must be greater
        /// than or equal to the <see cref="FileAlignment"/> member. The default value is the
        /// page size for the system.
        /// </summary>
        public uint SectionAlignment;

        /// <summary>
        /// The alignment of the raw data of sections in the image file, in bytes.
        /// The value should be a power of 2 between 512 and 64K (inclusive).
        /// The default is 512. If the <see cref="SectionAlignment"/> member is less than the system page
        /// size, this member must be the same as <see cref="SectionAlignment"/>.
        /// </summary>
        public uint FileAlignment;

        /// <summary>
        /// The major version number of the required operating system.
        /// </summary>
        public ushort MajorOperatingSystemVersion;

        /// <summary>
        /// The minor version number of the required operating system.
        /// </summary>
        public ushort MinorOperatingSystemVersion;

        /// <summary>
        /// The major version number of the image.
        /// </summary>
        public ushort MajorImageVersion;

        /// <summary>
        /// The minor version number of the image.
        /// </summary>
        public ushort MinorImageVersion;

        /// <summary>
        /// The major version number of the subsystem.
        /// </summary>
        public ushort MajorSubsystemVersion;

        /// <summary>
        /// The minor version number of the subsystem.
        /// </summary>
        public ushort MinorSubsystemVersion;

        /// <summary>
        /// This member is reserved and must be <c>0</c>.
        /// </summary>
        public uint Win32VersionValue;

        /// <summary>
        /// The size of the image, in bytes, including all headers. Must be a multiple
        /// of <see cref="SectionAlignment"/>.
        /// </summary>
        public uint SizeOfImage;

        /// <summary>
        /// The combined size of the following items, rounded to a multiple of the value
        /// specified in the <see cref="FileAlignment"/> member:
        /// <list type="ordered">
        ///   <item>
        ///     <see cref="IMAGE_DOS_HEADER.Lfanew"/>.
        ///   </item>
        ///   <item>
        ///      4-byte signature
        ///   </item>
        ///   <item>
        ///     Size of <see cref="IMAGE_FILE_HEADER"/>.
        ///   </item>
        ///   <item>
        ///     Size of optional header.
        ///   </item>
        ///   <item>
        ///     Size of all section headers.
        ///   </item>
        /// </list>
        /// </summary>
        public uint SizeOfHeaders;

        /// <summary>
        /// The image file checksum. The following files are validated at load time:
        /// all drivers, any DLL loaded at boot time, and any DLL loaded into a critical
        /// system process.
        /// </summary>
        public uint CheckSum;

        /// <summary>
        /// The subsystem required to run this image.
        /// </summary>
        public SubSystemType Subsystem;

        /// <summary>
        /// The DLL characteristics of the image.
        /// </summary>
        public DllCharacteristicsType DllCharacteristics;

        /// <summary>
        /// The number of bytes to reserve for the stack. Only the memory specified by the
        /// <see cref="SizeOfStackCommit"/> member is committed at load time; the rest is 
        /// made available one page at a time until this reserve size is reached.
        /// </summary>
        public uint SizeOfStackReserve;

        /// <summary>
        /// The number of bytes to commit for the stack.
        /// </summary>
        public uint SizeOfStackCommit;

        /// <summary>
        /// The number of bytes to reserve for the local heap. Only the memory specified by
        /// the <see cref="SizeOfHeapCommit"/> member is committed at load time; the rest is
        /// made available one page at a time until this reserve size is reached.
        /// </summary>
        public uint SizeOfHeapReserve;

        /// <summary>
        /// The number of bytes to commit for the local heap.
        /// </summary>
        public uint SizeOfHeapCommit;

        /// <summary>
        /// This member is obsolete.
        /// </summary>
        [Obsolete]
        public uint LoaderFlags;

        /// <summary>
        /// The number of directory entries in the remainder of the optional header.
        /// Each entry describes a location and size.
        /// </summary>
        public uint NumberOfRvaAndSizes;

        /// <inheritdoc/>
        uint IImageOptionalHeader.NumberOfRvaAndSizes
        {
            get
            {
                return NumberOfRvaAndSizes;
            }
        }

        /// <inheritdoc/>
        MagicType IImageOptionalHeader.Magic
        {
            get
            {
                return Magic;
            }
        }

        /// <inheritdoc/>
        ulong IImageOptionalHeader.ImageBase
        {
            get
            {
                return ImageBase;
            }
        }

        /// <inheritdoc/>
        uint IImageOptionalHeader.CheckSum
        {
            get
            {
                return CheckSum;
            }
        }

        /// <inheritdoc/>
        int IImageOptionalHeader.CheckSumOffset
        {
            get
            {
                return (int)Marshal.OffsetOf(typeof(IMAGE_OPTIONAL_HEADER_32), nameof(CheckSum));
            }
        }
    }
}
