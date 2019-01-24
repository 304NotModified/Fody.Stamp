using System.Xml.Linq;
using NUnit.Framework;

namespace Stamp.Fody.Tests
{
    [TestFixture]
    class UseFileVersionTests : PatchAssemblyTests
    {

        static XElement Config = XElement.Parse("<Stamp UseFileVersion=\"true\" />");

        public UseFileVersionTests() : base(Config)
        {

        }
    }
}