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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Fody.VersionResources
{
    /// <summary>
    /// Represents an individual version resource.
    /// </summary>
    internal class VersionResource
    {
        /// <summary>
        /// Gets or sets the fixed file info for the version resource.
        /// </summary>
        public VS_FIXEDFILEINFO? FixedFileInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the var file info (which contains a list of languages and their encodings)
        /// for the version resource.
        /// </summary>
        public Dictionary<ushort, Encoding> VarFileInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of string tables (one for each langauge defined in <see cref="VarFileInfo"/>) for the version
        /// resource.
        /// </summary>
        public List<StringTable> StringFileInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the size of the <see cref="StringFileInfo"/> data when serialized to a binary format.
        /// </summary>
        public long StringFileInfoSize
        {
            get
            {
                if (StringFileInfo == null)
                {
                    return 0;
                }

                long value = Marshal.SizeOf(typeof(VersionHeader));
                value += Encoding.Unicode.GetByteCount("StringFileInfo\0");
                value = Helpers.Align(value);

                value += StringFileInfo.Sum(v => v.Size);

                return value;
            }
        }

        /// <summary>
        /// Gets the size of the <see cref="VarFileInfo"/> data when serialized to a binary format.
        /// </summary>
        public long VarFileInfoSize
        {
            get
            {
                if (VarFileInfo == null)
                {
                    return 0;
                }

                long value = Marshal.SizeOf(typeof(VersionHeader));
                value += Encoding.Unicode.GetByteCount("VarFileInfo\0");
                value = Helpers.Align(value);

                value += VarSize;

                return value;
            }
        }

        /// <summary>
        /// Gets the size of the Var element when serialized to a binary format.
        /// </summary>
        public long VarSize
        {
            get
            {
                if (VarFileInfo == null)
                {
                    return 0;
                }

                long value = Marshal.SizeOf(typeof(VersionHeader));
                value += Encoding.Unicode.GetByteCount("Translation\0");
                value = Helpers.Align(value);

                value += VarFileInfo.Count * 4;

                return value;
            }
        }

        /// <summary>
        /// Gets the size of the <see cref="VersionResource"/> data when serialized to a binary format.
        /// </summary>
        public long Size
        {
            get
            {
                long value = Marshal.SizeOf(typeof(VersionHeader));
                value += Encoding.Unicode.GetByteCount("VS_VERSION_INFO\0");
                value = Helpers.Align(value);

                if (FixedFileInfo != null)
                {
                    value += Marshal.SizeOf(typeof(VS_FIXEDFILEINFO));
                }

                value = Helpers.Align(value);
                value += VarFileInfoSize;
                value += StringFileInfoSize;

                return value;
            }
        }
    }
}
