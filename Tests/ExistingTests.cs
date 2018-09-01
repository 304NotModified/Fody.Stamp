using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using LibGit2Sharp;
using Mono.Cecil;
using Xunit;

public class ExistingTests
{
    private Assembly assembly;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private string beforeAssemblyPath;

    private string afterAssemblyPath;

    public ExistingTests()
    {
        beforeAssemblyPath = AssemblyLocation.CreateBeforeAssemblyPath();

        afterAssemblyPath = AssemblyLocation.CreateAfter(beforeAssemblyPath);
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


    [Fact]
    public void EnsureAttributeExists()
    {
        var customAttributes = (AssemblyInformationalVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).First();
        Assert.NotNull(customAttributes.InformationalVersion);
        Assert.NotEmpty(customAttributes.InformationalVersion);
        Debug.WriteLine(customAttributes.InformationalVersion);
    }

    [Fact]
    public void Win32Resource()
    {
        var productVersion = FileVersionInfo.GetVersionInfo(afterAssemblyPath).ProductVersion;

        using (var repo = new Repository(Repository.Discover(Assembly.GetExecutingAssembly().CodeBase)))
        {
            var nameOfCurrentBranch = repo.Head.FriendlyName;
            Assert.StartsWith("1.0.0+" + nameOfCurrentBranch + ".", productVersion);
        }
    }


    [Fact]
    public void TemplateIsReplaced()
    {
        using (var repo = new Repository(Repository.Discover(Assembly.GetExecutingAssembly().CodeBase)))
        {
            var nameOfCurrentBranch = repo.Head.FriendlyName;

            var customAttributes = (AssemblyInformationalVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .First();
            Assert.StartsWith("1.0.0+" + nameOfCurrentBranch + ".", customAttributes.InformationalVersion);
        }
    }


#if(DEBUG)
    [Fact]
    public void PeVerify()
    {
        Verifier.Verify(beforeAssemblyPath, afterAssemblyPath);
    }
#endif

}