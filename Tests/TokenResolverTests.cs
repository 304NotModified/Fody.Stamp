using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LibGit2Sharp;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class TokenResolverTests
{
    private Repository _repo;
    private ModuleDefinition _moduleDefinition;
    private FormatStringTokenResolver _resolver;

    [TestFixtureSetUp]
    public void FixtureSetUp()
    {
        var beforeAssemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll");
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif

        _repo = new Repository(GitDirFinder.TreeWalkForGitDir(Environment.CurrentDirectory));
        _moduleDefinition = ModuleDefinition.ReadModule(beforeAssemblyPath);

        _resolver = new FormatStringTokenResolver();
    }

    [Test]
    public void Replace_version()
    {
        var result = _resolver.ReplaceTokens("%version%", _moduleDefinition, _repo);

        Assert.AreEqual("1.0.0.0", result);
    }

    [Test]
    public void Replace_version1()
    {
        var result = _resolver.ReplaceTokens("%version1%", _moduleDefinition, _repo);

        Assert.AreEqual("1", result);
    }

    [Test]
    public void Replace_version2()
    {
        var result = _resolver.ReplaceTokens("%version2%", _moduleDefinition, _repo);

        Assert.AreEqual("1.0", result);
    }

    [Test]
    public void Replace_version3()
    {
        var result = _resolver.ReplaceTokens("%version3%", _moduleDefinition, _repo);

        Assert.AreEqual("1.0.0", result);
    }

    [Test]
    public void Replace_version4()
    {
        var result = _resolver.ReplaceTokens("%version4%", _moduleDefinition, _repo);

        Assert.AreEqual("1.0.0.0", result);
    }
}