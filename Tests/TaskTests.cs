using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class TaskTests
{
    Assembly assembly;
    string beforeAssemblyPath;
    string afterAssemblyPath;

    public TaskTests()
    {
        beforeAssemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll");
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif

        afterAssemblyPath = beforeAssemblyPath.Replace(".dll", "2.dll");
        File.Copy(beforeAssemblyPath, afterAssemblyPath, true);

        var moduleDefinition = ModuleDefinition.ReadModule(afterAssemblyPath);
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

        assembly = Assembly.LoadFile(afterAssemblyPath);
    }


    [Test]
    public void EnsureAttributeExists()
    {
        var customAttributes = (AssemblyInformationalVersionAttribute)assembly
            .GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false)
            .First();
        Assert.IsNotNullOrEmpty(customAttributes.InformationalVersion);
        Debug.WriteLine(customAttributes.InformationalVersion);
    }

    [Test]
    public void Win32Resource()
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(afterAssemblyPath);
        Assert.IsNotNullOrEmpty(versionInfo.ProductVersion);
        Assert.IsNotNullOrEmpty(versionInfo.FileVersion);
        Debug.WriteLine(versionInfo.ProductVersion);
        Debug.WriteLine(versionInfo.FileVersion);
    }


#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(beforeAssemblyPath, afterAssemblyPath);
    }
#endif

}