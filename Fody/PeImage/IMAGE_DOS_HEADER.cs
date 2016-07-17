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
// ReSharper disable IdentifierTypo

namespace Fody.PeImage
{
    /// <summary>
    /// Represents a DOS executable header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_DOS_HEADER
    {
        /// <summary>
        /// Contains the "MZ" magic value.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] Magic;

        /// <summary>
        /// Bytes on last page of file
        /// </summary>
        public ushort Cblp;

        /// <summary>
        /// The pages in file.
        /// </summary>
        public ushort Cp;

        /// <summary>
        /// Relocations.
        /// </summary>
        public ushort Crlc;

        /// <summary>
        /// Size of header in paragraphs
        /// </summary>
        public ushort Cparhdr;

        /// <summary>
        /// Minimum extra paragraphs needed
        /// </summary>
        public ushort Minalloc;

        /// <summary>
        /// Maximum extra paragraphs needed
        /// </summary>
        public ushort Maxalloc;

        /// <summary>
        /// Initial (relative) SS value
        /// </summary>
        public ushort Ss;

        /// <summary>
        /// Initial SP value
        /// </summary>
        public ushort Sp;

        /// <summary>
        /// Checksum
        /// </summary>
        public ushort Csum;

        /// <summary>
        /// Initial IP value
        /// </summary>
        public ushort Ip;

        /// <summary>
        /// Initial (relative) CS value
        /// </summary>
        public ushort Cs;

        /// <summary>
        /// File address of relocation table
        /// </summary>
        public ushort Lfarlc;

        /// <summary>
        /// Overlay number
        /// </summary>
        public ushort Ovno;

        /// <summary>
        /// Reserved words.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] Res1;

        /// <summary>
        /// OEM identifier (for oeminfo)
        /// </summary>
        public ushort Oemid;

        /// <summary>
        /// OEM information; oemid specific
        /// </summary>
        public ushort Oeminfo;

        /// <summary>
        /// Reserved words.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public ushort[] Res2;

        /// <summary>
        /// File address of new exe header
        /// </summary>
        public int Lfanew;
    }
}
