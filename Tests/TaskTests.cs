using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;
using System.Xml.Linq;
using System;

[TestFixture]
public class TaskTests
{
    Assembly assembly;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    string beforeAssemblyPath;
    string afterAssemblyPath;
    protected XElement config = null;

    [OneTimeSetUp]
    public void Setup()
    {
        beforeAssemblyPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll"));
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif

        afterAssemblyPath = beforeAssemblyPath.Replace(".dll", $"{Guid.NewGuid().ToString()}.dll");
        File.Copy(beforeAssemblyPath, afterAssemblyPath, true);

        var moduleDefinition = ModuleDefinition.ReadModule(afterAssemblyPath);

        var versionInfo = FileVersionInfo.GetVersionInfo(afterAssemblyPath);
        Trace.WriteLine(String.Format("Before: AssemblyVersion={0}, FileVersion={1}, Config={2}",
            moduleDefinition.Assembly.Name.Version, versionInfo.FileVersion, config));

        var currentDirectory = AssemblyLocation.CurrentDirectory();
        var weavingTask = new ModuleWeaver
        {
            ModuleDefinition = moduleDefinition,
            AddinDirectoryPath = currentDirectory,
            SolutionDirectoryPath = currentDirectory,
            AssemblyFilePath = afterAssemblyPath,
            Config = config
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
        Assert.IsNotNull(customAttributes.InformationalVersion);
        Assert.IsNotEmpty(customAttributes.InformationalVersion);
        Trace.WriteLine($"InfoVersion: {customAttributes.InformationalVersion}");
    }

    [Test]
    public void Win32Resource()
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(afterAssemblyPath);
        Assert.IsNotNull(versionInfo.ProductVersion);
        Assert.IsNotEmpty(versionInfo.ProductVersion);
        Assert.IsNotNull(versionInfo.FileVersion);
        Assert.IsNotEmpty(versionInfo.FileVersion);
        Trace.WriteLine($"ProductVersion: {versionInfo.ProductVersion}");
        Trace.WriteLine($"FileVersion: {versionInfo.FileVersion}");
    }


#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(beforeAssemblyPath, afterAssemblyPath);
    }
#endif

}

[TestFixture]
class UseFileVersionTests : TaskTests
{
    public UseFileVersionTests()
    {
        config = XElement.Parse("<Stamp UseFileVersion=\"true\" />");
    }
}

[TestFixture]
class OverwriteFileVersionTests : TaskTests
{
    public OverwriteFileVersionTests()
    {
        config = XElement.Parse("<Stamp OverwriteFileVersion=\"false\" />");
    }
}