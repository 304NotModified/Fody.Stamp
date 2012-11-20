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
        if (customAttributes.Any(x => x.AttributeType.Name == "AssemblyInformationalVersionAttribute"))
        {
            throw new WeavingException("Already contains AssemblyInformationalVersionAttribute.");
        }
        var gitDir = GitDirFinder.TreeWalkForGitDir(SolutionDirectoryPath);
        if (gitDir == null)
        {
            LogWarning("No .git directory found.");
            return;
        }
        var versionAttribute = GetVersionAttribute();
        var constructor = ModuleDefinition.Import(versionAttribute.Methods.First(x => x.IsConstructor));
        var customAttribute = new CustomAttribute(constructor);
        using (var repo = new Repository(gitDir))
        {
	        var version = GetVersion(repo);
	        customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, version));
        }
	    customAttributes.Add(customAttribute);
    }

	string GetVersion(Repository repo)
	{
		var branch = repo.Head;
		var sha = branch.Tip.Sha;
		var assemblyVersion = ModuleDefinition.Assembly.Name.Version;
		if (repo.IsClean())
		{
			return string.Format("{0} Head:'{1}' Sha:{2}", assemblyVersion, repo.Head.Name, sha);
		}
		return string.Format("{0} Head:'{1}' Sha:{2} HasPendingChanges", assemblyVersion, repo.Head.Name, sha);
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
        return msCoreLib.MainModule.Types.First(x => x.Name == "AssemblyInformationalVersionAttribute");
    }

  
}