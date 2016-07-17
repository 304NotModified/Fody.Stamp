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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Fody.VersionResources
{
    /// <summary>
    /// Supports reading version resources from an resource embeded in a Windows PE file.
    /// </summary>
    internal class VersionResourceReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionResourceReader"/> class.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> which represents the embedded resource.
        /// </param>
        public VersionResourceReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Stream = stream;
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
        /// Reads a <see cref="VersionResource"/> object from the current <see cref="Stream"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="VersionResource"/> object.
        /// </returns>
        public VersionResource Read()
        {
            Stream.Position = 0;

            var value = new VersionResource();

            using (var stream = new SubStream(Stream, 0, Stream.Length, leaveParentOpen: true))
            using (var reader = new BinaryReader(stream, Encoding.Default))
            {
                var offset = Stream.Position;

                var versionInfo = reader.ReadVersionInfo();
                var end = offset + versionInfo.Header.Length;

                // The root element MUST be a "VS_VERSION_INFO" element of binary type.
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ms647001(v=vs.85).aspx
                // It contains at most three children - a VS_FIXEDFILEINFO object, a StringFileInfo struct
                // and a VarFileInfo struct.
                if (versionInfo.Key != "VS_VERSION_INFO")
                {
                    throw new VersionResourceFormatException();
                }

                if (versionInfo.Header.Type != VersionDataType.Binary)
                {
                    throw new VersionResourceFormatException();
                }

                // We know a VS_FIXEDFILEINFO struct is present if the ValueLength > 0
                if (versionInfo.Header.ValueLength != 0)
                {
                    // Read the file info
                    value.FixedFileInfo = reader.ReadStruct<VS_FIXEDFILEINFO>();
                    reader.Align();
                }

                // Read the children: At most one StringFileInfo and at most one VarFileInfo
                while (Stream.Position < end)
                {
                    var childOffset = Stream.Position;

                    var childInfo = reader.ReadVersionInfo();
                    var childEnd = childOffset + childInfo.Header.Length;

                    switch (childInfo.Key)
                    {
                        case "VarFileInfo":
                            if (childInfo.Header.Type != VersionDataType.Text)
                            {
                                throw new VersionResourceFormatException();
                            }

                            value.VarFileInfo = ReadVarFileInfo(reader);
                            break;

                        case "StringFileInfo":
                            if (childInfo.Header.Type != VersionDataType.Text)
                            {
                                throw new VersionResourceFormatException();
                            }

                            value.StringFileInfo = ReadStringFileInfo(reader, childEnd);
                            break;
                    }
                }

                return value;
            }
        }

        private Dictionary<ushort, Encoding> ReadVarFileInfo(BinaryReader reader)
        {
            // Var structure: https://msdn.microsoft.com/en-us/library/windows/desktop/ms646994(v=vs.85).aspx
            var versionInfo = reader.ReadVersionInfo();

            if (versionInfo.Key != "Translation")
            {
                throw new VersionResourceFormatException();
            }

            var count = versionInfo.Header.ValueLength / sizeof(uint);

            var value = new Dictionary<ushort, Encoding>(count);

            for (var i = 0; i < count; i++)
            {
                var pair = reader.ReadUInt32();
                var codePage = Helpers.HiWord(pair);
                var encoding = Encoding.GetEncoding(codePage);

                var languageIdentifier = Helpers.LoWord(pair);

                value.Add(languageIdentifier, encoding);
            }

            return value;
        }

        private List<StringTable> ReadStringFileInfo(BinaryReader reader, long end)
        {
            var value = new List<StringTable>();

            // Contains one or more string tables (one for each language),
            // each string table contains one or more strings (key/value pairs mapping
            // keys to strings for that language)
            while (Stream.Position < end)
            {
                // Read the string table
                var stringTable = new StringTable();
                value.Add(stringTable);

                var start = Stream.Position;
                var versionInfo = reader.ReadVersionInfo();

                if (versionInfo.Header.Type != VersionDataType.Text)
                {
                    throw new VersionResourceFormatException();
                }

                var pair = uint.Parse(versionInfo.Key, NumberStyles.AllowHexSpecifier);
                var codePage = Helpers.LoWord(pair);
                var encoding = Encoding.GetEncoding(codePage);

                var languageIdentifier = Helpers.HiWord(pair);
                stringTable.Language = languageIdentifier;
                stringTable.Encoding = encoding;

                while (Stream.Position < start + versionInfo.Header.Length)
                {
                    // Read the string data
                    var stringInfo = reader.ReadVersionInfo();
                    var text = reader.ReadUnicodeString();
                    reader.Align();

                    stringTable.Values.Add(stringInfo.Key, text);
                }
            }

            return value;
        }
    }
}
