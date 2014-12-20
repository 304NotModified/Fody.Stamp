using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class TokenResolverTests
{
    ModuleDefinition moduleDefinition;
    FormatStringTokenResolver resolver;

    [TestFixtureSetUp]
    public void FixtureSetUp()
    {
        var beforeAssemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll");
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif
        moduleDefinition = ModuleDefinition.ReadModule(beforeAssemblyPath);

        resolver = new FormatStringTokenResolver();
    }

    void DoWithCurrentRepo(Action<Repository> doWithRepo)
    {
        using (var repo = new Repository(Repository.Discover(Environment.CurrentDirectory)))
        {
            if (doWithRepo != null) doWithRepo(repo);
        }
    }

    [Test]
    public void Replace_version()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version%", moduleDefinition, repo, "");

                Assert.AreEqual("1.0.0.0", result);
            });
    }

    [Test]
    public void Replace_version1()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version1%", moduleDefinition, repo, "");

                Assert.AreEqual("1", result);
            });
    }

    [Test]
    public void Replace_version2()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version2%", moduleDefinition, repo, "");

                Assert.AreEqual("1.0", result);
            });
    }

    [Test]
    public void Replace_version3()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version3%", moduleDefinition, repo, "");

                Assert.AreEqual("1.0.0", result);
            });
    }

    [Test]
    public void Replace_version4()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version4%", moduleDefinition, repo, "");

                Assert.AreEqual("1.0.0.0", result);
            });
    }

    [Test]
    public void Replace_branch()
    {
        DoWithCurrentRepo(repo =>
            {
                var branchName = repo.Head.Name;

                var result = resolver.ReplaceTokens("%branch%", moduleDefinition, repo, "");

                Assert.AreEqual(branchName, result);
            });
    }

    [Test]
    public void Replace_githash()
    {
        DoWithCurrentRepo(repo =>
            {
                var sha = repo.Head.Tip.Sha;

                var result = resolver.ReplaceTokens("%githash%", moduleDefinition, repo, "");

                Assert.AreEqual(sha, result);
            });
    }

    [Test]
    public void Replace_haschanges()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%haschanges%", moduleDefinition, repo, "HasChanges");

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

                var result = resolver.ReplaceTokens("%user%", moduleDefinition, repo, "");

                Assert.IsTrue(result.EndsWith(currentUser));
            });
    }

    [Test]
    public void Replace_machine()
    {
        DoWithCurrentRepo(repo =>
            {
                var machineName = Environment.MachineName;

                var result = resolver.ReplaceTokens("%machine%", moduleDefinition, repo, "");

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

            Assert.AreEqual(now.ToString("yyMMddmm"), resolver.ReplaceTokens("%now:yyMMddmm%", moduleDefinition, repo, ""));
            Assert.AreEqual(utcNow.ToShortDateString(), resolver.ReplaceTokens("%utcnow%", moduleDefinition, repo, ""));
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
                var result = resolver.ReplaceTokens(replacementTokens, moduleDefinition, repo, "");

                Assert.AreEqual(expected, result);
            });
    }
}