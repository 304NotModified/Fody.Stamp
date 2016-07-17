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

namespace Fody.VersionResources
{
    /// <summary>
    /// Represents the common, fixed-size header of any struct inside the version information section.
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff468918(v=vs.85).aspx"/>
    internal struct VersionHeader
    {
        /// <summary>
        /// The length, in bytes, of the <see cref="VS_VERSIONINFO"/> structure. This length does not include any
        /// padding that aligns any subsequent version resource data on a 32-bit boundary.
        /// </summary>
        public ushort Length;

        /// <summary>
        /// The length, in bytes, of the Value member. This value is zero if there is no Value member associated
        /// with the current version structure.
        /// </summary>
        public ushort ValueLength;

        /// <summary>
        /// The type of data in the version resource. This member is 1 if the version resource contains text
        /// data and 0 if the version resource contains binary data.
        /// </summary>
        public VersionDataType Type;
    }
}
