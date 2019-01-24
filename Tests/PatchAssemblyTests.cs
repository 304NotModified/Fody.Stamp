namespace Stamp.Fody.Tests
{
    using System;
    using System.Diagnostics;
    using NUnit.Framework;

    public class PatchAssemblyTests : PatchAssemblyTestsBase
    {
        public PatchAssemblyTests() : base("AssemblyToProcess")
        {
        }

        protected override void AssertProductionVersion(string version)
        {
            AssertVersionWithHeadAndSha(version);
        }

        protected override void AssertFileVersion(string version)
        {
            Assert.AreEqual("1.0.0.0", version);
        }

        protected override void AssertInformationalVersion(string version)
        {
            AssertVersionWithHeadAndSha(version);
        }

        private static void AssertVersionWithHeadAndSha(string version)
        {
            Assert.IsNotNull(version);
            Assert.IsNotEmpty(version);
            StringAssert.Contains("1.0.0.0", version, "Missing number");
            StringAssert.Contains("Head:", version, "Missing Head");
            StringAssert.Contains("Sha:", version, "Missing Sha");
            Trace.WriteLine(version);
        }
    }
}