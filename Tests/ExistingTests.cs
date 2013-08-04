using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using LibGit2Sharp;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class ExistingTests
{
    Assembly assembly;
    string beforeAssemblyPath;
    string afterAssemblyPath;

    public ExistingTests()
    {
        beforeAssemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcessExistingAttribute\bin\Debug\AssemblyToProcessExistingAttribute.dll");
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif

        afterAssemblyPath = beforeAssemblyPath.Replace(".dll", "2.dll");
        File.Copy(beforeAssemblyPath, afterAssemblyPath, true);

        var moduleDefinition = ModuleDefinition.ReadModule(afterAssemblyPath);
	    var currentDirectory = AssemblyLocation.CurrentDirectory();
        using (var moduleWeaver = new ModuleWeaver
                                  {
                                      ModuleDefinition = moduleDefinition,
                                      AddinDirectoryPath = currentDirectory,
                                      SolutionDirectoryPath = currentDirectory,
                                      AssemblyFilePath = afterAssemblyPath,
                                  })
        {
            moduleWeaver.Execute();
            moduleDefinition.Write(afterAssemblyPath);
        }

        assembly = Assembly.LoadFile(afterAssemblyPath);
    }


    [Test]
    public void EnsureAttributeExists()
    {
        var customAttributes = (AssemblyInformationalVersionAttribute)assembly.GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false).First();
        Assert.IsNotNullOrEmpty(customAttributes.InformationalVersion);
        Debug.WriteLine(customAttributes.InformationalVersion);
    }
    [Test]
    public void Win32Resource()
    {
        var productVersion = FileVersionInfo.GetVersionInfo(afterAssemblyPath).ProductVersion;

        using (var repo = new Repository(GitDirFinder.TreeWalkForGitDir(Environment.CurrentDirectory)))
        {
            var nameOfCurrentBranch = repo.Head.Name;
            Assert.True(productVersion.StartsWith("1.0.0+" + nameOfCurrentBranch + "."));
        }
    }


    [Test]
    public void TemplateIsReplaced()
    {
        using (var repo = new Repository(GitDirFinder.TreeWalkForGitDir(Environment.CurrentDirectory)))
        {
            var nameOfCurrentBranch = repo.Head.Name;
            
            var customAttributes = (AssemblyInformationalVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .First();
            Assert.True(customAttributes.InformationalVersion.StartsWith("1.0.0+"+nameOfCurrentBranch+"."));
        }
    }


#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(beforeAssemblyPath, afterAssemblyPath);
    }
#endif

}