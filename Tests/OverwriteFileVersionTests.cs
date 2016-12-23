using System.Xml.Linq;
using NUnit.Framework;

namespace Stamp.Fody.Tests
{
    [TestFixture]
    class OverwriteFileVersionTests : PatchAssemblyTests
    {

        static XElement Config = XElement.Parse("<Stamp OverwriteFileVersion=\"false\" />");

        public OverwriteFileVersionTests(): base(Config)
        {
        }
    }
}