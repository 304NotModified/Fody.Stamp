using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibGit2Sharp;
using Mono.Cecil;
using Version = System.Version;
using Fody.PeImage;
using Fody.VersionResources;
using Mono.Collections.Generic;
using System.Collections.Generic;

public class ModuleWeaver
{
    public XElement Config { get; set; }
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public string SolutionDirectoryPath { get; set; }
    public string ProjectDirectoryPath { get; set; }
    public string AddinDirectoryPath { get; set; }
    public string AssemblyFilePath { get; set; }
    static bool isPathSet;
    readonly FormatStringTokenResolver formatStringTokenResolver;
    string assemblyInfoVersion;
    Version versionToUse;
    bool dotGitDirExists;

    Configuration _config;

    const string INFO_ATTRIBUTE = "AssemblyInformationalVersionAttribute";
    const string FILE_ATTRIBUTE = "AssemblyFileAttribute";

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
        formatStringTokenResolver = new FormatStringTokenResolver();
    }

    public void Execute()
    {
        SetSearchPath();

        _config = new Configuration(Config);

        LogInfo("Starting search for git repository in " + (_config.UseProject ? "ProjectDir" : "SolutionDir"));


        var customAttributes = ModuleDefinition.Assembly.CustomAttributes;

        var gitDir = Repository.Discover( _config.UseProject ? ProjectDirectoryPath : SolutionDirectoryPath );
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

            if (!_config.UseFileVersion)
                versionToUse = ModuleDefinition.Assembly.Name.Version;
            else
                versionToUse = GetAssemblyFileVersion(customAttributes);

            var customAttribute = GetCustomAttribute(customAttributes, INFO_ATTRIBUTE);
            if (customAttribute != null)
            {
                assemblyInfoVersion = (string) customAttribute.ConstructorArguments[0].Value;
                assemblyInfoVersion = formatStringTokenResolver.ReplaceTokens(assemblyInfoVersion, versionToUse, repo, _config.ChangeString);
                VerifyStartsWithVersion(assemblyInfoVersion);
                customAttribute.ConstructorArguments[0] = new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, assemblyInfoVersion);
            }
            else
            {
                var versionAttribute = GetVersionAttribute();
                var constructor = ModuleDefinition.ImportReference(versionAttribute.Methods.First(x => x.IsConstructor));
                customAttribute = new CustomAttribute(constructor);

                assemblyInfoVersion = $"{versionToUse} Head:'{repo.Head.FriendlyName}' Sha:{branch.Tip.Sha}{(repo.IsClean() ? "" : " " + _config.ChangeString)}";

                customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, assemblyInfoVersion));
                customAttributes.Add(customAttribute);
            }
        }
    }

    private CustomAttribute GetCustomAttribute(Collection<CustomAttribute> attributes, string attributeName)
    {
        return attributes.FirstOrDefault(x => x.AttributeType.Name == attributeName);
    }

    private Version GetAssemblyFileVersion(Collection<CustomAttribute> customAttributes)
    {        
        var afvAttribute = GetCustomAttribute(customAttributes, FILE_ATTRIBUTE);
        if (afvAttribute != null)
        {
            var assemblyFileVersionString = (string)afvAttribute.ConstructorArguments[0].Value;
            VerifyStartsWithVersion(assemblyFileVersionString);
            return Version.Parse(assemblyFileVersionString);
        }
        else
        {
            throw new WeavingException("AssemblyFileVersion attribute could not be found.");
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

    internal static Repository GetRepo(string gitDir)
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
        var msCoreAttribute = msCoreLib.MainModule.Types.FirstOrDefault(x => x.Name == INFO_ATTRIBUTE);
        if (msCoreAttribute != null)
        {
            return msCoreAttribute;
        }
        var systemRuntime = ModuleDefinition.AssemblyResolver.Resolve("System.Runtime");
        return systemRuntime.MainModule.Types.First(x => x.Name == INFO_ATTRIBUTE);
    }

    int? GetVerPatchWaitTimeout()
    {
        if (Config == null)
        {
            return null;
        }
        var xAttributes = Config.Attributes();
        var timeoutSetting = xAttributes.FirstOrDefault(attr => attr.Name.LocalName == "VerPatchWaitTimeoutInMilliseconds");
        if (timeoutSetting == null)
        {
            return null;
        }
        int timeoutInMilliseconds;
        if (int.TryParse(timeoutSetting.Value, out timeoutInMilliseconds) && timeoutInMilliseconds > 0)
        {
            return timeoutInMilliseconds;
        }
        return null;
    }

    public void AfterWeaving()
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
                if (_config.OverwriteFileVersion)
                    fixedFileInfo.FileVersion = versionToUse;
                fixedFileInfo.ProductVersion = versionToUse;
                versions.FixedFileInfo = fixedFileInfo;

                foreach (var stringTable in versions.StringFileInfo)
                {
                    if (_config.OverwriteFileVersion)
                        SetTableValue(stringTable.Values, "FileVersion", versionToUse.ToString());
                    SetTableValue(stringTable.Values, "ProductVersion", assemblyInfoVersion);
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