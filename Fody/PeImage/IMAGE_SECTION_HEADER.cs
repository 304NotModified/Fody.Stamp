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

using System.Runtime.InteropServices;
using System.Linq;

namespace Fody.PeImage
{
    /// <summary>
    /// Represents the image section header format.
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms680341(v=vs.85).aspx"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_SECTION_HEADER
    {
        /// <summary>
        /// An 8-byte, null-padded UTF-8 string. There is no terminating null character if the string is exactly eight characters long.
        /// For longer names, this member contains a forward slash (/) followed by an ASCII representation of a decimal number that is an
        /// offset into the string table. Executable images do not use a string table and do not support section names longer than eight characters.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public char[] Name;

        /// <summary>
        /// The physical address or the virtual size.
        /// </summary>
        public uint Misc;

        /// <summary>
        /// The address of the first byte of the section when loaded into memory, relative to the image base. For object files,
        /// this is the address of the first byte before relocation is applied.
        /// </summary>
        public uint VirtualAddress;

        /// <summary>
        /// The size of the initialized data on disk, in bytes. This value must be a multiple of the FileAlignment member of the
        /// <see cref="IMAGE_OPTIONAL_HEADER_32"/> structure. If this value is less than the <see cref="VirtualSize"/> member, the remainder
        /// of the section is filled with zeroes. If the section contains only uninitialized data, the member is zero.
        /// </summary>
        public uint SizeOfRawData;

        /// <summary>
        /// A file pointer to the first page within the COFF file. This value must be a multiple of the FileAlignment member of the
        /// <see cref="IMAGE_OPTIONAL_HEADER_32"/> structure. If a section contains only uninitialized data, set this member is zero.
        /// </summary>
        public uint PointerToRawData;

        /// <summary>
        /// A file pointer to the beginning of the relocation entries for the section. If there are no relocations, this value is zero.
        /// </summary>
        public uint PointerToRelocations;

        /// <summary>
        /// A file pointer to the beginning of the line-number entries for the section. If there are no COFF line numbers, this value is zero.
        /// </summary>
        public uint PointerToLinenumbers;

        /// <summary>
        /// The number of relocation entries for the section. This value is zero for executable images.
        /// </summary>
        public ushort NumberOfRelocations;

        /// <summary>
        /// The number of line-number entries for the section.
        /// </summary>
        public ushort NumberOfLinenumbers;

        /// <summary>
        /// The characteristics of the image.
        /// </summary>
        public IMAGE_SECTION_HEADER_CHARACTERISTICS Characteristics;

        /// <summary>
        /// Gets the name of the section.
        /// </summary>
        public string Section
        {
            get { return new string(Name.Take(Name.Count(c => c != '\0')).ToArray()); }
        }

        /// <summary>
        /// Gets the file address.
        /// </summary>
        public uint PhysicalAddress
        {
            get
            {
                return Misc;
            }
        }

        /// <summary>
        /// Gets the total size of the section when loaded into memory, in bytes. If this value is greater than the SizeOfRawData member,
        /// the section is filled with zeroes. This field is valid only for executable images and should be set to 0 for object files.
        /// </summary>
        public uint VirtualSize
        {
            get
            {
                return Misc;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Section;
        }
    }
}
