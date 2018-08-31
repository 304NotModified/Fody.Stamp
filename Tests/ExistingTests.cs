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
    private Assembly assembly;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private string beforeAssemblyPath;

    private string afterAssemblyPath;

    public ExistingTests()
    {
        beforeAssemblyPath = AssemblyLocation.CreateBeforeAssemblyPath();

        afterAssemblyPath = beforeAssemblyPath.Replace(".dll", "2.dll");
        File.Copy(beforeAssemblyPath, afterAssemblyPath, true);

        ModuleWeaver moduleWeaver;
        using (var moduleDefinition = ModuleDefinition.ReadModule(beforeAssemblyPath))
        {
            var currentDirectory = AssemblyLocation.CurrentDirectory();
            moduleWeaver = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AddinDirectoryPath = currentDirectory,
                SolutionDirectoryPath = currentDirectory,
                AssemblyFilePath = afterAssemblyPath,
            };

            moduleWeaver.Execute();
            moduleDefinition.Write(afterAssemblyPath);
        }
        moduleWeaver.AfterWeaving();

        assembly = Assembly.LoadFile(afterAssemblyPath);
    }


    [Test]
    public void EnsureAttributeExists()
    {
        var customAttributes = (AssemblyInformationalVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).First();
        Assert.IsNotNull(customAttributes.InformationalVersion);
        Assert.IsNotEmpty(customAttributes.InformationalVersion);
        Debug.WriteLine(customAttributes.InformationalVersion);
    }

    [Test]
    public void Win32Resource()
    {
        var productVersion = FileVersionInfo.GetVersionInfo(afterAssemblyPath).ProductVersion;

        using (var repo = new Repository(Repository.Discover(TestContext.CurrentContext.TestDirectory)))
        {
            var nameOfCurrentBranch = repo.Head.FriendlyName;
            StringAssert.StartsWith("1.0.0+" + nameOfCurrentBranch + ".", productVersion);
        }
    }


    [Test]
    public void TemplateIsReplaced()
    {
        using (var repo = new Repository(Repository.Discover(TestContext.CurrentContext.TestDirectory)))
        {
            var nameOfCurrentBranch = repo.Head.FriendlyName;

            var customAttributes = (AssemblyInformationalVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .First();
            StringAssert.StartsWith("1.0.0+" + nameOfCurrentBranch + ".", customAttributes.InformationalVersion);
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