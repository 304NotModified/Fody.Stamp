using System;
using System.Diagnostics;
using LibGit2Sharp;
using NUnit.Framework;

[TestFixture]
public class GitTests
{
    [Test]
    [Ignore]
    public void Foo()
    {
		using (var repo = new Repository(GitDirFinder.TreeWalkForGitDir(Environment.CurrentDirectory)))
		{
			var repositoryStatus = repo.Index.RetrieveStatus();
			var clean =
				repositoryStatus.Added.IsEmpty() &&
				repositoryStatus.Missing.IsEmpty() &&
				repositoryStatus.Modified.IsEmpty() &&
				repositoryStatus.Removed.IsEmpty() &&
				repositoryStatus.Staged.IsEmpty();
			Debug.WriteLine(clean);
			Debug.WriteLine(repo.Head.Name);
            Debug.WriteLine(repo.Head.Tip.Sha);
        }
    }
    [Test]
    [Ignore]
    public void Foo2()
    {
		using (var repo = new Repository(@"D:\Code\Anotar"))
		{
			var repositoryStatus = repo.Index.RetrieveStatus();
			var clean =
				repositoryStatus.Added.IsEmpty() &&
				repositoryStatus.Missing.IsEmpty() &&
				repositoryStatus.Modified.IsEmpty() &&
				repositoryStatus.Removed.IsEmpty() &&
				repositoryStatus.Staged.IsEmpty();
			Debug.WriteLine(clean);
			Debug.WriteLine(repo.Head.Name);
            Debug.WriteLine(repo.Head.Tip.Sha);
        }
    }
}