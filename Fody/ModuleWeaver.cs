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

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
    }

    public void Execute()
    {
        var customAttributes = ModuleDefinition.Assembly.CustomAttributes;
        if (customAttributes.Any(x => x.AttributeType.Name == "AssemblyInformationalVersionAttribute"))
        {
            throw new WeavingException("Already contains AssemblyInformationalVersionAttribute.");
        }
        var gitDir = TreeWalkForGitDir(SolutionDirectoryPath);
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
            var sha = repo.Head.Tip.Sha;
            var version = ModuleDefinition.Assembly.Name.Version +"/"+ sha;
            customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, version));
        }
        customAttributes.Add(customAttribute);
    }

    public TypeDefinition GetVersionAttribute()
    {
        var msCoreLib = ModuleDefinition.AssemblyResolver.Resolve("mscorlib");
       // AssemblyFileVersionAttribute
        // AssemblyInformationalVersionAttribute
        return msCoreLib.MainModule.Types.First(x => x.Name == "AssemblyInformationalVersionAttribute");
    }

    public string TreeWalkForGitDir(string currentDirectory)
    {
        while (true)
        {
            var gitDir = Path.Combine(currentDirectory, @".git");
            if (Directory.Exists(gitDir))
            {
                return gitDir;
            }
            try
            {
                var parent = Directory.GetParent(currentDirectory);
                if (parent == null)
                {
                    break;
                }
                currentDirectory = parent.FullName;
            }
            catch
            {
                // trouble with tree walk.
                return null;
            }
        }
        return null;
    }
}