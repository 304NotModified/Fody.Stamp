using System;
using System.Diagnostics;
using LibGit2Sharp;
using NUnit.Framework;
using Stamp.Fody.Internal;

[TestFixture]
public class GitTests
{
    [Test]
    [Explicit]
    public void Foo()
    {
		using (var repo = new Repository(Repository.Discover(TestContext.CurrentContext.TestDirectory)))
		{
			var repositoryStatus = repo.RetrieveStatus();
			var clean =
				repositoryStatus.Added.IsEmpty() &&
				repositoryStatus.Missing.IsEmpty() &&
				repositoryStatus.Modified.IsEmpty() &&
				repositoryStatus.Removed.IsEmpty() &&
				repositoryStatus.Staged.IsEmpty();
			Trace.WriteLine(clean);
            Trace.WriteLine(repo.Head.FriendlyName);
            Trace.WriteLine(repo.Head.Tip.Sha);
        }
    }
}