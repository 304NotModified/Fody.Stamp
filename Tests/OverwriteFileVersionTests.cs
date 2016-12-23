using System.Xml.Linq;
using NUnit.Framework;

namespace Stamp.Fody.Tests
{
    [TestFixture]
    [Ignore("todo")]
    class OverwriteFileVersionTests : PatchAssemblyTests
    {

        static XElement Config = XElement.Parse("<Stamp OverwriteFileVersion=\"false\" />");

        public OverwriteFileVersionTests(): base(Config)
        {
        }
    }
}