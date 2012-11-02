using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

[TestFixture]
public class TaskTests
{
    Assembly assembly;

    public TaskTests()
    {
        var projectPath = @"AssemblyToProcess\AssemblyToProcess.csproj";
#if (!DEBUG)
        projectPath = projectPath.Replace("Debug", "Release");
#endif
        var weaverHelper = new WeaverHelper(projectPath);
        assembly = weaverHelper.Assembly;
    }


    [Test]
    public void EnsureAttributeExists()
    {

        var customAttributes = (AssemblyInformationalVersionAttribute)assembly.GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false).First();
        Debug.WriteLine(customAttributes.InformationalVersion);
    }


#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assembly.CodeBase.Remove(0, 8));
    }
#endif

}