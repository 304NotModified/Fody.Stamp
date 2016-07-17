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
using System.IO;

namespace Fody.VersionResources
{
    /// <summary>
    /// Provides extension methods for the <see cref="BinaryReader"/> class.
    /// </summary>
    internal static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads a <see cref="VersionInfo"/> object form the stream.
        /// </summary>
        /// <param name="reader">
        /// The reader to read the data from.
        /// </param>
        /// <returns>
        /// A <see cref="VersionInfo"/> object.
        /// </returns>
        public static VersionInfo ReadVersionInfo(this BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var versionHeader = reader.ReadStruct<VersionHeader>();
            var key = reader.ReadUnicodeString();
            reader.Align();

            return new VersionInfo
            {
                Header = versionHeader,
                Key = key
            };
        }
    }
}
