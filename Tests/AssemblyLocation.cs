using System;
using System.IO;
using NUnit.Framework;

public static class AssemblyLocation
{
	public static string CurrentDirectory()
	{
		var assembly = typeof(AssemblyLocation).Assembly;
		var uri = new UriBuilder(assembly.CodeBase);
		var path = Uri.UnescapeDataString(uri.Path);

		return Path.GetDirectoryName(path);
	}

    public static string CreateBeforeAssemblyPath()
    {
        var beforeAssemblyPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\..\AssemblyToProcessExistingAttribute\bin\Debug\"));
        beforeAssemblyPath = Path.Combine(beforeAssemblyPath, @"netcoreapp2.0");
        beforeAssemblyPath = Path.Combine(beforeAssemblyPath, @"AssemblyToProcessExistingAttribute.dll");
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif
        return beforeAssemblyPath;
    }

    public static string CreateAfter(string beforeAssemblyPath)
    {
        return beforeAssemblyPath.Replace(".dll", $"{System.Guid.NewGuid().ToString()}.dll");
    }
}