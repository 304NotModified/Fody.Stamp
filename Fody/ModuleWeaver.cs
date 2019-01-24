using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Fody;

using LibGit2Sharp;
using Mono.Cecil;
using Version = System.Version;
using Fody.PeImage;
using Fody.VersionResources;
using Stamp.Fody;
using Stamp.Fody.Internal;

public class ModuleWeaver : BaseModuleWeaver
{
    private static bool isPathSet;
    private string assemblyInfoVersion;

    private bool dotGitDirExists;


    private const string InfoAttributeName = nameof(System.Reflection.AssemblyInformationalVersionAttribute);
    private const string FileAttributeName = nameof(System.Reflection.AssemblyFileVersionAttribute);

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
    }

    public override void Execute()
    {
        SetSearchPath();

        var config = new Configuration(Config);

        LogInfo("Starting search for git repository in " + (config.UseProject ? "ProjectDir" : "SolutionDir"));

        var customAttributes = ModuleDefinition.Assembly.CustomAttributes;

        var gitDir = Repository.Discover(config.UseProject ? ProjectDirectoryPath : SolutionDirectoryPath);
        if (gitDir == null)
        {
            LogWarning("No .git directory found.");
            return;
        }
        LogInfo("Found git repository in " + gitDir);

        dotGitDirExists = true;

        using (var repo = GetRepo(gitDir))
        {
            var branch = repo.Head;

            if (branch.Tip == null)
            {
                LogWarning("No Tip found. Has repo been initialized?");
                return;
            }

           

            var customAttribute = GetCustomAttribute(customAttributes, InfoAttributeName);
            if (customAttribute != null)
            {
                assemblyInfoVersion = (string)customAttribute.ConstructorArguments[0].Value;
                assemblyInfoVersion = FormatStringTokenResolver.ReplaceTokens(assemblyInfoVersion, versionToUse, repo, config.ChangeString);
                VerifyStartsWithVersion(assemblyInfoVersion);
                customAttribute.ConstructorArguments[0] = new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, assemblyInfoVersion);
            }
            else
            {
                var versionAttribute = GetVersionAttribute();
                var constructor = ModuleDefinition.ImportReference(versionAttribute.Methods.First(x => x.IsConstructor));
                customAttribute = new CustomAttribute(constructor);



                customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, assemblyInfoVersion));
                customAttributes.Add(customAttribute);
            }

            if (config.PatchFileVersion)
            {

            }

            if (config.PatchVersion)
            {

            }


            if (config.PatchInformationVersion)
            {
                Version versionToUse;
                if (config.PatchInformationVersionSource == InformationVersionSource.FileVersion)
                {
                    versionToUse = GetAssemblyFileVersion(customAttributes);
                }
                else
                {
                    versionToUse = ModuleDefinition.Assembly.Name.Version;
                }

                assemblyInfoVersion = CreateAssemblyInfoVersion(repo, branch, config, versionToUse);
            }
        }
    }

    private static string CreateAssemblyInfoVersion(Repository repo, Branch branch, Configuration config, Version versionToUse)
    {
        return $"{versionToUse} Head:'{repo.Head.FriendlyName}' Sha:{branch.Tip.Sha}{(repo.IsClean() ? "" : " " + config.ChangeString)}";
    }

    private static CustomAttribute GetCustomAttribute(ICollection<CustomAttribute> attributes, string attributeName)
    {
        return attributes.FirstOrDefault(x => x.AttributeType.Name == attributeName);
    }

    private Version GetAssemblyFileVersion(ICollection<CustomAttribute> customAttributes)
    {
        var afvAttribute = GetCustomAttribute(customAttributes, FileAttributeName);
        if (afvAttribute == null)
        {
            throw new WeavingException("AssemblyFileVersion attribute could not be found.");
        }

        var assemblyFileVersionString = (string)afvAttribute.ConstructorArguments[0].Value;
        VerifyStartsWithVersion(assemblyFileVersionString);
        return Version.Parse(assemblyFileVersionString);
    }

    private void VerifyStartsWithVersion(string versionString)
    {
        var prefix = new string(versionString.TakeWhile(x => char.IsDigit(x) || x == '.').ToArray());
        if (!Version.TryParse(prefix, out _))
        {
            throw new WeavingException("The version string must be prefixed with a valid Version. The following string does not: " + versionString);
        }
    }

    private static Repository GetRepo(string gitDir)
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

    private void SetSearchPath()
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

    private static string GetProcessorArchitecture()
    {
        return Environment.Is64BitProcess ? "amd64" : "x86";
    }

    private TypeDefinition GetVersionAttribute()
    {
        var msCoreLib = ModuleDefinition.AssemblyResolver.Resolve(new AssemblyNameReference("mscorlib", null));
        var msCoreAttribute = msCoreLib.MainModule.Types.FirstOrDefault(x => x.Name == "AssemblyInformationalVersionAttribute");
        if (msCoreAttribute != null)
        {
            return msCoreAttribute;
        }
        var systemRuntime = ModuleDefinition.AssemblyResolver.Resolve(new AssemblyNameReference("System.Runtime", null));
        return systemRuntime.MainModule.Types.First(x => x.Name == "AssemblyInformationalVersionAttribute");
    }

    /// <summary>
    /// Return a list of assembly names for scanning.
    /// Used as a list for <see cref="P:Fody.BaseModuleWeaver.FindType" />.
    /// </summary>
    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield break;
    }

    public override void AfterWeaving()
    {
        if (!dotGitDirExists)
        {
            return;
        }

        try
        {
            using (var fileStream = File.Open(AssemblyFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                var peReader = new PeImage(fileStream);
                peReader.ReadHeader();
                peReader.CalculateCheckSum();

                var versionStream = peReader.GetVersionResourceStream();

                var reader = new VersionResourceReader(versionStream);
                var versions = reader.Read();

                var fixedFileInfo = versions.FixedFileInfo.Value;
                fixedFileInfo.FileVersion = versionToUse;
                fixedFileInfo.ProductVersion = versionToUse;
                versions.FixedFileInfo = fixedFileInfo;

                foreach (var stringTable in versions.StringFileInfo)
                {
                    if (stringTable.Values.ContainsKey("FileVersion"))
                    {
                        stringTable.Values["FileVersion"] = versionToUse.ToString();
                    }

                    if (stringTable.Values.ContainsKey("ProductVersion"))
                    {
                        stringTable.Values["ProductVersion"] = assemblyInfoVersion;
                    }
                }

                versionStream.Position = 0;
                var writer = new VersionResourceWriter(versionStream);
                writer.Write(versions);
                peReader.SetVersionResourceStream(versionStream);

                peReader.WriteCheckSum();
            }
        }
        catch (Exception ex)
        {
            throw new WeavingException($"Failed to update the assembly information. {ex.Message}");
        }
    }
}