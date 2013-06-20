using System;
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
    static bool isPathSet;

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
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
            var assemblyVersion = ModuleDefinition.Assembly.Name.Version;

            CustomAttribute customAttribute = customAttributes.FirstOrDefault(x => x.AttributeType.Name == "AssemblyInformationalVersionAttribute");
            string version;
            if (customAttribute != null)
            {
                version = (string)customAttribute.ConstructorArguments[0].Value;
                version = ReplaceTokens(version, assemblyVersion, repo, branch);

                customAttribute.ConstructorArguments[0] = new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, version);
            }
            else
            {
                var versionAttribute = GetVersionAttribute();
                var constructor = ModuleDefinition.Import(versionAttribute.Methods.First(x => x.IsConstructor));
                customAttribute = new CustomAttribute(constructor);
                if (repo.IsClean())
                {
                    version = string.Format("{0} Head:'{1}' Sha:{2}", assemblyVersion, repo.Head.Name, branch.Tip.Sha);
                }
                else
                {
                    version = string.Format("{0} Head:'{1}' Sha:{2} HasPendingChanges", assemblyVersion, repo.Head.Name, branch.Tip.Sha);
                }
                customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, version));
                customAttributes.Add(customAttribute);
            }
        }
    }

    string ReplaceTokens(string template, Version assemblyVersion, Repository repo, Branch branch)
    {
        template = template.Replace("%version%", assemblyVersion.ToString());
        template = template.Replace("%version1%", assemblyVersion.ToString(1));
        template = template.Replace("%version2%", assemblyVersion.ToString(2));
        template = template.Replace("%version3%", assemblyVersion.ToString(3));
        template = template.Replace("%version4%", assemblyVersion.ToString(4));

        template = template.Replace("%githash%", branch.Tip.Sha);
        
        template = template.Replace("%branch%", repo.Head.Name);
        
        template = template.Replace("%haschanges%", repo.IsClean() ? "" : "HasChanges");

        return template.Trim();
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
        var pativeBinaries = Path.Combine(AddinDirectoryPath, "NativeBinaries", GetProcessorArchitecture());
        var existingPath = Environment.GetEnvironmentVariable("PATH");
        var newPath = string.Concat(pativeBinaries, Path.PathSeparator, existingPath);
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
        var mscoreAttribute = msCoreLib.MainModule.Types.FirstOrDefault(x => x.Name == "AssemblyInformationalVersionAttribute");
        if (mscoreAttribute != null)
        {
            return mscoreAttribute;
        }
        var systemRuntime = ModuleDefinition.AssemblyResolver.Resolve("System.Runtime");
        return systemRuntime.MainModule.Types.First(x => x.Name == "AssemblyInformationalVersionAttribute");
    }
}