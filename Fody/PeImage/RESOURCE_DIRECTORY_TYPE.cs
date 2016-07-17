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

// ReSharper disable IdentifierTypo
namespace Fody.PeImage
{
    /// <summary>
    /// Represents predefined resource types.
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms648009(v=vs.85).aspx"/>
    public enum RESOURCE_DIRECTORY_TYPE : uint
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Hardware-dependent cursor resource.
        /// </summary>
        RT_CURSOR = 1,

        /// <summary>
        /// Bitmap resource.
        /// </summary>
        RT_BITMAP = 2,

        /// <summary>
        /// Hardware-dependent icon resource.
        /// </summary>
        RT_ICON = 3,

        /// <summary>
        /// Menu resource.
        /// </summary>
        RT_MENU = 4,

        /// <summary>
        /// Dialog box.
        /// </summary>
        RT_DIALOG = 5,

        /// <summary>
        /// String-table entry.
        /// </summary>
        RT_STRING = 6,

        /// <summary>
        /// Font directory resource.
        /// </summary>
        RT_FONTDIR = 7,

        /// <summary>
        /// Font resource.
        /// </summary>
        RT_FONT = 8,

        /// <summary>
        /// Accelerator table.
        /// </summary>
        RT_ACCELERATOR = 9,

        /// <summary>
        /// Application-defined resource (raw data).
        /// </summary>
        RT_RCDATA = 10,

        /// <summary>
        /// Message-table entry.
        /// </summary>
        RT_MESSAGETABLE = 11,

        /// <summary>
        /// Hardware-independent cursor resource.
        /// </summary>
        RT_GROUP_CURSOR2 = 12,

        /// <summary>
        /// Hardware-independent cursor resource.
        /// </summary>
        RT_GROUP_CURSOR4 = 14,

        /// <summary>
        /// Version resource.
        /// </summary>
        RT_VERSION = 16,

        /// <summary>
        /// Allows a resource editing tool to associate a string with an .rc file.
        /// Typically, the string is the name of the header file that provides symbolic names.
        /// The resource compiler parses the string but otherwise ignores the value.
        /// For example, <c>1 DLGINCLUDE "MyFile.h"</c>
        /// </summary>
        RT_DLGINCLUDE = 17,

        /// <summary>
        /// Plug and Play resource.
        /// </summary>
        RT_PLUGPLAY = 19,

        /// <summary>
        /// VXD.
        /// </summary>
        RT_VXD = 20,

        /// <summary>
        /// Animated cursor.
        /// </summary>
        RT_ANICURSOR = 21,

        /// <summary>
        /// Animated icon.
        /// </summary>
        RT_ANIICON = 22,

        /// <summary>
        /// HTML resource.
        /// </summary>
        RT_HTML = 23,

        /// <summary>
        /// Side-by-Side Assembly Manifest.
        /// </summary>
        RT_MANIFEST = 24,

        /// <summary>
        /// MFC CDialog
        /// </summary>
        RT_DLGINIT = 240,

        /// <summary>
        /// MFC CToolBarCtrl
        /// </summary>
        RT_TOOLBAR = 241,
    }
}
