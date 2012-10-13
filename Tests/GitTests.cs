using System.Diagnostics;
using LibGit2Sharp;
using NUnit.Framework;

[TestFixture]
public class GitTests
{
    [Test]
    public void Foo()
    {
        using (var repo = new Repository(@"C:\Users\Simon\Code\GitHub\NotifyPropertyWeaver\.git"))
        {  
            Debug.WriteLine(repo.Head.Tip.Sha);
        }
    }
}