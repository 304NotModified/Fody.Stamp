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
        beforeAssemblyPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll"));
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif

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

        AssertVersionWithSha(customAttributes.InformationalVersion);
    }

    [Test]
    public void Win32Resource()
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(afterAssemblyPath);
        
        AssertVersionWithSha(versionInfo.ProductVersion);
        
        Assert.IsNotNull(versionInfo.FileVersion);
        Assert.IsNotEmpty(versionInfo.FileVersion);
        Assert.AreEqual("1.0.0.0",versionInfo.FileVersion);
    }

    private static void AssertVersionWithSha(string version)
    {
        Assert.IsNotNull(version);
        Assert.IsNotEmpty(version);
        StringAssert.Contains("1.0.0.0", version, "Missing number");
        StringAssert.Contains("Head:", version, message: "Missing Head");
        StringAssert.Contains("Sha:", version, message: "Missing Sha");
        Trace.WriteLine(version);
    }

#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(beforeAssemblyPath, afterAssemblyPath);
    }
#endif

}