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

namespace Fody.PeImage
{
    /// <summary>
    /// Each directory contains the 32-bit Name of the entry and an offset,
    /// relative to the beginning of the resource directory of the data associated
    /// with this directory entry.  If the name of the entry is an actual text
    /// string instead of an integer Id, then the high order bit of the name field
    /// is set to one and the low order 31-bits are an offset, relative to the
    /// beginning of the resource directory of the string, which is of type
    /// <c>IMAGE_RESOURCE_DIRECTORY_STRING</c>.  Otherwise the high bit is clear and the
    /// low-order 16-bits are the integer Id that identify this resource directory
    /// entry. If the directory entry is yet another resource directory (i.e. a
    /// subdirectory), then the high order bit of the offset field will be
    /// set to indicate this.  Otherwise the high bit is clear and the offset
    /// field points to a resource data entry.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_RESOURCE_DIRECTORY_ENTRY
    {
        /// <summary>
        /// Directory name offset.
        /// </summary>
        public uint NameOffset;

        /// <summary>
        /// Directory data offset.
        /// </summary>
        public uint OffsetToData;

        /// <summary>
        /// Gets a value indicating whether the directory name is a string.
        /// </summary>
        public bool IsNameString
        {
            get { return (NameOffset & 0x80000000) > 0; }
        }

        /// <summary>
        /// Gets an offset, relative to the beginning of the resource directory of the string, which is of type
        /// <c>IMAGE_RESOURCE_DIRECTORY_STRING.</c>
        /// </summary>
        public uint NameAddress
        {
            get { return NameOffset & 0x7FFFFFFF; }
        }

        /// <summary>
        /// Gets the type of the resource directory.
        /// </summary>
        public RESOURCE_DIRECTORY_TYPE NameType
        {
            get { return IsNameString ? RESOURCE_DIRECTORY_TYPE.Undefined : (RESOURCE_DIRECTORY_TYPE)NameAddress; }
        }

        /// <summary>
        /// Gets a value indicating whether the entry is a directory.
        /// </summary>
        public bool IsDirectory
        {
            get { return (OffsetToData & 0x80000000) > 0; }
        }

        /// <summary>
        /// Offset to the child <see cref="IMAGE_RESOURCE_DIRECTORY"/> or <see cref="IMAGE_RESOURCE_DATA_ENTRY"/>
        /// structure.
        /// </summary>
        public uint DirectoryAddress
        {
            get { return OffsetToData & 0x7FFFFFFF; }
        }

        /// <summary>
        /// Gets a value indicating whether the child structure is a <see cref="IMAGE_RESOURCE_DATA_ENTRY"/> structure.
        /// </summary>
        public bool IsDataEntry
        {
            get { return !IsNameString && !IsDirectory; }
        }
    }
}
