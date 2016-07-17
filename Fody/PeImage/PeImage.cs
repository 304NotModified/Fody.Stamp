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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Fody.PeImage
{
    using Fody.VersionResources;

    /// <summary>
    /// Reads Windows PE files
    /// </summary>
    internal class PeImage
    {
        /// <summary>
        /// A value indicating whether the header has been read. Set by <see cref="ReadHeader"/> and read by <see cref="EnsureHeaderRead"/>
        /// </summary>
        private bool headerRead;

        /// <summary>
        /// The offset to the optional header. Available once the header has been read.
        /// </summary>
        private long optionalHeaderOffset;

        /// <summary>
        /// Offset to the checksum field. Available once the header has been read.
        /// </summary>
        private long checksumOffset;

        /// <summary>
        /// A list containing all data directories in the file.
        /// </summary>
        private List<IMAGE_DATA_DIRECTORY> directories;

        /// <summary>
        /// A list containing all sections in the file.
        /// </summary>
        private List<IMAGE_SECTION_HEADER> sections;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeImage"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which points to the Windows PE file.
        /// </param>
        public PeImage(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Stream = stream;
        }

        /// <summary>
        /// Gets the <see cref="Stream"/> which points to the Windows PE file.
        /// </summary>
        public Stream Stream
        {
            get;
            private set;
        }

        /// <summary>
        /// Reads the header of the Windows PE file.
        /// </summary>
        public void ReadHeader()
        {
            var dosHeader = ReadStruct<IMAGE_DOS_HEADER>(0);

            if (Encoding.ASCII.GetString(dosHeader.Magic) != "MZ")
            {
                throw new PeFormatException();
            }

            // Skip the stub program and go to the NT headers
            // Read the NT header
            var ntHeader = ReadStruct<IMAGE_NT_HEADERS>(dosHeader.Lfanew);

            if (Encoding.ASCII.GetString(ntHeader.Signature) != "PE\0\0")
            {
                throw new PeFormatException();
            }

            optionalHeaderOffset = dosHeader.Lfanew + Marshal.SizeOf(typeof(IMAGE_NT_HEADERS));
            long optionalHeaderSize = ntHeader.FileHeader.SizeOfOptionalHeader;

            // The NT header is followed by the optional header.
            var optionalHeader = ReadOptionalHeader(optionalHeaderOffset, optionalHeaderSize);
            checksumOffset = optionalHeaderOffset + optionalHeader.CheckSumOffset;

            // The directories are considered part of the optional header.
            var directoriesSize = optionalHeader.NumberOfRvaAndSizes * Marshal.SizeOf(typeof(IMAGE_DATA_DIRECTORY));
            var directoriesOffset = optionalHeaderOffset + optionalHeaderSize - directoriesSize;

            var sectionHeaderOffset = directoriesOffset + directoriesSize;

            directories = ReadStructArray<IMAGE_DATA_DIRECTORY>(directoriesOffset, (int)optionalHeader.NumberOfRvaAndSizes);
            sections = ReadStructArray<IMAGE_SECTION_HEADER>(sectionHeaderOffset, ntHeader.FileHeader.NumberOfSections);

            headerRead = true;
        }

        /// <summary>
        /// Updates the file checksum.
        /// </summary>
        public void WriteCheckSum()
        {
            var checksum = CalculateCheckSum();

            using (var stream = new SubStream(Stream, checksumOffset, 4, leaveParentOpen: true))
            using (var writer = new BinaryWriter(stream, Encoding.Unicode))
            {
                writer.Write(checksum);
            }

            Stream.Flush();
        }

        /// <summary>
        /// Calculates the checksum for the current image.
        /// </summary>
        /// <returns>
        /// The checksum of the current image.
        /// </returns>
        public uint CalculateCheckSum()
        {
            EnsureHeaderRead();

            ulong checksum = 0;
            var top = Math.Pow(2, 32);

            Stream.Position = 0;

            using (var stream = new SubStream(Stream, 0, Stream.Length, leaveParentOpen: true))
            using (var reader = new BinaryReader(stream, Encoding.Unicode))
            {
                while (Stream.Position < Stream.Length)
                {
                    if (Stream.Position == checksumOffset)
                    {
                        reader.ReadUInt32();
                        continue;
                    }

                    var value = reader.ReadUInt32();

                    checksum = (checksum & 0xffffffff) + value + (checksum >> 32);
                    if (checksum > top)
                    {
                        checksum = (checksum & 0xffffffff) + (checksum >> 32);
                    }
                }
            }

            checksum = (checksum & 0xffff) + (checksum >> 16);
            checksum = checksum + (checksum >> 16);
            checksum = checksum & 0xffff;

            checksum += (uint)Stream.Length;
            return (uint)checksum;
        }

        /// <summary>
        /// Gets the offset to the version resource.
        /// </summary>
        /// <returns>
        /// The offset to the version resource.
        /// </returns>
        private long GetResourceEntryOffset()
        {
            EnsureHeaderRead();

            var resourceTable = directories[2];

            var offset = RvaToFileOffset(resourceTable.VirtualAddress);
            var directory = ReadDirectory(offset);

            // http://stackoverflow.com/questions/12396665/c-library-to-read-exe-version-from-linux
            var versionResource = directory.GetIdEntry(RESOURCE_DIRECTORY_TYPE.RT_VERSION);
            var resourceSection = sections.Single(s => new string(s.Name) == ".rsrc\0\0\0");

            offset = resourceSection.PointerToRawData + (long)versionResource.Value.DirectoryAddress;
            directory = ReadDirectory(offset);

            // Should only contain one item
            if (directory.NamedEntries.Count > 0)
            {
                throw new PeFormatException();
            }

            if (directory.IdEntries.Count != 1)
            {
                throw new PeFormatException();
            }

            var versionEntry = directory.GetIdEntry(1);

            offset = resourceSection.PointerToRawData + (long)versionEntry.Value.DirectoryAddress;
            directory = ReadDirectory(offset);

            // Should only contain one item
            if (directory.NamedEntries.Count > 0)
            {
                throw new PeFormatException();
            }

            if (directory.IdEntries.Count != 1)
            {
                throw new PeFormatException();
            }

            // Get the data entry
            offset = resourceSection.PointerToRawData + (long)directory.IdEntries[0].DirectoryAddress;
            return offset;
        }

        /// <summary>
        /// Updates the version resource data with the data from a stream.
        /// </summary>
        /// <param name="stream">
        /// The stream which was returned by <see cref="GetVersionResourceStream"/>.
        /// </param>
        public void SetVersionResourceStream(Stream stream)
        {
            var offset = GetResourceEntryOffset();
            var entry = ReadStruct<IMAGE_RESOURCE_DATA_ENTRY>(offset);

            entry.Size = (uint)stream.Length;

            WriteStruct(offset, entry);

            RvaToFileOffset(entry.OffsetToData);
        }

        /// <summary>
        /// Reads the version resource from the file.
        /// </summary>
        /// <returns>
        /// A <see cref="VersionResourceReader"/> which contains the version resource.
        /// </returns>
        public Stream GetVersionResourceStream()
        {
            var offset = GetResourceEntryOffset();
            var entry = ReadStruct<IMAGE_RESOURCE_DATA_ENTRY>(offset);

            offset = RvaToFileOffset(entry.OffsetToData);

            Stream resourceStream = new SubStream(Stream, offset, entry.Size, leaveParentOpen: true);
            return resourceStream;
        }

        /// <summary>
        /// Reads a resource directory.
        /// </summary>
        /// <param name="offset">
        /// The offset at which the resource directory is located.
        /// </param>
        /// <returns>
        /// A <see cref="ResourceDirectory"/> object.
        /// </returns>
        private ResourceDirectory ReadDirectory(long offset)
        {
            var resourceDirectory = ReadStruct<IMAGE_RESOURCE_DIRECTORY>(offset);
            var value = new ResourceDirectory();

            using (Stream stream = new SubStream(
                Stream,
                offset + Marshal.SizeOf(typeof(IMAGE_RESOURCE_DIRECTORY)),
                resourceDirectory.NumberOfEntries * Marshal.SizeOf(typeof(IMAGE_RESOURCE_DIRECTORY_ENTRY)),
                leaveParentOpen: true))
            using (var reader = new BinaryReader(stream))
            {
                for (var i = 0; i < resourceDirectory.NumberOfNamedEntries; i++)
                {
                    value.NamedEntries.Add(reader.ReadStruct<IMAGE_RESOURCE_DIRECTORY_ENTRY>());
                }

                for (var i = 0; i < resourceDirectory.NumberOfIdEntries; i++)
                {
                    value.IdEntries.Add(reader.ReadStruct<IMAGE_RESOURCE_DIRECTORY_ENTRY>());
                }
            }

            return value;
        }

        /// <summary>
        /// Reads an array of structs.
        /// </summary>
        /// <typeparam name="T">
        /// The type of struct to read.
        /// </typeparam>
        /// <param name="offset">
        /// The offset at which the struct is located.
        /// </param>
        /// <param name="count">
        /// The number of structs to read.
        /// </param>
        /// <returns>
        /// A list which contains all structs.
        /// </returns>
        private List<T> ReadStructArray<T>(long offset, int count)
        {
            var value = new List<T>(count);

            using (Stream stream = new SubStream(Stream, offset, count * Marshal.SizeOf(typeof(T)), leaveParentOpen: true))
            using (var reader = new BinaryReader(stream))
            {
                for (var i = 0; i < count; i++)
                {
                    value.Add(reader.ReadStruct<T>());
                }
            }

            return value;
        }

        /// <summary>
        /// Reads a struct.
        /// </summary>
        /// <typeparam name="T">
        /// The type of struct to read.
        /// </typeparam>
        /// <param name="offset">
        /// The offset at which the struct is located.
        /// </param>
        /// <returns>
        /// The requested struct.
        /// </returns>
        private T ReadStruct<T>(long offset)
        {
            using (Stream stream = new SubStream(Stream, offset, Marshal.SizeOf(typeof(T)), leaveParentOpen: true))
            using (var reader = new BinaryReader(stream))
            {
                return reader.ReadStruct<T>();
            }
        }

        /// <summary>
        /// Writes a struct.
        /// </summary>
        /// <typeparam name="T">
        /// The type of struct to write.
        /// </typeparam>
        /// <param name="offset">
        /// The offset at which the struct is located.
        /// </param>
        /// <param name="value">
        /// The struct to write.
        /// </param>
        private void WriteStruct<T>(long offset, T value)
        {
            using (Stream stream = new SubStream(Stream, offset, Marshal.SizeOf(typeof(T)), leaveParentOpen: true))
            using (var writer = new BinaryWriter(stream))
            {
                writer.WriteStruct(value);
            }
        }

        /// <summary>
        /// Reads the optional header.
        /// </summary>
        /// <param name="offset">
        /// The offset at which the optional header is located.
        /// </param>
        /// <param name="size">
        /// The size of the optional header.
        /// </param>
        /// <returns>
        /// A <see cref="IImageOptionalHeader"/> which represents the optional header.
        /// </returns>
        private IImageOptionalHeader ReadOptionalHeader(long offset, long size)
        {
            using (Stream stream = new SubStream(Stream, offset, size, leaveParentOpen: true))
            using (var reader = new BinaryReader(stream))
            {
                // The NT header is followed by the optional header. There's a 32-bit
                // and a 64-bit variant. The magic indicates which one we're dealing with
                var magic = (MagicType)reader.ReadUInt16();

                // Go to the start
                stream.Seek(0, SeekOrigin.Begin);

                IImageOptionalHeader optionalHeader;

                switch (magic)
                {
                    case MagicType.IMAGE_NT_OPTIONAL_HDR32_MAGIC:
                        var optionalHeader32 = reader.ReadStruct<IMAGE_OPTIONAL_HEADER_32>();
                        optionalHeader = optionalHeader32;
                        break;

                    case MagicType.IMAGE_NT_OPTIONAL_HDR64_MAGIC:
                        var optionalHeader64 = reader.ReadStruct<IMAGE_OPTIONAL_HEADER_64>();
                        optionalHeader = optionalHeader64;
                        break;

                    default:
                        throw new PeFormatException();
                }

                if (magic != optionalHeader.Magic)
                {
                    throw new PeFormatException();
                }

                return optionalHeader;
            }
        }

        /// <summary>
        /// Throws an exception if the header has not yet been read.
        /// </summary>
        private void EnsureHeaderRead()
        {
            if (!headerRead)
            {
                throw new InvalidOperationException("You must first read the header by calling ReadHeader()");
            }
        }

        /// <summary>
        /// Convers a relative virtual address (RVA) to an offset in the file.
        /// </summary>
        /// <param name="rva">
        /// The relative value offset.
        /// </param>
        /// <returns>
        /// The offset in the <see cref="Stream"/> at which the data is located.
        /// </returns>
        private long RvaToFileOffset(long rva)
        {
            EnsureHeaderRead();

            var matches = sections.Where(s => rva >= s.VirtualAddress && rva < s.VirtualAddress + s.SizeOfRawData);

            if (matches.Count() != 1)
            {
                throw new PeFormatException();
            }

            var section = matches.Single();

            var offset = rva - section.VirtualAddress + section.PointerToRawData;

            return offset;
        }
    }
}
