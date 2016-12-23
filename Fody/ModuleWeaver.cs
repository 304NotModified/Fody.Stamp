using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Fody;

using LibGit2Sharp;
using Mono.Cecil;
using Version = System.Version;
using Fody.PeImage;
using Fody.VersionResources;
using Mono.Collections.Generic;
using Stamp.Fody;
using Stamp.Fody.Internal;

public class ModuleWeaver : BaseModuleWeaver
{
    private const string InfoAttributeName = nameof(System.Reflection.AssemblyInformationalVersionAttribute);
    private const string FileAttributeName = nameof(System.Reflection.AssemblyFileVersionAttribute);
    private static bool _isPathSet;
    private string _assemblyInfoVersion;
    private Version _versionToUse;
    private bool _dotGitDirExists;

   
    private Configuration _config;
    private bool _doPatch;

    private Collection<CustomAttribute> CustomAttributes => ModuleDefinition.Assembly.CustomAttributes;

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
    }

    public override void Execute()
    {
        SetSearchPath();

        var config = new Configuration(Config);
        _config = config;

        LogInfo("Starting search for git repository in " + (config.UseProject ? "ProjectDir" : "SolutionDir"));

        _doPatch = true;

        var gitDir = Repository.Discover(config.UseProject ? ProjectDirectoryPath : SolutionDirectoryPath);
        if (gitDir == null)
        {
            LogWarning("No .git directory found.");
            return;
        }
        LogInfo("Found git repository in " + gitDir);

        _dotGitDirExists = true;

        using (var repo = GetRepo(gitDir))
        {
            var branch = repo.Head;

            if (branch.Tip == null)
            {
                LogWarning("No Tip found. Has repo been initialized?");
                return;
            }

           

            if (_config.PatchInformationVersionSource == InformationVersionSource.Version)
                _versionToUse = ModuleDefinition.Assembly.Name.Version;
            else
                _versionToUse = GetAssemblyFileVersion(CustomAttributes);

            var assemblyInformationalVersionAttribute = GetAssemblyInformationalVersionAttribute(CustomAttributes);
            if (assemblyInformationalVersionAttribute != null)
            {
                //have already [AssemblyInformationalVersion], so read tokens and update 
                _assemblyInfoVersion = (string)assemblyInformationalVersionAttribute.ConstructorArguments[0].Value;
                _assemblyInfoVersion = CreateAssemblyInfoByReplacingTokens(repo, config);
                VerifyStartsWithVersion(_assemblyInfoVersion);
                assemblyInformationalVersionAttribute.ConstructorArguments[0] = CreateCustomFileVersionAttribute(_assemblyInfoVersion);
            }
            else
            {
                var versionAttribute = GetAssemblyInformationalVersionAttributeTypeInfo();
                var constructor = ModuleDefinition.ImportReference(versionAttribute.Methods.First(x => x.IsConstructor));

                _assemblyInfoVersion = CreateAssemblyInfoVersion(repo, branch, config);

                var customAttributeArgument = CreateCustomFileVersionAttribute(_assemblyInfoVersion);

                assemblyInformationalVersionAttribute = new CustomAttribute(constructor);
                assemblyInformationalVersionAttribute.ConstructorArguments.Add(customAttributeArgument);
                CustomAttributes.Add(assemblyInformationalVersionAttribute);
            }
        }
    }

    private CustomAttributeArgument CreateCustomFileVersionAttribute(string assemblyInfoVersion)
    {
        return new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, assemblyInfoVersion);
    }

    private string CreateAssemblyInfoByReplacingTokens(Repository repo, Configuration config)
    {
        return FormatStringTokenResolver.ReplaceTokens(_assemblyInfoVersion, _versionToUse, repo, config.ChangeString);
    }

    private string CreateAssemblyInfoVersion(Repository repo, Branch branch, Configuration config)
    {
        return $"{_versionToUse} Head:'{repo.Head.FriendlyName}' Sha:{branch.Tip.Sha}{(repo.IsClean() ? "" : " " + config.ChangeString)}";
    }

    private static CustomAttribute GetAssemblyInformationalVersionAttribute(ICollection<CustomAttribute> customAttributes)
    {
        var customAttribute = GetCustomAttribute(customAttributes, "AssemblyInformationalVersionAttribute");
        return customAttribute;
    }

    private Version GetAssemblyFileVersion(ICollection<CustomAttribute> customAttributes)
    {
        var afvAttribute = GetCustomAttribute(customAttributes, FileAttributeName);
        if (afvAttribute == null)
        {
            throw new WeavingException("AssemblyFileVersion attribute could not be found.");
        }

        var assemblyFileVersionString = (string) afvAttribute.ConstructorArguments[0].Value;
        VerifyStartsWithVersion(assemblyFileVersionString);
        return Version.Parse(assemblyFileVersionString);
    }

    private static CustomAttribute GetCustomAttribute(ICollection<CustomAttribute> attributes, string attributeName)
    {
        return attributes.FirstOrDefault(x => x.AttributeType.Name == attributeName);
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
        if (_isPathSet)
        {
            return;
        }
        _isPathSet = true;
        var nativeBinaries = Path.Combine(AddinDirectoryPath, "NativeBinaries", GetProcessorArchitecture());
        var existingPath = Environment.GetEnvironmentVariable("PATH");
        var newPath = string.Concat(nativeBinaries, Path.PathSeparator, existingPath);
        Environment.SetEnvironmentVariable("PATH", newPath);
    }

    private static string GetProcessorArchitecture()
    {
        return Environment.Is64BitProcess ? "amd64" : "x86";
    }

    private TypeDefinition GetAssemblyInformationalVersionAttributeTypeInfo()
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
        if (!_dotGitDirExists)
        {
            return;
        }

        if (!_doPatch)
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

                if (versions.FixedFileInfo != null)
                {
                    var fixedFileInfo = versions.FixedFileInfo.Value;

                if (_config.PatchFileVersion)
                {
                    fixedFileInfo.FileVersion = _versionToUse;
                }

                if (_config.PatchProductionVersion)
                {
                    fixedFileInfo.ProductVersion = _versionToUse;
                }

                    versions.FixedFileInfo = fixedFileInfo;
                }
                else
                {
                    LogWarning("versions.FixedFileInfo was null, so not patched");
                }

                foreach (var stringTable in versions.StringFileInfo)
                {
                    SetTableValue(stringTable.Values, "FileVersion", _versionToUse.ToString());
                    SetTableValue(stringTable.Values, "ProductVersion", _assemblyInfoVersion);
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

    private void SetTableValue(Dictionary<string, string> dict, string key, string value)
    {
        if (dict.ContainsKey(key))
            dict[key] = value;
    }
}