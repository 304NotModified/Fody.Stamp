using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class TokenResolverTests
{
    System.Version version;
    FormatStringTokenResolver resolver;
    string beforeAssemblyPath;

    [OneTimeSetUp]
    public void FixtureSetUp()
    {
        beforeAssemblyPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll"));
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif
        var moduleDefinition = ModuleDefinition.ReadModule(beforeAssemblyPath);
        version = moduleDefinition.Assembly.Name.Version;

        resolver = new FormatStringTokenResolver();
    }

    void DoWithCurrentRepo(Action<Repository> doWithRepo)
    {
        using (var repo = new Repository(Repository.Discover(TestContext.CurrentContext.TestDirectory)))
        {
            doWithRepo?.Invoke(repo);
        }
    }

    [Test]
    public void Replace_version()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version%", version, repo, "");

                Assert.AreEqual("1.0.0.0", result);
            });
    }

    [Test]
    public void Replace_version1()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version1%", version, repo, "");

                Assert.AreEqual("1", result);
            });
    }

    [Test]
    public void Replace_version2()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version2%", version, repo, "");

                Assert.AreEqual("1.0", result);
            });
    }

    [Test]
    public void Replace_version3()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version3%", version, repo, "");

                Assert.AreEqual("1.0.0", result);
            });
    }

    [Test]
    public void Replace_version4()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version4%", version, repo, "");

                Assert.AreEqual("1.0.0.0", result);
            });
    }

    [Test]
    public void Replace_branch()
    {
        DoWithCurrentRepo(repo =>
            {
                var branchName = repo.Head.Name;

                var result = resolver.ReplaceTokens("%branch%", version, repo, "");

                Assert.AreEqual(branchName, result);
            });
    }

    [Test]
    public void Replace_githash()
    {
        DoWithCurrentRepo(repo =>
            {
                var sha = repo.Head.Tip.Sha;

                var result = resolver.ReplaceTokens("%githash%", version, repo, "");

                Assert.AreEqual(sha, result);
            });
    }

    [Test]
    public void Replace_haschanges()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%haschanges%", version, repo, "HasChanges");

                if (repo.IsClean())
                {
                    Assert.AreEqual(string.Empty, result);
                }
                else
                {
                    Assert.AreEqual("HasChanges", result);
                }
            });
    }

    [Test]
    public void Replace_user()
    {
        DoWithCurrentRepo(repo =>
            {
                var currentUser = Environment.UserName;

                var result = resolver.ReplaceTokens("%user%", version, repo, "");

                Assert.IsTrue(result.EndsWith(currentUser));
            });
    }

    [Test]
    public void Replace_machine()
    {
        DoWithCurrentRepo(repo =>
            {
                var machineName = Environment.MachineName;

                var result = resolver.ReplaceTokens("%machine%", version, repo, "");

                Assert.AreEqual(machineName, result);
            });
    }

    [Test]
    public void Replace_time()
    {
        DoWithCurrentRepo(repo =>
        {
            var now = DateTime.Now;
            var utcNow = DateTime.UtcNow;

            Assert.AreEqual(now.ToString("yyMMddmm"), resolver.ReplaceTokens("%now:yyMMddmm%", version, repo, ""));
            Assert.AreEqual(utcNow.ToShortDateString(), resolver.ReplaceTokens("%utcnow%", version, repo, ""));
        });
    }

    [Test]
    public void Replace_environment_variables()
    {
        DoWithCurrentRepo(repo =>
            {
                var environmentVariables = Environment.GetEnvironmentVariables();
                var expected = string.Join("--", environmentVariables.Values.Cast<string>());

                var replacementTokens = string.Join("--", environmentVariables.Keys.Cast<string>()
                                                                              .Select(key => "%env[" + key + "]%")
                                                                              .ToArray());
                var result = resolver.ReplaceTokens(replacementTokens, version, repo, "");

                Assert.AreEqual(expected, result);
            });
    }
}