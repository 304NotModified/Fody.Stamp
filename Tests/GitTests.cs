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
        using (var repo = new Repository(@".git"))
        {  
            Debug.WriteLine(repo.Head.Tip.Sha);
        }
    }
}