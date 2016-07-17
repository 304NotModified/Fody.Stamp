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
using System.Runtime.InteropServices;
using System.Text;

namespace Fody.VersionResources
{
    /// <summary>
    /// Represents an individual string table. A string table contains string data for a specific language.
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646992(v=vs.85).aspx"/>
    internal class StringTable
    {
        /// <summary>
        /// Gets or sets the language code for the language to which the string table applies.
        /// </summary>
        public ushort Language
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the encoding of the data in the string table.
        /// </summary>
        public Encoding Encoding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a dictionary which contains all the items in the string table.
        /// </summary>
        public Dictionary<string, string> Values
        { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the size of the string table, if serialized to binary data.
        /// </summary>
        public long Size
        {
            get
            {
                long size = Marshal.SizeOf(typeof(VersionHeader));
                size += (8 + 1) * 2; // szKey
                size = Helpers.Align(size);

                foreach (var value in Values)
                {
                    long valueSize = Marshal.SizeOf(typeof(VersionHeader));
                    valueSize += Encoding.Unicode.GetByteCount(value.Key + '\0');
                    valueSize = Helpers.Align(valueSize);
                    valueSize += Encoding.Unicode.GetByteCount(value.Value + '\0');
                    valueSize = Helpers.Align(valueSize);

                    size += valueSize;
                }

                return size;
            }
        }
    }
}
