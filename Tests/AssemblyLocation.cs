using System;
using System.IO;
using System.Reflection;

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
        var beforeAssemblyPath = Path.GetFullPath(Path.Combine(Assembly.GetExecutingAssembly().CodeBase, @"..\..\..\..\AssemblyToProcessExistingAttribute\bin\Debug\"));

#if NETCORE21
        beforeAssemblyPath = Path.Combine(beforeAssemblyPath, @"netcoreapp2.0");
#elif NET452
         beforeAssemblyPath = Path.Combine(beforeAssemblyPath, @"net452");
#else
#error Fix path for this platform

#endif
        beforeAssemblyPath = Path.Combine(beforeAssemblyPath, @"AssemblyToProcessExistingAttribute.dll");
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif
        return beforeAssemblyPath;
    }

    public static string CreateAfter(string beforeAssemblyPath)
    {
        return beforeAssemblyPath.Replace(".dll", $"{Guid.NewGuid().ToString()}.dll");
    }
}