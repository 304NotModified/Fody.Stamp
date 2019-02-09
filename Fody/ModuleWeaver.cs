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
using Stamp.Fody.Internal;

public class ModuleWeaver : BaseModuleWeaver
{
    private static bool _isPathSet;
    private readonly FormatStringTokenResolver _formatStringTokenResolver;
    private string _assemblyInfoVersion;
    private Version _assemblyVersion;
    private bool _dotGitDirExists;

    private Collection<CustomAttribute> CustomAttributes => ModuleDefinition.Assembly.CustomAttributes;

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
        _formatStringTokenResolver = new FormatStringTokenResolver();
    }

    public override void Execute()
    {
        SetSearchPath();

        var config = new Configuration(Config);

        LogInfo("Starting search for git repository in " + (config.UseProject ? "ProjectDir" : "SolutionDir"));

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

            _assemblyVersion = ModuleDefinition.Assembly.Name.Version;

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
        return _formatStringTokenResolver.ReplaceTokens(_assemblyInfoVersion, ModuleDefinition, repo, config.ChangeString);
    }

    private string CreateAssemblyInfoVersion(Repository repo, Branch branch, Configuration config)
    {
        return $"{_assemblyVersion} Head:'{repo.Head.FriendlyName}' Sha:{branch.Tip.Sha}{(repo.IsClean() ? "" : " " + config.ChangeString)}";
    }

    private static CustomAttribute GetAssemblyInformationalVersionAttribute(Collection<CustomAttribute> customAttributes)
    {
        var customAttribute = customAttributes.FirstOrDefault(x => x.AttributeType.Name == "AssemblyInformationalVersionAttribute");
        return customAttribute;
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
                    fixedFileInfo.FileVersion = _assemblyVersion;
                    fixedFileInfo.ProductVersion = _assemblyVersion;
                    versions.FixedFileInfo = fixedFileInfo;
                }
                else
                {
                    LogWarning("versions.FixedFileInfo was null, so not patched");
                }

                foreach (var stringTable in versions.StringFileInfo)
                {
                    SetTableValue(stringTable.Values, "FileVersion", _assemblyVersion.ToString());
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