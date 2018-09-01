using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Mono.Cecil;
using Xunit;

public class TokenResolverTests
{
    private ModuleDefinition moduleDefinition;
    private FormatStringTokenResolver resolver;


    public TokenResolverTests()
    {
        var beforeAssemblyPath = AssemblyLocation.CreateBeforeAssemblyPath();


        moduleDefinition = ModuleDefinition.ReadModule(beforeAssemblyPath);

        resolver = new FormatStringTokenResolver();
    }

    private void DoWithCurrentRepo(Action<Repository> doWithRepo)
    {
        using (var repo = new Repository(Repository.Discover(Assembly.GetExecutingAssembly().CodeBase)))
        {
            doWithRepo?.Invoke(repo);
        }
    }

    [Fact]
    public void Replace_version()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version%", moduleDefinition, repo, "");

                Assert.Equal("1.0.0.0", result);
            });
    }

    [Fact]
    public void Replace_version1()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version1%", moduleDefinition, repo, "");

                Assert.Equal("1", result);
            });
    }

    [Fact]
    public void Replace_version2()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version2%", moduleDefinition, repo, "");

                Assert.Equal("1.0", result);
            });
    }

    [Fact]
    public void Replace_version3()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version3%", moduleDefinition, repo, "");

                Assert.Equal("1.0.0", result);
            });
    }

    [Fact]
    public void Replace_version4()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%version4%", moduleDefinition, repo, "");

                Assert.Equal("1.0.0.0", result);
            });
    }

    [Fact]
    public void Replace_branch()
    {
        DoWithCurrentRepo(repo =>
            {
                var branchName = repo.Head.FriendlyName;

                var result = resolver.ReplaceTokens("%branch%", moduleDefinition, repo, "");

                Assert.Equal(branchName, result);
            });
    }

    [Fact]
    public void Replace_githash()
    {
        DoWithCurrentRepo(repo =>
            {
                var sha = repo.Head.Tip.Sha;

                var result = resolver.ReplaceTokens("%githash%", moduleDefinition, repo, "");

                Assert.Equal(sha, result);
            });
    }

    [Fact]
    public void Replace_haschanges()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%haschanges%", moduleDefinition, repo, "HasChanges");

                if (repo.IsClean())
                {
                    Assert.Equal(string.Empty, result);
                }
                else
                {
                    Assert.Equal("HasChanges", result);
                }
            });
    }

    [Fact]
    public void Replace_user()
    {
        DoWithCurrentRepo(repo =>
            {
                var currentUser = Environment.UserName;

                var result = resolver.ReplaceTokens("%user%", moduleDefinition, repo, "");

                Assert.True(result.EndsWith(currentUser));
            });
    }

    [Fact]
    public void Replace_machine()
    {
        DoWithCurrentRepo(repo =>
            {
                var machineName = Environment.MachineName;

                var result = resolver.ReplaceTokens("%machine%", moduleDefinition, repo, "");

                Assert.Equal(machineName, result);
            });
    }
    [Fact]
    public void Replace_tags()
    {
        DoWithCurrentRepo(repo =>
            {
                var result = resolver.ReplaceTokens("%lasttag%", moduleDefinition, repo, "");

                // tags in this repose should have the format %.%.%
                var match = Regex.IsMatch(result, @"^\d+\.\d+\.\d+$");
                Assert.True(match, $"no match for '{result}'");
            });
    }

    [Fact]
    public void Replace_time()
    {
        DoWithCurrentRepo(repo =>
        {
            var now = DateTime.Now;
            var utcNow = DateTime.UtcNow;

            Assert.Equal(now.ToString("yyMMddmm"), resolver.ReplaceTokens("%now:yyMMddmm%", moduleDefinition, repo, ""));
            Assert.Equal(utcNow.ToShortDateString(), resolver.ReplaceTokens("%utcnow%", moduleDefinition, repo, ""));
        });
    }

    [Fact]
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

                Assert.Equal(expected, result);
            });
    }
}