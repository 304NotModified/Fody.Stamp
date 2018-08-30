using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using LibGit2Sharp;
using Mono.Cecil;
using Version = System.Version;
using Fody.PeImage;
using Fody.VersionResources;

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
    Version assemblyVersion;
    bool dotGitDirExists;

    private List<string> _osxFiles = new List<string>(); 

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
        formatStringTokenResolver = new FormatStringTokenResolver();
    }

    public void Execute()
    {
        SetSearchPath();

        var config = new Configuration(Config);

        LogInfo("Starting search for git repository in " + (config.UseProject ? "ProjectDir" : "SolutionDir"));


        var customAttributes = ModuleDefinition.Assembly.CustomAttributes;

        var gitDir = Repository.Discover( config.UseProject ? ProjectDirectoryPath : SolutionDirectoryPath );
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
                assemblyInfoVersion = (string) customAttribute.ConstructorArguments[0].Value;
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

    [DllImport("libc")]
    static extern int uname(IntPtr buf);

    //From Managed.Windows.Forms/XplatUI
    static bool IsRunningOnMac()
    {
        IntPtr buf = IntPtr.Zero;
        try
        {
            buf = Marshal.AllocHGlobal(8192);
            // This is a hacktastic way of getting sysname from uname ()
            if (uname(buf) == 0)
            {
                string os = Marshal.PtrToStringAnsi(buf);
                if (os == "Darwin")
                    return true;
            }
        }
        catch
        {
        }
        finally
        {
            if (buf != IntPtr.Zero)
                Marshal.FreeHGlobal(buf);
        }
        return false;
    }

    void SetSearchPath()
    {
        if (isPathSet)
        {
            return;
        }
        isPathSet = true;


        if (Path.DirectorySeparatorChar == '\\')
        {
            var nativeBinaries = Path.Combine(AddinDirectoryPath, "NativeBinaries", "win", GetProcessorArchitecture());
            var existingPath = Environment.GetEnvironmentVariable("PATH");
            var newPath = string.Concat(nativeBinaries, Path.PathSeparator, existingPath);
            Environment.SetEnvironmentVariable("PATH", newPath);
            LogInfo("Detected windows, set PATH to: " + newPath);
        }
        else if (IsRunningOnMac())
        {
            var nativeBinaries = Path.Combine(AddinDirectoryPath, "NativeBinaries", "osx");
            var existingPath = Environment.GetEnvironmentVariable("DYLD_LIBRARY_PATH");
            var newPath = string.Concat(nativeBinaries, Path.PathSeparator, existingPath);
            Environment.SetEnvironmentVariable("DYLD_LIBRARY_PATH", newPath);
            LogInfo("Detected osx, set DYLD_LIBRARY_PATH to: " + newPath);
            LogInfo("Detected osx, DYLD_LIBRARY_PATH might not be supported, also copying dylib to current dir: " + Directory.GetCurrentDirectory());
            string[] files = Directory.GetFiles(nativeBinaries);
            foreach (string s in files)
            {
                // Use static Path methods to extract only the file name from the path.
                var fileName = Path.GetFileName(s);
                var destFile = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                LogInfo($"Copying {fileName} to {destFile}");
                _osxFiles.Add(destFile);
                File.Copy(s, destFile, true);
            }
        }
        else if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            var nativeBinaries = Path.Combine(AddinDirectoryPath, "NativeBinaries", "linux");
            var existingPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
            var newPath = string.Concat(nativeBinaries, Path.PathSeparator, existingPath);
            Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", newPath);
            LogInfo("Detected linux, set LD_LIBRARY_PATH to: " + newPath);
        }
        else
        {
            throw new WeavingException("Could not determine OS");
        }
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
        var msCoreLib = ModuleDefinition.AssemblyResolver.Resolve(new AssemblyNameReference("mscorlib", null));
        var msCoreAttribute = msCoreLib.MainModule.Types.FirstOrDefault(x => x.Name == "AssemblyInformationalVersionAttribute");
        if (msCoreAttribute != null)
        {
            return msCoreAttribute;
        }
        var systemRuntime = ModuleDefinition.AssemblyResolver.Resolve(new AssemblyNameReference("System.Runtime", null));
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
            foreach (var osxFile in _osxFiles)
            {
                File.Delete(osxFile);
            }  
        }
        catch
        { }

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
                fixedFileInfo.FileVersion = assemblyVersion;
                fixedFileInfo.ProductVersion = assemblyVersion;
                versions.FixedFileInfo = fixedFileInfo;

                foreach (var stringTable in versions.StringFileInfo)
                {
                    if (stringTable.Values.ContainsKey("FileVersion"))
                    {
                        stringTable.Values["FileVersion"] = assemblyVersion.ToString();
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