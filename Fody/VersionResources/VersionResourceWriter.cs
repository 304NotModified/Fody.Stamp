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
using System.Runtime.InteropServices;
using System.Text;

namespace Fody.VersionResources
{
    /// <summary>
    /// Writes <see cref="VersionInfo"/> data to a <see cref="Stream"/>.
    /// </summary>
    internal class VersionResourceWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionResourceReader"/> class.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> which represents the embedded resource.
        /// </param>
        public VersionResourceWriter(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            this.Stream = stream;
        }

        /// <summary>
        /// Gets the <see cref="Stream"/> which represents the embedded resource.
        /// </summary>
        public Stream Stream
        {
            get;
            private set;
        }

        /// <summary>
        /// Writes a <see cref="VersionResource"/> object to the current <see cref="Stream"/>.
        /// </summary>
        /// <param name="resource">
        /// The <see cref="VersionResource"/> to write.
        /// </param>
        public void Write(VersionResource resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (this.Stream.Length < resource.Size)
            {
                this.Stream.SetLength(resource.Size);
            }

            using (SubStream stream = new SubStream(this.Stream, 0, this.Stream.Length, leaveParentOpen: true))
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Unicode))
            {
                // Write the version resource header
                this.WriteHeader(
                    writer,
                    resource.Size,
                    resource.FixedFileInfo == null ? 0 : Marshal.SizeOf(typeof(VS_FIXEDFILEINFO)),
                    VersionDataType.Binary,
                    "VS_VERSION_INFO");

                if (resource.FixedFileInfo != null)
                {
                    writer.WriteStruct(resource.FixedFileInfo.Value);
                }

                writer.Align();

                if (resource.VarFileInfo != null)
                {
                    this.WriteHeader(writer, resource.VarFileInfoSize, 0, VersionDataType.Text, "VarFileInfo");

                    this.WriteVarFileInfo(writer, resource);
                }

                if (resource.StringFileInfo != null)
                {
                    this.WriteHeader(writer, resource.StringFileInfoSize, 0, VersionDataType.Text, "StringFileInfo");

                    this.WriteStringFileInfo(writer, resource);
                }
            }
        }

        private void WriteVarFileInfo(BinaryWriter writer, VersionResource resource)
        {
            this.WriteHeader(writer, resource.VarSize, resource.VarFileInfo.Count * 4, VersionDataType.Binary, "Translation");

            foreach (var value in resource.VarFileInfo)
            {
                var codePage = (ushort)value.Value.CodePage;
                var languageIdentifier = value.Key;

                writer.Write(languageIdentifier);
                writer.Write(codePage);
            }
        }

        private void WriteStringFileInfo(BinaryWriter writer, VersionResource resource)
        {
            foreach (var value in resource.StringFileInfo)
            {
                ushort languageIdentifier = value.Language;
                ushort codePage = (ushort)value.Encoding.CodePage;

                uint key = (uint)languageIdentifier << 4 | codePage;

                this.WriteHeader(writer, value.Size, 0, VersionDataType.Text, key.ToString("x8"));

                foreach (var pair in value.Values)
                {
                    long valueLength = Encoding.Unicode.GetByteCount(pair.Value + '\0');
                    long keyLength = Encoding.Unicode.GetByteCount(pair.Key + '\0');

                    long length = Marshal.SizeOf(typeof(VersionHeader));
                    length += keyLength;
                    length = Helpers.Align(length);
                    length += valueLength;
                    length = Helpers.Align(length);

                    this.WriteHeader(writer, length, valueLength / sizeof(short), VersionDataType.Text, pair.Key);
                    writer.WriteUnicodeString(pair.Value);
                    writer.Align();
                }
            }
        }

        private void WriteHeader(BinaryWriter writer, long length, long valueLength, VersionDataType binary, string key)
        {
            VersionHeader header = new VersionHeader()
            {
                Length = (ushort)length,
                ValueLength = (ushort)valueLength,
                Type = binary
            };

            writer.WriteStruct(header);
            writer.WriteUnicodeString(key);
            writer.Align();

        }
    }
}
