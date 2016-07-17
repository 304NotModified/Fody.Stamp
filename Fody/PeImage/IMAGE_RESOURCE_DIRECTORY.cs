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
    /// Represents the resource directory</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_RESOURCE_DIRECTORY
    {
        /// <summary>
        /// Unused
        /// </summary>
        public uint Characteristics;

        /// <summary
        /// >Unused
        /// </summary>
        public uint TimeDateStamp;

        /// <summary>
        /// Major directory version
        /// </summary>
        public ushort MajorVersion;

        /// <summary
        /// >Minor directory version
        /// </summary>
        public ushort MinorVersion;

        /// <summary>
        /// Number of subdirectories with names
        /// </summary>
        public ushort NumberOfNamedEntries;

        /// <summary>
        /// Number of subdirectories with IDs
        /// </summary>
        public ushort NumberOfIdEntries;

        /// <summary>
        /// Gets the directory version
        /// </summary>
        public Version Version
        {
            get { return new Version(MajorVersion, MinorVersion); }
        }

        /// <summary>
        /// Gets a value indicating whether this directory contains subdirectories
        /// </summary>
        public bool ContainsEntries
        {
            get { return NumberOfEntries > 0; }
        }

        /// <summary>
        /// Gets the number of subdirectories in this directory.
        /// </summary>
        public ushort NumberOfEntries
        {
            get { return checked((ushort)(NumberOfIdEntries + NumberOfNamedEntries)); }
        }
    }
}
