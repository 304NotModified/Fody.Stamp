using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibGit2Sharp;
using Mono.Cecil;
using Version = System.Version;
using Fody.PeImage;
using Fody.VersionResources;
using System.Reflection;

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
    readonly FormatStringTokenResolver formatStringTokenResolver;
    string assemblyInfoVersion;
    Version assemblyVersion;
    bool dotGitDirExists;
    static string stampDirectory = null;

    /// <summary>
    /// Initializes static members of the <see cref="ModuleWeaver"/> class. Makes sure
    /// the LibGit2Sharp.dll library is loaded correctly.
    /// </summary>
    static ModuleWeaver()
    {
        // Load the LibGitSharp assembly directly. A path needs to be specified when this assembly
        // is loaded to make sure the <dllmap> entries are loaded correctly on Mono (Linux/Mac OS support)
        // We assume Stamp.Fody is installed as a NuGet package in packages/; we can get the path to 
        // packages/ by loading any assembly that is loaded as a NuGet package, too.
        // Because Stamp is loaded dynamically, we cannot just get the location of the Stamp assembly - that
        // would be an empty value.
        // Instead, get Mono.Cecil (part of the Fody package), which we know lives in /packages,
        // and get the Stamp.Fody directory from there.
        // See https://github.com/Fody/Stamp/pull/15 for the motivation for this approach.
        //
        // This applies only if the Stamp.Fody assembly is loaded dynamically; if it is loaded using
        // a full path (e.g. when running Stamp.Fody's unit tests), this does not apply.
		if (string.IsNullOrEmpty(typeof(ModuleWeaver).Assembly.Location))
		{
			Console.WriteLine($"Dynamic assembly - Getting directory of {typeof(NativeType).Assembly.Location}");
            var stampPath = Path.GetDirectoryName(typeof(NativeType).Assembly.Location);

            // Stamp.Fody uses 3-digit version numbers
            var version = typeof(ModuleWeaver).Assembly.GetName().Version.ToString(3);

            stampDirectory = Path.Combine(Path.GetDirectoryName(stampPath), $"Stamp.Fody.{version}");
            var libGitPath = Path.Combine(stampDirectory, "LibGit2Sharp.dll");

            Assembly.LoadFrom(libGitPath);
        }
        else
        {
			Console.WriteLine($"Non dynamic assembly - Getting directory of {typeof(ModuleWeaver).Assembly.Location}"); 
            stampDirectory = Path.GetDirectoryName(typeof(ModuleWeaver).Assembly.Location);
        }
    }

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
        formatStringTokenResolver = new FormatStringTokenResolver();
    }

    public void Execute()
    {
        // When the native assemblies are loaded, the current directory *must* be set to the
        // directory of the Stamp.Fody assembly, because of some limitations on how Mono
        // handles the dllmap entries.
        // We set CurrentDirectory back to its original valuat the end of this script.
        var currentDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = stampDirectory;

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

            assemblyVersion = ModuleDefinition.Assembly.Name.Version;

            var customAttribute = customAttributes.FirstOrDefault(x => x.AttributeType.Name == "AssemblyInformationalVersionAttribute");
            if (customAttribute != null)
            {
                assemblyInfoVersion = (string)customAttribute.ConstructorArguments[0].Value;
                assemblyInfoVersion = formatStringTokenResolver.ReplaceTokens(assemblyInfoVersion, ModuleDefinition, repo, config.ChangeString);
                VerifyStartsWithVersion(assemblyInfoVersion);
                customAttribute.ConstructorArguments[0] = new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, assemblyInfoVersion);
            }
            else
            {
                var versionAttribute = GetVersionAttribute();
                var constructor = ModuleDefinition.ImportReference(versionAttribute.Methods.First(x => x.IsConstructor));
                customAttribute = new CustomAttribute(constructor);

                assemblyInfoVersion = $"{assemblyVersion} Head:'{repo.Head.FriendlyName}' Sha:{branch.Tip.Sha}{(repo.IsClean() ? "" : " " + config.ChangeString)}";

                customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, assemblyInfoVersion));
                customAttributes.Add(customAttribute);
            }
        }

        Environment.CurrentDirectory = currentDirectory;
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
        if (!dotGitDirExists)
        {
            return;
        }

        try
        {
            using (FileStream fileStream = File.Open(AssemblyFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                PeImage peReader = new PeImage(fileStream);
                peReader.ReadHeader();

                var versionStream = peReader.GetVersionResourceStream();

                var reader = new VersionResourceReader(versionStream);
                var versions = reader.Read();

                var fixedFileInfo = versions.FixedFileInfo.Value;
                fixedFileInfo.FileVersion = this.assemblyVersion;
                fixedFileInfo.ProductVersion = this.assemblyVersion;
                versions.FixedFileInfo = fixedFileInfo;

                foreach (var stringTable in versions.StringFileInfo)
                {
                    if (stringTable.Values.ContainsKey("FileVersion"))
                    {
                        stringTable.Values["FileVersion"] = this.assemblyVersion.ToString();
                    }

                    if (stringTable.Values.ContainsKey("ProductVersion"))
                    {
                        stringTable.Values["ProductVersion"] = this.assemblyInfoVersion;
                    }
                }

                versionStream.Position = 0;
                VersionResourceWriter writer = new VersionResourceWriter(versionStream);
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