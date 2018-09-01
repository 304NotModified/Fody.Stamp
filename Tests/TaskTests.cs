using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Xunit;

public class TaskTests
{
    private Assembly assembly;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private string beforeAssemblyPath;
    private string afterAssemblyPath;

    public TaskTests()
    {
        beforeAssemblyPath = AssemblyLocation.CreateBeforeAssemblyPath();

        afterAssemblyPath = AssemblyLocation.CreateAfter(beforeAssemblyPath);
        File.Copy(beforeAssemblyPath, afterAssemblyPath, true);

        using (var moduleDefinition = ModuleDefinition.ReadModule(beforeAssemblyPath))
        {
            var currentDirectory = AssemblyLocation.CurrentDirectory();

            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AddinDirectoryPath = currentDirectory,
                SolutionDirectoryPath = currentDirectory,
                AssemblyFilePath = afterAssemblyPath,
            };

            weavingTask.Execute();
            moduleDefinition.Write(afterAssemblyPath);

            weavingTask.AfterWeaving();
        }

        assembly = Assembly.LoadFile(afterAssemblyPath);
    }


    [Fact]
    public void EnsureAttributeExists()
    {
        var customAttributes = (AssemblyInformationalVersionAttribute)assembly
            .GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false)
            .First();
        Assert.NotNull(customAttributes.InformationalVersion);
        Assert.NotEmpty(customAttributes.InformationalVersion);
        Trace.WriteLine(customAttributes.InformationalVersion);
    }

    [Fact]
    public void Win32Resource()
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(afterAssemblyPath);
        Assert.NotNull(versionInfo.ProductVersion);
        Assert.NotEmpty(versionInfo.ProductVersion);
        Assert.NotNull(versionInfo.FileVersion);
        Assert.NotEmpty(versionInfo.FileVersion);
        Trace.WriteLine(versionInfo.ProductVersion);
        Trace.WriteLine(versionInfo.FileVersion);
    }


#if(DEBUG)
    [Fact]
    public void PeVerify()
    {
        Verifier.Verify(beforeAssemblyPath, afterAssemblyPath);
    }
#endif

}