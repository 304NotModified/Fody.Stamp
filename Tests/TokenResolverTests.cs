namespace Stamp.Fody.Tests
{
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Mono.Cecil;
using NUnit.Framework;
using Version = System.Version;
using Stamp.Fody.Internal;

[TestFixture]
public class TokenResolverTests
{
    public Version version;


    public TokenResolverTests()
    {
        var beforeAssemblyPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll"));
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif
        var moduleDefinition = ModuleDefinition.ReadModule(beforeAssemblyPath);
        version = moduleDefinition.Assembly.Name.Version;

    }

    private void DoWithCurrentRepo(Action<Repository> doWithRepo)
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
                var result = FormatStringTokenResolver.ReplaceTokens("%version%", version, repo, "");

                Assert.AreEqual("1.0.0.0", result);
            });
    }

    [Test]
    public void Replace_version1()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = FormatStringTokenResolver.ReplaceTokens("%version1%", version, repo, "");

                Assert.AreEqual("1", result);
            });
    }

    [Test]
    public void Replace_version2()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = FormatStringTokenResolver.ReplaceTokens("%version2%", version, repo, "");

                Assert.AreEqual("1.0", result);
            });
    }

    [Test]
    public void Replace_version3()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = FormatStringTokenResolver.ReplaceTokens("%version3%", version, repo, "");

                Assert.AreEqual("1.0.0", result);
            });
    }

    [Test]
    public void Replace_version4()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = FormatStringTokenResolver.ReplaceTokens("%version4%", version, repo, "");

                Assert.AreEqual("1.0.0.0", result);
            });
    }

    [Test]
    public void Replace_branch()
    {
        DoWithCurrentRepo(repo =>
            {
                var branchName = repo.Head.FriendlyName;

                var result = FormatStringTokenResolver.ReplaceTokens("%branch%", version, repo, "");

                Assert.AreEqual(branchName, result);
            });
    }

    [Test]
    public void Replace_githash()
    {
        DoWithCurrentRepo(repo =>
            {
                var sha = repo.Head.Tip.Sha;

                var result = FormatStringTokenResolver.ReplaceTokens("%githash%", version, repo, "");

                Assert.AreEqual(sha, result);
            });
    }

    [Test]
    public void Replace_haschanges()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = FormatStringTokenResolver.ReplaceTokens("%haschanges%", version, repo, "HasChanges");

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

                var result = FormatStringTokenResolver.ReplaceTokens("%user%", version, repo, "");

                Assert.IsTrue(result.EndsWith(currentUser));
            });
    }

    [Test]
    public void Replace_machine()
    {
        DoWithCurrentRepo(repo =>
            {
                var machineName = Environment.MachineName;

                var result = FormatStringTokenResolver.ReplaceTokens("%machine%", version, repo, "");

                Assert.AreEqual(machineName, result);
            });
    }
    [Test]
    public void Replace_tags()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = FormatStringTokenResolver.ReplaceTokens("%lasttag%", version, repo, "");

                // tags in this repose should have the format %.%.%
                var match = Regex.IsMatch(result, @"^\d+\.\d+\.\d+$");
                Assert.IsTrue(match, "no match for '{0}'", result);
            });
    }

    [Test]
    public void Replace_time()
    {
        DoWithCurrentRepo(repo =>
        {
            var now = DateTime.Now;
            var utcNow = DateTime.UtcNow;

            Assert.AreEqual(now.ToString("yyMMdd"), FormatStringTokenResolver.ReplaceTokens("%now:yyMMdd%", version, repo, ""));
            Assert.AreEqual(utcNow.ToShortDateString(), FormatStringTokenResolver.ReplaceTokens("%utcnow%", version, repo, ""));
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
                var result = FormatStringTokenResolver.ReplaceTokens(replacementTokens, version, repo, "");

                Assert.AreEqual(expected, result);
            });
    }
    }
}