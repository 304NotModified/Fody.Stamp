using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class TaskTests
{
    private Assembly assembly;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private string beforeAssemblyPath;
    private string afterAssemblyPath;

    public TaskTests()
    {
        beforeAssemblyPath = AssemblyLocation.CreateBeforeAssemblyPath();

        afterAssemblyPath = beforeAssemblyPath.Replace(".dll", "2.dll");
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


    [Test]
    public void EnsureAttributeExists()
    {
        var customAttributes = (AssemblyInformationalVersionAttribute)assembly
            .GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false)
            .First();
        Assert.IsNotNull(customAttributes.InformationalVersion);
        Assert.IsNotEmpty(customAttributes.InformationalVersion);
        Trace.WriteLine(customAttributes.InformationalVersion);
    }

    [Test]
    public void Win32Resource()
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(afterAssemblyPath);
        Assert.IsNotNull(versionInfo.ProductVersion);
        Assert.IsNotEmpty(versionInfo.ProductVersion);
        Assert.IsNotNull(versionInfo.FileVersion);
        Assert.IsNotEmpty(versionInfo.FileVersion);
        Trace.WriteLine(versionInfo.ProductVersion);
        Trace.WriteLine(versionInfo.FileVersion);
    }


#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(beforeAssemblyPath, afterAssemblyPath);
    }
#endif

}