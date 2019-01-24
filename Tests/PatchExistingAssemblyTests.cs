namespace Stamp.Fody.Tests
{
    using LibGit2Sharp;
    using NUnit.Framework;

    public class PatchExistingAssemblyTests : PatchAssemblyTestsBase
    {
        public PatchExistingAssemblyTests() : base("AssemblyToProcessExistingAttribute")
        {
        }

        protected override void AssertProductionVersion(string version)
        {
            AssertVersionWithBranch(version);
        }

        protected override void AssertFileVersion(string version)
        {
            Assert.AreEqual("1.0.0.0", version);
        }

        protected override void AssertInformationalVersion(string version)
        {
            AssertVersionWithBranch(version);
        }

        private static void AssertVersionWithBranch(string version)
        {
            using (var repo = new Repository(Repository.Discover(TestContext.CurrentContext.TestDirectory)))
            {
                var nameOfCurrentBranch = repo.Head.FriendlyName;
                StringAssert.StartsWith("1.0.0+" + nameOfCurrentBranch + ".", version);
            }

        }
    }
}