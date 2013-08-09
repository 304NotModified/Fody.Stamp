using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Mono.Cecil;

public class ModuleWeaver
{
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public string SolutionDirectoryPath { get; set; }
    public string AddinDirectoryPath { get; set; }
    public string AssemblyFilePath { get; set; }
    static bool isPathSet;
    readonly FormatStringTokenResolver formatStringTokenResolver;
    string assemblyInfoVersion;
    Version assemblyVersion;

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
        formatStringTokenResolver = new FormatStringTokenResolver();
    }

    public void Execute()
    {
        SetSearchPath();
        var customAttributes = ModuleDefinition.Assembly.CustomAttributes;

        var gitDir = GitDirFinder.TreeWalkForGitDir(SolutionDirectoryPath);
        if (gitDir == null)
        {
            LogWarning("No .git directory found.");
            return;
        }

        using (var repo = GetRepo(gitDir))
        {
            var branch = repo.Head;
            if (branch.Tip == null)
            {
                LogWarning("No Tip found. Has repo been initialize?");
                return;
            }
            assemblyVersion = ModuleDefinition.Assembly.Name.Version;

            var customAttribute = customAttributes.FirstOrDefault(x => x.AttributeType.Name == "AssemblyInformationalVersionAttribute");
            if (customAttribute != null)
            {
                assemblyInfoVersion = (string) customAttribute.ConstructorArguments[0].Value;
                assemblyInfoVersion = formatStringTokenResolver.ReplaceTokens(assemblyInfoVersion, ModuleDefinition, repo);
                VerifyStartsWithVersion(assemblyInfoVersion);
                customAttribute.ConstructorArguments[0] = new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, assemblyInfoVersion);
            }
            else
            {
                var versionAttribute = GetVersionAttribute();
                var constructor = ModuleDefinition.Import(versionAttribute.Methods.First(x => x.IsConstructor));
                customAttribute = new CustomAttribute(constructor);
                if (repo.IsClean())
                {
                    assemblyInfoVersion = string.Format("{0} Head:'{1}' Sha:{2}", assemblyVersion, repo.Head.Name, branch.Tip.Sha);
                }
                else
                {
                    assemblyInfoVersion = string.Format("{0} Head:'{1}' Sha:{2} HasPendingChanges", assemblyVersion, repo.Head.Name, branch.Tip.Sha);
                }
                customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, assemblyInfoVersion));
                customAttributes.Add(customAttribute);
            }
        }
    }

    void VerifyStartsWithVersion(string versionString)
    {
        var prefix = new string(versionString.TakeWhile(x => char.IsDigit(x) || x == '.').ToArray());
        Version fake;
        if (!Version.TryParse(prefix, out fake))
        {
            throw new WeavingException("The version string must be prefixed with a valid Version. The following string does not: " + versionString);
        }
    }

    static Repository GetRepo(string gitDir)
    {
        try
        {
            return new Repository(gitDir);
        }
        catch (Exception exception)
        {
            if (exception.Message.Contains("LibGit2Sharp.Core.NativeMethods") || exception.Message.Contains("FilePathMarshaler"))
            {
                throw new WeavingException("Restart of Visual Studio required due to update of 'Stamp.Fody'.");
            }
            throw;
        }
    }

    void SetSearchPath()
    {
        if (isPathSet)
        {
            return;
        }
        isPathSet = true;
        var nativeBinaries = Path.Combine(AddinDirectoryPath, "NativeBinaries", GetProcessorArchitecture());
        var existingPath = Environment.GetEnvironmentVariable("PATH");
        var newPath = string.Concat(nativeBinaries, Path.PathSeparator, existingPath);
        Environment.SetEnvironmentVariable("PATH", newPath);
    }

    static string GetProcessorArchitecture()
    {
        if (Environment.Is64BitProcess)
        {
            return "amd64";
        }
        return "x86";
    }

    TypeDefinition GetVersionAttribute()
    {
        var msCoreLib = ModuleDefinition.AssemblyResolver.Resolve("mscorlib");
        var msCoreAttribute = msCoreLib.MainModule.Types.FirstOrDefault(x => x.Name == "AssemblyInformationalVersionAttribute");
        if (msCoreAttribute != null)
        {
            return msCoreAttribute;
        }
        var systemRuntime = ModuleDefinition.AssemblyResolver.Resolve("System.Runtime");
        return systemRuntime.MainModule.Types.First(x => x.Name == "AssemblyInformationalVersionAttribute");
    }

    public void AfterWeaving()
    {
        var verPatchPath = Path.Combine(AddinDirectoryPath, "verpatch.exe");
        var arguments = string.Format("{0} /pv \"{1}\" /high /va {2}", AssemblyFilePath, assemblyInfoVersion, assemblyVersion);
        LogInfo(string.Format("Patching version using: {0} {1}", verPatchPath, arguments));
        var startInfo = new ProcessStartInfo
                        {
                            FileName = verPatchPath,
                            Arguments = arguments,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            WorkingDirectory = Path.GetTempPath()
                        };
        using (var process = Process.Start(startInfo))
        {
            process.WaitForExit(200);

            if (process.ExitCode == 0)
            {
                return;
            }
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            var message = string.Format(
@"Failed to apply product version to Win32 resources.
Output: {0}
Error: {1}", output, error);
            throw new WeavingException(message);
        }
    }
}